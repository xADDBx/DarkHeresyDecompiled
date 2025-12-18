using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Actions;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Owlcat.AI;

public static class AiBrainHelper
{
	public interface IThreatsInfo
	{
		IReadOnlyCollection<BaseUnitEntity> AooUnits { get; }

		IReadOnlyCollection<BaseUnitEntity> OverwatchUnits { get; }

		IReadOnlyCollection<AreaEffectEntity> AreaEffects { get; }

		IReadOnlyCollection<AreaEffectEntity> DamagingOnMoveAreaEffects { get; }
	}

	private class ThreatsInfo : IThreatsInfo
	{
		public HashSet<BaseUnitEntity> aooUnits = new HashSet<BaseUnitEntity>();

		public HashSet<BaseUnitEntity> overwatchUnits = new HashSet<BaseUnitEntity>();

		public HashSet<AreaEffectEntity> areaEffects = new HashSet<AreaEffectEntity>();

		public HashSet<AreaEffectEntity> damagingOnMoveAreaEffects = new HashSet<AreaEffectEntity>();

		public IReadOnlyCollection<BaseUnitEntity> AooUnits => aooUnits;

		public IReadOnlyCollection<BaseUnitEntity> OverwatchUnits => overwatchUnits;

		public IReadOnlyCollection<AreaEffectEntity> AreaEffects => areaEffects;

		public IReadOnlyCollection<AreaEffectEntity> DamagingOnMoveAreaEffects => damagingOnMoveAreaEffects;
	}

	private delegate bool ThreatCheck(AreaEffectLogic aeLogic, BaseUnitEntity unit);

	private static int lastFrame = -1;

	private static BaseUnitEntity lastUnit = null;

	private static Dictionary<GraphNode, ThreatsInfo> m_Cache = new Dictionary<GraphNode, ThreatsInfo>();

	private static readonly ThreatCheck[] ThreatChecks = new ThreatCheck[3] { CheckDealDamage, CheckApplyDOT, CheckApplyBuffWithDamage };

	private static readonly ThreatCheck[] DamageOnMoveChecks = new ThreatCheck[2] { CheckDamageOnMove, CheckApplyDOTOnMove };

	public static IThreatsInfo EmptyThreatsInfo => new ThreatsInfo();

	private static bool IsValidCacheForUnit(BaseUnitEntity unit)
	{
		if (Time.frameCount == lastFrame)
		{
			return lastUnit == unit;
		}
		return false;
	}

	public static Dictionary<GraphNode, IThreatsInfo> GatherThreatsData(BaseUnitEntity unit)
	{
		Dictionary<GraphNode, ThreatsInfo> dictionary = GatherThreatsDataInternal(unit);
		Dictionary<GraphNode, IThreatsInfo> dictionary2 = new Dictionary<GraphNode, IThreatsInfo>();
		foreach (KeyValuePair<GraphNode, ThreatsInfo> item in dictionary)
		{
			dictionary2.TryAdd(item.Key, item.Value);
		}
		return dictionary2;
	}

	public static IThreatsInfo GetThreatsData(BaseUnitEntity unit, GraphNode node)
	{
		if (GatherThreatsDataInternal(unit).TryGetValue(node, out var value))
		{
			return value;
		}
		return EmptyThreatsInfo;
	}

	private static Dictionary<GraphNode, ThreatsInfo> GatherThreatsDataInternal(BaseUnitEntity unit)
	{
		if (IsValidCacheForUnit(unit))
		{
			return m_Cache;
		}
		m_Cache.Clear();
		foreach (UnitGroupMemory.UnitInfo enemy in unit.CombatGroup.Memory.Enemies)
		{
			BaseUnitEntity unit2 = enemy.Unit;
			if (unit2.CanMakeAttackOfOpportunity(unit))
			{
				foreach (GraphNode item in unit2.GetThreateningArea())
				{
					if (!m_Cache.TryGetValue(item, out var value))
					{
						value = new ThreatsInfo();
						m_Cache.Add(item, value);
					}
					value.aooUnits.Add(unit2);
				}
			}
			PartOverwatch optional = unit2.GetOptional<PartOverwatch>();
			if (optional == null)
			{
				continue;
			}
			foreach (GridNodeBase item2 in optional.OverwatchArea)
			{
				if (!m_Cache.TryGetValue(item2, out var value2))
				{
					value2 = new ThreatsInfo();
					m_Cache.Add(item2, value2);
				}
				value2.overwatchUnits.Add(unit2);
			}
		}
		foreach (AreaEffectEntity areaEffect in Game.Instance.EntityPools.AreaEffects)
		{
			if (!IsThreateningArea(areaEffect, unit))
			{
				continue;
			}
			foreach (GridNodeBase coveredNode in areaEffect.CoveredNodes)
			{
				if (!m_Cache.TryGetValue(coveredNode, out var value3))
				{
					value3 = new ThreatsInfo();
					m_Cache.Add(coveredNode, value3);
				}
				value3.areaEffects.Add(areaEffect);
				if (CheckThreats(areaEffect, unit, DamageOnMoveChecks))
				{
					value3.damagingOnMoveAreaEffects.Add(areaEffect);
				}
			}
		}
		lastFrame = Time.frameCount;
		lastUnit = unit;
		return m_Cache;
	}

