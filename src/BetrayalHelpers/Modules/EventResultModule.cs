using System;
using System.Collections.Generic;
using System.Linq;
using PoeHUD.Framework.Helpers;
using PoeHUD.Hud.Preload;
using PoeHUD.Plugins;
using PoeHUD.Poe.RemoteMemoryObjects;
using SharpDX;

namespace BetrayalHelpers.Modules
{
	public class EventResultModule : BaseModule
	{
		private readonly List<SwapNpcEventResult> _eventResults = new List<SwapNpcEventResult>();
		private readonly List<RankEventResult> _rankResults = new List<RankEventResult>();

		public EventResultModule(EventParser eventParser, Core core)
		{
			eventParser.OnEventParsed += RecalculateResult;
			core.OnUiClose += Clear;
		}

		public void DrawRewards()
		{
			var rect = IngameUi.SyndicatePanel.TextElement;
			var drawPos = rect.GetClientRect().TopLeft.Translate(5, 5);
			var fontSize = FontSize;
			foreach (var eventResult in _eventResults)
			{
				Utils.DrawTextWithBackground(eventResult.TargetName + ":", fontSize, drawPos);
				drawPos.Y += fontSize;


				var textSize = new Size2(0, fontSize);

                if (!string.IsNullOrEmpty(eventResult.LostStuff))
                {
                    textSize = Utils.DrawTextWithBackground(eventResult.LostStuff, fontSize, drawPos, Color.Red);
                }

				if (!string.IsNullOrEmpty(eventResult.NewStuff))
					Utils.DrawTextWithBackground("=>" + eventResult.NewStuff, fontSize, drawPos.Translate(textSize.Width, textSize.Height - fontSize), Color.LightGreen);
				else
					Utils.DrawTextWithBackground("=>Nothing", fontSize, drawPos.Translate(textSize.Width, textSize.Height - fontSize), Color.Red);

				drawPos.Y += fontSize + 5;
			}

			if (_rankResults.Count > 5)
				drawPos.X += 400 * IngameUi.SyndicatePanel.Scale;

			foreach (var eventResult in _rankResults)
			{
				if (eventResult.IsLvlUp)
				{
					if (eventResult.NoRankChanges)
					{
						Utils.DrawTextWithBackground(eventResult.TargetName + $" Already top rank", fontSize, drawPos, Color.LightGray);
					}
					else
					{
						var tSize = Utils.DrawTextWithBackground(eventResult.TargetName + $" Rank: {eventResult.OldRank}->{eventResult.NewRank}", fontSize, drawPos, Color.LightGreen);

						if (!string.IsNullOrEmpty(eventResult.GoToFraction))
						{
							Utils.DrawTextWithBackground($" Go to fraction: {eventResult.GoToFraction}", fontSize, drawPos.Translate(tSize.Width, 0), Color.LightGreen);
							drawPos.Y += fontSize;
							if (!string.IsNullOrEmpty(eventResult.NewStuff)) Utils.DrawTextWithBackground($"Will get: {eventResult.NewStuff}", fontSize, drawPos, Color.LightGray);
						}
						else
						{
							drawPos.Y += fontSize;
						}

						if (!string.IsNullOrEmpty(eventResult.Stuff))
							Utils.DrawTextWithBackground(eventResult.Stuff, fontSize, drawPos);
						else
							Utils.DrawTextWithBackground("Nothing to lose...", fontSize, drawPos);
					}
				}
				else
				{
					if (eventResult.NewRank <= 0)
					{
						Utils.DrawTextWithBackground(eventResult.TargetName + $" Will be kicked from fraction! (Rank->0)", fontSize, drawPos, Color.Red);

						if (!string.IsNullOrEmpty(eventResult.Stuff))
						{
							drawPos.Y += fontSize;
							Utils.DrawTextWithBackground(eventResult.Stuff, fontSize, drawPos, Color.Red);
						}
					}
					else
					{
						Utils.DrawTextWithBackground(eventResult.TargetName + $" Rank: {eventResult.OldRank}->{eventResult.NewRank}", fontSize, drawPos, Color.Red);
						drawPos.Y += fontSize;
						Utils.DrawTextWithBackground(eventResult.Stuff, fontSize, drawPos);
					}
				}


				drawPos.Y += fontSize + 5;
			}
		}

