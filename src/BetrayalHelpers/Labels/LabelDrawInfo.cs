using System.Collections.Generic;
using PoeHUD.Poe.RemoteMemoryObjects;
using SharpDX;

namespace BetrayalHelpers.Labels
{
	public struct LabelDrawInfo
	{
		public BetrayalSyndicateState State;
		public RectangleF MasterRect;
		public RectangleF RestrictRect;
		public SyndicatePositionReward RewardConfig;
		public List<SyndicatePositionReward> MasterRewards;
		public bool IsEventGuy;

		public string TargetName => State.Target.Name;
	}
}