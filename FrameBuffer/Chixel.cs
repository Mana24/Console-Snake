// MOST OF THE CODE IN THIS FILE IS BY Quill18 from https://github.com/quill18/ld36 I modified it to suit my usecase more

using System;

namespace ConsoleSemiExtensions
{
	public class Chixel
	{
		public Chixel(char glyph, ConsoleColor fg_color = ConsoleColor.White, ConsoleColor bg_color = ConsoleColor.Black)
		{
			this.Glyph = glyph;
			this.ForegroundColor = fg_color;
			this.BackgroundColor = bg_color;
			this.Dirty = true;
		}

		public Chixel(Chixel other)
		{
			this.Glyph = other.Glyph;
			this.ForegroundColor = other.ForegroundColor;
			this.BackgroundColor = other.BackgroundColor;
			this.Dirty = true;
		}

		public char Glyph { get; set; }
		public ConsoleColor ForegroundColor { get; set; }
		public ConsoleColor BackgroundColor { get; set; }
		public bool Dirty { get; set; }

		public bool Equals(Chixel chixel)
		{
			if (chixel == null) return false;
			if (this.Glyph != chixel.Glyph) return false;
			if (this.ForegroundColor != chixel.ForegroundColor) return false;
			if (this.BackgroundColor != chixel.BackgroundColor) return false;
			return true;
		}

		public static Chixel GlobalEmpty
		{
			get
			{
				if (_globalEmpty != null)
				{
					return _globalEmpty;
				}
				else
				{
					_globalEmpty = new Chixel(' ');
					return _globalEmpty;
				}
			}

			set
			{
				_globalEmpty = value;
			}
		}
		private static Chixel _globalEmpty;
	}
}