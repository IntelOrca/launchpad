using IntelOrca.Launchpad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;

namespace IntelOrca.LaunchpadTests
{
	class Bulldog
	{
		private LaunchpadDevice mLaunchpadDevice;
		private List<Dog> mDogs = new List<Dog>();
		private Random mRandom = new Random();

		private long mCurrentTicks = 0;
		private long mNextDogTick = 2000;

		private int mMisses, mHits;

		public Bulldog(LaunchpadDevice device)
		{
			mLaunchpadDevice = device;
			mLaunchpadDevice.ButtonPressed += mLaunchpadDevice_ButtonPressed;
		}

		private void mLaunchpadDevice_ButtonPressed(object sender, ButtonPressEventArgs e)
		{
			if (e.Type == ButtonType.Grid) {
				var pressedDogs = mDogs.Where(d => (int)d.X == e.X && (int)d.Y == e.Y);
				pressedDogs.ToList().ForEach(d => d.Killed = true);
				if (pressedDogs.Count() > 0)
					SystemSounds.Beep.Play();
			}
		}

		public void Play()
		{
			long last_tick = Environment.TickCount;
			long delay = 12;

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
			mDogs.ForEach(d => d.Update());

			var missedDogs = mDogs.Where(d => d.LeftGrid);
			var hitDogs = mDogs.Where(d => d.Killed);

			mMisses += missedDogs.Count();
			mHits += hitDogs.Count();

			mDogs.RemoveAll(d => d.LeftGrid || d.Killed);

			if (mDogs.Count < 8) {
				if (mCurrentTicks > mNextDogTick) {
					mDogs.Add(GetNewDog());
					mNextDogTick = mCurrentTicks + mRandom.Next(100, 1000);
				}
			}

			Console.Clear();
			Console.WriteLine("Misses: {0:0000}", mMisses);
			Console.WriteLine("Hits:   {0:0000}", mHits);
			Console.WriteLine("Score:  {0:0000}", (mHits * 10) - (mMisses * 5));
		}

		private void Draw()
		{
			ButtonBrightness[,] redgrid = new ButtonBrightness[8, 8];
			ButtonBrightness[,] greengrid = new ButtonBrightness[8, 8];

			mDogs.ForEach(d => {
				if (!(d.X >= 0 && d.X < 8 && d.Y >= 0 && d.Y < 8))
					return;

				redgrid[(int)d.X, (int)d.Y] = ButtonBrightness.Full;
				greengrid[(int)d.X, (int)d.Y] = ButtonBrightness.Full;
			});

			for (int y = 0; y < 8; y++)
				for (int x = 0; x < 8; x++)
					mLaunchpadDevice[x, y].SetBrightness(redgrid[x, y], greengrid[x, y]);
			mLaunchpadDevice.Refresh();
		}

		private Dog GetNewDog()
		{
			Dog dog = new Dog();

			if (mRandom.Next(2) == 0) {
				// Row
				dog.Y = mRandom.Next(8);
				dog.VX = mRandom.NextDouble() / 5;
				dog.X = 0;
				if (mRandom.Next(2) == 0) {
					dog.X = 7;
					dog.VX *= -1;
				}
				
			} else {
				// Column
				dog.X = mRandom.Next(8);
				dog.VY = mRandom.NextDouble() / 5;
				dog.Y = 0;
				if (mRandom.Next(2) == 0) {
					dog.Y = 7;
					dog.VY *= -1;
				}
			}

			return dog;
		}

		class Dog
		{
			public Dog() { }

			public void Update()
			{
				X += VX;
				Y += VY;
			}

			public bool LeftGrid
			{
				get
				{
					if (X < 0 && VX <= 0)
						return true;
					if (Y < 0 && VY <= 0)
						return true;
					if (X >= 8 && VX >= 0)
						return true;
					if (Y >= 8 && VY >= 0)
						return true;
					return false;
				}
			}

			public bool Killed { get; set; }
			public int Colour { get; set; }
			public double X { get; set; }
			public double Y { get; set; }
			public double VX { get; set; }
			public double VY { get; set; }
		}
	}
}
