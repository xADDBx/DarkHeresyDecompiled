using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

public static class InitiativeHelper
{
	private enum InitiativeType
	{
		Squad,
		SquadLeader,
		SquadMember,
		Independent,
		Multi,
		Other
	}

	public static void Roll(IEnumerable<MechanicEntity> newCombatants, bool relax)
	{
		relax &= !EtudeBracketForceInitiativeOrder.Any;
		if (!Game.Instance.Controllers.TurnController.TurnBasedModeActive || newCombatants.Empty())
		{
			return;
		}
		List<MechanicEntity> list = newCombatants.Where((MechanicEntity i) => i.Initiative.Empty).ToTempList();
		list.Sort(OrderEntitiesByInitiativeType);
		foreach (MechanicEntity item in list)
		{
			RollInitiative(item);
		}
		if (relax)
		{
			RelaxInitiativeRolls(list);
		}
		TryForceInitiativeOrder(list);
		foreach (MechanicEntity item2 in list)
		{
			ApplyInitiative(item2);
		}
		PostProcessInitiative(list);
		foreach (MechanicEntity item3 in list)
		{
			UpdateBuffsInitiative(item3);
		}
	}

	public static void Update()
	{
		foreach (MechanicEntity mechanicEntity in Game.Instance.EntityPools.MechanicEntities)
		{
			UpdateBuffsInitiative(mechanicEntity);
		}
		foreach (MechanicEntity mechanicEntity2 in Game.Instance.EntityPools.MechanicEntities)
		{
			if (!Game.Instance.Controllers.TurnController.TurnBasedModeActive)
			{
				mechanicEntity2.Initiative.Clear();
			}
			else if (mechanicEntity2 is AreaEffectEntity areaEffectEntity)
			{
				areaEffectEntity.UpdateCombatInitiative();
			}
		}
	}

	private static void RollInitiative(MechanicEntity entity)
	{
		if (entity.GetInitiativeRollProvider() == entity)
		{
			float? num = entity.GetCombatStateOptional()?.OverrideInitiative;
			if (num.HasValue)
			{
				float valueOrDefault = num.GetValueOrDefault();
				entity.Initiative.Roll = Math.Max(1f, valueOrDefault);
			}
			else
			{
				MechanicEntity initiator = ((entity is UnitSquad unitSquad) ? unitSquad.InitiativeRoller : entity);
				entity.Initiative.Roll = Math.Max(1f, Rulebook.Trigger(new RuleRollInitiative(initiator)).Result);
			}
		}
	}

	private static void RelaxInitiativeRolls(IEnumerable<MechanicEntity> entities)
	{
		InitiativeDistribution random = InitiativeDistribution.GetRandom();
		InitiativeDistribution.Range[] array = random?.Ranges;
		if (array != null && array.Length > 0)
		{
			IEnumerable<MechanicEntity> enumerable = CollectInitiativeSubjects(entities);
			if (!enumerable.Empty())
			{
				Func<float> relaxedInitiativeRollIterator = GetRelaxedInitiativeRollIterator(enumerable);
				RelaxInitiativeRolls(random, enumerable, relaxedInitiativeRollIterator);
			}
		}
	}

	private static void RelaxInitiativeRolls(InitiativeDistribution distribution, IEnumerable<MechanicEntity> subjects, Func<float> getNextInitiativeRoll)
	{
		(IEnumerable<MechanicEntity> player, IEnumerable<MechanicEntity> npc) tuple = SplitByFaction(subjects);
		List<MechanicEntity> list = tuple.player.OrderBy((MechanicEntity i) => i.Initiative.Roll).ToTempList();
		List<MechanicEntity> list2 = tuple.npc.OrderBy((MechanicEntity i) => i.Initiative.Roll).ToTempList();
		List<MechanicEntity> list4;
		List<MechanicEntity> list5;
		if (!distribution.StartsFromPlayer)
		{
			List<MechanicEntity> list3 = list;
			list4 = list2;
			list5 = list3;
		}
		else
		{
			List<MechanicEntity> list3 = list2;
			list4 = list;
			list5 = list3;
		}
		InitiativeDistribution.Range[] ranges = distribution.Ranges;
		foreach (InitiativeDistribution.Range range in ranges)
		{
			int num = PFStatefulRandom.Mechanics.Range(Math.Max(1, range.Min), Math.Max(1, range.Max) + 1);
			MechanicEntity value;
			while (num-- > 0 && list4.TryPop(out value))
			{
				value.Initiative.Roll = getNextInitiativeRoll();
			}
			if (list5.Empty())
			{
				break;
			}
			List<MechanicEntity> list6 = list5;
			List<MechanicEntity> list3 = list4;
			list4 = list6;
			list5 = list3;
		}
		MechanicEntity value2;
		while (list4.TryPop(out value2))
		{
			value2.Initiative.Roll = getNextInitiativeRoll();
			if (!list5.Empty())
			{
				List<MechanicEntity> list7 = list5;
				List<MechanicEntity> list3 = list4;
				list4 = list7;
				list5 = list3;
			}
		}
	}

