using IntelOrca.Launchpad;
using Midi;
using System;
using System.Threading;

namespace IntelOrca.LaunchpadTests
{
	class RainSequencer
	{
		const int NumCols = 16;
		const int NumRows = 8;

		private LaunchpadDevice mLaunchpadDevice;

		private bool[,] mSequence = new bool[NumCols, NumRows];
		private bool[] mRemove = new bool[NumCols];
		private int mSequenceOffset;
		private int mColumnOffset;
		private int mMode, mSoundType;
		private int mConfirmTime;
		private bool mForceDraw;

		private int mTempo = 8 * 60;

		OutputDevice mOutputDevice;
		private int mInstrument = 0;
		private int mPercussion = 0;

		public RainSequencer(LaunchpadDevice device)
		{
			mLaunchpadDevice = device;
			mOutputDevice = OutputDevice.InstalledDevices[0];
			mOutputDevice.Open();

			mLaunchpadDevice.ButtonPressed += mLaunchpadDevice_ButtonPressed;

/*
			Random rand = new Random();
			for (int y = 0; y < NumRows; y++)
				for (int x = 0; x < NumRows; x++)
					if (rand.Next(0, 12) == 0)
						mSequence[x, y] = true;
 * */
		}

		private int SeqYtoButtonY(int y)
		{
			return 0;
		}

		private int ButtonYtoSeqY(int y)
		{
			return (y + NumRows - mSequenceOffset) % NumRows;
		}

		private int ButtonXtoColX(int x)
		{
			return (x + NumCols - mColumnOffset) % NumCols;
		}

		private void mLaunchpadDevice_ButtonPressed(object sender, ButtonPressEventArgs e)
		{
			if (e.Type == ButtonType.Grid) {
				if (e.Y == 7) {
					mRemove[ButtonXtoColX(e.X)] = !mRemove[ButtonXtoColX(e.X)];
					if (mMode == 1)
						PlayNoise(e.X);
				} else {
					mSequence[ButtonXtoColX(e.X), ButtonYtoSeqY(e.Y)] = !mSequence[ButtonXtoColX(e.X), ButtonYtoSeqY(e.Y)];
				}

				mForceDraw = true;
			} else if (e.Type == ButtonType.Toolbar) {
				switch (e.ToolbarButton) {
				case ToolbarButton.Up:
					mSequenceOffset = (mSequenceOffset + NumRows - 1) % NumRows;
					break;
				case ToolbarButton.Down:
					mSequenceOffset = (mSequenceOffset + 1) % NumRows;
					break;
				case ToolbarButton.Left:
					mColumnOffset = (mColumnOffset + NumCols - 1) % NumCols;
					break;
				case ToolbarButton.Right:
					mColumnOffset = (mColumnOffset + 1) % NumCols;
					break;
				case ToolbarButton.Mixer:
					mMode = (mMode == 0 ? 1 : 0);
					break;
				case ToolbarButton.Session:
					if (mConfirmTime > 0) {
						for (int y = 0; y < NumRows; y++)
							for (int x = 0; x < NumCols; x++)
								mSequence[x, y] = false;
						mConfirmTime = 0;
					} else {
						mConfirmTime = 1000;
					}
					break;
				}
				mForceDraw = true;
			} else if (e.Type == ButtonType.Side) {
				switch (e.SidebarButton) {
				case SideButton.Volume:
					if (mTempo < 1980)
					mTempo += 20;
					break;
				case SideButton.Pan:
					if (mTempo > 20)
						mTempo -= 20;
					break;
				case SideButton.SoundA:
					if (mSoundType == 0) {
						mPercussion = (mPercussion + 1) % (Percussion.OpenTriangle - Percussion.BassDrum2 + 1);
						mOutputDevice.SendProgramChange(Channel.Channel1, (Instrument)mInstrument);
					} else {
						mSoundType = 0;
					}
					break;
				case SideButton.SoundB:
					if (mSoundType == 1) {
						mInstrument = (mInstrument + 1) % 127;
						mOutputDevice.SendProgramChange(Channel.Channel1, (Instrument)mInstrument);
					} else {
						mSoundType = 1;
					}
					break;
				case SideButton.Arm:
					mSequenceOffset = 0;
					mColumnOffset = 0;
					break;
				}
			}
		}

		public void Run()
		{
			long last_tick = Environment.TickCount;
			long delay = 1;
			long duration = 0;

			while (true) {
				delay = (int)(1000.0 / (mTempo / 60.0));
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

				Update();
				Draw();
			}
		}

