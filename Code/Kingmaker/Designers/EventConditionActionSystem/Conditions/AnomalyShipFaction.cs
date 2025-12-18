using System;
using System.Diagnostics.CodeAnalysis;
using Kingmaker.ElementsSystem;
using Kingmaker.Gameplay.Features.Reputation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("a712e10a564f4b2585a10100aeb078d6")]
public class AnomalyShipFaction : Condition
{
	[NotNull]
	public FactionType Faction;

	protected override string GetConditionCaption()
	{
		return "Check faction of currently interacted anomaly";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
