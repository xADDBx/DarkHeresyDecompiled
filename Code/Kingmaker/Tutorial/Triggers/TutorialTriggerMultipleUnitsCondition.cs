using System;
using Kingmaker.Blueprints.Area;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Enums;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("f214b77d4b183ee49b7014b67091592a")]
public class TutorialTriggerMultipleUnitsCondition : TutorialTrigger, IAreaHandler, ISubscriber
{
	public UnitCondition TriggerCondition;

	public int MinimumUnitsCount = 4;

	public bool AllowOnGlobalMap;

	public void HandleUnitConditionsChanged(UnitCondition condition)
	{
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		if (!AllowOnGlobalMap)
		{
			BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
			if (currentlyLoadedArea == null || !currentlyLoadedArea.IsPartyArea)
			{
				TryToTrigger();
			}
		}
	}

	private void TryToTrigger()
	{
	}
}
