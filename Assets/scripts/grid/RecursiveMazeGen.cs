using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RecursiveMazeGen : AbMazeManager {

	public override void GenerateMaze(out Cell[,] outMaze)
	{
		outMaze = new Cell[mazeWidth, mazeHeight];
		short[,] parentNode = new short[mazeWidth, mazeHeight];
		for (int i = 0; i < mazeWidth; i++)
		{
			for (int j = 0; j < mazeHeight; j++)
			{
				parentNode [i, j] = -1;
			}
		}


	}

	private void recHelper(ref short[,] outMaze, int x, int y)
	{
		int randomWall = Random.Range (0, 4);

	}


	//Note: End and start point do not have to necessarily be end points TBH, they could but we can also just make 3 end points for the keys and connect the start and end point anywhere in our maze

	//OPTION A (examples available and proven)
	//Generate sparse maze using any of the methods. Keep track of the end points (I guess?)
	//fill in dead ends until we are left with a certain number of dead ends (not sure how many)
	//Find good spots for rooms (aka key rooms) and put them there. 
	//Note: I'm not sure that this algorithm ensures that we get a specific number of rooms. 

	//OPTION B 
	//Generate sparse maze using any of the methods. Keep track of the end points (I guess?)
	//Put rooms on the end points , if it overlaps with other parts of the maze, remove those cells and cave in those passages
		// Caving in a passage means we start at a dead end
		// We simply remove a cell and close the wall of the cell it was adjacent to and then move to that cell
		// If that cell has 3 walls, it is another dead end, repeat. 
		// If that cell has 4 walls, remove it and we are done. 
		// If that cell has 2 walls, then it is a passage way, leave it as it is, we're done.
	//Repeat until we have 3 rooms. 
	//Pick two remaining end points as the start and end point, try to space them out.
		//If we have <2 end points
	//Now use the same removal algorithm to remove the rest of the endpoints we don't want 
		//NOTE: Keep two end points, to for the start of our maze, one for the end of it. (Since the maze doesn't have to topologically match the terrain above we really don't care where those end points are)

}
