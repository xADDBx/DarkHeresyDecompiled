using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.View.Spawners;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using UnityEngine;

namespace Kingmaker.Designers;

public static class GameHelper
{
	public static class Quests
	{
		public static void GiveObjective(BlueprintQuestObjective objecive)
		{
			Game.Instance.QuestBook.GiveObjective(objecive);
		}

		public static void CompleteObjective(BlueprintQuestObjective objecive)
		{
			Game.Instance.QuestBook.CompleteObjective(objecive);
		}

		public static void FailObjective(BlueprintQuestObjective objecive)
		{
			Game.Instance.QuestBook.FailObjective(objecive);
		}

		public static IEnumerable<Quest> GetList()
		{
			return Game.Instance.QuestBook.Quests;
		}

		public static QuestState GetQuestState(BlueprintQuest quest)
		{
			return Game.Instance.QuestBook.GetQuestState(quest);
		}

		public static Quest GetQuest(BlueprintQuest quest)
		{
			return Game.Instance.QuestBook.GetQuest(quest);
		}

		public static QuestObjectiveState GetObjectiveState(BlueprintQuestObjective objective)
		{
			return Game.Instance.QuestBook.GetObjectiveState(objective);
		}

		public static bool IsObjectiveStarted([NotNull] BlueprintQuestObjective objective)
		{
			return Game.Instance.QuestBook.Quests.FirstOrDefault((Quest q) => q.IsActive && q.Objectives.FirstOrDefault((QuestObjective o) => o.Blueprint == objective && o.IsVisible && o.State == QuestObjectiveState.Started) != null) != null;
		}
	}

	public static AbstractUnitSpawnerEntity GetUnitSpawner(EntityReference entityReference)
	{
		return entityReference.FindData() as AbstractUnitSpawnerEntity;
	}

	public static BaseUnitEntity GetPlayerCharacter()
	{
		return Game.Instance.Player.MainCharacterEntity;
	}

	public static BaseUnitEntity GetPlayerCharacterOriginal()
	{
		return Game.Instance.Player.MainCharacterOriginalEntity;
	}

	public static BaseUnitEntity RecruitNPC([CanBeNull] BaseUnitEntity npc, BlueprintUnit blueprint)
	{
		return Game.Instance.Controllers.EntitySpawner.RecruitNPC(npc, blueprint);
	}

	public static void DealDirectDamage(BaseUnitEntity source, BaseUnitEntity target, int damage)
	{
		Rulebook.Instance.TriggerEvent(new RuleDealDamage(source, target, new IntermediateDamage(DamageType.Direct, damage)));
	}

	public static void HealDamage(BaseUnitEntity source, BaseUnitEntity target, int amount)
	{
		Rulebook.Instance.TriggerEvent(RuleHealDamage.Setup(source, target).Base(amount).Create());
	}

	public static Buff ApplyBuff(AbstractUnitEntity target, BlueprintBuff buff, Rounds? duration = null)
	{
		return target.Buffs.Add(buff, target, null, duration);
	}

	public static void RemoveBuff(AbstractUnitEntity target, BlueprintBuff blueprint)
	{
		if (target is LightweightUnitEntity lightweightUnitEntity)
		{
			lightweightUnitEntity.RemoveBuffFx(blueprint);
		}
		else if (target.Buffs != null)
		{
			target.Facts.Remove(blueprint);
		}
	}

	public static void KillUnit(AbstractUnitEntity unit, MechanicEntity killer, int impulseMultiplier, UnitDismemberType dismemberType = UnitDismemberType.None, DismembermentLimbsApartType? dismembermentLimbsApartType = null)
	{
		unit.LifeState.ScriptedKill = true;
		unit.LifeState.ForceDismember = dismemberType;
		unit.LifeState.DismembermentLimbsApartType = dismembermentLimbsApartType;
		unit.Wake(1f);
		if (killer != null)
		{
			unit.Health.LastHandledDamage = new RuleDealDamage(killer, unit, new IntermediateDamage(DamageType.Direct, 0));
			if (unit.View is UnitEntityView unitEntityView)
			{
				unitEntityView.AddRagdollImpulse((unit.Position - killer.Position).normalized, impulseMultiplier, DamageType.Direct);
			}
		}
		else
		{
			unit.Health.LastHandledDamage = null;
		}
	}

	public static void DestroyUnit(BaseUnitEntity unit)
	{
		Game.Instance.Controllers.EntityDestroyer.Destroy(unit);
	}

