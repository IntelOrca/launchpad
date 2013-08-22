using Midi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelOrca.Launchpad
{
	public enum ToolbarButton { Up, Down, Left, Right, Session, User1, User2, Mixer }
	public enum SideButton { Volume, Pan, SoundA, SoundB, Stop, TrackOn, Solo, Arm }

	public class LaunchpadDevice
	{
		private InputDevice mInputDevice;
		private OutputDevice mOutputDevice;

		private bool mDoubleBuffered;
		private bool mDoubleBufferedState;

		private readonly LaunchpadButton[] mToolbar = new LaunchpadButton[8];
		private readonly LaunchpadButton[] mSide = new LaunchpadButton[8];
		private readonly LaunchpadButton[,] mGrid = new LaunchpadButton[8, 8];

		public event EventHandler<ButtonPressEventArgs> ButtonPressed;

		public LaunchpadDevice() : this(0) { }

		public LaunchpadDevice(int index)
		{
			InitialiseButtons();

			int i = 0;
			mInputDevice = InputDevice.InstalledDevices.Where(x => x.Name.Contains("Launchpad")).
				FirstOrDefault(x => i++ == index);
			i = 0;
			mOutputDevice = OutputDevice.InstalledDevices.Where(x => x.Name.Contains("Launchpad")).
				FirstOrDefault(x => i++ == index);

			if (mInputDevice == null)
				throw new LaunchpadException("Unable to find input device.");
			if (mOutputDevice == null)
				throw new LaunchpadException("Unable to find output device.");

			mInputDevice.Open();
			mOutputDevice.Open();

			mInputDevice.StartReceiving(new Clock(120));
			mInputDevice.NoteOn += mInputDevice_NoteOn;
			mInputDevice.ControlChange += mInputDevice_ControlChange;

			Reset();
		}

		private void InitialiseButtons()
		{
			for (int i = 0; i < 8; i++) {
				mToolbar[i] = new LaunchpadButton(this, ButtonType.Toolbar, 104 + i);
				mSide[i] = new LaunchpadButton(this, ButtonType.Side, i * 16 + 8);
			}

			for (int y = 0; y < 8; y++)
				for (int x = 0; x < 8; x++)
					mGrid[x, y] = new LaunchpadButton(this, ButtonType.Grid, y * 16 + x);
		}

		private void StartDoubleBuffering()
		{
			mDoubleBuffered = true;
			mDoubleBufferedState = false;
			mOutputDevice.SendControlChange(Channel.Channel1, (Control)0, 32 | 16 | 1 );
		}

		public void Refresh()
		{
			if (!mDoubleBufferedState)
				mOutputDevice.SendControlChange(Channel.Channel1, (Control)0, 32 | 16 | 4);
			else
				mOutputDevice.SendControlChange(Channel.Channel1, (Control)0, 32 | 16 | 1);
			mDoubleBufferedState = !mDoubleBufferedState;
		}

		private void EndDoubleBuffering()
		{
			mOutputDevice.SendControlChange(Channel.Channel1, (Control)0, 32 | 16);
			mDoubleBuffered = false;
		}

		public void Reset()
		{
			mOutputDevice.SendControlChange(Channel.Channel1, (Control)0, 0);
			Buttons.ToList().ForEach(x => x.RedBrightness = x.GreenBrightness = ButtonBrightness.Off);
		}

		private void mInputDevice_NoteOn(NoteOnMessage msg)
		{
			LaunchpadButton button = GetButton(msg.Pitch);
			if (button == null)
				return;

			button.State = (ButtonPressState)msg.Velocity;

			if (ButtonPressed != null && button.State == ButtonPressState.Down) {
				if ((int)msg.Pitch % 16 == 8)
					ButtonPressed.Invoke(this, new ButtonPressEventArgs((SideButton)((int)msg.Pitch / 16)));
				else
					ButtonPressed.Invoke(this, new ButtonPressEventArgs((int)msg.Pitch % 16, (int)msg.Pitch / 16));
			}
		}

		private void mInputDevice_ControlChange(ControlChangeMessage msg)
		{
			ToolbarButton toolbarButton = (ToolbarButton)((int)msg.Control - 104);

			LaunchpadButton button = GetButton(toolbarButton);
			if (button == null)
				return;

			button.State = (ButtonPressState)msg.Value;
			if (ButtonPressed != null && button.State == ButtonPressState.Down) {
				ButtonPressed.Invoke(this, new ButtonPressEventArgs(toolbarButton));
			}
		}

		public LaunchpadButton GetButton(ToolbarButton toolbarButton)
		{
			return mToolbar[(int)toolbarButton];
		}

		public LaunchpadButton GetButton(SideButton sideButton)
		{
			return mSide[(int)sideButton];
		}

		private LaunchpadButton GetButton(Pitch pitch)
		{
			int x = (int)pitch % 16;
			int y = (int)pitch / 16;
			if (x < 8 && y < 8)
				return mGrid[x, y];
			else if (x == 8 && y < 8)
				return mSide[y];

			return null;
		}

		public bool DoubleBuffered
		{
			get { return mDoubleBuffered; }
			set
			{
				if (mDoubleBuffered)
					EndDoubleBuffering();
				else
					StartDoubleBuffering();
			}
		}

		public LaunchpadButton this[int x, int y]
		{
			get { return mGrid[x, y]; }
		}

		public IEnumerable<LaunchpadButton> Buttons
		{
			get
			{
				for (int y = 0; y < 8; y++)
					for (int x = 0; x < 8; x++)
						yield return mGrid[x, y];
			}
		}

		internal OutputDevice OutputDevice
		{
			get { return mOutputDevice; }
		}
	}
}
