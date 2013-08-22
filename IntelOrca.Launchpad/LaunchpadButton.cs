using Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelOrca.Launchpad
{
	public enum ButtonType { Grid, Toolbar, Side }
	public enum ButtonBrightness { Off, Low, Medium, Full };
	public enum ButtonPressState { Up = 0, Down = 127 };

	public class LaunchpadButton
	{
		private LaunchpadDevice mLaunchpadDevice;
		private ButtonBrightness mRedBrightness, mGreenBrightness;
		private ButtonPressState mState;

		private ButtonType mType;
		private int mIndex;

		internal LaunchpadButton(LaunchpadDevice launchpadDevice, ButtonType type, int index)
		{
			mLaunchpadDevice = launchpadDevice;
			mType = type;
			mIndex = index;
		}

		public void TurnOnLight()
		{
			SetBrightness(ButtonBrightness.Full, ButtonBrightness.Full);
		}

		public void TurnOffLight()
		{
			SetBrightness(ButtonBrightness.Off, ButtonBrightness.Off);
		}

		public void SetBrightness(ButtonBrightness red, ButtonBrightness green)
		{
			if (mRedBrightness == red && mGreenBrightness == green)
				return;

			mRedBrightness = red;
			mGreenBrightness = green;

			int vel = ((int)mGreenBrightness << 4) | (int)mRedBrightness;

			if (!mLaunchpadDevice.DoubleBuffered)
				vel |= 12;
			
			SetLED(vel);
		}

		private void SetLED(int value)
		{
			if (mType == ButtonType.Toolbar)
				mLaunchpadDevice.OutputDevice.SendControlChange(Channel.Channel1, (Control)mIndex, value);
			else
				mLaunchpadDevice.OutputDevice.SendNoteOn(Channel.Channel1, (Pitch)mIndex, value);
		}

		public ButtonBrightness RedBrightness
		{
			get { return mRedBrightness; }
			internal set { mRedBrightness = value; }
		}

		public ButtonBrightness GreenBrightness
		{
			get { return mGreenBrightness; }
			internal set { mGreenBrightness = value; }
		}

		public ButtonPressState State
		{
			get { return mState; }
			internal set { mState = value; }
		}
	}
}
