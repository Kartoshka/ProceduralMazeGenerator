using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

//TODO: Compartementalize the add start room and add end room code

/*
 * Maze generation using a recursive backtracking algorithm 
 * Overall algorithm:
 * 	Spawn rooms in the maze where possible
 * 	Spawn start point and end point at the south and north edges respectively of the maze
 * 	Fill in the empty space with corridors using the recursive backtracking algorithm (described underneath)
 * 	Connect the rooms and the start and end point with the maze
 * 	Find all dead ends and cave them in until we are left with a specific amount
 *
 * Reference: http://journal.stuffwithstuff.com/2014/12/21/rooms-and-mazes/
 */

public class RecursiveMazeGen : AbMazeManager {

	#region RoomInfo
	//Number of rooms to spawn
	public int numOfRooms = 3;
	//Dimensions of rooms
	public int roomHeight = 4;
	public int roomWidth = 4;
	#endregion

	#region dead end configuration
	//How many dead ends to have left at the end
	public int deadEndsToLeave = 5;
	//Flag used during the recrusive algorithm to determine whether we have come back from a dead end or not
    private bool advancing = false;

	//A list of all the dead ends we have
	private ArrayList deadEndLocations = new ArrayList();

	#endregion

	//Used to keep track of empty cells we need to fill during the fill out phase
	private ArrayList locationsLeft = new ArrayList();


	public override void Init()
	{
		base.Init ();

		//Reset and initialize data
		deadEndLocations.Clear ();
		//Initialize the maze
		mazeCells = new Cell[mazeWidth, mazeHeight];

		//Initialize the maze to be completely empty
		for (int i = 0; i < mazeWidth; i++)
		{
			for (int j = 0; j < mazeHeight; j++)
			{
				
				SetCell (null, new IntVector2 (i, j));
				locationsLeft.Add (new IntVector2 (i, j));

			}
		}
	}

	public override void GenerateMaze()
	{

		//Add start and End rooms 

		//Spawn the start room at the south most edge
		IntVector2 startPos = new IntVector2 (Random.Range (0, mazeWidth - 1),0);
		AddCell (startPos, CellType.Start);
		//Make an opening to open the maze
        RemoveWall(startPos, Directions.SOUTH);


		//Spawn the exit at the northmost edge
		IntVector2 endPos = new IntVector2 (Random.Range (0, mazeWidth - 1), mazeHeight-1);
		AddCell (endPos, CellType.End);
		//Make an opening to exit the maze
        RemoveWall(endPos, Directions.NORTH);

		//Keep track of our start and end point so we can position our maze
        this.endPoint = endPos;
		this.startPoint = startPos;

		//Areas are no longer available to be filled out
		locationsLeft.Remove (startPos);
		locationsLeft.Remove (endPos);

		//Spawn rooms in the maze
		GenerateRooms ();

		//Fill the empty space between rooms and start and exit
		FillEmptySpace ();

		//Connect start and end point to corridor

		//We open a random wall of the start/end point to connect it to the maze, but we keep track of how many times we've failed to increase the probability by the end.
		int attempts = 0;

		//create a flag to keep track of whether we successfully opened a wall or not, the attempts flag cannot actually ensure we open a wall if the last wall we check does have a cell behind it
		bool wallOpened = false;
		while (!wallOpened)
		{
			for (int c = 0; c < 4; c++)
			{
				//We check whether there's a cell on the other side of the wall and it isn't a room (we don't want to create a loop).
				Cell outCell;
				GetCell (startPos + ((Directions)c).ToVector (), out outCell);
				if (outCell != null && outCell.type != CellType.Room)
				{
					//Random chance to select a wall, chance increases so that if none is selected by the end it's guarnateed to select the last wall
					if (Random.Range (0.0f, 1.0f) < 1.0f*(++attempts) / 4)
					{
						Cell roomCell;
						GetCell (startPos,out roomCell);

						//Remove walls of the current cell and the cell on the other side of the wall
						roomCell.RemoveWall (((Directions)c));
						outCell.RemoveWall (((Directions)c).Opposite ());

						if (outCell.type == CellType.DeadEnd)
						{
							deadEndLocations.Remove (outCell.position);
						}
						outCell.type = CellType.Corridor;
						wallOpened = true;
						break;
					}
				}
			}
		}

		//TODO: put code into function to avoid the copy/paste 
		//Same code as above but for the exit
		wallOpened = false;
		attempts = 0;
		while (!wallOpened)
		{
			for (int c = 0; c < 4; c++)
			{
				Cell outCell;
				GetCell (endPos + ((Directions)c).ToVector (), out outCell);
				if (outCell != null && outCell.type != CellType.Room)
				{
					//TODO: look at how I randomly pick walls again, it is functional but maybe there's a way that results in cooler mazes?
					//Random chance to select a wall, chance increases so that if none is selected by the end it's guarnateed to select the last wall
					if (Random.Range (0.0f, 1.0f) < 1.0f*(++attempts) / 4)
					{
						Cell roomCell;
						GetCell (endPos,out roomCell);

						roomCell.RemoveWall (((Directions)c));
						outCell.RemoveWall (((Directions)c).Opposite ());

						if (outCell.type == CellType.DeadEnd)
						{
							deadEndLocations.Remove (outCell.position);
						}
						outCell.type = CellType.Corridor;
						wallOpened = true;
						break;
					}
				}
			}
		}


		//Connect rooms to the maze
		ConnectRooms ();

		//Cave in the dead ends to make the maze sparse
		CaveInDeadEnds ();
    }