		public void Clear()
		{
			_eventResults.Clear();
			_rankResults.Clear();
		}

		public void RecalculateResult(Tuple<string, object> evnt)
		{
			Clear();

			var action = evnt.Item1;
			if (action == "SwapNPCJob")
			{
				#region

				var realEvent = (Betrayal2Target2FractionEventArgs) evnt.Item2;

				var lostRewardTarget1 = Settings.Rewards.Find(x => x.MasterName == realEvent.Target1Name && x.Fraction == realEvent.Target1State.Job.Name);
				var lostRewardTarget2 = Settings.Rewards.Find(x => x.MasterName == realEvent.Target2Name && x.Fraction == realEvent.Target2State.Job.Name);

				var newRewardTarget1 = Settings.Rewards.Find(x => x.MasterName == realEvent.Target1Name && x.Fraction == realEvent.NewFraction1Name);
				var newRewardTarget2 = Settings.Rewards.Find(x => x.MasterName == realEvent.Target2Name && x.Fraction == realEvent.NewFraction2Name);

				_eventResults.Add(new SwapNpcEventResult {TargetName = realEvent.Target1Name, LostStuff = lostRewardTarget1?.Reward, NewStuff = newRewardTarget1?.Reward});
				_eventResults.Add(new SwapNpcEventResult {TargetName = realEvent.Target2Name, LostStuff = lostRewardTarget2?.Reward, NewStuff = newRewardTarget2?.Reward});

				#endregion
			}
			else if (action == "RemoveNPCFromOrg")
			{
				#region

				var realEvent = (BetrayalTargetEventArgs) evnt.Item2;
				var lostRewardTarget = Settings.Rewards.Find(x => x.MasterName == realEvent.TargetName && x.Fraction == realEvent.TargetState.Job.Name);

				if (lostRewardTarget != null)
					_eventResults.Add(new SwapNpcEventResult {TargetName = realEvent.TargetName, LostStuff = lostRewardTarget.Reward});
				else
					_eventResults.Add(new SwapNpcEventResult {TargetName = realEvent.TargetName, LostStuff = CalcIfNothingLose(realEvent.TargetName)});

				#endregion
			}
			else if (action == "DownrankRivalsUprankMyDivision")
			{
				#region

				var realEvent = (Betrayal2FractionEventArgs) evnt.Item2;

				var fraction1Guys = ServerData.BetrayalData.SyndicateStates.Where(x => x.Job.Name == realEvent.NewFraction1Name);
				var fraction2Guys = ServerData.BetrayalData.SyndicateStates.Where(x => x.Job.Name == realEvent.NewFraction2Name);

				//Up rank
				foreach (var fraction1Guy in fraction1Guys)
					UpRank(fraction1Guy);

				//Down rank
				foreach (var fraction2Guy in fraction2Guys)
					LoseRank(fraction2Guy);

				#endregion
			}
			else if (action == "Execute" || action == "PromoteNPC")
			{
				var realEvent = (BetrayalTargetFractionEventArgs) evnt.Item2;
				MoveFraction(realEvent);
			}
			else if (action == "StealRanks")
			{
				if (evnt.Item2 is Betrayal2TargetEventArgs realEvent)
				{
					LoseRank(realEvent.Target1State, realEvent.Target1State.Rank.RankInt); //lose all ranks?
					UpRank(realEvent.Target2State);
				}
				else if (evnt.Item2 is BetrayalTargetEventArgs realEvent2)
				{
					LoseRank(realEvent2.TargetState);
				}
			}
		}

