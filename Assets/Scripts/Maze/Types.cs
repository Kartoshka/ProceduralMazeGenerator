using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Types
{
	//Enum to indivate what the cell is
	[System.Serializable]
	public enum CellType
    {
        Start,
        End,
        Corridor,
        Void,
        DeadEnd,
		Room,
		CorridorToRoom,
        RoomEntry,
        Undefined
    };

    public class Cell
    {
		//Position within the maze
		public IntVector2 position;
		//Type of the cell
        public CellType type;

        // 0 : EAST, 1 : NORTH, 2 : WEST, 3 : SOUTH
        public bool[] walls = new bool[4];
        public int numWalls = 4;
		//Randomize list of available walls, used for randomly picking a wall
        public LinkedList<Directions> availableWalls = new LinkedList<Directions>();

		public Cell(IntVector2 position)
        {
			//randomly insert walls in the list of available walls
            bool[] added = new bool[4];
            for (short i = 0; i < 4; i++)
            {
                int choice = Random.Range(0, 3);
                while (added[choice])
                {
                    choice = ((choice + 1) % 4);
                }
                availableWalls.AddFirst((Directions)choice);
                walls[i] = true;
                added[choice] = true;
            }
            type = CellType.Undefined;
			this.position = position;
        }

        public bool RemoveWall(Directions pos)
        {
            if (pos != Directions.NONE)
            {
                walls[(int)pos] = false;
                numWalls--;
                return true;
            }

            return false;
        }

		public void AddWall(Directions pos)
		{
			if (pos != Directions.NONE)
			{
				if (walls [(int)pos])
				{
					return;
				} else
				{
					walls [(int)pos] = true;
					numWalls++;
				}
			}
		}
    }


	public class Room 
	{
		public int width;
		public int height;
        public IntVector2 pos;
		public bool connected = false;
		public IntVector2[,] cells;
        public Cell exit;
	}

    public enum Directions:int
    {
        EAST = 0 ,
        NORTH = 1,
        WEST = 2,
        SOUTH = 3,
        NONE = -1
    }

    /*
     * 2d vector class using integers allows for faster computations and smaller memory consumption
     * Reference for IntVector2 class: https://forum.unity.com/threads/random-level-generator.342587/#post-2215083
     */

	[System.Serializable]
    public class IntVector2
    {
		[SerializeField]
        public int x;
		[SerializeField]
        public int y;

        public static IntVector2 up = new IntVector2(0, 1); //north
        public static IntVector2 right = new IntVector2(1, 0);  //east
        public static IntVector2 down = new IntVector2(0, -1);  //south
        public static IntVector2 left = new IntVector2(-1, 0);  //west

        public static IntVector2 zero = new IntVector2(0, 0);

        public IntVector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int sqrMagnitude
        {
            get { return x * x + y * y; }
        }

        //Casts
        public static implicit operator Vector2(IntVector2 From)
        {
            return new Vector2(From.x, From.y);
        }

        public static implicit operator IntVector2(Vector2 From)
        {
            return new IntVector2((int)From.x, (int)From.y);
        }

        public static implicit operator string(IntVector2 From)
        {
            return "(" + From.x + ", " + From.y + ")";
        }

        //Operators
        public static IntVector2 operator +(IntVector2 a, IntVector2 b)
        {
            return new IntVector2(a.x + b.x, a.y + b.y);
        }

        public static IntVector2 operator +(IntVector2 a, Vector2 b)
        {
            return new IntVector2(a.x + (int)b.x, a.y + (int)b.y);
        }

        public static IntVector2 operator -(IntVector2 a, IntVector2 b)
        {
            return new IntVector2(a.x - b.x, a.y - b.y);
        }

        public static IntVector2 operator -(IntVector2 a, Vector2 b)
        {
            return new IntVector2(a.x - (int)b.x, a.y - (int)b.y);
        }

        public static IntVector2 operator *(IntVector2 a, int b)
        {
            return new IntVector2(a.x * b, a.y * b);
        }

        public static bool operator ==(IntVector2 iv1, IntVector2 iv2)
        {
            return (iv1.x == iv2.x && iv1.y == iv2.y);
        }

        public static bool operator !=(IntVector2 iv1, IntVector2 iv2)
        {
            return (iv1.x != iv2.x || iv1.y != iv2.y);
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            IntVector2 p = obj as IntVector2;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (x == p.x) && (y == p.y);
        }

        public bool Equals(IntVector2 p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (x == p.x) && (y == p.y);
        }

        public override int GetHashCode()
        {
            return x ^ y;
        }
    }

	/*
	 * Helpers to convert directions to rotation and position for wall instatiation
	 * 
	 */
    public static class DirectionHelpers
    {
        public static IntVector2 ToVector(this Directions dir)
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
                    return IntVector2.left;
                default:
                    return IntVector2.zero;

            }
        }

        public static Quaternion ToRotation(this Directions dir)
        {
            switch(dir)
            {
                case Directions.EAST:
                case Directions.WEST:
                    return Quaternion.Euler(0, 180, 90);
                case Directions.NORTH:
                case Directions.SOUTH:
                    return Quaternion.Euler(0,90, 90);
                default:
                    return Quaternion.identity;
            }
        }

        public static Directions FromInt(int dir)
        {
            switch (dir)
            {
                case 0:
                    return Directions.EAST;
                case 1:
                    return Directions.NORTH;
                case 2:
                    return Directions.WEST;
                case 3:
                    return Directions.SOUTH;
                default:
                    return Directions.NONE;
            }
        }

		

        public static Directions Opposite(this Directions dir)
        {
            //Works as long as we keep covnention East = 0 North =1 West =2 South = 3
            return FromInt((((int)dir) + 2) % 4);
        }
    }
}