		private void PlayNoise(int tone)
		{
			new Thread(new ThreadStart(() => {
				if (mSoundType == 0) {
					mOutputDevice.SendPercussion(Percussion.BassDrum2 + mPercussion + tone, 127);
				} else {
					mOutputDevice.SendNoteOn(Channel.Channel1, Pitch.A4 + tone, 127);
					Thread.Sleep((int)(1000.0 / (mTempo / 60.0)));
					mOutputDevice.SendNoteOff(Channel.Channel1, Pitch.A4 + tone, 127);
				}

				// Console.Beep(100 * (tone + 3), 100);
			})).Start();
		}

		private void DiscardRedBeats()
		{
			int y = ButtonYtoSeqY(7);
			for (int x = 0; x < NumCols; x++)
				if (mSequence[x, y] && mRemove[x])
					mSequence[x, y] = false;
		}

		private void PlayBeats()
		{
			int y = ButtonYtoSeqY(7);
			for (int x = 0; x < NumCols; x++)
				if (mSequence[x, y])
					PlayNoise(x);
		}

		private void Update()
		{
			if (mMode == 1)
				return;

			DiscardRedBeats();
			mSequenceOffset = (mSequenceOffset + 1) % NumRows;
			PlayBeats();
		}

		private void Draw()
		{
			ButtonBrightness[,] redgrid = new ButtonBrightness[8, 8];
			ButtonBrightness[,] greengrid = new ButtonBrightness[8, 8];

			for (int y = 0; y < 7; y++)
				for (int x = 0; x < 8; x++)
					if (mSequence[ButtonXtoColX(x), ButtonYtoSeqY(y)])
						redgrid[x, y] = greengrid[x, y] = ButtonBrightness.Full;

			for (int x = 0; x < 8; x++) {
				ButtonBrightness brightness = (mSequence[ButtonXtoColX(x), ButtonYtoSeqY(7)] ? ButtonBrightness.Full : ButtonBrightness.Low);

				if (!mRemove[ButtonXtoColX(x)])
					greengrid[x, 7] = brightness;
				else
					redgrid[x, 7] = brightness;
			}

			for (int y = 0; y < 8; y++)
				for (int x = 0; x < 8; x++)
					mLaunchpadDevice[x, y].SetBrightness(redgrid[x, y], greengrid[x, y]);

			if (mConfirmTime > 0)
				mLaunchpadDevice.GetButton(ToolbarButton.Session).TurnOnLight();
			else
				mLaunchpadDevice.GetButton(ToolbarButton.Session).TurnOffLight();

			if (mMode == 1)
				mLaunchpadDevice.GetButton(ToolbarButton.Mixer).SetBrightness(ButtonBrightness.Full, ButtonBrightness.Off);
			else
				mLaunchpadDevice.GetButton(ToolbarButton.Mixer).SetBrightness(ButtonBrightness.Off, ButtonBrightness.Full);

			if (mSoundType == 0) {
				mLaunchpadDevice.GetButton(SideButton.SoundA).TurnOnLight();
				mLaunchpadDevice.GetButton(SideButton.SoundB).TurnOffLight();
			} else {
				mLaunchpadDevice.GetButton(SideButton.SoundA).TurnOffLight();
				mLaunchpadDevice.GetButton(SideButton.SoundB).TurnOnLight();
			}

			mLaunchpadDevice.GetButton(ToolbarButton.Up).TurnOnLight();
			mLaunchpadDevice.GetButton(ToolbarButton.Down).TurnOnLight();
			mLaunchpadDevice.GetButton(ToolbarButton.Left).TurnOnLight();
			mLaunchpadDevice.GetButton(ToolbarButton.Right).TurnOnLight();

			mLaunchpadDevice.GetButton(SideButton.Volume).TurnOnLight();
			mLaunchpadDevice.GetButton(SideButton.Pan).TurnOnLight();
			mLaunchpadDevice.GetButton(SideButton.Arm).TurnOnLight();

			mLaunchpadDevice.Refresh();

			Console.SetCursorPosition(0, 0);
			for (int y = 0; y < NumRows; y++) {
				for (int x = 0; x < NumCols; x++) {
					if (mSequence[x, y])
						Console.Write("X");
					else
						Console.Write(".");
				}
				Console.WriteLine();
			}
		}
	}
}
