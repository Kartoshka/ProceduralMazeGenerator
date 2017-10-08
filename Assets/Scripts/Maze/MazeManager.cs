using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;


/*
 * AbMazeManager is a class which gathers all common methods required to generate the maze
 * It takes care of actually instantiating the maze once it is completed
 * Children of this class must implement GenerateMaze() which is required to fill up the maze array with the result of whatever algorithm it implements.
 */ 
public abstract class AbMazeManager : MonoBehaviour {

    public delegate void GameLogicAction(AbMazeManager source);
    public static event GameLogicAction OnMazeSolved;

    //TODO: Connect the maze to an arbitrary start and end point (make a method that generates a corridor between two different areas, it's totally detached from the cells
    //TODO: Create keys, and exit logic

	//Prefabs for the entrance and exit of the maze
    public CellInstance startPrefab;
    public CellInstance endPrefab;

	//prefabs for the floor and ceiling cell gameobjects of the maze
    public CellInstance groundPrefab;
    public CellInstance roomFloorPrefab;
    public RoomTriggerTrap roomTrigger;

	//prefab for a dead end 
    public CellInstance deadEndPrefab;

	//Prefabs for walls (regular maze walls and exit maze walls)
    public GameObject wallPrefab;
	public UnlockDoor exitPrefab;

	//Keep an instance of the exit door to notify it of key additions
	private UnlockDoor exitInstance;

	//Location at which our maze will move to attach itself
    public Transform entryAttachPosition;
	//Exit corridor we will spawn at the end of the maze for the exit
    public GameObject exitCorridor;

	//Enemy prefab to spawn enemies
    public MazeTraversal enemy;
	//Key prefab to spawn keys
	public Key keyPrefab;

    //A cell in the maze is a cube cellsize * cellsize * cellsize
    public int cellSize = 1;
    //Dimensions of the maze
    [SerializeField]
    public int mazeWidth = 10;
    [SerializeField]
    public int mazeHeight = 10;

	//We have an array of the cells at each index and an array of booleans for faster checks
    protected Cell[,] mazeCells;
    protected bool[,] cellOccupied;

	//We keep a list of the rooms we've spawned and their entry points
    protected LinkedList<Cell> roomEntries;
    protected Room[] rooms;

    //Used to keep track of where we set our entrance and exit
    protected IntVector2 startPoint = IntVector2.zero;
 	protected IntVector2 endPoint = IntVector2.zero;

	//Initialize any variables required
    public virtual void Init()
    {
        roomEntries = new LinkedList<Cell>();
        cellOccupied = new bool[mazeWidth, mazeHeight];
    }

	//Instantiate every cell of the maze
    public void InstantiateMaze()
    {
        for (int i = 0; i < mazeWidth; i++)
        {
            for (int j = 0; j < mazeHeight; j++)
            {
                Cell c = GetCell(new IntVector2(i, j));
                if (c != null)
                {
                    InstantiateCell(c);
                }
            }
        }
    }

	//Instantiate the exit corridor of the maze
    public void InstantiateEnd()
    {
        if (exitCorridor != null)
        {
			//We know the exit will have nothing to its north because we spawn it at that range
            Vector3 offset = new Vector3(Directions.NORTH.ToVector().x * cellSize, 0, Directions.NORTH.ToVector().y * cellSize);
            Instantiate(exitCorridor, ToGlobalPos(endPoint) + offset, exitCorridor.transform.rotation);
        }
    }

	//Instantiate enemies and allow them to calculate their trajectories through the maze
	//They aren't active when we spawn them, they will activate when triggered
    public void SpawnEnemies()
    {
        if (enemy != null)
        {
            int i = 1;
            foreach (Room r in rooms)
            {
                Vector3 middle = (ToGlobalPos(r.pos) + ToGlobalPos(r.pos + new IntVector2(r.width - 1, r.height - 1))) * 0.5f;

                MazeTraversal traverser = Instantiate(enemy.gameObject, middle + new Vector3(0, cellSize * 0.5f, 0), Quaternion.identity).GetComponent<MazeTraversal>();
                traverser.gameObject.transform.SetParent(this.gameObject.transform);
				//Initialize the enemy which doesn't set him as active, just makes it generate its path it will travel and be ready for when it is activated
                traverser.Init(this, r.exit);
                traverser.triggerCell = r.exit;
                traverser.name = "Enemy " + (i++);
            }
        }
    }