	public static bool CheckSkillResult(BaseUnitEntity unit, StatType statType, int difficultyClass, out bool isCriticalFail, RulePerformSkillCheck.VoicingType voice = RulePerformSkillCheck.VoicingType.All, bool? ensureSuccess = null)
	{
		RulePerformSkillCheck rulePerformSkillCheck = new RulePerformSkillCheck(unit, statType, difficultyClass)
		{
			Voice = voice
		};
		if (ensureSuccess.HasValue)
		{
			if (ensureSuccess.GetValueOrDefault())
			{
				rulePerformSkillCheck.ChanceRule.ForceResultModifiers.Add(1, rulePerformSkillCheck);
			}
			else
			{
				rulePerformSkillCheck.ChanceRule.ForceResultModifiers.Add(-1, rulePerformSkillCheck);
			}
		}
		RulePerformSkillCheck rulePerformSkillCheck2 = TriggerSkillCheck(rulePerformSkillCheck);
		isCriticalFail = rulePerformSkillCheck2.ResultIsCriticalFail;
		return rulePerformSkillCheck2.ResultIsSuccess;
	}

	public static RulePerformSkillCheck TriggerSkillCheck(RulePerformSkillCheck skillCheck, IEvalContext context = null, bool allowPartyCheckInCamp = true)
	{
		if (allowPartyCheckInCamp && Game.Instance.Player.CapitalPartyMode)
		{
			RulePerformPartySkillCheck rulePerformPartySkillCheck = new RulePerformPartySkillCheck(skillCheck);
			TriggerRule(rulePerformPartySkillCheck, context);
			return rulePerformPartySkillCheck.SkillCheck;
		}
		return TriggerRule(skillCheck, context);
	}

	public static TEvent TriggerRule<TEvent>(TEvent rule, IEvalContext context = null) where TEvent : RulebookEvent
	{
		MechanicsContext mechanicsContext = context?.Fact?.MaybeContext;
		if (mechanicsContext != null)
		{
			return mechanicsContext.TriggerRule(rule);
		}
		return Rulebook.Trigger(rule);
	}

	public static void AddFogOfWarRevealPoint(Transform transform)
	{
		FogOfWarControllerData.AddRevealer(transform);
	}

	public static void RemoveFogOfWarRevealPoint(Transform transform)
	{
		FogOfWarControllerData.RemoveRevealer(transform);
	}

	public static bool CheckLOS(Vector3 p1, Vector3 p2)
	{
		return LosCalculations.GetDirectLos(p1, p2);
	}

	public static bool CheckLOS(MechanicEntity unit1, MechanicEntity unit2)
	{
		return (LosCalculations.CoverType)LosCalculations.GetWarhammerLos(unit1, unit2) != LosCalculations.CoverType.LosBlocker;
	}

	public static IEnumerable<BaseUnitEntity> GetTargetsAround(Vector3 point, int radius, bool checkLOS = true, bool includeDead = false)
	{
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.EntityPools.AllBaseUnits)
		{
			if ((!allBaseUnit.LifeState.IsDead || includeDead) && !allBaseUnit.Features.IsUntargetable && allBaseUnit.IsUnitInRangeCells(point, radius, checkLOS))
			{
				yield return allBaseUnit;
			}
		}
	}

	public static bool IsUnitInRangeCells(this BaseUnitEntity unit, Vector3 point, int radius, bool checkLOS = true)
	{
		if (unit.InRangeInCells(point, radius))
		{
			if (checkLOS)
			{
				return CheckLOS(point, unit.Position);
			}
			return true;
		}
		return false;
	}

	public static int GetItemEnhancementBonus(ItemEntityWeapon weapon)
	{
		return 0;
	}

	public static int GetItemEnhancementBonus(ItemEntity item)
	{
		return 0;
	}

	public static int GetArmorEnhancementBonus(BlueprintItemArmor blueprint)
	{
		return 0;
	}

	public static int GetWeaponEnhancementBonus(BlueprintItemWeapon weapon)
	{
		return 0;
	}

	public static bool IsAttackingGreenNPC([NotNull] this MechanicEntity attacker, [NotNull] MechanicEntity target)
	{
		if (!ContextData<CommandAction.PlayerData>.Current && !target.IsPlayerFaction && !target.IsNeutral && !GetPlayerCharacter().CombatGroup.IsEnemy(target.GetCombatGroupOptional()?.Group))
		{
			return !target.IsInCombat;
		}
		return false;
	}

	public static void RemoveCompanionFromParty(BaseUnitEntity value)
	{
		Game.Instance.Player.RemoveCompanion(value);
	}

	public static void AddCompanionToParty(BaseUnitEntity value)
	{
		Game.Instance.Player.AddCompanion(value);
	}

	public static TimeSpan GetGameTimeFromStart()
	{
		return Game.Instance.Controllers.TimeController.GameTime - ConfigRoot.Instance.Calendar.GetStartTime();
	}
}
