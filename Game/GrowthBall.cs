using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleSemiExtensions;

namespace SnakeClone
{
	class GrowthBall
	{
		public static Chixel ANewBallsChixel { get; set; }
		public static Dictionary<(int x, int y), GrowthBall> GrowthBallDict = new Dictionary<(int x, int y), GrowthBall>();

		public Chixel BallChixel { get; protected set; }

		public int GrowthValue = 1;
		public int x { get; protected set; }
		public int y { get; protected set; }

		public GrowthBall(int x, int y, Chixel ballChixel)
		{
			// Classic Constructor stuff
			GrowthBallDict.Add((x, y), this);
			BallChixel = ballChixel;
			this.x = x;
			this.y = y;

			// Setting the ball
			FrameBuffer.Instance.SetChixel(x, y, ballChixel);
		}

		public GrowthBall(int x, int y, Chixel ballChixel, int growthValue) 
			: this (x, y, ballChixel)
		{
			this.GrowthValue = growthValue;
		}

		public static void Spawn(int left, int top, int width, int height, int growthValue = 1)
		{
			bool successfullySpawned = false;
			Random random = new Random();

			while (!successfullySpawned)
			{
				int spawnX = random.Next(left, width);
				int spawnY = random.Next(top, height);

				if (Equals(FrameBuffer.Instance.GetChixel(spawnX, spawnY), Chixel.GlobalEmpty)
					|| FrameBuffer.Instance.GetChixel(spawnX, spawnY) == null)
				{
					new GrowthBall(spawnX, spawnY, ANewBallsChixel, growthValue);
					successfullySpawned = true;
				}
			}
		}

		public static void Spawn(int growthValue = 1)
		{
			Spawn(FrameBuffer.Instance.Left, FrameBuffer.Instance.Top,
				FrameBuffer.Instance.Width, FrameBuffer.Instance.Height, growthValue);
		}

		public static bool EatAt(int x, int y)
		{
			if (GrowthBallDict.ContainsKey((x, y)))
			{
				GrowthBallDict.Remove((x, y));
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
