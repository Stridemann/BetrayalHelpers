using System;
using System.Collections.Generic;
using PoeHUD.Controllers;

namespace BetrayalHelpers.Modules
{
	public sealed class EventDecisionCalculator : BaseModule
	{
		private List<EventSnapshot> Events = new List<EventSnapshot>();
		public EventDecisionCalculator(Core core, EventParser eventParser)
		{
			core.OnEventUiOpen += OnEventUiOpen;
			GameController.Area.OnAreaChange += AreaOnOnAreaChange;
			core.OnDestroy += CoreOnOnDestroy;
			eventParser.OnEventParsed += EventParserOnOnEventParsed;
		}
		//Execute- (increased rank by 1)(Allocation status does not matter here; only relation status.)

		//If 3 members: 
		//Can execute

		//If 2 members:
		//	if hostile or neutral:
		//		Execute
		//	if frendly:
		//		Betray (Swap Leader, Remove NPC, Steal Rank, Steal Intelligence, Destroy all items of Rival Division, Down Rank Rivals/ Uprank Division)
		//			

		//if 1 member:
		//	Bargain (Promote NPC, Swap Jobs, befriends another, Gain Intelligence, NPC Leaves, Removes All from Prison, Destroy all items in Division, Remove all Rivalries, 
		//			Drop Unique Item, Drop Currency Items, Drop additional rare item with Veiled Mod, Drop a Map, Drop a Scarab)

		private void EventParserOnOnEventParsed(Tuple<string, object> tuple)
		{
			
		}

	

		private void AreaOnOnAreaChange(AreaController areaController)
		{
			Events.Clear();
		}

		private void OnEventUiOpen()
		{
			
		}

		private void CoreOnOnDestroy()
		{
			GameController.Area.OnAreaChange -= AreaOnOnAreaChange;
		}

		private class EventSnapshot
		{

		}
	}
}