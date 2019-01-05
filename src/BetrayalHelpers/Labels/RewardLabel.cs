using BetrayalHelpers.Modules;
using SharpDX;
using SharpDX.Direct3D9;
using PoeHUD.Framework.Helpers;

namespace BetrayalHelpers.Labels
{
	public sealed class RewardLabel : BaseModule, IDrawLabel
	{
		public void Draw(LabelDrawInfo info)
		{
			if (info.RewardConfig == null || string.IsNullOrEmpty(info.RewardConfig.Reward))
				return;

			var rewardDrawPos = info.MasterRect.Center.Translate(0, info.MasterRect.Height / 2 - FontSize - 15);
			if (info.RestrictRect.Contains(rewardDrawPos) && !info.IsEventGuy)
				return;

			var textSize = Graphics.DrawText(info.RewardConfig.Reward, FontSize, rewardDrawPos, Settings.RewardColor.Value, FontDrawFlags.Center | FontDrawFlags.Top);
			Graphics.DrawBox(new RectangleF(rewardDrawPos.X - textSize.Width / 2f - 5, rewardDrawPos.Y, textSize.Width + 10, textSize.Height + 1), Color.Black);
		}
	}
}