	public static bool IsThreateningArea(AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
		return CheckThreats(areaEffect, unit, ThreatChecks);
	}

	private static bool CheckThreats(AreaEffectEntity areaEffect, BaseUnitEntity unit, IEnumerable<ThreatCheck> checkList)
	{
		if (!areaEffect.IsSuitableTargetType(unit))
		{
			return false;
		}
		List<BlueprintComponent> list = TempList.Get<BlueprintComponent>();
		list.AddRange(areaEffect.Blueprint.ComponentsArray);
		list.AddRange(areaEffect.Blueprint.ComponentsArray.Where((BlueprintComponent c) => c is AreaEffectClusterComponent).SelectMany((BlueprintComponent c) => (c as AreaEffectClusterComponent)?.ClusterLogicBlueprint.ComponentsArray));
		foreach (AreaEffectLogic item in list.OfType<AreaEffectLogic>())
		{
			foreach (ThreatCheck check in checkList)
			{
				if (check(item, unit))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool CheckDamageOnMove(AreaEffectLogic aeLogic, BaseUnitEntity unit)
	{
		if (!(aeLogic is AreaEffectRunAction areaEffectRunAction))
		{
			return false;
		}
		return IsDealDamage(areaEffectRunAction.UnitMove);
	}

	private static bool CheckApplyDOTOnMove(AreaEffectLogic aeLogic, BaseUnitEntity unit)
	{
		if (!(aeLogic is AreaEffectRunAction areaEffectRunAction))
		{
			return false;
		}
		return IsApplyDOT(areaEffectRunAction.UnitMove);
	}

	private static bool CheckDealDamage(AreaEffectLogic aeLogic, BaseUnitEntity unit)
	{
		if (!(aeLogic is AreaEffectRunAction areaEffectRunAction))
		{
			return false;
		}
		if (!IsDealDamage(areaEffectRunAction.UnitEnter) && !IsDealDamage(areaEffectRunAction.UnitExit) && !IsDealDamage(areaEffectRunAction.UnitMove))
		{
			return IsDealDamage(areaEffectRunAction.Round);
		}
		return true;
	}

	private static bool CheckApplyDOT(AreaEffectLogic aeLogic, BaseUnitEntity unit)
	{
		if (!(aeLogic is AreaEffectRunAction areaEffectRunAction))
		{
			return false;
		}
		if (!IsApplyDOT(areaEffectRunAction.UnitEnter) && !IsApplyDOT(areaEffectRunAction.UnitExit) && !IsApplyDOT(areaEffectRunAction.UnitMove))
		{
			return IsApplyDOT(areaEffectRunAction.Round);
		}
		return true;
	}

	private static bool CheckApplyBuffWithDamage(AreaEffectLogic aeLogic, BaseUnitEntity unit)
	{
		if (!(aeLogic is AreaEffectBuff areaEffectBuff))
		{
			return false;
		}
		BlueprintComponent[] componentsArray = areaEffectBuff.Buff.ComponentsArray;
		for (int i = 0; i < componentsArray.Length; i++)
		{
			if (componentsArray[i] is AddFactContextActions addFactContextActions && (IsDealDamage(addFactContextActions.Activated) || IsDealDamage(addFactContextActions.Deactivated) || IsDealDamage(addFactContextActions.NewRound)))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsDealDamage(ActionList actionList)
	{
		return actionList.Actions.Any(IsDealDamage);
	}

	private static bool IsDealDamage(GameAction a)
	{
		if (!(a is ContextActionDealDamage))
		{
			DodgeActions obj = a as DodgeActions;
			if (obj == null || !obj.ActionsOnDodge.Actions.Contains((GameAction a2) => a2 is ContextActionDealDamage))
			{
				return (a as DodgeActions)?.ActionsOnHit.Actions.Contains((GameAction a2) => a2 is ContextActionDealDamage) ?? false;
			}
		}
		return true;
	}

	private static bool IsApplyDOT(ActionList actionList)
	{
		return actionList.Actions.Any(IsApplyDOT);
	}

	private static bool IsApplyDOT(GameAction a)
	{
		if (!(a is ContextActionApplyDOT))
		{
			DodgeActions obj = a as DodgeActions;
			if (obj == null || !obj.ActionsOnDodge.Actions.Contains((GameAction a2) => a2 is ContextActionApplyDOT))
			{
				return (a as DodgeActions)?.ActionsOnHit.Actions.Contains((GameAction a2) => a2 is ContextActionApplyDOT) ?? false;
			}
		}
		return true;
	}
}
