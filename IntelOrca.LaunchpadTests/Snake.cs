using IntelOrca.Launchpad;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelOrca.LaunchpadTests
{
	struct Point
	{
		public Point(int x, int y)
			: this()
		{
			X = x;
			Y = y;
		}

		public int X { get; set; }
		public int Y { get; set; }
	}

	class Snake
	{
		private LaunchpadDevice mLaunchpadDevice;

		private Random mRandom = new Random();
		private long mCurrentTicks = 0;

		private Point[] mBody;
		private Point mDirection;

		private bool mFoodActive;
		private Point mFood;

		public Snake(LaunchpadDevice device)
		{
			mLaunchpadDevice = device;

			mLaunchpadDevice.ButtonPressed += mLaunchpadDevice_ButtonPressed;

			Restart();
		}

		private void mLaunchpadDevice_ButtonPressed(object sender, ButtonPressEventArgs e)
		{
			if (e.Type == ButtonType.Grid) {
				mFood.X = e.X;
				mFood.Y = e.Y;
				mFoodActive = true;
			}
		}

		public void Run()
		{
			long last_tick = Environment.TickCount;
			long delay = 12;
			delay = 12 * 6;

			while (true) {
				if (Environment.TickCount - last_tick < delay)
					continue;
				mCurrentTicks += Environment.TickCount - last_tick;
				last_tick = Environment.TickCount;

				Update();
				Draw();
			}
		}

		private void Update()
		{
			SetSnakeDirection();
			MoveSnake();
		}

		private void Draw()
		{
			ButtonBrightness[,] redgrid = new ButtonBrightness[8, 8];
			ButtonBrightness[,] greengrid = new ButtonBrightness[8, 8];

			// Draw snake
			foreach (Point p in mBody)
				if (InBounds(p))
					greengrid[p.X, p.Y] = ButtonBrightness.Full;
			if (InBounds(mBody[0]))
				redgrid[mBody[0].X, mBody[0].Y] = ButtonBrightness.Full;

			// Draw food
			if (mFoodActive)
				redgrid[mFood.X, mFood.Y] = ButtonBrightness.Full;

			// Invalidate
			for (int y = 0; y < 8; y++)
				for (int x = 0; x < 8; x++)
					mLaunchpadDevice[x, y].SetBrightness(redgrid[x, y], greengrid[x, y]);
			mLaunchpadDevice.Refresh();
		}

		private void Restart()
		{
			mBody = new Point[4];
			for (int y = 8; y < 8 + 4; y++)
				mBody[y - 8] = new Point(3, y);
			mDirection = new Point(0, -1);

			mFoodActive = false;

			// PlaceFood();
		}

		private void MoveSnake()
		{
			for (int i = mBody.Length - 1; i > 0; i--)
				mBody[i] = mBody[i - 1];
			mBody[0].X += mDirection.X;
			mBody[0].Y += mDirection.Y;

			for (int i = 1; i < mBody.Length; i++) {
				if (mBody[0].X == mBody[i].X && mBody[0].Y == mBody[i].Y)
					Restart();
			}

			if (mBody[0].X == mFood.X && mBody[0].Y == mFood.Y) {
				mFoodActive = false;
				ExtendSnake();
				// PlaceFood();
			}
		}

		public void SetSnakeDirection()
		{
			bool danger = false;

			Point head = mBody[0];
			if (head.X == 7 && mDirection.X > 0) {
				if (head.Y < 7) mDirection = new Point(0, 1);
				else mDirection = new Point(0, -1);
				danger = true;
			} else if (head.X == 0 && mDirection.X < 0) {
				if (head.Y < 7) mDirection = new Point(0, 1);
				else mDirection = new Point(0, -1);
				danger = true;
			}

			if (head.Y == 7 && mDirection.Y > 0) {
				if (head.X < 7) mDirection = new Point(1, 0);
				else mDirection = new Point(-1, 0);
				danger = true;
			} else if (head.Y == 0 && mDirection.Y < 0) {
				if (head.X < 7) mDirection = new Point(1, 0);
				else mDirection = new Point(-1, 0);
				danger = true;
			}

			if (!danger && mFoodActive) {
				if (mDirection.X != 0) {
					if (mBody[0].X == mFood.X) {
						if (mBody[0].Y < mFood.Y) mDirection = new Point(0, 1);
						else mDirection = new Point(0, -1);
					}
				} else {
					if (mBody[0].Y == mFood.Y) {
						if (mBody[0].X < mFood.X) mDirection = new Point(1, 0);
						else mDirection = new Point(-1, 0);
					}
				}
			}
		}

		private void ExtendSnake()
		{
			Point[] newBody = new Point[mBody.Length + 1];
			newBody[0] = mBody[0];
			Array.Copy(mBody, 0, newBody, 1, mBody.Length);
			mBody = newBody;
		}

		private void PlaceFood()
		{
			List<Point> possiblePlaces = new List<Point>();
			for (int y = 0; y < 8; y++)
				for (int x = 0; x < 8; x++)
					if (!mBody.Contains(new Point(x, y)))
						possiblePlaces.Add(new Point(x, y));

			int index = mRandom.Next(possiblePlaces.Count);
			mFood = possiblePlaces[index];
		}

		private bool InBounds(Point p)
		{
			return (p.X >= 0 && p.Y >= 0 && p.X < 8 && p.Y < 8);
		}
	}
}
