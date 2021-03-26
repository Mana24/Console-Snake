// MOST OF THE CODE IN THIS FILE IS BY Quill18 from https://github.com/quill18/ld36 I modified it to suit my usecase more

using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleSemiExtensions
{
	public class FrameBuffer
	{
		public int Left { get; protected set; }
		public int Top { get; protected set; }
		public int Width { get; protected set; }
		public int Height { get; protected set; }

		private Chixel[,] chixels;
		private Dictionary<(int x, int y), Chixel> dirtyChixels;

		private int lastWindowWidth = -1;
		private int lastWindowHeight = -1;

		private bool forceDirty = false;

		public FrameBuffer(int left, int top, int width, int height)
		{
			_instance = this;

			this.Left = left;
			this.Top = top;
			this.Width = width;
			this.Height = height;
			Clear();
		}

		static public FrameBuffer Instance
		{
			get { return _instance; }
		}

		static private FrameBuffer _instance;

		public void Clear()
		{
			Console.Clear();
			chixels = new Chixel[this.Width, this.Height];
			dirtyChixels = new Dictionary<(int x, int y), Chixel>();
		}

		public void DrawFrame()
		{

			if (lastWindowWidth != Console.WindowWidth || lastWindowHeight != Console.WindowHeight)
			{
				if (_instance == this)
				{
					Console.Clear();
				}
				forceDirty = true;

				lastWindowWidth = Console.WindowWidth;
				lastWindowHeight = Console.WindowHeight;
			}

			if (forceDirty)
			{
				for (int y = 0; y < Height; y++)
				{
					if (y < Console.WindowHeight)
					{
						for (int x = 0; x < Width; x++)
						{
							if (x < Console.WindowWidth)
							{
								Chixel ch = this.chixels[x, y];
								if (ch != null)
								{
									Console.SetCursorPosition(x + Left, y + Top);
									Console.ForegroundColor = ch.ForegroundColor;
									Console.BackgroundColor = ch.BackgroundColor;
									Console.Write(ch.Glyph);
									ch.Dirty = false;
								}
							}
						}
					}
				}
				forceDirty = false;
			}
			else
			{
				foreach ((int x, int y) in dirtyChixels.Keys)
				{
					Chixel ch = dirtyChixels[(x, y)];
					Console.SetCursorPosition(x + Left, y + Top);
					Console.ForegroundColor = ch.ForegroundColor;
					Console.BackgroundColor = ch.BackgroundColor;
					Console.Write(ch.Glyph);
					ch.Dirty = false;

					System.Diagnostics.Debug.WriteLine($"Wrote chixel at x: {x}, y: {y} ");
				}
				dirtyChixels.Clear();
			}
		}

		public Chixel GetChixel(int x, int y)
		{
			x -= Left;
			y -= Top;
			if (x < 0 || x >= Width || y < 0 || y >= Height)
			{
				return null;
			}

			return this.chixels[x, y];
		}

		public void SetChixel(int x, int y, Chixel chixel)
		{
			SetChixel(x, y, chixel.Glyph, chixel.ForegroundColor, chixel.BackgroundColor);
		}

		public void SetChixel(int x, int y, Char c, ConsoleColor fg_color = ConsoleColor.White, ConsoleColor bg_color = ConsoleColor.Black)
		{
			x -= Left;
			y -= Top;
			// check that the chixel is actually changed
			// if so, update values and set dirty

			if (x < 0 || x >= Width || y < 0 || y >= Height)
			{
				return;
			}

			Chixel ch = this.chixels[x, y];
			if (ch != null && ch.Glyph == c && ch.ForegroundColor == fg_color && ch.BackgroundColor == bg_color)
			{
				// No change.
				return;
			}

			Chixel chix = this.chixels[x, y] = new Chixel(c, fg_color, bg_color);
			

			if (dirtyChixels.ContainsKey((x, y)))
			{
				dirtyChixels[(x, y)] = chix;
			}
			else
			{
				dirtyChixels.Add((x, y), chix);
			}
		}

		public void Write(int x, int y, string s, ConsoleColor fg_color = ConsoleColor.White, ConsoleColor bg_color = ConsoleColor.Black)
		{
			// TODO: Detect ANSI escapes and output as a single write.

			int initX = x;

			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == '\n')
				{
					x = initX;
					y++;
					continue;
				}

				SetChixel(x, y, s[i], fg_color, bg_color);
				x++;
			}
		}
	}
}
