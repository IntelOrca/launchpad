using IntelOrca.Launchpad;

namespace IntelOrca.LaunchpadTests
{
	class ToggleGrid
	{
		private LaunchpadDevice mLaunchpadDevice;

		public ToggleGrid(LaunchpadDevice device)
		{
			mLaunchpadDevice = device;

			mLaunchpadDevice.DoubleBuffered = false;
			mLaunchpadDevice.ButtonPressed += mLaunchpadDevice_ButtonPressed;

			mLaunchpadDevice.GetButton(ToolbarButton.Session).SetBrightness(ButtonBrightness.Full, ButtonBrightness.Full);

			/*
			for (int y = 0; y < 4; y++) {
				for (int x = 0; x < 4; x++) {
					mLaunchpadDevice[x, y].SetBrightness((ButtonBrightness)x, (ButtonBrightness)y);
				}
			}
			*/
		}

		private void mLaunchpadDevice_ButtonPressed(object sender, ButtonPressEventArgs e)
		{
			if (e.Type == ButtonType.Grid) {
				LaunchpadButton button = mLaunchpadDevice[e.X, e.Y];
				if (button.RedBrightness == ButtonBrightness.Off && button.GreenBrightness == ButtonBrightness.Off)
					button.SetBrightness(ButtonBrightness.Full, ButtonBrightness.Full);
				else
					button.SetBrightness(ButtonBrightness.Off, ButtonBrightness.Off);

				/*
				if (button.RedBrightness == ButtonBrightness.Off && button.GreenBrightness == ButtonBrightness.Off)
					button.SetBrightness(ButtonBrightness.Full, ButtonBrightness.Off);
				else if (button.RedBrightness == ButtonBrightness.Full && button.GreenBrightness == ButtonBrightness.Off)
					button.SetBrightness(ButtonBrightness.Off, ButtonBrightness.Full);
				else if (button.RedBrightness == ButtonBrightness.Off && button.GreenBrightness == ButtonBrightness.Full)
					button.SetBrightness(ButtonBrightness.Full, ButtonBrightness.Full);
				else
					button.SetBrightness(ButtonBrightness.Off, ButtonBrightness.Off);
				*/
			} else if (e.Type == ButtonType.Toolbar) {
				if (e.ToolbarButton == ToolbarButton.Session) {
					for (int y = 0; y < 8; y++)
						for (int x = 0; x < 8; x++)
							mLaunchpadDevice[x, y].TurnOffLight();
				}
			}
		}

		public void Run()
		{
			while (true) ;
		}
	}
}
