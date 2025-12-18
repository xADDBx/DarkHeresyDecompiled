using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Gameplay.Features.Reputation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("3821866a2e27cdb429f019da0833a621")]
public class FactionReputationLevelGetter : IntPropertyGetter
{
	public FactionType Faction;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Get Reputation level with chosen Faction";
	}

	protected override int GetBaseValue()
	{
		if (Faction != 0)
		{
			return ReputationHelper.GetCurrentReputationLevel(Faction);
		}
		return 0;
	}
}
