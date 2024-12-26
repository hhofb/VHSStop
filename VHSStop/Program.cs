using System;
using System.Drawing;
using System.Threading;
using WindowsInput;
using WindowsInput.Native;
using CSharpDiscordWebhook.NET.Discord;

namespace ScreenColorChecker
{
	class Program
	{
		static async Task Main(string[] args)
		{
			InputSimulator sim = new InputSimulator();
			// Set the area of the screen to check
			Rectangle area = new Rectangle(399, 58, 740, 415); // x, y, width, height
			Color targetColor = Color.FromArgb(0, 0, 0);
			Color targetColor2 = Color.FromArgb(0, 0, 244);// Replace with the color you want to check for (e.g., red)
			int durationInSeconds = 2; // Duration in seconds that the color should be present

			while (true)
			{
				bool found = CheckForColorInAreaOverTime(area, targetColor, targetColor2, durationInSeconds);
				if (found)
				{
					Console.WriteLine("Video Done!");
					await SendMessage();
					sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.SHIFT); //Shortcut in OBS to stop the recording
					while (true)
					{
						Console.Beep(1000, 1000);
						Console.Beep(500, 1000);
						Thread.Sleep(2000);
					}
					
				}
					
				else
					Console.WriteLine("...\n");
			}
		}

		static bool CheckForColorInAreaOverTime(Rectangle area, Color targetColor, Color targetColor2, int durationInSeconds)
		{
			int checkInterval = 100; // Interval in milliseconds between checks
			int checksRequired = (durationInSeconds * 1000) / checkInterval;
			int consecutiveChecks = 0;

			for (int i = 0; i < checksRequired; i++)
			{
				if (IsColorInArea(area, targetColor, targetColor2, 20))
				{
					consecutiveChecks++;
					if (consecutiveChecks >= checksRequired)
					{
						return true; // Color has been present for the entire duration
					}
				}
				else
				{
					consecutiveChecks = 0; // Reset if the color is not found
				}

				Thread.Sleep(checkInterval);
			}

			return false; // Color was not consistently present for the specified duration
		}

		static bool IsColorInArea(Rectangle area, Color targetColor, Color targetColor2, int tolerance)
		{
			using (Bitmap bitmap = new Bitmap(area.Width, area.Height))
			{
				using (Graphics g = Graphics.FromImage(bitmap))
				{
					g.CopyFromScreen(area.Location, Point.Empty, area.Size);
				}

				int matchingPixels = 0;
				int totalPixels = area.Width * area.Height;
				int threshold = (int)(totalPixels * 0.8);

				for (int x = 0; x < bitmap.Width; x++)
				{
					for (int y = 0; y < bitmap.Height; y++)
					{
						Color pixelColor = bitmap.GetPixel(x, y);
						if (IsColorInRange(pixelColor, targetColor, targetColor2, tolerance))
						{
							matchingPixels++;
							if (matchingPixels >= threshold)
							{
								return true; // 90% or more of the pixels match
							}
						}
					}
				}
			}
			
			return false; // Less than 90% of the pixels match the color
		}

		static bool IsColorInRange(Color pixelColor, Color targetColor, Color targetColor2, int tolerance)
		{
			return ((Math.Abs(pixelColor.R - targetColor.R) <= tolerance) || (Math.Abs(pixelColor.R - targetColor2.R) <= tolerance)) &&
				   ((Math.Abs(pixelColor.G - targetColor.G) <= tolerance) || (Math.Abs(pixelColor.G - targetColor2.G) <= tolerance)) &&
				   ((Math.Abs(pixelColor.B - targetColor.B) <= tolerance) || (Math.Abs(pixelColor.B - targetColor2.B) <= tolerance));
		}

		static async Task SendMessage()
		{
			DiscordWebhook hook = new DiscordWebhook();
			hook.Uri = new Uri("<Discord Webhook URL>");

			DiscordMessage message = new DiscordMessage();
			message.Content = "VHS Done, ping! @everyone";
			message.Username = "VHSStop";


			//message
			await hook.SendAsync(message);
		}
	}
}
