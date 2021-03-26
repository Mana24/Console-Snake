using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleSemiExtensions;

namespace SnakeClone
{
	public class PosChixel : Chixel
	{
		public int x;
		public int y;

		public PosChixel(int locX, int locY, char glyph, ConsoleColor fg_color = ConsoleColor.White, ConsoleColor bg_color = ConsoleColor.Black)
			: base(glyph, fg_color, bg_color)
		{
			x = locX;
			y = locY;
		}

		public PosChixel(int locX, int locY, Chixel other)
			: base (other)
		{
			x = locX;
			y = locY;
		}

		public PosChixel(PosChixel other) : this(other.x, other.y, other) { }

		public void Set()
		{
			FrameBuffer.Instance.SetChixel(this);
		}
	}
}
