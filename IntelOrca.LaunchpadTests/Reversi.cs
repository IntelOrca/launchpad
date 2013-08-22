using IntelOrca.Launchpad;
using Midi;
using System;
using System.Collections.Generic;

namespace IntelOrca.LaunchpadTests
{
	class Reversi
	{
		private Random mRandom = new Random();

		private LaunchpadDevice mLaunchpadDevice;
		private int[,] mGrid = new int[8, 8];
		private int mPlayerTurn = 1;
		private int mPlayerWinning = 0;

		private int mConfirmTime;
		private bool mForceDraw;
		private int mGameState = 0;
		private bool mShowPossibleMoves = true;
		private bool mSolo = false;

		private int mFlashCounter = 0;

		private List<Tuple<int, int>> mPossiblePlaces = new List<Tuple<int, int>>();

		OutputDevice mOutputDevice;

		public Reversi(LaunchpadDevice device)
		{
			mLaunchpadDevice = device;

			mLaunchpadDevice.DoubleBuffered = false;
			mLaunchpadDevice.ButtonPressed += mLaunchpadDevice_ButtonPressed;

			mOutputDevice = OutputDevice.InstalledDevices[0];
			mOutputDevice.Open();

			Restart();
		}

		private void mLaunchpadDevice_ButtonPressed(object sender, ButtonPressEventArgs e)
		{
			if (e.Type == ButtonType.Grid) {
				if (CanPlaceAt(e.X, e.Y))
					PlaceAt(e.X, e.Y);
			} else if (e.Type == ButtonType.Side) {
				if (e.SidebarButton == SideButton.TrackOn) {
					mShowPossibleMoves = !mShowPossibleMoves;
					mForceDraw = true;
				} else if (e.SidebarButton == SideButton.Solo) {
					mSolo = !mSolo;
				}
			} else if (e.Type == ButtonType.Toolbar) {
				if (e.ToolbarButton == ToolbarButton.Session) {
					if (mConfirmTime > 0) {
						Restart();
						mConfirmTime = 0;
					} else {
						mConfirmTime = 1000;
					}
				}
			}
		}

		private void Restart()
		{
			for (int y = 0; y < 8; y++)
				for (int x = 0; x < 8; x++)
					mGrid[x, y] = 0;

			mGrid[3, 3] = 1;
			mGrid[4, 3] = 2;
			mGrid[3, 4] = 2;
			mGrid[4, 4] = 1;
			mGameState = 0;
			SetPlayerGo(1);

			mOutputDevice.SendPercussion(Percussion.LongWhistle, 127);
		}

		private bool CanPlaceAt(int x, int y)
		{
			if (mGrid[x, y] != 0)
				return false;

			for (int dy = -1; dy <= 1; dy++)
				for (int dx = -1; dx <= 1; dx++)
					if (CheckDirection(x, y, dx, dy))
						return true;

			return false;
		}

		private bool CheckDirection(int x, int y, int dx, int dy, int state = 0)
		{
			x += dx;
			y += dy;
			if (!InBounds(x, y))
				return false;
			if (state == 0) {
				if (mGrid[x, y] == 0 || mGrid[x, y] == mPlayerTurn)
					return false;
				return CheckDirection(x, y, dx, dy, 1);
			} else if (state == 1) {
				if (mGrid[x, y] == 0)
					return false;
				if (mGrid[x, y] == mPlayerTurn)
					return true;
				return CheckDirection(x, y, dx, dy, 1);
			}

			return false;
		}

		private bool InBounds(int x, int y)
		{
			return (x >= 0 && y >= 0 && x < 8 && y < 8);
		}

		private void PlaceAt(int x, int y)
		{
			if (mPlayerTurn == 1)
				mOutputDevice.SendPercussion(Percussion.SnareDrum1, 127);
			else if (mPlayerTurn == 2)
				mOutputDevice.SendPercussion(Percussion.SnareDrum2, 127);

			for (int dy = -1; dy <= 1; dy++)
				for (int dx = -1; dx <= 1; dx++)
					if (CheckDirection(x, y, dx, dy))
						SwapDirection(x, y, dx, dy);

			mGrid[x, y] = mPlayerTurn;
			SetPlayerGo((mPlayerTurn == 1 ? 2 : 1));
		}

		private void SwapDirection(int x, int y, int dx, int dy)
		{
			x += dx;
			y += dy;
			if (mGrid[x, y] != mPlayerTurn) {
				mGrid[x, y] = mPlayerTurn;
				SwapDirection(x, y, dx, dy);
			}
		}

		private void UpdatePossiblePlaces()
		{
			mPossiblePlaces.Clear();
			for (int y = 0; y < 8; y++)
				for (int x = 0; x < 8; x++)
					if (CanPlaceAt(x, y))
						mPossiblePlaces.Add(new Tuple<int, int>(x, y));
		}