	//Instantiate a cell based on its type
    private void InstantiateCell(Cell c)
    {
        Vector3 cellPos = new Vector3(c.position.x * cellSize, 0, c.position.y * cellSize);
        CellInstance instanceCell;
        if (c.type == CellType.DeadEnd)
        {
            instanceCell = Instantiate(deadEndPrefab.gameObject, Vector3.zero, Quaternion.identity).GetComponent<CellInstance>();
        } else if (c.type == CellType.Room)
        {
            instanceCell = Instantiate(roomFloorPrefab.gameObject, Vector3.zero, Quaternion.identity).GetComponent<CellInstance>();
        } else if (c.type == CellType.Start)
        {
            instanceCell = Instantiate(startPrefab.gameObject, Vector3.zero, Quaternion.identity).GetComponent<CellInstance>();
        } else if (c.type == CellType.End)
        {
            instanceCell = Instantiate(endPrefab.gameObject, Vector3.zero, Quaternion.identity).GetComponent<CellInstance>();
        }
        else if(c.type == CellType.RoomEntry)
        {
            instanceCell = Instantiate(roomTrigger.gameObject, Vector3.zero, Quaternion.identity).GetComponent<CellInstance>();
        }
        else
        {
            instanceCell = Instantiate(groundPrefab.gameObject, Vector3.zero, Quaternion.identity).GetComponent<CellInstance>();
        }

		//Spawn a ceiling
        GameObject ceiling = Instantiate(groundPrefab.gameObject, Vector3.zero, Quaternion.identity);
        ceiling.name = "Ceiling";
        ceiling.transform.localScale = new Vector3(cellSize, instanceCell.transform.localScale.y, cellSize);

        instanceCell.name = "Cell (" + c.position.x + "," + c.position.y + ")";
        instanceCell.transform.localScale = new Vector3(cellSize, instanceCell.transform.localScale.y, cellSize);
        instanceCell.associatedCell = c;

        ceiling.transform.localPosition = cellPos + new Vector3(0, cellSize, 0);
        instanceCell.gameObject.transform.SetParent(this.gameObject.transform);
        instanceCell.transform.localPosition = cellPos;
        ceiling.transform.SetParent(instanceCell.gameObject.transform);

		//Spawn each wall based on its position and direction
        for (int i = 0; i < 4; i++)
        {
            if (c.walls[i])
            {
                Quaternion rotation = ((Directions)i).ToRotation();
                IntVector2 vec2 = ((Directions)i).ToVector();
                //Might want to multiply by cellSize, but right now we don't have to because the parent cell is scaled anyway
                Vector3 wallPos = new Vector3(cellSize*vec2.x  * 0.5f, cellSize*0.5f, cellSize*vec2.y * 0.5f) + cellPos;

				GameObject instanceWall=  Instantiate(wallPrefab, Vector3.zero, rotation);

                instanceWall.transform.localScale = new Vector3(cellSize, instanceWall.transform.localScale.y, cellSize);
                instanceWall.transform.localPosition = wallPos;
                instanceWall.name = ((Directions)i).ToString() + "Wall";
                instanceWall.transform.SetParent(instanceCell.gameObject.transform);
            }
			//If we're spawning the end cell, we'll spawn the locked door to prevent player from escaping
			else if (c.type == CellType.End && ((Directions)i) != Directions.NORTH)
			{
				Quaternion rotation = ((Directions)i).ToRotation();
				IntVector2 vec2 = ((Directions)i).ToVector();
				//TODO: Might want to multiply by cellSize, but right now we don't have to because the parent cell is scaled anyway
				Vector3 wallPos = new Vector3(cellSize*vec2.x  * 0.5f, cellSize*0.5f, cellSize*vec2.y * 0.5f) + cellPos;

				exitInstance =  Instantiate(exitPrefab, Vector3.zero, rotation) as UnlockDoor;
				exitInstance.transform.localScale = new Vector3(cellSize, exitInstance.transform.localScale.y, cellSize);

				exitInstance.transform.localPosition = wallPos;
				exitInstance.name = ((Directions)i).ToString() + "LOCKED DOOR";
				exitInstance.transform.SetParent(instanceCell.gameObject.transform);
			} 
        }
    }