	private static IEnumerable<MechanicEntity> CollectInitiativeSubjects(IEnumerable<MechanicEntity> entities)
	{
		return entities.Where((MechanicEntity i) => i.GetInitiativeRollProvider() == i);
	}

	private static Func<float> GetRelaxedInitiativeRollIterator(IEnumerable<MechanicEntity> subjects)
	{
		int num = subjects.Count();
		float num2 = Math.Max(1f, subjects.Min((MechanicEntity i) => i.Initiative.Roll));
		float num3 = Math.Max(1f, subjects.Max((MechanicEntity i) => i.Initiative.Roll));
		float step = Math.Min(1f, (num3 - num2) / (float)num);
		float current = num2 + step * (float)num;
		return delegate
		{
			float result = Math.Max(1f, current);
			current -= step;
			return result;
		};
	}

	private static (IEnumerable<MechanicEntity> player, IEnumerable<MechanicEntity> npc) SplitByFaction(IEnumerable<MechanicEntity> subjects)
	{
		return (player: subjects.Where((MechanicEntity i) => i.IsInPlayerParty), npc: subjects.Where((MechanicEntity i) => !i.IsInPlayerParty));
	}

	private static void TryForceInitiativeOrder(List<MechanicEntity> entities)
	{
		ApplyEtudeForceInitiativeOverrides(entities);
		CollectPersistentOverrideEntities(entities);
		List<MechanicEntity> entitiesWithOverrides = (from e in entities
			where e.Initiative.Override != null
			orderby e.Initiative.Override.InnerPriority descending
			select e).ToList();
		if (entitiesWithOverrides.Empty())
		{
			return;
		}
		List<MechanicEntity> calculatedUnits = (from unit in Game.Instance.Controllers.TurnController.AllUnits
			where unit.IsInCombat && !entitiesWithOverrides.Contains(unit)
			select unit into u
			orderby u.Initiative.Roll descending
			select u).ToList();
		int i = 0;
		int num = 0;
		while (!entitiesWithOverrides.Empty())
		{
			MechanicEntity mechanicEntity = entitiesWithOverrides[i];
			MechanicEntity mechanicEntity2 = null;
			int num2;
			if (mechanicEntity.Initiative.Override.PercentPosition < 0)
			{
				mechanicEntity2 = mechanicEntity.Initiative.Override.FollowEntity ?? mechanicEntity.Initiative.Override.UnitEvaluator.GetValue();
				if (mechanicEntity2 == null || !mechanicEntity2.IsInCombat)
				{
					MoveToCalculated(mechanicEntity);
					num = 0;
					continue;
				}
				if (entitiesWithOverrides.Contains(mechanicEntity2))
				{
					if (++num >= entitiesWithOverrides.Count)
					{
						mechanicEntity.Initiative.Roll = 15762825 - 100 * mechanicEntity.Initiative.Override.InnerPriority;
						MoveToCalculated(mechanicEntity);
						num = 0;
					}
					else
					{
						i = (i + 1) % entitiesWithOverrides.Count;
					}
					continue;
				}
				num2 = calculatedUnits.IndexOf(mechanicEntity2);
			}
			else
			{
				num2 = calculatedUnits.Count * mechanicEntity.Initiative.Override.PercentPosition / 100 - 1;
				if (num2 == -1)
				{
					mechanicEntity.Initiative.Roll = 15762825 - 100 * mechanicEntity.Initiative.Override.InnerPriority - mechanicEntity.Initiative.Override.PercentPosition;
					MoveToCalculated(mechanicEntity);
					num = 0;
					continue;
				}
				mechanicEntity2 = calculatedUnits[num2];
			}
			num = 0;
			if (num2 == calculatedUnits.Count - 1)
			{
				mechanicEntity.Initiative.Roll = mechanicEntity2.Initiative.Roll / 2f;
			}
			else
			{
				mechanicEntity.Initiative.Roll = (mechanicEntity2.Initiative.Roll + calculatedUnits[num2 + 1].Initiative.Roll) / 2f;
			}
			MoveToCalculated(mechanicEntity);
		}
		void MoveToCalculated(MechanicEntity e)
		{
			entitiesWithOverrides.RemoveAt(i);
			calculatedUnits.Add(e);
			calculatedUnits.Sort((MechanicEntity u1, MechanicEntity u2) => u2.Initiative.Roll.CompareTo(u1.Initiative.Roll));
			if (i >= entitiesWithOverrides.Count)
			{
				i = 0;
			}
		}
	}