		private void SetPlayerGo(int n)
		{
			mPlayerTurn = n;
			UpdatePossiblePlaces();
			if (mPossiblePlaces.Count == 0) {
				mPlayerTurn = (mPlayerTurn == 1 ? 2 : 1);
				UpdatePossiblePlaces();
				if (mPossiblePlaces.Count == 0) {
					mGameState = 1;
					mConfirmTime = int.MaxValue;
					mOutputDevice.SendPercussion(Percussion.CrashCymbal1, 127);
				}
			}
			mPlayerWinning = GetWinner();

			if (mPossiblePlaces.Count > 0 && mSolo && mPlayerTurn == 2) {
				int p = mRandom.Next(0, mPossiblePlaces.Count);
				PlaceAt(mPossiblePlaces[p].Item1, mPossiblePlaces[p].Item2);
			}

			mForceDraw = true;
		}

		private int GetWinner()
		{
			int p1 = 0, p2 = 0;
			for (int y = 0; y < 8; y++) {
				for (int x = 0; x < 8; x++) {
					if (mGrid[x, y] == 1)
						p1++;
					else if (mGrid[x, y] == 2)
						p2++;
				}
			}
			if (p1 > p2)
				return 1;
			else if (p2 > p1)
				return 2;
			else
				return 0;
		}

		public void Run()
		{
			long last_tick = Environment.TickCount;
			long delay = 1;
			long duration = 0;

			while (true) {
				delay = (int)(1000.0 / 60.0);
				duration = Environment.TickCount - last_tick;
				if (duration < delay) {
					if (mForceDraw) {
						Draw();
						mForceDraw = false;
					}

					continue;
				}
				last_tick = Environment.TickCount;
				mConfirmTime = Math.Max(0, mConfirmTime - (int)duration);
				mFlashCounter = (mFlashCounter + 1) % 20;

				Draw();
			}
		}

		private void Draw()
		{
			ButtonBrightness[,] redgrid = new ButtonBrightness[8, 8];
			ButtonBrightness[,] greengrid = new ButtonBrightness[8, 8];

			for (int y = 0; y < 8; y++) {
				for (int x = 0; x < 8; x++) {
					if (mGrid[x, y] == 1) {
						redgrid[x, y] = ButtonBrightness.Full;
					} else if (mGrid[x, y] == 2) {
						greengrid[x, y] = ButtonBrightness.Full;
					} else if (mShowPossibleMoves && mFlashCounter < 10) {
						if (mPossiblePlaces.Exists(p => p.Item1 == x && p.Item2 == y)) {
							redgrid[x, y] = ButtonBrightness.Low;
							greengrid[x, y] = ButtonBrightness.Low;
						}
					}
				}
			}

			for (int y = 0; y < 8; y++)
				for (int x = 0; x < 8; x++)
					mLaunchpadDevice[x, y].SetBrightness(redgrid[x, y], greengrid[x, y]);

			if (mPlayerTurn == 1) {
				mLaunchpadDevice.GetButton(ToolbarButton.User1).SetBrightness(ButtonBrightness.Full, ButtonBrightness.Off);
				mLaunchpadDevice.GetButton(ToolbarButton.User2).SetBrightness(ButtonBrightness.Off, ButtonBrightness.Off);
			} else if (mPlayerTurn == 2) {
				mLaunchpadDevice.GetButton(ToolbarButton.User1).SetBrightness(ButtonBrightness.Off, ButtonBrightness.Off);
				mLaunchpadDevice.GetButton(ToolbarButton.User2).SetBrightness(ButtonBrightness.Off, ButtonBrightness.Full);
			}

			if (mPlayerWinning == 1) {
				mLaunchpadDevice.GetButton(ToolbarButton.Mixer).SetBrightness(ButtonBrightness.Full, ButtonBrightness.Off);
			} else if (mPlayerWinning == 2) {
				mLaunchpadDevice.GetButton(ToolbarButton.Mixer).SetBrightness(ButtonBrightness.Off, ButtonBrightness.Full);
			} else {
				mLaunchpadDevice.GetButton(ToolbarButton.Mixer).SetBrightness(ButtonBrightness.Off, ButtonBrightness.Off);
			}

			if (mShowPossibleMoves)
				mLaunchpadDevice.GetButton(SideButton.TrackOn).TurnOnLight();
			else
				mLaunchpadDevice.GetButton(SideButton.TrackOn).TurnOffLight();

			if (mConfirmTime > 0)
				mLaunchpadDevice.GetButton(ToolbarButton.Session).TurnOnLight();
			else
				mLaunchpadDevice.GetButton(ToolbarButton.Session).TurnOffLight();

			if (mSolo)
				mLaunchpadDevice.GetButton(SideButton.Solo).TurnOnLight();
			else
				mLaunchpadDevice.GetButton(SideButton.Solo).TurnOffLight();
		}
	}
}
