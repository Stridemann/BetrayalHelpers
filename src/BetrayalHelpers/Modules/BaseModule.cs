using System;
using PoeHUD.Controllers;
using PoeHUD.Hud.UI;
using PoeHUD.Plugins;
using PoeHUD.Poe.RemoteMemoryObjects;

namespace BetrayalHelpers.Modules
{
	public abstract class BaseModule
	{
		public Graphics Graphics => BasePlugin.API.Graphics;
		public GameController GameController => BasePlugin.API.GameController;
		public IngameState IngameState => GameController.Game.IngameState;
		public ServerData ServerData => GameController.Game.IngameState.ServerData;
		public IngameUIElements IngameUi => GameController.Game.IngameState.IngameUi;
		public int FontSize => (int) Math.Round(Settings.FontSize.Value * IngameUi.SyndicatePanel.Scale) * 3;
		public Settings Settings => Core.Instance.Settings;
		
		public void LogMessage(object message, float displayTime)
		{
			BasePlugin.LogMessage(message, displayTime);
		}

		public void LogError(object message, float displayTime)
		{
			BasePlugin.LogError(message, displayTime);
		}
	}
}