	private static void ApplyEtudeForceInitiativeOverrides(List<MechanicEntity> entities)
	{
		List<BaseUnitEntity> list = EtudeBracketForceInitiativeOrder.GetOrderOptional()?.ToList();
		if (list == null || list.Count == 0 || Game.Instance.Controllers.TurnController.CombatRound != 0)
		{
			return;
		}
		MechanicEntity mechanicEntity = null;
		foreach (BaseUnitEntity item in list)
		{
			MechanicEntity mechanicEntity2 = item;
			if (mechanicEntity2 == null || !mechanicEntity2.IsInCombat || !entities.Contains(mechanicEntity2))
			{
				MechanicEntity mechanicEntity3 = item;
				if (mechanicEntity3 != null && mechanicEntity3.IsInCombat)
				{
					mechanicEntity = mechanicEntity3;
				}
				continue;
			}
			if (mechanicEntity != null)
			{
				mechanicEntity2.Initiative.Overrides.Add(new InitiativeOverride
				{
					InnerPriority = 100,
					PercentPosition = -1,
					FollowEntity = mechanicEntity,
					EtudeEnforcement = true
				});
			}
			else
			{
				mechanicEntity2.Initiative.Overrides.Add(new InitiativeOverride
				{
					InnerPriority = 100,
					PercentPosition = 0,
					EtudeEnforcement = true
				});
			}
			mechanicEntity = mechanicEntity2;
		}
	}

	private static void CollectPersistentOverrideEntities(List<MechanicEntity> entities)
	{
		foreach (MechanicEntity allUnit in Game.Instance.Controllers.TurnController.AllUnits)
		{
			if (allUnit.IsInCombat && !entities.Contains(allUnit))
			{
				InitiativeOverride @override = allUnit.Initiative.Override;
				if (@override != null && @override.Persistent)
				{
					allUnit.Initiative.Clear();
					RollInitiative(allUnit);
					entities.Add(allUnit);
				}
			}
		}
	}

	private static void ApplyInitiative(MechanicEntity entity)
	{
		Initiative initiative = entity.Initiative;
		float value = (entity.Initiative.Roll = entity.GetInitiativeRollProvider().Initiative.Roll);
		initiative.Value = value;
		entity.Initiative.Order = CalculateOrder(entity);
	}

