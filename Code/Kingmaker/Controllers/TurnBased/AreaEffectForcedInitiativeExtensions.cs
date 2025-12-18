using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility.UnityExtensions;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

public static class AreaEffectForcedInitiativeExtensions
{
	public static float GetInitiativeValue(this AreaEffectForcedInitiative initiative)
	{
		TurnController turnController = Game.Instance.Controllers.TurnController;
		if (!turnController.InCombat)
		{
			return 0f;
		}
		IEnumerable<MechanicEntity> source = turnController.TurnOrder.UnitsOrder.Where((MechanicEntity i) => i.Initiative.InterruptingOrder == 0);
		switch (initiative)
		{
		case AreaEffectForcedInitiative.First:
			return source.First().Initiative.Value + 1f;
		case AreaEffectForcedInitiative.Last:
			return 0f;
		case AreaEffectForcedInitiative.Previous:
		{
			MechanicEntity currentUnit = turnController.CurrentUnit;
			if (currentUnit == null)
			{
				return 0f;
			}
			MechanicEntity mechanicEntity2 = source.TakeWhile((MechanicEntity u) => u != currentUnit).LastOrDefault();
			if (mechanicEntity2 == null)
			{
				return 0f;
			}
			return currentUnit.Initiative.Value + (mechanicEntity2.Initiative.Value - currentUnit.Initiative.Value) / 2f;
		}
		case AreaEffectForcedInitiative.Next:
		{
			MechanicEntity currentUnit = turnController.CurrentUnit;
			if (currentUnit == null)
			{
				return 0f;
			}
			MechanicEntity mechanicEntity = source.SkipWhile((MechanicEntity u) => u != currentUnit).Skip(1).FirstOrDefault();
			if (mechanicEntity == null)
			{
				return 0f;
			}
			return currentUnit.Initiative.Value - (currentUnit.Initiative.Value - mechanicEntity.Initiative.Value) / 2f;
		}
		case AreaEffectForcedInitiative.Skip25PctOfRound:
			return GetInitiativeByRoundProgress(0.25f);
		case AreaEffectForcedInitiative.Skip50PctOfRound:
			return GetInitiativeByRoundProgress(0.5f);
		case AreaEffectForcedInitiative.Skip75PctOfRound:
			return GetInitiativeByRoundProgress(0.75f);
		default:
			throw new ArgumentOutOfRangeException("initiative", initiative, null);
		}
	}

	private static float GetInitiativeByRoundProgress(float progress)
	{
		TurnOrderQueue turnOrder = Game.Instance.Controllers.TurnController.TurnOrder;
		IEnumerable<MechanicEntity> first = turnOrder.CurrentRoundUnitsOrder.Where((MechanicEntity i) => IsCountable(i) && i.Initiative.InterruptingOrder == 0);
		IEnumerable<MechanicEntity> second = turnOrder.NextRoundUnitsOrder.Where((MechanicEntity i) => IsCountable(i) && i.Initiative.InterruptingOrder == 0);
		List<MechanicEntity> list;
		using (first.Concat(second).ToPooledList(out list))
		{
			if (list.Count == 0)
			{
				return 0f;
			}
			if (list.Count == 1)
			{
				return list[0].Initiative.Value + 1f;
			}
			progress = Math.Clamp(progress, 0f, 1f);
			int num = Math.Clamp(Mathf.RoundToInt((float)list.Count * progress), 0, list.Count - 2);
			float value = list[num].Initiative.Value;
			float value2 = list[num + 1].Initiative.Value;
			return (value > value2) ? (value - (value - value2) / 2f) : (value2 + 1f);
		}
	}

	private static bool IsCountable(MechanicEntity entity)
	{
		if (entity is UnitEntity)
		{
			if (!entity.IsInSquad)
			{
				goto IL_001a;
			}
		}
		else if (entity is UnitSquad)
		{
			goto IL_001a;
		}
		return false;
		IL_001a:
		return true;
	}
}
