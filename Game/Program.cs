using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleSemiExtensions;
using System.Threading;
using System.IO;

namespace SnakeClone
{
	class Program
	{
		const int frameTimeDefault = 70;
		static int frameTimeReal = frameTimeDefault;
		static Snake snake;
		static string HighScoreMessage; //Determined on snake death
		static bool pressedEscape;
		static bool IsRestarting;

		static void Main(string[] args)
		{
			// Setting stuff up.
			Console.WindowHeight = 30; Console.WindowWidth = 61;
			Console.CursorVisible = false;
			GrowthBall.ANewBallsChixel = new Chixel('█', ConsoleColor.Red);
			// Making sure there is an instance of FrameBuffer in existance
			FrameBuffer frame = new FrameBuffer(0, 0, Console.WindowWidth, Console.WindowHeight);

			InitalizeGame();

			// Them controls start here. Made it IsBackground to make sure it dies when the main thread dies
			Thread keylistener = CreateKeyListener();
			keylistener.Start();

			// Main Game loop
			while (!pressedEscape)
			{
				// If the snake is dead, don't do things that snakes do when they're alive like moving
				// I am not happy how handling death and restarts turned out. This is unreadable crap!
				if (!snake.Dead)
				{
					if (!snake.Move()) { snake.OnDie(); };
					if (GrowthBall.GrowthBallDict.Count < 1)
					{
						GrowthBall.Spawn(3);
					}
				}
				else if (IsRestarting)
				{
					FrameBuffer.Instance.Clear();
					GrowthBall.GrowthBallDict.Clear();

					// The stuff with the keylistener is so that it doesn't steal the Game's fist ReadKey
					// Which causes the game to require two key presses to unpause after restart
					keylistener.Abort();

					InitalizeGame();

					keylistener = CreateKeyListener();
					keylistener.Start();

					IsRestarting = false;
				}
				FrameBuffer.Instance.DrawFrame();
				Thread.Sleep(frameTimeReal);
			}
		}

		static void InitalizeGame()
		{
			snake = new Snake(7,
				new Chixel('█', ConsoleColor.White),
				new Chixel('█', ConsoleColor.Gray));

			snake.Died += new Action<Snake>(DealWithScoring);
			snake.Died += new Action<Snake>(DrawGameOverScreen);

			FrameBuffer.Instance.DrawFrame();
			// The game starts paused
			ControlGame(Console.ReadKey(true).Key);

			// Spawn a ball after the player unpauses
			GrowthBall.Spawn(3);
		}

		static Thread CreateKeyListener()
		{
			return new Thread(new ThreadStart(ControlGameInfinitely)) { IsBackground = true };
		}

		static void DealWithScoring(Snake paraSnake)
		{
			string highScorePath = "HighScore.txt";
			if (!(File.Exists(highScorePath))) { new StreamWriter(highScorePath).Close(); }
			StreamReader reader = new StreamReader(highScorePath);

			string fileContents = reader.ReadToEnd();
			reader.Close();

			if (fileContents.Equals("") || int.Parse(fileContents) < paraSnake.CurrentLength)
			{
				StreamWriter writer = new StreamWriter(highScorePath);
				writer.Write(paraSnake.CurrentLength);
				writer.Close();

				HighScoreMessage = $"{paraSnake.CurrentLength} NEW!";
			}
			else
			{
				HighScoreMessage = fileContents;
			}
		}

		static void DrawGameOverScreen(Snake paraSnake)
		{
			string[] messages = new string[]
			{
				"8\"\"\"\"8                      8\"\"\"88                    8 \n8    \" eeeee eeeeeee eeee   8    8 ee   e eeee eeeee  88\n8e     8   8 8  8  8 8      8    8 88   8 8    8   8  88\n88  ee 8eee8 8e 8  8 8eee   8    8 88  e8 8eee 8eee8e 88\n88   8 88  8 88 8  8 88     8    8  8  8  88   88   8\n88eee8 88  8 88 8  8 88ee   8eeee8  8ee8  88ee 88   8 88",
				"Press ESC to exit\nPress R to Restart",
				$"Your Score: {paraSnake.CurrentLength}\nHigh Score: {HighScoreMessage}"
			};

			int lastY = 0;

			foreach (string message in messages)
			{
				int messageHeight = message.Split('\n').Length + 1;
				int messageWidth = message.Split('\n').Max((string s) => s.Length);

				int locX = (FrameBuffer.Instance.Width - messageWidth) / 2;
				int locY = (FrameBuffer.Instance.Height - messageHeight) / 2;

				FrameBuffer.Instance.Write(locX, locY + lastY, message);

				lastY += (messageHeight/2) + 2;
			}
		}

		static void ControlGame(ConsoleKey key)
		{
			// movement
			switch (key)
			{
				case ConsoleKey.RightArrow:
				case ConsoleKey.D:
					snake.deltaDirectionStack.Push(Directions.RIGHT);
					break;
				case ConsoleKey.UpArrow:
				case ConsoleKey.W:
					snake.deltaDirectionStack.Push(Directions.UP);
					break;
				case ConsoleKey.LeftArrow:
				case ConsoleKey.A:
					snake.deltaDirectionStack.Push(Directions.LEFT);
					break;
				case ConsoleKey.DownArrow:
				case ConsoleKey.S:
					snake.deltaDirectionStack.Push(Directions.DOWN);
					break;
			}

			// other controls
			if (key == ConsoleKey.Escape) pressedEscape = true;
			if (key == ConsoleKey.R)
			{
				if (snake.Dead)
				{
					IsRestarting = true;
				}
			}

			if (key == ConsoleKey.Z)
			{
				frameTimeReal = frameTimeReal == frameTimeDefault ? frameTimeDefault / 2 : frameTimeDefault;
			}
		}

		static void ControlGameInfinitely()
		{
			while (!pressedEscape)
				ControlGame(Console.ReadKey(true).Key);
		}
	}
}
