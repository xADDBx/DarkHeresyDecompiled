using System.Collections.Generic;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.AI;

[ComponentName("AI/RandomEntityEvaluator")]
[TypeId("a730247eed914513acaad80e8b8e83ee")]
public class RandomEntityEvaluator : EntityEvaluator
{
	public BlueprintReference<BlueprintFaction> Faction;

	public override string GetCaption()
	{
		if (Faction != null)
		{
			return "Random from faction: (" + Faction.NameSafe() + ")";
		}
		return "Random";
	}

	protected override Entity GetValueInternal()
	{
		List<AbstractUnitEntity> list = new List<AbstractUnitEntity>();
		foreach (AbstractUnitEntity combatParticipant in Game.Instance.EntityPools.CombatParticipants)
		{
			if (Faction != null && combatParticipant.GetOptional<PartFaction>()?.Blueprint.AssetGuid == Faction.Guid)
			{
				list.Add(combatParticipant);
			}
		}
		if (list.Count > 0)
		{
			return list[Random.Range(0, list.Count)];
		}
		return null;
	}
}
