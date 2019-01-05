using System;
using BetrayalHelpers.Modules;
using PoeHUD.Framework.Helpers;
using SharpDX;
using SharpDX.Direct3D9;

namespace BetrayalHelpers.Labels
{
	public sealed class MasterNameLabel : BaseModule, IDrawLabel
	{
		public void Draw(LabelDrawInfo info)
		{
			var champNamePos = info.MasterRect.Center.Translate(0, -info.MasterRect.Height / 2 + 5);
			if (info.RestrictRect.Contains(champNamePos) && !info.IsEventGuy) 
				return;

			var champNameSize = Graphics.DrawText(
				$"{info.State.Target.Name}: {(info.State.Job.Name[0] == 'N' ? '-' : info.State.Job.Name[0])}",
				(int) Math.Round(FontSize * 1.5f), 
				champNamePos, 
				Color.White, FontDrawFlags.Center | FontDrawFlags.Top);
			Graphics.DrawBox(new RectangleF(champNamePos.X - champNameSize.Width / 2f - 5, champNamePos.Y - 1, champNameSize.Width + 10, champNameSize.Height + 1), Color.Black);

		}
	}
}