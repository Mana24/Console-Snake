using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleSemiExtensions;

namespace SnakeClone
{
	public static class Utility
	{
		public static bool IsEven(this int enteredInt) 
		{
			if ((enteredInt % 2) == 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool IsOdd(this int enteredInt)
		{
			return !(enteredInt.IsEven());
		}

		public static bool BothShareParity(int firstInt, int secondInt)
		{
			if ((firstInt.IsEven() && secondInt.IsEven()) || (firstInt.IsOdd() && secondInt.IsOdd()))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static void SetChixel(this FrameBuffer frame, PosChixel posChixel)
		{
			frame.SetChixel(posChixel.x, posChixel.y, posChixel);
		}

		public static (int x, int y) DirectionToXY(Directions direction)
		{
			switch (direction)
			{
				case Directions.RIGHT:
					return (1, 0);
				case Directions.UP:
					return (0, -1);
				case Directions.LEFT:
					return (-1, 0);
				case Directions.DOWN:
				default:
					return (0, 1);
			}
		}
	}
}
