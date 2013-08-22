using IntelOrca.Launchpad;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IntelOrca.LaunchpadTests
{
	class ScrollingLetters
	{
		class CharacterDefinition
		{
			private char mKey;
			private int mWidth;
			private int mHeight;
			private List<Point> mPoints = new List<Point>();

			public CharacterDefinition(StringReader sr)
			{
				string key;

				// Read key
				while ((key = sr.ReadLine()).Length == 0);
				mKey = key[0];

				// Read characters
				mHeight = 5;
				for (int y = 0; y < mHeight; y++) {
					string line = sr.ReadLine();
					for (int x = 0; x < line.Length; x++)
						if (line[x] == 'X')
							mPoints.Add(new Point(x, y));
				}

				mWidth = mPoints.Max(p => p.X) + 1;
			}

			public override int GetHashCode()
			{
				return mKey.GetHashCode();
			}

			public char Key
			{
				get { return mKey; }
			}

			public int Width
			{
				get { return mWidth; }
				set { mWidth = value; }
			}

			public int Height
			{
				get { return mHeight; }
				set { mHeight = value; }
			}

			public Point[] Points
			{
				get { return mPoints.ToArray(); }
			}

			public struct Point
			{
				public Point(int x, int y) : this() { X = x; Y = y; }

				public int X { get; set; }
				public int Y { get; set; }

				public override string ToString()
				{
					return String.Format("X = {0} Y = {1}", X, Y);
				}
			}
		}

		private static Dictionary<char, CharacterDefinition> mCharacterDefinitions = GetCharacterDefinitions();

		private LaunchpadDevice mLaunchpadDevice;
		private string mText = String.Empty;
		private int mTextOffset = 8;

		private bool[,] mGrid = new bool[8, 8];

		private static Dictionary<char, CharacterDefinition> GetCharacterDefinitions()
		{
			var defs = new Dictionary<char, CharacterDefinition>();

			StringReader sr = new StringReader(File.ReadAllText("text.txt"));
			while (sr.Peek() != -1) {
				if (Char.IsWhiteSpace((char)sr.Peek())) {
					sr.Read();
					continue;
				}

				CharacterDefinition cd = new CharacterDefinition(sr);
				defs.Add(cd.Key, cd);
			}

			return defs;
		}

		public ScrollingLetters(LaunchpadDevice device)
		{
			mLaunchpadDevice = device;
		}

		private void SetGrid(int x, int y, bool value)
		{
			if (x >= 0 && y >= 0 && x < 8 && y < 8)
				mGrid[x, y] = value;
		}

		private int RenderCharacter(char c, int x, int y)
		{
			if (mCharacterDefinitions.ContainsKey(c)) {
				CharacterDefinition cd = mCharacterDefinitions[c];
				if (x + cd.Width >= 0 && x < 8)
					Array.ForEach(cd.Points, p => SetGrid(x + p.X, y + p.Y, true));
				return cd.Width;
			}

			return 4;
		}

		private void RenderGrid()
		{
			int x, y;

			for (y = 0; y < 8; y++)
				for (x = 0; x < 8; x++)
					mGrid[x, y] = false;

			x = mTextOffset;
			y = 1;
			foreach (char c in mText) {
				x += RenderCharacter(c, x, y) + 1;
			}

			for (y = 0; y < 8; y++) {
				for (x = 0; x < 8; x++) {
					if (mGrid[x, y])
						mLaunchpadDevice[x, y].SetBrightness(ButtonBrightness.Full, ButtonBrightness.Full);
					else
						mLaunchpadDevice[x, y].SetBrightness(ButtonBrightness.Off, ButtonBrightness.Off);
				}
			}
			mLaunchpadDevice.Refresh();
		}

		public void ScrollText()
		{
			long last_tick = 0;
			long delay = 100;

			while (true) {
				if (Environment.TickCount - last_tick < delay)
					continue;
				last_tick = Environment.TickCount;

				RenderGrid();

				mTextOffset--;
				if (mTextOffset < -(mText.Length * 5 + 2))
					mTextOffset = 8;
			}
		}

		public string Text
		{
			get { return mText; }
			set { mText = value; }
		}
	}
}
