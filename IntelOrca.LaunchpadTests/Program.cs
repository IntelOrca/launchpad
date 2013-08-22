using IntelOrca.Launchpad;
using System;

namespace IntelOrca.LaunchpadTests
{
	class Program
	{
		static void Main(string[] args)
		{
			LaunchpadDevice device;

			Console.WriteLine("Launchpad Tests");
			Console.WriteLine("Ted John 2013");
			Console.WriteLine("");

			try {
				device = new LaunchpadDevice();
				device.DoubleBuffered = true;

				Console.WriteLine("Launchpad found");
			} catch {
				Console.WriteLine("No launchpad found");
				Console.ReadLine();
				return;
			}

			Console.WriteLine("");
			Console.WriteLine("0: Grid toggle");
			Console.WriteLine("1: Scrolling message");
			Console.WriteLine("2: Bulldog");
			Console.WriteLine("3: Rain sequencer");
			Console.WriteLine("4: Reversi");
			Console.WriteLine("5: Snake");

			int i;
			while (!Int32.TryParse(Console.ReadLine(), out i)) {
				Console.WriteLine("Try again...");
			}

			switch (i) {
			case 0:
				ToggleGrid toggleGrid = new ToggleGrid(device);
				toggleGrid.Run();
				break;
			case 1:
				Console.Write("Type a message:");
				string message = Console.ReadLine();

				ScrollingLetters scrollingLetters = new ScrollingLetters(device);
				scrollingLetters.Text = message.ToUpper();
				scrollingLetters.ScrollText();
				break;
			case 2:
				Bulldog bulldog = new Bulldog(device);
				bulldog.Play();
				break;
			case 3:
				RainSequencer rain = new RainSequencer(device);
				rain.Run();
				break;
			case 4:
				Reversi reversi = new Reversi(device);
				reversi.Run();
				break;
			case 5:
				Snake snake = new Snake(device);
				snake.Run();
				break;
			default:
				Console.WriteLine("No such application");
				break;
			}
		}
	}
}
