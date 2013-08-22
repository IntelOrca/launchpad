using System;

namespace IntelOrca.Launchpad
{
	public class ButtonPressEventArgs : EventArgs
	{
		private ButtonType mType;
		private ToolbarButton mToolbarButton;
		private SideButton mSidebarButton;
		private int mX, mY;

		public ButtonPressEventArgs(ToolbarButton toolbarButton)
		{
			mType = ButtonType.Toolbar;
			mToolbarButton = toolbarButton;
		}

		public ButtonPressEventArgs(SideButton sideButton)
		{
			mType = ButtonType.Side;
			mSidebarButton = sideButton;
		}

		public ButtonPressEventArgs(int x, int y)
		{
			mType = ButtonType.Grid;
			mX = x;
			mY = y;
		}

		public ButtonType Type
		{
			get { return mType; }
		}

		public ToolbarButton ToolbarButton
		{
			get { return mToolbarButton; }
		}

		public SideButton SidebarButton
		{
			get { return mSidebarButton; }
		}

		public int X
		{
			get { return mX; }
		}

		public int Y
		{
			get { return mY; }
		}
	}
}
