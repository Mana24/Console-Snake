using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ConsoleSemiExtensions;
using System.Collections.Concurrent;
using System.IO;

namespace SnakeClone
{
	class Snake
	{
		public int GrowthMultiplier = 1;
		public event Action<Snake> Died = delegate { };

		public bool IsGrowing
		{
			get { return (growth > 0); }
		}

		// Don't change the type. It's very important that it remains this exact same type
		public Directions CurrentDirection { get; protected set; }

		protected int growth;
		protected List<PosChixel> chixelList;

		/// <summary>
		/// Gets applied to Direction on Move.
		/// Prevents a bug that killed the player because 
		/// the snake would change direction before moving 
		/// (i.e making those changes real)
		/// </summary>
		public ConcurrentStack<Directions> deltaDirectionStack { get; protected set; } = new ConcurrentStack<Directions>();

		public bool Dead { get; protected set; }
		public int CurrentLength { get; protected set; }
		public Chixel HeadChixel { get; protected set; }
		public Chixel BodyChixel { get; protected set; }

		public Snake(int currentLength, Chixel headChixel, Chixel bodyChixel, 
			Directions startingDirection = Directions.RIGHT)
		{
			// Classic constructor stuff
			this.CurrentLength = currentLength;
			this.chixelList = new List<PosChixel>();
			this.BodyChixel = bodyChixel;
			this.HeadChixel = headChixel;
			this.CurrentDirection = startingDirection;
			this.deltaDirectionStack = new ConcurrentStack<Directions>();

			// Setting the snake up so that the head is in the center and its body is on its left.
			chixelList.Add(new PosChixel(FrameBuffer.Instance.Width/2, FrameBuffer.Instance.Height/2, headChixel));
			for (int i = 1; i < currentLength; i++)
			{
				chixelList.Add(new PosChixel(chixelList.Last().x - 1, chixelList.Last().y, bodyChixel));
			}
			
			DrawSnake();
		}

		// I am a bit sad that this method turned out to be responsable for
		// managing the whole snake and not just moving it
		public bool Move()
		{
			PosChixel snakeHead = chixelList.First();
			PosChixel snakeTail = chixelList.Last();

			// Apply movement gotten from the controls to snake

			// Why is there a stack here, you ask? Well it's made so that if (between updates) the player presses 
			// something that causes a change in direction
			// then follows it up by something that doesn't, the latest change causing press is applied and not caneled
			// this will hopefully make the controls more responsive.
			while(deltaDirectionStack.Count != 0)
			{
				Directions outDirection;
				if (!deltaDirectionStack.TryPop(out outDirection)) continue;
				if (DoesDirectionCauseTurn(outDirection))
				{
					CurrentDirection = outDirection;
					break;
				}
			}
			deltaDirectionStack.Clear();

			// Change in the position of the head.
			(int deltaX, int deltaY) = Utility.DirectionToXY(CurrentDirection);

			int newX = snakeHead.x + deltaX;

			// Implement looping around the screen like pacman 
			if (newX > FrameBuffer.Instance.Width -1)
			{
				newX = FrameBuffer.Instance.Left;
			}
			else if (newX < FrameBuffer.Instance.Left)
			{
				newX = FrameBuffer.Instance.Width -1;
			}

			int newY = snakeHead.y + deltaY;

			// Implement looping around the screen like pacman
			if (newY > FrameBuffer.Instance.Height -1)
			{
				newY = FrameBuffer.Instance.Top;
			}
			else if (newY < FrameBuffer.Instance.Top)
			{
				newY = FrameBuffer.Instance.Height -1;
			}

			// Eat the Growthball if on it;
			if (GrowthBall.GrowthBallDict.ContainsKey((newX, newY)))
			{
				this.Grow(GrowthBall.GrowthBallDict[(newX, newY)].GrowthValue);
				GrowthBall.EatAt(newX, newY);
			}

			Chixel newPosChixel = FrameBuffer.Instance.GetChixel(newX, newY);

			// Check if that change will result in a death. (Checking if there is a snakechixel and 
			// also makeing sure that that chixel isn't the tail chixel because that wouldn't
			// result in a death)

			// Had to abstract these booleans because I couldn't think about them straight
			bool newPosHasSamePosAsTail = snakeTail.x == newX && snakeTail.y == newY && !IsGrowing;
			bool newPosChixelLooksLikeBodyChixel = newPosChixel?.Glyph == BodyChixel.Glyph &&
				newPosChixel?.ForegroundColor == BodyChixel.ForegroundColor &&
				newPosChixel?.BackgroundColor == BodyChixel.BackgroundColor;

			//This is gonna be one ugly if statement //Should be able to step on its old tail position
			if (newPosChixel != null && !newPosHasSamePosAsTail && newPosChixelLooksLikeBodyChixel) 
			{
				return false;
			}

			// Add the extra tail bit if the snake is growing. I am not so sure how this works but it does.
			// list.Append didn't work // THAT'S BECAUSE IT DOESN'T RETURN VOID, YOU DUMMY
			if (IsGrowing)
			{
				chixelList.Add(new PosChixel(snakeTail));
				growth--;
			}

			FrameBuffer.Instance.SetChixel(snakeTail.x, snakeTail.y, Chixel.GlobalEmpty);

			// Move the tail to the position of the head
			snakeTail.x = snakeHead.x;
			snakeTail.y = snakeHead.y;

			// Move Head so it's not on top of the "tail"
			snakeHead.x = newX;
			snakeHead.y = newY;

			// Reorder the list to make way for the new tail chixel
			chixelList.Remove(snakeTail);
			chixelList.Insert(1, snakeTail);
			
			// Set the snake on the framebuffer
			DrawSnake();

			return true;
		}

		public void Grow(int growthAmount = 1)
		{
			growth += growthAmount * GrowthMultiplier;
			CurrentLength += growthAmount * GrowthMultiplier;
		}

		public void OnDie()
		{
			Dead = true;
			Died(this);
		}

		//Snake Specific utilities

		/// <summary>
		/// Sets the snake's chixel's on the framebuffer
		/// </summary>
		public void DrawSnake()
		{
			foreach (PosChixel snakeChixel in chixelList)
			{
				snakeChixel.Set();
			}
		}

		public List<PosChixel> GetChixelList()
		{
			return chixelList;
		}

		/// <summary>
		/// Checks the <paramref name="direction"/> and CurrentDirection to see if the input <paramref name="direction"/> causes a turn
		/// when applied to CurrentDirection
		/// </summary>
		/// <param name="direction"></param>
		/// <returns>True if the <paramref name="direction"/> causes the snake to turn its head when applied. False otherwise</returns>
		public bool DoesDirectionCauseTurn(Directions direction)
		{
			// this relys on UP, DOWN being odd, and RIGHT, LEFT being even.
			return !(Utility.BothShareParity((int)direction, (int)CurrentDirection));
		}
	}

	public enum Directions : int
	{
		// Plz don't change anything Snake depends on this order of things and the number
		// of things here
		RIGHT = 0, UP = 1, LEFT = 2, DOWN = 3
	}
}
