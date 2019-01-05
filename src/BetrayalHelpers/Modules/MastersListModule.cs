using System;
using System.Collections.Generic;
using System.Linq;
using PoeHUD.Controllers;
using PoeHUD.Plugins;
using PoeHUD.Poe.Components;
using SharpDX;

namespace BetrayalHelpers.Modules
{
	public class MastersListModule
	{
		private readonly Dictionary<string, List<EventMaster>> _eventMasters = new Dictionary<string, List<EventMaster>>();
		private static GameController GameController => BasePlugin.API.GameController;

		public MastersListModule(Core core)
		{
			core.OnUiOpen += UpdateMasterEntities;
		}

		private void UpdateMasterEntities()
		{
			_eventMasters.Clear();
			var ents = GameController.Entities.Where(x => x.IsAlive && x.IsHostile && x.HasComponent<Monster>()).ToList();

			var bStates = GameController.Game.IngameState.ServerData.BetrayalData.SyndicateStates;
			foreach (var entityWrapper in ents)
			{
				var path = entityWrapper.Path;
				var subIndex = path.IndexOf("@", StringComparison.Ordinal);
				if (subIndex != -1)
					path = path.Substring(0, subIndex);

				var variety = GameController.Files.MonsterVarieties.TranslateFromMetadata(path);
				foreach (var betrayalSyndicateState in bStates)
					if (betrayalSyndicateState.Target.MonsterVariety == variety)
					{
						var fraction = betrayalSyndicateState.Job.Name;
						if (!_eventMasters.TryGetValue(fraction, out var masterList))
						{
							masterList = new List<EventMaster>();
							_eventMasters.Add(fraction, masterList);
						}

						masterList.Add(new EventMaster {Name = betrayalSyndicateState.Target.Name, Fraction = fraction});
						break;
					}
			}
		}

		public void DrawMasters()
		{
			var drawPos = new Vector2(10, 150);
			foreach (var eventMasterList in _eventMasters)
			{
				Utils.DrawTextWithBackground(eventMasterList.Key, 30, drawPos);
				drawPos.Y += 30;

				foreach (var eventMaster in eventMasterList.Value)
				{
					Utils.DrawTextWithBackground(eventMaster.Name, 20, drawPos);
					drawPos.Y += 20;
				}

				drawPos.Y += 30;
			}
		}
	}

	public class EventMaster
	{
		public string Fraction;
		public string Name;
	}
}