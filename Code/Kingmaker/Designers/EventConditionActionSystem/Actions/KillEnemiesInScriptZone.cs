using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/KillEnemiesInScriptZone")]
[AllowMultipleComponents]
[TypeId("50f0d77f94209854e967f0b7d3e2a133")]
public class KillEnemiesInScriptZone : GameAction
{
	[SerializeReference]
	public MechanicEntityEvaluator Killer;

	[Tooltip("Works only if the Killer is set. If 0, body just falls on the ground, 1 is a standard impulse. For bigger impulse, try set it up a bit higher.")]
	public int ImpulseMultiplier = 1;

	public UnitDismemberType Dismember;

	[ShowIf("LimpsApartSelected")]
	[SerializeField]
	private DismembermentLimbsApartType m_DismemberingAnimation;

	public bool DisableBattleLog;

	[Tooltip("If true, kills only enemies. If false, kills all non-party units.")]
	public bool OnlyEnemies = true;

	[Tooltip("If true, includes dead units in the kill list.")]
	public bool IncludeDead;

	[SerializeField]
	[Tooltip("Remove all buffs from unit before killing. Ensures guaranteed death even with protective buffs.")]
	public bool RemoveAllBuffs;

	[SerializeField]
	[Tooltip("Remove only HealthGuard buffs that prevent unit from dying. Less aggressive than RemoveAllBuffs.")]
	public bool RemoveHealthGuards = true;

	[SerializeField]
	[Tooltip("For mechanisms (units with armor instead of HP), force armor durability to 0 before killing.")]
	public bool ForceKillMechanisms = true;

	private bool LimpsApartSelected => Dismember == UnitDismemberType.LimbsApart;

	public override string GetDescription()
	{
		return "Убивает всех врагов в текущей script zone\n" + $"OnlyEnemies: {OnlyEnemies}\n" + $"IncludeDead: {IncludeDead}\n" + (DisableBattleLog ? "Log disabled" : "Log enabled");
	}

	protected override void RunAction()
	{
		SceneEntitiesState sceneEntitiesState = ContextData<ScriptZoneTriggerData>.Current?.State;
		if (sceneEntitiesState == null)
		{
			PFLog.Default.Error("KillEnemiesInScriptZone: No script zone context found");
			return;
		}
		ScriptZoneEntity scriptZoneEntity = sceneEntitiesState.AllEntityData.OfType<ScriptZoneEntity>().FirstOrDefault();
		if (scriptZoneEntity == null)
		{
			PFLog.Default.Error("KillEnemiesInScriptZone: Script zone entity not found");
			return;
		}
		BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
		if (mainCharacterEntity == null)
		{
			PFLog.Default.Error("KillEnemiesInScriptZone: Player character not found");
			return;
		}
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		foreach (ScriptZoneEntity.UnitInfo item in scriptZoneEntity.InsideUnits.ToTempList())
		{
			BaseUnitEntity baseUnitEntity = item.Reference.ToBaseUnitEntity();
			if (baseUnitEntity == null || !item.IsValid || (!IncludeDead && baseUnitEntity.LifeState.IsDead) || baseUnitEntity.IsPlayerFaction || Game.Instance.Player.PartyAndPets.Contains(baseUnitEntity))
			{
				continue;
			}
			if (OnlyEnemies)
			{
				if (mainCharacterEntity.IsEnemy(baseUnitEntity))
				{
					list.Add(baseUnitEntity);
				}
			}
			else
			{
				list.Add(baseUnitEntity);
			}
		}
		MechanicEntity killer = Killer?.GetValue();
		foreach (BaseUnitEntity item2 in list)
		{
			try
			{
				PrepareUnitForDeath(item2);
				if (DisableBattleLog)
				{
					item2.GetOrCreate<Kill.SilentDeathUnitPart>();
				}
				GameHelper.KillUnit(item2, killer, ImpulseMultiplier, Dismember, LimpsApartSelected ? new DismembermentLimbsApartType?(m_DismemberingAnimation) : null);
				UnitLifeController.ForceTickOnUnit(item2);
			}
			catch (Exception ex)
			{
				PFLog.Default.Error("KillEnemiesInScriptZone: Error killing unit " + item2.CharacterName + ": " + ex.Message);
			}
		}
		if (list.Count > 0)
		{
			PFLog.Default.Log($"KillEnemiesInScriptZone: Killed {list.Count} units");
		}
	}

	private void PrepareUnitForDeath(AbstractUnitEntity unit)
	{
		if (RemoveAllBuffs)
		{
			RemoveAllBuffsFromUnit(unit);
		}
		else if (RemoveHealthGuards)
		{
			RemoveHealthGuardBuffs(unit);
		}
		if (ForceKillMechanisms)
		{
			PartHealth healthOptional = unit.GetHealthOptional();
			if (healthOptional != null && healthOptional.IsCountHpAsArmor)
			{
				unit.GetOptional<PartArmor>()?.SetDurabilityLeft(0);
			}
		}
	}

	private void RemoveAllBuffsFromUnit(AbstractUnitEntity unit)
	{
		foreach (Buff item in new List<Buff>(unit.Buffs.RawFacts))
		{
			try
			{
				unit.Facts.Remove(item);
			}
			catch (Exception ex)
			{
				PFLog.Default.Warning($"KillEnemiesInScriptZone: Failed to remove buff {item} from {unit.CharacterName}: {ex.Message}");
			}
		}
	}

	private void RemoveHealthGuardBuffs(AbstractUnitEntity unit)
	{
		List<Buff> list = new List<Buff>();
		foreach (Buff rawFact in unit.Buffs.RawFacts)
		{
			if (rawFact.Blueprint.GetComponent<HealthGuard>() != null)
			{
				list.Add(rawFact);
			}
		}
		foreach (Buff item in list)
		{
			try
			{
				unit.Facts.Remove(item);
			}
			catch (Exception ex)
			{
				PFLog.Default.Warning($"KillEnemiesInScriptZone: Failed to remove HealthGuard buff {item} from {unit.CharacterName}: {ex.Message}");
			}
		}
	}

	public override string GetCaption()
	{
		return "Kill Enemies in Script Zone (" + (OnlyEnemies ? "enemies only" : "all non-party") + ")";
	}
}
