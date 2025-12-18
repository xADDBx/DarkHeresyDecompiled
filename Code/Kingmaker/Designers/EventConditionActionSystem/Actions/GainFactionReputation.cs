using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Gameplay.Features.Reputation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("0c999ae67aa244d183f40f6aeff494e1")]
[PlayerUpgraderAllowed(false)]
public class GainFactionReputation : GameAction
{
	public int Reputation;

	public FactionType Faction;

	public override string GetCaption()
	{
		return $"Gain {Reputation} points for {Faction.ToString()}";
	}

	protected override void RunAction()
	{
	}
}
