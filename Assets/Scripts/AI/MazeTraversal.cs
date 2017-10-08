 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

/*
 * Depth first search maze traversal algorithm
 * 
 */
[RequireComponent(typeof(Rigidbody))]
public class MazeTraversal : Enemy {

	//we need a reference to the maze in order to get the world position of the locations we want to go to
	private AbMazeManager maze;
	private bool[,] discoveredCells;
	private bool initialized = false;

	//Travel speed of the enemy
	public float speed = 2;


	//We keep a list of all the 2d locations in the maze we want to travel to
	LinkedList<IntVector2> traversalPositions;
	//Enumerator to the location we want to go to
	IEnumerator<IntVector2> currentTarget;


	//Rigibody used for movement
	Rigidbody rb;

	//Initialize all arrays and variables required
	public void Init(AbMazeManager maze, Cell start)
	{
		this.maze = maze;
		discoveredCells = new bool[maze.mazeWidth, maze.mazeHeight];
		traversalPositions  = new LinkedList<IntVector2>();
		RecTraverse (start);

		currentTarget = traversalPositions.GetEnumerator ();
		currentTarget.MoveNext ();
		rb = this.GetComponent<Rigidbody> ();
		initialized = true;
	}

	//When the enemy is properly initialized, not dead it will try to go to the next location in the list
	public void Update()
	{
		if (!initialized || !active || dead)
			return;
		
		Vector3 currentPos = this.transform.position;
		Vector3 targetPos = maze.ToGlobalPos(currentTarget.Current);
		targetPos.y = currentPos.y;

		//Calculate whether our distance to our next position is less than 0.1, if so we pick a new target
		float distanceToGoal = Vector2.Distance (new Vector2(targetPos.x,targetPos.z),new Vector2(currentPos.x,currentPos.z));
		if (distanceToGoal > 0.1)
		{
			//If we're not close, set our velocity to the direction of the target location from our current position at the specified speed
			rb.velocity = (targetPos - currentPos).normalized * speed;
		} else
		{
			//If we've reached the end of our total path, we restart! 
			if (!currentTarget.MoveNext ())
			{
				currentTarget.Reset ();
				currentTarget.MoveNext ();
			} else
			{
				targetPos = maze.ToGlobalPos(currentTarget.Current);
			}
		}
	}
		


	/*
	 * Recursive depth first search traversal of the maze that plots out a trajectory for the traverser
	 */
	private void RecTraverse(Cell currentCell)
	{
		//Set the cell we are at as traveled to 
		discoveredCells [currentCell.position.x, currentCell.position.y] = true;
		//Push this cell to the end of our list (treating the list like a queue)
		traversalPositions.AddLast (currentCell.position);

		//For every unvisited neighbour
		LinkedList<Cell> neighbours = maze.GetLinkedNeighbours (currentCell);
		foreach (Cell n in neighbours)
		{
			//If it is valid (not a room, start, or end) 
			if (!discoveredCells[n.position.x,n.position.y] && n.type != CellType.Start && n.type != CellType.End && n.type != CellType.Room)
			{
				//Recursive call to travel down it
				RecTraverse (n);
				//When emerge from recursion, add to our traversal list so that our enemy can go back on its path!
				traversalPositions.AddLast (currentCell.position);
			}
		}
	}


}