	//choose an available location (no cell) and start generating a maze from there
	private void CaveInDeadEnds()
	{
		while (deadEndLocations.Count > deadEndsToLeave)
		{
			IntVector2 pos = deadEndLocations [0] as IntVector2;
			deadEndLocations.RemoveAt (0);
			RecCloseCorridor (pos);
		}
	}


	//Given a cell which is a dead end (has 3 walls) it will cave in the entire passage leading to that dead end
	//It will stop when it reaches the point from which the corridor branched off.
	//The algorithm is recursive
	private void RecCloseCorridor(IntVector2 pos)
	{
		
		Cell deadEnd = GetCell (pos);
		//Base case 0, not a cell
		if (deadEnd == null)
		{
			return;
		}
		else if (deadEnd.numWalls < 3)
		{
			//Base case 1, this is not a dead end, we can stop because it's a corridor
			return;
		} else if (deadEnd.numWalls == 4)
		{
			//Base case 2: we've caved into a cell that leads to nothing else (means we didn't start at a dead end or we caved the entire maze (woops)
			RemoveCell (pos);
		} 
		else
		{
			//Recursive call: There is only one open wall, find it 
			Directions cardinalDir = Directions.NONE;
			for (int i = 0; i < 4; i++)
			{
				if (!deadEnd.walls [i])
				{
					cardinalDir = ((Directions)i);
					break;
				}
			}

			//Remove our current cell (because we're a dead end)
			RemoveCell (pos);
			//Add a wall to the cell we lead to
			AddWall(pos + cardinalDir.ToVector(), cardinalDir.Opposite());
			//Recursively call the function on the cell we lead to
			RecCloseCorridor (pos + cardinalDir.ToVector ());
		}
	}

	//Fils in all empty locations with cells using the generation algorithm
	private void FillEmptySpace()
	{
		//When there are no more empty cells, we're done
		while (locationsLeft.Count>0)
		{
			//Pick a random empty cell and start the recursive algorithm
			int rngIndex = Random.Range (0, locationsLeft.Count-1);
			IntVector2 posRecStart = locationsLeft [rngIndex] as IntVector2;

			//remove that location from the list
			locationsLeft.RemoveAt (rngIndex);
			//Try to put a cell there
			Cell startCell = AddCell (posRecStart, CellType.DeadEnd);

			if (startCell != null)
			{
				//This cell is now the start point ( a dead end) to a new maze that will fill out the empty space it can reach
				deadEndLocations.Add (posRecStart);
				//Start the recursive backtracking algorithm on it
				RecGeneration(posRecStart);
			}
		}
	}

	//Recursive backtracking
	// Summary: 
	// Start at a cell
	// for every unoccupied neighbour
	// 	pick a random one
	// make a connection to that neighbour and call the recursive algorithm on that cell
	private void RecGeneration(IntVector2 cellPos)
	{
		Cell cell;
		GetCell(cellPos, out cell);

		if (cell != null)
		{
			//We want to ignore neighbours we've already visited, so we keep a list of "available neighbours" from which we pick
			while (cell.availableWalls.Count > 0)
			{
				Directions dir = cell.availableWalls.First.Value;
				IntVector2 neighbourPos = cell.availableWalls.First.Value.ToVector() + cellPos;
				//See whether the randomly chosen available neighbour cell has not been visited
				if (inBounds(neighbourPos) && !cellOccupied[neighbourPos.x,neighbourPos.y])
				{
					//If so, open the wall in that direction
					RemoveWall(cellPos,dir);

					//Add a cell in that location
					AddCell (neighbourPos, CellType.Corridor);
					//Remove a wall at that new cell that points back to the current cell
					RemoveWall(neighbourPos, dir.Opposite());
					//The neighbour is now no longer empty!
					locationsLeft.Remove (neighbourPos);

					//We are also going to call the recursive algorithm, so flag it so we can detect dead ends
					advancing = true;
					//Recursively call the algorithm on this new neighbour
					RecGeneration(neighbourPos);
				}
				else
				{
					//Remove the availability of a wall because there's a cell to the side
					cell.availableWalls.RemoveFirst();
				}
			}

			//The base case is that we no longer have any neighbouring cells to visit, so we're done with this cell and are now going up in the recursion

			//We keep track of whether we came up from the recursion for the first time (not going up multiple times)
			//That indicates we had just hit a dead end!
			if(advancing)
			{
				//When we go back up, it is not a dead end (naturally) so falg it false
				advancing = false;
				//Specify that the cell is a dead end and add it to the list
				cell.type = CellType.DeadEnd;
				deadEndLocations.Add (cellPos);
			}
		}
	}

