using System;
using System.Text.RegularExpressions;
using System.Threading;
using PoeHUD.Poe.RemoteMemoryObjects;

namespace BetrayalHelpers.Modules
{
	public class EventParser : BaseModule
	{
		//StealIntelligence - +12 Intervention Intelligence. Hillock and Jorgin become Rivals
		//GainIntelligence - +6 Intervention Intelligence
		//NPCBefriendsAnother - Leo and Gravicius become Trusted. +4 Transportation Intelligence
		//StealRanks - +2 rank(s) to Cameria. Korell loses all ranks. Cameria and Korell become Rivals
		//DestroyAllItemsInDivision - Destroy all items of Fortification members
		//RemoveAllRivalries - Remove all rivalries in the Immortal Syndicate
		//RemoveNPCFromOrg - It That Fled removed from the Immortal Syndicate
		//PromoteNPC - Riker moves to Intervention. Jorgin and Riker become Trusted
		//Execute - +1 rank to Leo
		//DownrankRivalsUprankMyDivision - //+1 rank to Research members. -1 rank to Intervention members
		//To kick leader - downgrade rank to members. If some member have more rank it will be a leader

		private readonly Regex SwapNpcJobRegex;
		private readonly Regex DowngradeRankRegex;
		private readonly Regex ExecuteRegex;
		private readonly Regex LoseRanksRegex;
		private readonly Regex MoveRegex; //Move xxx. XXX and YY rtusted
		private readonly Regex LvlUpLvlDownRanksRegex;


		public EventParser(Core core)
		{
			SwapNpcJobRegex = new Regex("(?'Target1Name'[a-zA-Z ]+) moves to (?'Target1JobName'[a-zA-Z]+). (?'Target2Name'[a-zA-Z ]+) moves to (?'Target2JobName'[a-zA-Z]+)", RegexOptions.Compiled);
			DowngradeRankRegex = new Regex("1 rank to (?'Fraction1'[a-zA-Z]+) members. -1 rank to (?'Fraction2'[a-zA-Z]+) members", RegexOptions.Compiled);
			ExecuteRegex = new Regex(" moves to (?'NewFraction'[a-zA-Z]+)", RegexOptions.Compiled);
			LoseRanksRegex = new Regex("(?'TargetName'[a-zA-Z ]+) loses all ranks", RegexOptions.Compiled);
			MoveRegex = new Regex("(?'TargetName'[a-zA-Z ]+) moves to (?'NewFraction'[a-zA-Z]+).", RegexOptions.Compiled);
			LvlUpLvlDownRanksRegex = new Regex(" to (?'LvlUpTargetName'[a-zA-Z ]+). (?'LoseRanksTargetName'[a-zA-Z ]+) loses all ranks.", RegexOptions.Compiled);
			core.OnEventUiOpen += ParseEvent;
		}

		public event Action<Tuple<string, object>> OnEventParsed = delegate { };

