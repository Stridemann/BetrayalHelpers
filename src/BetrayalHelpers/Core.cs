using System;
using System.Collections.Generic;
using System.Linq;
using BetrayalHelpers.Labels;
using BetrayalHelpers.Modules;
using PoeHUD.Plugins;
using SharpDX;

namespace BetrayalHelpers
{
	public class Core : BaseSettingsPlugin<Settings>
	{
		public static Core Instance;

		private readonly List<IDrawLabel> _labels = new List<IDrawLabel>();
		private MastersListModule _mastersList;
		private EventParser _eventParser;
		private EventResultModule _eventResult;
		private bool _wasOpened;
		public event Action OnUiOpen = delegate { };
		public event Action OnUiClose = delegate { };
		public event Action OnEventUiOpen = delegate { };
		public event Action OnDestroy = delegate { };

		public Core()
		{
			PluginName = "BetrayalHelpers";
			Instance = this;
		}

		public override void Initialise()
		{
			_eventParser = new EventParser(this);
			_eventResult = new EventResultModule(_eventParser, this);
			_mastersList = new MastersListModule(this);

			_labels.Add(new MasterNameLabel());
			_labels.Add(new RewardLabel());
			_labels.Add(new MasterPossibleRewardsLabel());

			SetDefaultRewards();
		}

		public override void Render()
		{
			if (!GameController.InGame)
				return;

			var syndicatePanel = IngameUi.SyndicatePanel;
			if (!syndicatePanel.IsVisible)
			{
				if (_wasOpened)
				{
					_wasOpened = false;
					OnUiClose();
				}
			
				return;
			}

			var recalculateResult = false;
			if (!_wasOpened)
			{
				_wasOpened = true;
				recalculateResult = true;
				OnUiOpen();
			}

			var restrictRect = new RectangleF();
			var eventElement = syndicatePanel.EventElement;
			if (eventElement.IsVisible)
			{
				restrictRect = eventElement.GetChildAtIndex(4).GetClientRect();
				var second = eventElement.GetChildAtIndex(5).GetClientRect();
				restrictRect.Width = second.X - restrictRect.X + second.Width + 250 * IngameUi.SyndicatePanel.Scale;

				if (recalculateResult)
					OnEventUiOpen();

				_eventResult.DrawRewards();
			}
			
			foreach (var state in ServerData.BetrayalData.SyndicateStates)
			{
				var masterRewards = Settings.Rewards.Where(x => x.MasterName == state.Target.Name).ToList();

				var drawInfo = new LabelDrawInfo
				{
					State = state,
					MasterRect = state.UIElement.GetClientRect(),
					RestrictRect = restrictRect,
					MasterRewards = masterRewards,
					RewardConfig = masterRewards.FirstOrDefault(x => x.Fraction == state.Job.Name),
					IsEventGuy = ServerData.BetrayalData.BetrayalEventData?.Target1 == state.Target
				};

				foreach (var label in _labels) label.Draw(drawInfo);
			}

			_mastersList.DrawMasters();
		}

		public override void OnPluginDestroyForHotReload()
		{
			OnDestroy();
		}

		private void SetDefaultRewards()
		{
			Settings.Rewards.Clear();
			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "It That Fled",
				Fraction = "Research",
				ShouldBeLeader = true,
				Reward = "Upgrade breachstones"
			});
			//Settings.Rewards.Add(new SyndicatePositionReward
			//{
			//	MasterName = "Cameria",
			//	Fraction = "Transportation",
			//	ShouldBeLeader = true,
			//	Reward = "Timeworn unique",
			//	Priority = 1
			//});
			//Settings.Rewards.Add(new SyndicatePositionReward
			//{
			//	MasterName = "Cameria",
			//	Fraction = "Fortification",
			//	ShouldBeLeader = true,
			//	Reward = "Harbinger orbs",
			//	Priority = 2
			//});
			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "Cameria",
				Fraction = "Intervention",
				ShouldBeLeader = true,
				Reward = "Sulphite scarab",
				Priority = 3
			});
			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "Gravicius",
				Fraction = "Transportation",
				ShouldBeLeader = true,
				Reward = "Full stack of div cards",
				Priority = 1
			});
			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "Gravicius",
				Fraction = "Intervention",
				Reward = "Divination Scarab",
				Priority = 2
			});
			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "Rin",
				Fraction = "Intervention",
				ShouldBeLeader = true,
				Reward = "Cartography scarab"
			});
			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "Riker",
				Fraction = "Transportation",
				ShouldBeLeader = true,
				Reward = "Pick a currency item",
				Priority = 2
			});
			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "Riker",
				Fraction = "Intervention",
				ShouldBeLeader = true,
				Reward = "Pick a div. card",
				Priority = 1
			});


			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "Aisling",
				Fraction = "Research",
				Reward = "Adding veiled mod"
			});
			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "Elreon",
				Fraction = "Intervention",
				Reward = "Reliquary scarab"
			});
			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "Haku",
				Fraction = "Intervention",
				Reward = "Strongbox scarab"
			});
			//Settings.Rewards.Add(new SyndicatePositionReward
			//{
			//	MasterName = "Hillock",
			//	Fraction = "Transportation",
			//	Reward = "%Q on wep"
			//});
			//Settings.Rewards.Add(new SyndicatePositionReward
			//{
			//	MasterName = "Hillock",
			//	Fraction = "Fortification",
			//	Reward = "%Q on armour"
			//});
			//Settings.Rewards.Add(new SyndicatePositionReward
			//{
			//	MasterName = "Hillock",
			//	Fraction = "Intervention",
			//	Reward = "%Q on flasks"
			//});
			//Settings.Rewards.Add(new SyndicatePositionReward
			//{
			//	MasterName = "Hillock",
			//	Fraction = "Research",
			//	Reward = "%Q on maps"
			//});
			//Settings.Rewards.Add(new SyndicatePositionReward
			//{
			//	MasterName = "Janus",
			//	Fraction = "Research",
			//	Reward = "Perandus C./Cadiro"
			//});
			//Settings.Rewards.Add(new SyndicatePositionReward
			//{
			//	MasterName = "Janus",
			//	Fraction = "Intervention",
			//	Reward = "Perandus Scarab"
			//});
			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "Jorgin",
				Fraction = "Research",
				Reward = "Talisman"
			});
			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "Jorgin",
				Fraction = "Intervention",
				Reward = "Beastiary scarab"
			});
			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "Korell",
				Fraction = "Fortification",
				Reward = "Map fragments",
				Priority = 2
			});
			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "Korell",
				Fraction = "Research",
				Reward = "Fossils",
				Priority = 1
			});
			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "Guff",
				Fraction = "Transportation",
				Reward = "Chaos spam an item",
				Priority = 1
			});
			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "Vorici",
				Fraction = "Research",
				Reward = "White sockets",
				Priority = 1
			});
			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "Leo",
				Fraction = "Fortification",
				Reward = "Currency",
				Priority = 2
			});
			Settings.Rewards.Add(new SyndicatePositionReward
			{
				MasterName = "Leo",
				Fraction = "Research",
				Reward = "Exalt an item",
				Priority = 2
			});

			//Kill
			//Settings.Rewards.Add(new SyndicatePositionReward {MasterName = "Vagan"});
			//Settings.Rewards.Add(new SyndicatePositionReward {MasterName = "Tora"}); //?
			//Settings.Rewards.Add(new SyndicatePositionReward {MasterName = "Aisling"}); //?
			//Settings.Rewards.Add(new SyndicatePositionReward {MasterName = "Haku"});
		}

	}
}