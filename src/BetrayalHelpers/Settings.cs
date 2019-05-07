using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PoeHUD.Controllers;
using SharpDX;
using PoeHUD.Plugins;
using PoeHUD.Hud.Settings;
using PoeHUD.Poe.RemoteMemoryObjects;

namespace BetrayalHelpers
{
	//All properties and public fields of this class will be saved to file
	public class Settings : SettingsBase
	{
		public Settings()
		{
			Enable = true;
			//AddDefault();
		}

		//[Menu("PosX")]
		//public RangeNode<int> PosX { get; set; } = new RangeNode<int>(10, 0, 2000);

		//[Menu("PosY")]
		//public RangeNode<int> PosY { get; set; } = new RangeNode<int>(120, 0, 2000);

		[Menu("Font Size")]
		public RangeNode<int> FontSize { get; set; } = new RangeNode<int>(7, 1, 40);

		[Menu("Reward color")]
		public ColorNode RewardColor { get; set; } = new ColorNode(Color.LightGreen);

		[Menu("Swap color")]
		public ColorNode SwapColor { get; set; } = new ColorNode(Color.Orange);

		public List<SyndicatePositionReward> Rewards = new List<SyndicatePositionReward>();
	}

	public class SyndicatePositionReward
	{
		public string MasterName;
		public string Fraction;
		public bool ShouldBeLeader;
		public string Reward;
		public int Priority;

		public BetrayalJob Job => string.IsNullOrEmpty(Fraction) ? null : GameController.Instance.Files.BetrayalJobs.EntriesList.FirstOrDefault(x => x.Name == Fraction);
	}
}