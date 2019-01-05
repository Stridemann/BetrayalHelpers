using PoeHUD.Hud.UI;
using PoeHUD.Plugins;
using SharpDX;

namespace BetrayalHelpers
{
	public class Utils
	{
		private static Graphics Graphics => BasePlugin.API.Graphics;
		public static Size2 DrawTextWithBackground(string text, int height, Vector2 position)
		{
			return DrawTextWithBackground(text, height, position, Color.White);
		}

		public static Size2 DrawTextWithBackground(string text, int height, Vector2 position, Color color)
		{
			if (string.IsNullOrEmpty(text))
				text = "%NoText%";
			var textSize = Graphics.DrawText(text, height, position, color);
			Graphics.DrawBox(new RectangleF(position.X - 5, position.Y, textSize.Width + 20, textSize.Height + 1), Color.Black);
			return textSize;
		}
	}
}