	//TODO: Potential problem is that it'll get stuck in an infinite loop if the maze is too small for the rooms you want to fit
	/*
	 * Generate rooms in all the empty spots until we generate the required number of rooms
	 */
	private void GenerateRooms()
	{
		rooms = new Room[numOfRooms];

		//For the number of rooms we want
		for (int i = 0; i < numOfRooms; i++)
		{
			IntVector2 pos = IntVector2.zero;
			//We keep trying to place the room by generating random locations and seeing if it fits
			//Problem: in theory can cause infinite loop if our maze doesn't have enough area
			do
			{
				pos = new IntVector2 (Random.Range (0, mazeWidth - 1), Random.Range (0, mazeHeight - 1));

			} while(!RoomFits (this.roomWidth, this.roomHeight, pos));

			rooms[i] = AddRoom (this.roomWidth, this.roomHeight, pos);
		}

	}

	//Check whether the room fits, we know it's a rectangle so it's a simple calculation
	private bool RoomFits(int roomWidth, int roomHeight, IntVector2 startPoint)
	{
		for (int i = 0; i < roomWidth; i++)
		{
			for (int j = 0; j < roomHeight; j++)
			{
				IntVector2 cellLocation = startPoint + new IntVector2 (i, j);
				if (!inBounds (cellLocation) || GetCell (cellLocation) != null)
				{
					return false;
				}
			}
		}

		return true;
	}

	//Connect the rooms to the maze around them (assuming there is a maze next to us :P)
	private void ConnectRooms()
	{
		//Connect every room to the maze
		foreach (Room r in rooms)
		{
			if (r.connected)
			{
				return;
			}
			bool roomDone = false;

			//Keep attempting to connect the room until we do (problem: if there is no way to connect it aka the room is isolated, it'll not work)
			while (!roomDone)
			{
				//Like for the start and exit point, we modify the probability of spawning an opening basd on the number of attempts.
				int attempts = 0;
				Cell outCell;

				//Check every wall in the room and pick those that could connect us
				for (int i = 0; i < r.width; i++)
				{
					for (int j = 0; j < r.height; j++)
					{
						for (int c = 0; c < 4; c++)
						{
							GetCell (r.cells [i, j] + ((Directions)c).ToVector (), out outCell);
							if (outCell != null && outCell.type != CellType.Room)
							{
								//Random chance to select a wall, chance increases so that if none is selected by the end it's guarnateed to select the last wall
								if (Random.Range (0.0f, 1.0f) < 1.0f*(++attempts) / (roomWidth * roomHeight))
								{
									//If we've chosen a cell, then 
									Cell roomCell;
									GetCell (r.cells [i, j],out roomCell);

									RemoveWall (roomCell.position,((Directions)c));
                                    roomCell.type = CellType.RoomEntry;

									RemoveWall (outCell.position,((Directions)c).Opposite ());

									//Flag it as an entrance to a room
                                    outCell.type = CellType.CorridorToRoom;
									//If that position was a dead end, it no longer is
									deadEndLocations.Remove (outCell.position);
									roomDone = true;
                                    r.exit = roomCell;

									break;
								}
							}
						}
						if (roomDone)
						{
							break;
						}
					}
					if (roomDone)
					{
						break;
					}
				}
			}

			r.connected = roomDone;
		}
	}

	//Spawn a room in an area defined by a position and dimensions assuming it fits
	private Room AddRoom(int roomWidth, int roomHeight, IntVector2 startPoint)
	{
		Room result = new Room ();
		result.width = roomWidth;
		result.height = roomHeight;
		result.cells = new IntVector2[roomWidth, roomHeight];
        result.pos = startPoint;

		if (roomWidth == 1 && roomHeight == 1)
		{
			AddCell (startPoint, CellType.Room);
			result.cells [0, 0] = startPoint;
			locationsLeft.Remove (startPoint);
		} else
		{
			for (int i = 0; i < roomWidth; i++)
			{
				for (int j = 0; j < roomHeight; j++)
				{
					IntVector2 cellLocation = startPoint + new IntVector2 (i, j);
					Cell newCell = new Cell (cellLocation);
					newCell.type = CellType.Room;
					if (i == 0)
					{
						newCell.RemoveWall (Directions.EAST);
					} else if (i == roomWidth - 1)
					{
						newCell.RemoveWall (Directions.WEST);
					} else
					{
						newCell.RemoveWall (Directions.EAST);
						newCell.RemoveWall (Directions.WEST);
					}

					if (j == 0)
					{
						newCell.RemoveWall (Directions.NORTH);
					} else if (j == roomHeight - 1)
					{
						newCell.RemoveWall (Directions.SOUTH);
					} else
					{
						newCell.RemoveWall (Directions.SOUTH);
						newCell.RemoveWall (Directions.NORTH);
					}

					SetCell (newCell, cellLocation);
					cellOccupied [cellLocation.x, cellLocation.y] = true;
					result.cells [i, j] = cellLocation;
					locationsLeft.Remove (cellLocation);
				}
			}
		}
		return result;
	}






}
