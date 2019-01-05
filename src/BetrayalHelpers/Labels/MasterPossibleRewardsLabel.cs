using BetrayalHelpers.Modules;
using PoeHUD.Framework.Helpers;
using SharpDX;

namespace BetrayalHelpers.Labels
{
	public sealed class MasterPossibleRewardsLabel : BaseModule, IDrawLabel
	{
		public void Draw(LabelDrawInfo info)
		{
			var variantsPos = info.MasterRect.TopLeft;
			variantsPos.Y += 40;


			foreach (var variant in info.MasterRewards)
			{
				if (variant.Fraction == info.State.Job.Name)
					continue;

				var curDrawPos = variantsPos.Translate(15, 5);
				if (!info.RestrictRect.Contains(curDrawPos) || info.IsEventGuy)
				{
					var textSize = Graphics.DrawText(variant.Reward + " :", FontSize, curDrawPos, Settings.SwapColor.Value);

					Graphics.DrawBox(new RectangleF(curDrawPos.X - 5, curDrawPos.Y, textSize.Width + 20, textSize.Height + 1), Color.Black);

					curDrawPos = curDrawPos.Translate(textSize.Width + 5, 0);
					Graphics.DrawText(variant.Fraction[0].ToString(), FontSize, curDrawPos, Color.IndianRed);
				}

				variantsPos.Y += FontSize * 1.2f;
			}
		}
	}
}