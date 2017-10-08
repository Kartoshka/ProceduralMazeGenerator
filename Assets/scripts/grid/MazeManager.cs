using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbMazeManager : MonoBehaviour {
	//Ideas: everytime the weird orb is traversed, darken the sky, since the orb is the only source of light, it becomes increasingly visible.
	public enum CellType
	{
		Start,
		End,
		Corridor,
		Void,
		DeadEnd
	};
		
	public class Cell
	{
		// 0 : EAST, 1 : NORTH, 2 : WEST, 3 : SOUTH
		bool[] walls = new bool[4];
		LinkedList<short> availableWalls = new LinkedList<short> ();

		public Cell()
		{
			bool[] added = new bool[4];
			for(short i=0; i<4;i++)
			{
				short choice = (short)Random.Range(0,3);
				while(added[choice])
				{
					choice = (short)((choice +1)%4);
					walls[i] = false;
				}
				availableWalls.AddFirst(choice);
				added[choice] = true;
			}
		}

		public Cell(bool random)
		{
			if(!random)
			{
				for(short i=0; i<4;i++)
				{
					availableWalls.AddLast(i);
					walls[i] = false;
				}
			}
			else
			{
				bool[] added = new bool[4];
				for(short i=0; i<4;i++)
				{
					short choice = (short)Random.Range(0,3);
					while(added[choice])
					{
						choice = (short)((choice +1)%4);
						walls[i] = false;
					}
					availableWalls.AddFirst(choice);
					added[choice] = true;
				}
			}
		}
	}

	public GameObject mazePrefabTest;
	//It's a square
	public int cellSize = 1;
	//Dimensions of the maze
	[SerializeField]
	public int mazeWidth = 10;
	[SerializeField]
	public int mazeHeight = 10;

	private CellType[,] mazeCells;
	private Cell[,]    mazeWalls;

	// Use this for initialization
	void Start () {
		GenerateMaze (out mazeWalls);
		//At the end of the maze we put a door which unlocks to reveal a slope which leads back to the exit
	}	

	//Generate and return the maze
	abstract public void GenerateMaze(out Cell[,] outMaze);
}


public enum Directions
{
	EAST, 
	NORTH,
	WEST,
	SOUTH
}

//Taken from Marrt
public class IntVector2 {
	public int x;
	public int y;

	public static IntVector2 up		= new IntVector2( 0, 1 );	//north
	public static IntVector2 right	= new IntVector2( 1, 0 );	//east
	public static IntVector2 down	= new IntVector2( 0,-1 );	//south
	public static IntVector2 left	= new IntVector2(-1, 0 );	//west

	public static IntVector2 zero	= new IntVector2( 0, 0 );

	public IntVector2(int x, int y) {
		this.x = x;
		this.y = y;
	}

	public int sqrMagnitude{
		get { return x * x + y * y; }
	}

	//Casts
	public static implicit operator Vector2(IntVector2 From){
		return new Vector2(From.x, From.y);
	}

	public static implicit operator IntVector2(Vector2 From){
		return new IntVector2((int)From.x, (int)From.y);
	}

	public static implicit operator string(IntVector2 From){
		return "(" + From.x + ", " + From.y + ")";
	}

	//Operators
	public static IntVector2 operator +(IntVector2 a, IntVector2 b) {
		return new IntVector2(a.x + b.x, a.y + b.y);
	}

	public static IntVector2 operator +(IntVector2 a, Vector2 b) {
		return new IntVector2(a.x + (int)b.x, a.y + (int)b.y);
	}

	public static IntVector2 operator -(IntVector2 a, IntVector2 b) {
		return new IntVector2(a.x - b.x, a.y - b.y);
	}

	public static IntVector2 operator -(IntVector2 a, Vector2 b) {
		return new IntVector2(a.x - (int)b.x, a.y - (int)b.y);
	}

	public static IntVector2 operator *(IntVector2 a, int b) {
		return new IntVector2(a.x * b, a.y * b);
	}		

	public static bool operator == (IntVector2 iv1, IntVector2 iv2){
		return (iv1.x == iv2.x && iv1.y == iv2.y);
	}

	public static bool operator != (IntVector2 iv1, IntVector2 iv2){
		return (iv1.x != iv2.x || iv1.y != iv2.y);
	}

	public override bool Equals(System.Object obj){
		// If parameter is null return false.
		if (obj == null){
			return false;
		}

		// If parameter cannot be cast to Point return false.
		IntVector2 p = obj as IntVector2;
		if ((System.Object)p == null){
			return false;
		}

		// Return true if the fields match:
		return (x == p.x) && (y == p.y);
	}

	public bool Equals(IntVector2 p){
		// If parameter is null return false:
		if ((object)p == null){
			return false;
		}

		// Return true if the fields match:
		return (x == p.x) && (y == p.y);
	}

	public override int GetHashCode(){
		return x ^ y;
	}	
}

public static class DirectionHelpers
{
	public static IntVector2 ToDirection(this Directions dir)
	{
		switch (dir)
		{
		case Directions.NORTH:
			return IntVector2.up;
		case Directions.EAST:
			return IntVector2.right;
		case Directions.SOUTH:
			return IntVector2.down;
		case Directions.WEST:
		default:
			return IntVector2.left;

		}
	}
}