	private static void PostProcessInitiative(List<MechanicEntity> entities)
	{
		foreach (MechanicEntity entity2 in entities)
		{
			MechanicEntity entity = entity2;
			PartMultiInitiative multiInitiative = entity.GetMultiInitiative();
			if (multiInitiative == null)
			{
				continue;
			}
			if (multiInitiative.ByEnemiesCount)
			{
				multiInitiative.AdditionalTurnsCount = Game.Instance.Controllers.TurnController.AllUnits.Count((MechanicEntity e) => !e.IsDeadOrUnconscious && e.IsInCombat && entity.IsEnemy(e)) - 1;
			}
			IEnumerable<InitiativePlaceholderEntity> enumerable = multiInitiative.EnsurePlaceholders();
			multiInitiative.Placeholders = enumerable.ToList();
			int num = multiInitiative.AdditionalTurnsCount + 1;
			MechanicEntity[] array = (from unit in Game.Instance.Controllers.TurnController.AllUnits
				where unit.IsInCombat && unit.IsEnemy(entity)
				select unit into u
				orderby u.Initiative.Value descending
				select u).ToArray();
			float[] array2 = array.Select((MechanicEntity unit) => unit.Initiative.Value).ToArray();
			float num2 = (float)array2.Count() / (float)num;
			int num3 = 0;
			int num4;
			foreach (InitiativePlaceholderEntity item in enumerable)
			{
				num4 = Mathf.FloorToInt(num2 * (float)num3);
				if (num4 >= array2.Count() - 1)
				{
					if (multiInitiative.ByEnemiesCount)
					{
						item.CorrespondingEnemy = array.Last();
						item.Initiative.LastTurn = item.CorrespondingEnemy.Initiative.LastTurn;
					}
					OverrideInitiative(item, array2.Last() / 2f);
				}
				else
				{
					if (multiInitiative.ByEnemiesCount)
					{
						item.CorrespondingEnemy = array[num4];
						item.Initiative.LastTurn = item.CorrespondingEnemy.Initiative.LastTurn;
					}
					OverrideInitiative(item, (array2[num4] + array2[num4 + 1]) / 2f);
				}
				num3++;
			}
			num4 = Mathf.FloorToInt(num2 * (float)num3);
			if (num4 >= array2.Count() - 1)
			{
				if (multiInitiative.ByEnemiesCount)
				{
					multiInitiative.CorrespondingEnemy = array.Last();
					entity.Initiative.LastTurn = multiInitiative.CorrespondingEnemy.Initiative.LastTurn;
				}
				OverrideInitiative(entity, array2.Last() / 2f);
			}
			else
			{
				if (multiInitiative.ByEnemiesCount)
				{
					multiInitiative.CorrespondingEnemy = array[num4];
					entity.Initiative.LastTurn = multiInitiative.CorrespondingEnemy.Initiative.LastTurn;
				}
				OverrideInitiative(entity, (array2[num4] + array2[num4 + 1]) / 2f);
			}
			void OverrideInitiative(MechanicEntity e, float value)
			{
				e.Initiative.Roll = entity.Initiative.Roll;
				e.Initiative.Value = value;
				e.Initiative.Order = CalculateOrder(e);
			}
		}
	}

	private static int CalculateOrder(MechanicEntity entity)
	{
		return Game.Instance.Controllers.TurnController.AllUnits.Count((MechanicEntity i) => i != entity && Math.Abs(i.Initiative.Value - entity.Initiative.Value) < 1E-06f);
	}

	private static int OrderEntitiesByInitiativeType(MechanicEntity e1, MechanicEntity e2)
	{
		return GetInitiativeType(e1).CompareTo(GetInitiativeType(e2));
	}

	private static InitiativeType GetInitiativeType(MechanicEntity entity)
	{
		if (entity is UnitSquad)
		{
			return InitiativeType.Squad;
		}
		PartSquad squadOptional = entity.GetSquadOptional();
		if (squadOptional != null && squadOptional.IsLeader)
		{
			return InitiativeType.SquadLeader;
		}
		if (entity.GetSquadOptional() != null)
		{
			return InitiativeType.SquadMember;
		}
		if (entity.GetInitiativeRollProvider() == entity)
		{
			return InitiativeType.Independent;
		}
		if (entity.GetMultiInitiative() == null)
		{
			return InitiativeType.Other;
		}
		return InitiativeType.Multi;
	}

	private static void UpdateBuffsInitiative(MechanicEntity entity)
	{
		foreach (Buff rawFact in entity.Buffs.RawFacts)
		{
			rawFact.UpdateCombatInitiative();
		}
	}

	private static MechanicEntity GetInitiativeRollProvider(this MechanicEntity entity)
	{
		UnitSquad unitSquad = entity.GetSquadOptional()?.Squad;
		if (unitSquad != null)
		{
			return unitSquad;
		}
		if ((bool)entity.Features.IsFirstInFight || (bool)entity.Features.IsLastInFight)
		{
			return entity;
		}
		MechanicEntity mechanicEntity = entity.GetSummonedMonsterOption()?.Summoner;
		if (mechanicEntity != null)
		{
			return mechanicEntity;
		}
		return entity;
	}
}
