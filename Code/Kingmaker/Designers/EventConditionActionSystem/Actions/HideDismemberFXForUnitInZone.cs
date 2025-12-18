using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.MapObjects.SriptZones;
using Kingmaker.Visual.Particles;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/HideDismemberFXForUnitInZone")]
[AllowMultipleComponents]
[TypeId("cfd7d28259eacc441b8288f670afe003")]
[PlayerUpgraderAllowed(true)]
public class HideDismemberFXForUnitInZone : GameAction
{
	[ValidateNotNull]
	[AllowedEntityType(typeof(ScriptZone))]
	public EntityReference ScriptZone;

	[SerializeField]
	[Tooltip("Destroy FX immediately (true) or hide it (false)")]
	public bool DestroyFX = true;

	[SerializeField]
	[Tooltip("Include player faction units")]
	public bool IncludePlayerFaction;

	[SerializeField]
	[Tooltip("Only affect units that are dead")]
	public bool OnlyDeadUnits = true;

	[SerializeField]
	[Tooltip("Log debug information")]
	public bool DebugLog;

	public override string GetDescription()
	{
		return "Скрывает/удаляет Dismember FX для всех юнитов в ScriptZone " + ScriptZone?.EntityNameInEditor;
	}

	protected override void RunAction()
	{
		ScriptZone scriptZone = ScriptZone.FindView() as ScriptZone;
		if (scriptZone?.Data == null)
		{
			PFLog.Default.Warning("HideDismemberFXForUnitInZone: ScriptZone not found or has no data: " + ScriptZone?.EntityNameInEditor);
			return;
		}
		if (DebugLog)
		{
			PFLog.Default.Log("HideDismemberFXForUnitInZone: Processing ScriptZone " + scriptZone.name);
		}
		List<BaseUnitEntity> unitsInZone = GetUnitsInZone(scriptZone);
		if (DebugLog)
		{
			PFLog.Default.Log($"HideDismemberFXForUnitInZone: Found {unitsInZone.Count} units in zone");
		}
		if (unitsInZone.Count == 0)
		{
			if (DebugLog)
			{
				PFLog.Default.Log("HideDismemberFXForUnitInZone: No units found in zone");
			}
			return;
		}
		int num = 0;
		foreach (BaseUnitEntity item in unitsInZone)
		{
			if (DebugLog)
			{
				PFLog.Default.Log($"HideDismemberFXForUnitInZone: Checking unit {item.CharacterName}, IsDead: {item.LifeState.IsDead}, IsPlayer: {item.Faction.IsPlayer}");
			}
			if (ShouldProcessUnit(item))
			{
				ProcessUnitFX(item);
				num++;
			}
		}
		if (DebugLog)
		{
			PFLog.Default.Log($"HideDismemberFXForUnitInZone: Processed FX for {num} units");
		}
	}

	private List<BaseUnitEntity> GetUnitsInZone(ScriptZone scriptZone)
	{
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		List<ScriptZoneEntity.UnitInfo> insideUnits = scriptZone.Data.InsideUnits;
		if (DebugLog)
		{
			PFLog.Default.Log($"HideDismemberFXForUnitInZone: ScriptZone has {insideUnits.Count} units inside");
		}
		foreach (ScriptZoneEntity.UnitInfo item in insideUnits)
		{
			if (!item.IsValid)
			{
				continue;
			}
			BaseUnitEntity baseUnitEntity = item.Reference.Entity?.ToBaseUnitEntity();
			if (baseUnitEntity == null || !baseUnitEntity.IsInState)
			{
				continue;
			}
			if (DebugLog)
			{
				PFLog.Default.Log($"HideDismemberFXForUnitInZone: Found unit in zone: {baseUnitEntity.CharacterName}, IsDead: {baseUnitEntity.LifeState.IsDead}");
			}
			if (OnlyDeadUnits && !baseUnitEntity.LifeState.IsDead)
			{
				if (DebugLog)
				{
					PFLog.Default.Log("HideDismemberFXForUnitInZone: Skipping " + baseUnitEntity.CharacterName + " - not dead");
				}
			}
			else if (!OnlyDeadUnits && baseUnitEntity.LifeState.IsDead)
			{
				if (DebugLog)
				{
					PFLog.Default.Log("HideDismemberFXForUnitInZone: Skipping " + baseUnitEntity.CharacterName + " - is dead but OnlyDeadUnits=false");
				}
			}
			else
			{
				list.Add(baseUnitEntity);
			}
		}
		return list;
	}

	private bool ShouldProcessUnit(BaseUnitEntity unit)
	{
		if (OnlyDeadUnits && !unit.LifeState.IsDead)
		{
			if (DebugLog)
			{
				PFLog.Default.Log("HideDismemberFXForUnitInZone: " + unit.CharacterName + " - not dead, skipping");
			}
			return false;
		}
		if (!IncludePlayerFaction && unit.Faction.IsPlayer)
		{
			if (DebugLog)
			{
				PFLog.Default.Log("HideDismemberFXForUnitInZone: " + unit.CharacterName + " - player faction, skipping");
			}
			return false;
		}
		return true;
	}

	private void ProcessUnitFX(BaseUnitEntity unit)
	{
		if (DebugLog)
		{
			PFLog.Default.Log("HideDismemberFXForUnitInZone: Processing FX for " + unit.CharacterName);
		}
		Transform fxRoot = FxHelper.FxRoot;
		if (fxRoot == null)
		{
			if (DebugLog)
			{
				PFLog.Default.Log("HideDismemberFXForUnitInZone: FxRoot not found!");
			}
			return;
		}
		int num = 0;
		for (int num2 = fxRoot.childCount - 1; num2 >= 0; num2--)
		{
			Transform child = fxRoot.GetChild(num2);
			if (!(child == null) && child.name.Contains("Dismember"))
			{
				if (DebugLog)
				{
					PFLog.Default.Log("HideDismemberFXForUnitInZone: Found Dismember FX: " + child.name);
				}
				if (DestroyFX)
				{
					FxHelper.Destroy(child.gameObject, immediate: true);
					if (DebugLog)
					{
						PFLog.Default.Log("HideDismemberFXForUnitInZone: Destroyed Dismember FX: " + child.name);
					}
				}
				else
				{
					child.gameObject.SetActive(value: false);
					if (DebugLog)
					{
						PFLog.Default.Log("HideDismemberFXForUnitInZone: Hidden Dismember FX: " + child.name);
					}
				}
				num++;
			}
		}
		if (DebugLog)
		{
			if (num == 0)
			{
				PFLog.Default.Log("HideDismemberFXForUnitInZone: No Dismember FX found for " + unit.CharacterName);
			}
			else
			{
				PFLog.Default.Log($"HideDismemberFXForUnitInZone: Processed {num} FX objects for {unit.CharacterName}");
			}
		}
	}

	public override string GetCaption()
	{
		string text = ScriptZone?.EntityNameInEditor ?? "???";
		string obj = (DestroyFX ? "Destroy" : "Hide");
		string text2 = (OnlyDeadUnits ? " (corpses only)" : "");
		return obj + " Dismember FX for units in " + text + text2;
	}
}