	//Instatiate keys
	public void SpawnKeys()
	{
		if (keyPrefab != null)
		{
			foreach (Room r in rooms)
			{
				//Spawn a key in the middle of the room
				Vector3 middle = (ToGlobalPos(r.pos) + ToGlobalPos(r.pos + new IntVector2(r.width - 1, r.height - 1))) * 0.5f;

				Key key = Instantiate (keyPrefab.gameObject, middle + new Vector3 (0, cellSize * 0.5f, 0), Quaternion.identity).GetComponent<Key>();

				//Notify the locked exit we've added a key
				if (exitInstance != null)
				{
					exitInstance.AddKey (key);
				}
			}
		}
	}

    //Generate and fill out the cell array for the maze
	//In the future could implemenet different algorithms to generate a maze and still use the same base class to instatiate
    abstract public void GenerateMaze();

	//Verify if coordinates are inside the maze
    protected bool inBounds(IntVector2 coords)
    {
        return !(coords.x < 0 || coords.x >= mazeWidth || coords.y < 0 || coords.y >= mazeHeight);
    }

	//Return a copy of a cell if it is i bound
    protected Cell GetCell(IntVector2 pos)
    {
        if(!inBounds(pos))
        {
            return null;
        }
        else
        {
            return mazeCells[pos.x, pos.y];
        }
    }

	//give a reference to a cell if it is in bounds
    protected void GetCell(IntVector2 pos, out Cell c)
    {
        if (!inBounds(pos))
        {
            c = null;
        }
        else
        {
            c = mazeCells[pos.x, pos.y];
        }
    }

	//Change a cell at a given location
    protected bool SetCell(Cell newCell, IntVector2 location)
    {
        if (!inBounds(location))
        {
            return false;
        }
        else
        {
            mazeCells[location.x, location.y] = newCell;
			cellOccupied [location.x, location.y] = (newCell!=null);
            return true;
        }
    }

	//Add a cell at a location
	protected Cell AddCell(IntVector2 location, CellType type = CellType.Undefined)
	{
		Cell c = new Cell (location);
		c.type = type;
		if (SetCell (c, location))
		{
			return c;
		}
		else
		{
			return null;
		}
	}

	//Set a cell to null
	protected void RemoveCell(IntVector2 location)
	{
		SetCell (null, location);
	}

	//Return whether a cell is used
	public bool HasCell(IntVector2 location)
	{
		return inBounds (location) && cellOccupied [location.x, location.y];
	}

	//Remove a wall from a cell in a specific direction
	public bool RemoveWall(IntVector2 pos, Directions dir)
	{
		if (HasCell (pos))
		{
			return mazeCells [pos.x, pos.y].RemoveWall (dir);
		}
		else
		{
			return false;
		}
	}

	//Add a wall to a cel in a specific direction
	public void AddWall(IntVector2 pos, Directions dir)
	{
		if (HasCell (pos))
		{
			mazeCells [pos.x, pos.y].AddWall (dir);
		}
	}

	//Get all cells that are neighbour to a cell: a neighbour is one step away and not blocked by a wall
	public LinkedList<Cell> GetLinkedNeighbours(Cell cell)
	{
		LinkedList<Cell> result = new LinkedList<Cell> ();
		for (int c = 0; c < 4; c++)
		{
			Directions d = (Directions)c;
			if(!cell.walls[c] && GetCell(cell.position+d.ToVector()) !=null)
			{
				result.AddLast(GetCell (cell.position + d.ToVector ()));	
			}

		}
		return result;
	}

	//Transform a 2d maze coordinate to a 3d world position
	public Vector3 ToGlobalPos(IntVector2 pos)
	{
		return new Vector3(pos.x*cellSize,0,pos.y*cellSize) + this.transform.position;
	}

	/*
	 * Getter functions
	 */

	public Cell[,] GetMazeCells()
	{
		return mazeCells;
	}

    public Vector3 GetStartPosition()
    {
        return ToGlobalPos(this.startPoint);
    }

	//Repositions the maze based on a specified entrance of the same cell size 
    public void SetEntrance(Vector3 entrancePosition)
    {
        this.transform.position = entrancePosition - ToGlobalPos(startPoint) + new Vector3(Directions.NORTH.ToVector().x * cellSize, 0, Directions.NORTH.ToVector().y * cellSize);
    }

    public Vector3 GetExitPositionAlign()
    {
        return ToGlobalPos(this.endPoint);
    }

	//Notify events that the maze was solved
    private void MazeSolved()
    {
        if (OnMazeSolved != null)
            OnMazeSolved(this);
    }
}