		private void UpRank(BetrayalSyndicateState fraction1Guy)
		{
			var stuff = Settings.Rewards.Find(x => x.MasterName == fraction1Guy.Target.Name && x.Fraction == fraction1Guy.Job.Name);
			var noRankChanges = fraction1Guy.Rank.RankInt == 3;

			string gotoFraction = null;
			string newStuff = null;
			if (fraction1Guy.Rank.RankInt == 0)
			{
                LogError("BetrayalHelpers plugin: ", 5);
				var job = GetCanGoToJob();
                            
				if (job == null)
				{
					LogError("Can't find current mission type! (not found in preloads that showed on screen)", 5);
				}
				else
				{
					gotoFraction = job.Name;
					var getStuff = Settings.Rewards.Find(x => x.MasterName == fraction1Guy.Target.Name && x.Fraction == job.Name);
					newStuff = getStuff?.Reward;
				}
			}


			_rankResults.Add(new RankEventResult
			{
				TargetName = fraction1Guy.Target.Name,
				OldRank = fraction1Guy.Rank.RankInt,
				NewRank = Math.Max(fraction1Guy.Rank.RankInt + 1, 3),
				Stuff = stuff?.Reward,
				IsLvlUp = true,
				NoRankChanges = noRankChanges,
				GoToFraction = gotoFraction,
				NewStuff = newStuff
			});
		}

		private BetrayalJob GetCanGoToJob()
		{
			foreach (var s in PreloadAlertPlugin.alerts)
			foreach (var betrayalJob in GameController.Files.BetrayalJobs.EntriesList)
				if (s.Text.Contains(betrayalJob.Name))
					return betrayalJob;

			return null;
		}

		private void MoveFraction(BetrayalTargetFractionEventArgs args)
		{
			var targetName = args.TargetName;
			var lostRewardTarget = Settings.Rewards.Find(x => x.MasterName == targetName && x.Fraction == args.TargetState.Job.Name);
			var newRewardTarget = Settings.Rewards.Find(x => x.MasterName == targetName && x.Fraction == args.NewFractionName);
			var lostStuff = lostRewardTarget == null ? CalcIfNothingLose(targetName) : lostRewardTarget.Reward;
			_eventResults.Add(new SwapNpcEventResult {TargetName = targetName, LostStuff = lostStuff, NewStuff = newRewardTarget?.Reward});
		}

		private string CalcIfNothingLose(string targetName)
		{
			var looseStuffReport = "Nothing to lose...";
			var loseStuff = Settings.Rewards.Where(x => x.MasterName == targetName).ToList();

			if (loseStuff.Count > 0)
			{
				looseStuffReport = $"No lose. Cases:{Environment.NewLine}";
				foreach (var stuff in loseStuff)
					if (!string.IsNullOrEmpty(stuff.Fraction)) //Mean kill
						looseStuffReport += $"{stuff.Fraction[0]}: {stuff.Reward}" + Environment.NewLine;
			}

			return looseStuffReport;
		}

		private void LoseRank(BetrayalSyndicateState fractionGuy, int loseRank = 1)
		{
			var stuff = Settings.Rewards.Find(x => x.MasterName == fractionGuy.Target.Name && x.Fraction == fractionGuy.Job.Name);
			_rankResults.Add(new RankEventResult
			{
				TargetName = fractionGuy.Target.Name,
				OldRank = fractionGuy.Rank.RankInt,
				NewRank = fractionGuy.Rank.RankInt - loseRank,
				Stuff = stuff?.Reward
			});
		}


		private class SwapNpcEventResult
		{
			public string LostStuff = "";
			public string NewStuff;
			public string TargetName = "";
		}

		private class RankEventResult
		{
			public string GoToFraction;
			public bool IsLvlUp;
			public int NewRank; //-1 mean kicked out from fraction
			public string NewStuff;
			public bool NoRankChanges;
			public int OldRank;
			public string Stuff = "";
			public string TargetName = "";
		}
	}
}