		private void ParseEvent()
		{
			Thread.Sleep(100);
			var action = ServerData.BetrayalData.BetrayalEventData.Action.Id;
			var eventText = IngameUi.SyndicatePanel.EventText;
			eventText = eventText.Trim();

			void Broadcast(object evnt)
			{
				OnEventParsed(new Tuple<string, object>(action, evnt));
			}

			if (action == "SwapNPCJob")
			{
				// Hillock moves to Fortification. Cameria moves to Transportation
				var matches = SwapNpcJobRegex.Match(eventText);
				var target1Name = matches.Groups["Target1Name"].Value;
				var target2Name = matches.Groups["Target2Name"].Value;
				var newFraction1Name = matches.Groups["Target1JobName"].Value;
				var newFraction2Name = matches.Groups["Target2JobName"].Value;

				var target1 = ServerData.BetrayalData.SyndicateStates.Find(x => x.Target.Name == target1Name);
				var target2 = ServerData.BetrayalData.SyndicateStates.Find(x => x.Target.Name == target2Name);

				Broadcast(new Betrayal2Target2FractionEventArgs(target1, target2, newFraction1Name, newFraction2Name));
			}
			else if (action == "RemoveNPCFromOrg")
			{
				var targetName = eventText.Substring(0, eventText.IndexOf(" removed", StringComparison.Ordinal));
				var targetData = ServerData.BetrayalData.SyndicateStates.Find(x => x.Target.Name == targetName);
				if (targetData == null)
					throw new NotImplementedException($"Can't find master with name '{targetName}' or parse event: {eventText}");

				Broadcast(new BetrayalTargetEventArgs(targetData));
			}
			else if (action == "DownrankRivalsUprankMyDivision")
			{
				var match = DowngradeRankRegex.Match(eventText);
				var downgradeFraction = match.Groups["Fraction1"].Value;
				var upgradeFraction = match.Groups["Fraction2"].Value;
				Broadcast(new Betrayal2FractionEventArgs(downgradeFraction, upgradeFraction));
			}
			else if (action == "Execute")
			{
				var match = ExecuteRegex.Match(eventText);
				if (match.Success)
				{
					var newFractionName = match.Groups["NewFraction"].Value;
					var targetName = ServerData.BetrayalData.BetrayalEventData.Target1.Name;
					var targetData = ServerData.BetrayalData.SyndicateStates.Find(x => x.Target.Name == targetName);
					Broadcast(new BetrayalTargetFractionEventArgs(targetData, newFractionName));
				}
			}
			else if (action == "StealRanks")
			{
				var match = LvlUpLvlDownRanksRegex.Match(eventText);
				if (match.Success)
				{
					var lvlUpTargetName = match.Groups["LvlUpTargetName"].Value;
					var loseRanksTarget = match.Groups["LoseRanksTargetName"].Value;
					var lvlDnTarget = ServerData.BetrayalData.SyndicateStates.Find(x => x.Target.Name == loseRanksTarget);
					var lvlUpTarget = ServerData.BetrayalData.SyndicateStates.Find(x => x.Target.Name == lvlUpTargetName);
					Broadcast(new Betrayal2TargetEventArgs(lvlDnTarget, lvlUpTarget));
				}
				else
				{
					match = LoseRanksRegex.Match(eventText);
					var loseRankTargetName = match.Groups["TargetName"].Value;
					var lvlDnTarget = ServerData.BetrayalData.SyndicateStates.Find(x => x.Target.Name == loseRankTargetName);
					Broadcast(new BetrayalTargetEventArgs(lvlDnTarget));
				}
			}
			else if (action == "PromoteNPC")
			{
				var match = MoveRegex.Match(eventText);
				var targetName = match.Groups["TargetName"].Value;
				var newFractionName = match.Groups["NewFraction"].Value;
				var targetData = ServerData.BetrayalData.SyndicateStates.Find(x => x.Target.Name == targetName);

				Broadcast(new BetrayalTargetFractionEventArgs(targetData, newFractionName));
			}
			else
			{
				Broadcast(null);
			}
		}
	}

	public class BetrayalTargetEventArgs
	{
		public string TargetName;
		public BetrayalSyndicateState TargetState;

		public BetrayalTargetEventArgs(BetrayalSyndicateState targetState)
		{
			TargetState = targetState;
			TargetName = targetState.Target.Name;
		}
	}

	public class BetrayalTargetFractionEventArgs
	{
		public string NewFractionName;
		public string TargetName;
		public BetrayalSyndicateState TargetState;

		public BetrayalTargetFractionEventArgs(BetrayalSyndicateState targetState, string newFractionName)
		{
			TargetState = targetState;
			NewFractionName = newFractionName;
			TargetName = targetState.Target.Name;
		}
	}

	public class Betrayal2TargetEventArgs
	{
		public string Target1Name;
		public BetrayalSyndicateState Target1State;
		public string Target2Name;
		public BetrayalSyndicateState Target2State;

		public Betrayal2TargetEventArgs(BetrayalSyndicateState target1State, BetrayalSyndicateState target2State)
		{
			Target1State = target1State;
			Target2State = target2State;
			Target1Name = target1State.Target.Name;
			Target2Name = target2State.Target.Name;
		}
	}

	public class Betrayal2Target2FractionEventArgs
	{
		public string NewFraction1Name;
		public string NewFraction2Name;
		public string Target1Name;
		public BetrayalSyndicateState Target1State;
		public string Target2Name;
		public BetrayalSyndicateState Target2State;

		public Betrayal2Target2FractionEventArgs(BetrayalSyndicateState target1State, BetrayalSyndicateState target2State, string newFraction1Name, string newFraction2Name)
		{
			Target1State = target1State;
			Target2State = target2State;
			NewFraction1Name = newFraction1Name;
			NewFraction2Name = newFraction2Name;
			Target1Name = target1State.Target.Name;
			Target2Name = target2State.Target.Name;
		}
	}

	public class Betrayal2FractionEventArgs
	{
		public string NewFraction1Name;
		public string NewFraction2Name;

		public Betrayal2FractionEventArgs(string newFraction1Name, string newFraction2Name)
		{
			NewFraction1Name = newFraction1Name;
			NewFraction2Name = newFraction2Name;
		}
	}
}