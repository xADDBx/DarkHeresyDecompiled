using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Mechanics.Damage;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Sound;
using Kingmaker.UI.InputSystems;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Kingmaker.View.Roaming;
using ObservableCollections;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Profiling;

namespace Kingmaker.Cheats;

public class CheatsCombat
{
	private const string CrTable = "Assets/Mechanics/Blueprints/Classes/Basic/CRTable.asset";

	private const string CrTableGuid = "19b09eaa18b203645b6f1d5f2edcb1e4";

	private static readonly string[] peacefulUnitsGuids = new string[1] { "318c93fc2e7941f5915fc1768f43da42" };

	private static readonly string[] enemyUnitsGuids = new string[1] { "8ea20fccb8b649d58bc68e97187b8792" };

	public static bool CombatTextDebugEnabled = false;

	public static void RegisterCommands(KeyboardAccess keyboard)
	{
		if (BuildModeUtility.CheatsEnabled)
		{
			EventBus.Subscribe(new CheatsCombat());
			keyboard.Bind("Kill", delegate
			{
				CheatsHelper.Run("kill @mouseover");
			});
			keyboard.Bind("KillAll", delegate
			{
				CheatsHelper.Run("kill_all @mouseover");
			});
			keyboard.Bind("Damage", delegate
			{
				CheatsHelper.Run("damage @mouseover");
			});
			keyboard.Bind("DamageALot", delegate
			{
				CheatsHelper.Run("damage @mouseover true");
			});
			keyboard.Bind("Heal", delegate
			{
				CheatsHelper.Run("heal @mouseover");
			});
			keyboard.Bind("SetMP100", delegate
			{
				CheatsHelper.Run("setmp100");
			});
			keyboard.Bind("Cheat_MakeEnemy", delegate
			{
				CheatsHelper.Run("summon null null @cursor");
			});
			keyboard.Bind("Cheat_AddToSelectedUnits", AddToSelectedUnits);
			keyboard.Bind("RestAll", delegate
			{
				CheatsHelper.Run("rest_all");
			});
			keyboard.Bind("CheatRestCurrentUnit", RestCurrentUnit);
			keyboard.Bind("AllAudioMute", AudioMuteManager.ToggleAllMute);
			keyboard.Bind("MusicMute", AudioMuteManager.ToggleMusicMute);
			SmartConsole.RegisterCommand("attach_buff", AttachBuff);
			SmartConsole.RegisterCommand("list_mobs", ListMobs);
			SmartConsole.RegisterCommand("iddqd", Iddqd);
			SmartConsole.RegisterCommand("full_buff_please", FullBuffPlease);
			SmartConsole.RegisterCommand("empowered", Empowered);
			SmartConsole.RegisterCommand("damage_ability_score", DamageAbilityScore);
			SmartConsole.RegisterCommand("drain_ability_score", DrainAbilityScore);
			SmartConsole.RegisterCommand("info", Info);
			SmartConsole.RegisterCommand("encounter_info", EncounterInfo);
			SmartConsole.RegisterCommand("manual_combat_begin", ManualCombatBegin);
			SmartConsole.RegisterCommand("manual_combat_end", ManualCombatEnd);
			SmartConsole.RegisterCommand("manual_combat_next_round", ManualCombatNextRound);
			SmartConsole.RegisterCommand("end_preparation_turn", EndPreparationTurn);
			SmartConsole.RegisterCommand("activate_peril", ActivatePeril);
			SmartConsole.RegisterCommand("activate_phenomena", ActivatePhenomena);
		}
	}

	private static void ManualCombatBegin(string parameters)
	{
		Game.Instance.Controllers.TurnController.BeginManualCombat();
	}

	private static void ManualCombatEnd(string parameters)
	{
		Game.Instance.Controllers.TurnController.EndManualCombat();
	}

	private static void ManualCombatNextRound(string parameters)
	{
		Game.Instance.Controllers.TurnController.NextRoundForManualCombat();
	}

	private static void EndPreparationTurn(string parameters)
	{
		Game.Instance.Controllers.TurnController.RequestEndPreparationTurn();
	}

	private static void EncounterInfo(string parameters)
	{
		List<BlueprintUnit> list = (from u in Game.Instance.EntityPools.AllUnits
			where u.IsInCombat && !u.IsPlayerFaction
			select u.Blueprint).ToList();
		PFLog.SmartConsole.Log($"Encounter CR: {GetEncounterCr()}");
		foreach (BlueprintUnit item in list)
		{
			PFLog.SmartConsole.Log(Utilities.GetBlueprintPath(item) ?? "");
		}
	}

	private static int GetEncounterCr()
	{
		BlueprintStatProgression blueprint = Utilities.GetBlueprint<BlueprintStatProgression>("Assets/Mechanics/Blueprints/Classes/Basic/CRTable.asset");
		if (!blueprint)
		{
			blueprint = Utilities.GetBlueprint<BlueprintStatProgression>("19b09eaa18b203645b6f1d5f2edcb1e4");
		}
		if (!blueprint)
		{
			PFLog.SmartConsole.Log("CR table not found at Assets/Mechanics/Blueprints/Classes/Basic/CRTable.asset or 19b09eaa18b203645b6f1d5f2edcb1e4, cannot calculate");
			return -1;
		}
		return Utilities.GetTotalChallengeRating((from u in Game.Instance.EntityPools.AllUnits
			where u.IsInCombat && !u.IsPlayerFaction
			select u.Blueprint).ToList());
	}

	private static void Info(string parameters)
	{
		BaseUnitEntity unitUnderMouse = Utilities.GetUnitUnderMouse();
		if (unitUnderMouse == null)
		{
			PFLog.SmartConsole.Log("No unit under mouse");
		}
		else
		{
			PFLog.SmartConsole.Log("AssetPath: " + Utilities.GetBlueprintPath(unitUnderMouse.Blueprint));
		}
	}

	private static void ListMobs(string parameters)
	{
		string value = Utilities.GetParamString(parameters, 1, null) ?? "";
		foreach (BlueprintUnit scriptableObject in Utilities.GetScriptableObjects<BlueprintUnit>())
		{
			string blueprintPath = Utilities.GetBlueprintPath(scriptableObject);
			if (blueprintPath.Contains(value))
			{
				PFLog.SmartConsole.Log(blueprintPath);
			}
		}
	}

	[Cheat(Name = "kill_all", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void KillAll(MechanicEntity entity)
	{
		bool flag = false;
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.EntityPools.AllBaseUnits)
		{
			if (allBaseUnit.CombatState.IsInCombat && allBaseUnit.CombatGroup.IsEnemy(GameHelper.GetPlayerCharacter()) && allBaseUnit != entity)
			{
				flag = true;
				KillUnit(allBaseUnit);
			}
		}
		if (Game.Instance.IsPaused)
		{
			Game.Instance.StopMode(GameModeType.Pause);
		}
		if (!flag)
		{
			CheatsCommon.CleanSpace();
		}
	}

	[Cheat(ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void Kill(BaseUnitEntity unit)
	{
		if (unit == null)
		{
			UtilityMessageBox.SendWarning("No unit under mouse");
		}
		else
		{
			KillUnit(unit);
		}
	}

	[Cheat(ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void Damage(MechanicEntity entity, bool tryToKill = false)
	{
		if (entity == null)
		{
			UtilityMessageBox.SendWarning("No entity under mouse");
		}
		else if (tryToKill)
		{
			int value = entity.GetHealthOptional()?.HitPointsLeft ?? 1000;
			Rulebook.Trigger(new RuleDealDamage(entity, entity, DamageType.Direct.CreateDamage(value)));
		}
		else
		{
			Rulebook.Trigger(new RuleDealDamage(entity, entity, ConfigRoot.Instance.Cheats.TestDamage.CreateDamage()));
		}
	}

	[Cheat(ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void Heal(MechanicEntity entity)
	{
		if (entity == null)
		{
			UtilityMessageBox.SendWarning("No entity under mouse");
			return;
		}
		RuleHealDamage.FluentOptional fluentOptional = RuleHealDamage.Setup(entity, entity).Base(10);
		PartHealth healthOptional = entity.GetHealthOptional();
		Rulebook.Trigger(fluentOptional.Strategy((healthOptional != null && healthOptional.Damage == 0) ? DamageStrategy.ArmorOnly : DamageStrategy.Default).Create());
	}

	public static void KillUnit(BaseUnitEntity unit)
	{
		unit.LifeState.MarkedForDeath = true;
		unit.Wake(1f);
		unit.Health.LastHandledDamage = null;
	}

	[Cheat(Name = "list_combat_units", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ListCombatUnits()
	{
		int num = 0;
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.EntityPools.AllBaseUnits)
		{
			if (allBaseUnit.IsInCombat)
			{
				string text = (allBaseUnit.IsPlayerFaction ? "Ally" : "Enemy");
				PFLog.SmartConsole.Log("[" + text + "] " + allBaseUnit.CharacterName + " (" + Utilities.GetBlueprintPath(allBaseUnit.Blueprint) + ")");
				num++;
			}
		}
		if (num == 0)
		{
			PFLog.SmartConsole.Log("No units currently in combat");
		}
		else
		{
			PFLog.SmartConsole.Log($"Total units in combat: {num}");
		}
	}

	[Cheat(Name = "list_combat_buffs", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ListCombatBuffs()
	{
		int num = 0;
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.EntityPools.AllBaseUnits)
		{
			if (allBaseUnit.IsInCombat)
			{
				string text = (allBaseUnit.IsPlayerFaction ? "Ally" : "Enemy");
				List<string> list = allBaseUnit.Buffs.Enumerable.Select((Buff b) => (b.ExpirationInRounds <= 0) ? b.Blueprint.name : $"{b.Blueprint.name}({b.ExpirationInRounds})").ToList();
				string text2 = ((list.Count > 0) ? string.Join(", ", list) : "<no buffs>");
				PFLog.SmartConsole.Log("[" + text + "] " + allBaseUnit.CharacterName + ": " + text2);
				num++;
			}
		}
		if (num == 0)
		{
			PFLog.SmartConsole.Log("No units currently in combat");
		}
	}

	[Cheat(Name = "list_buffs", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ListBuffs(BaseUnitEntity targetUnit)
	{
		if (targetUnit != null)
		{
			PFLog.SmartConsole.Log("Buffs on " + Utilities.GetBlueprintPath(targetUnit.Blueprint));
			PrintBuffs(targetUnit.Buffs.Enumerable);
			return;
		}
		foreach (BaseUnitEntity selectedUnit in Game.Instance.Controllers.SelectionCharacter.SelectedUnits)
		{
			PFLog.Default.Log("Buffs on " + selectedUnit.CharacterName);
			PrintBuffs(selectedUnit.Buffs.Enumerable);
		}
	}

	[Cheat(Name = "list_buffs_mouse", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ListBuffsUnderMouse()
	{
		BaseUnitEntity unitUnderMouse = Utilities.GetUnitUnderMouse();
		if (unitUnderMouse == null)
		{
			PFLog.SmartConsole.Log("No unit under mouse");
			return;
		}
		List<Buff> list = unitUnderMouse.Buffs.Enumerable.ToList();
		PFLog.SmartConsole.Log($"Buffs on {unitUnderMouse.CharacterName} ({Utilities.GetBlueprintPath(unitUnderMouse.Blueprint)}): {list.Count}");
		foreach (Buff item in list)
		{
			string text = ((item.ExpirationInRounds > 0) ? $"; Rounds Left: {item.ExpirationInRounds}" : "");
			PFLog.SmartConsole.Log("  " + Utilities.GetBlueprintPath(item.Blueprint) + text);
		}
	}

	public static void PrintBuffs(IEnumerable<Buff> buffs)
	{
		foreach (Buff buff in buffs)
		{
			PFLog.SmartConsole.Log(Utilities.GetBlueprintPath(buff.Blueprint) + "; Rounds Left: " + buff.ExpirationInRounds);
		}
	}

	[Cheat(Name = "detach_all_buffs", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void DetachAllBuffs(BaseUnitEntity unit)
	{
		if (unit == null)
		{
			throw new Exception("Need target unit");
		}
		foreach (Buff item in new List<Buff>(unit.Buffs.Enumerable))
		{
			unit.Facts.Remove(item);
			PFLog.SmartConsole.Log(unit.CharacterName, item?.ToString() + "detached");
		}
	}

	[Cheat(Name = "rest_all", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void RestAll()
	{
		foreach (BaseUnitEntity allCharactersAndStarship in Game.Instance.Player.AllCharactersAndStarships)
		{
			allCharactersAndStarship.Restore();
		}
	}

	private static void RestCurrentUnit()
	{
		BaseUnitEntity baseUnitEntity = (Game.Instance.Controllers.TurnController?.CurrentUnit as BaseUnitEntity) ?? Game.Instance.Controllers.SelectionCharacter.SelectedUnitInUI.Value;
		if (baseUnitEntity != null)
		{
			baseUnitEntity.CombatState.ResetActionAndMovementPoints();
			baseUnitEntity.CombatState.AttackInRoundCount = 0;
			baseUnitEntity.CombatState.AttackedInRoundCount = 0;
			baseUnitEntity.CombatState.HitInRoundCount = 0;
			baseUnitEntity.CombatState.GotHitInRoundCount = 0;
			baseUnitEntity.GetAbilityCooldownsOptional()?.Clear();
			baseUnitEntity.GetTwoWeaponFightingOptional()?.ResetAttacks();
			EventBus.RaiseEvent(delegate(IActionBarSlotsUpdatedHandler h)
			{
				h.HandleActionBarSlotsUpdated();
			});
		}
	}

	[Cheat(ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SetMP100()
	{
		if (Game.Instance.Player.IsInCombat)
		{
			Game.Instance.Controllers.SelectionCharacter.SelectedUnitInUI.Value.CombatState.ResetMovementPointsCheat();
		}
	}

	private static void AttachBuff(string parameters)
	{
		string paramString = Utilities.GetParamString(parameters, 1, "Can't parse buff name from given parameters");
		BlueprintBuff blueprint = Utilities.GetBlueprint<BlueprintBuff>(paramString);
		if (blueprint == null)
		{
			PFLog.SmartConsole.Log("Cannot find buff by name: {0}", paramString);
		}
		else
		{
			Utilities.GetUnitUnderMouse()?.AddFact(blueprint, null, new BuffDuration(null, BuffEndCondition.RemainAfterCombat, BuffExpireMoment.TurnStart));
		}
	}

	private static void AddToSelectedUnits()
	{
		if (TryGetFactToAdd(out var fact))
		{
			AddFactToSelectedUnits(fact);
		}
	}

	private static void AddFactToSelectedUnits(BlueprintMechanicEntityFact fact)
	{
		if (fact == null)
		{
			return;
		}
		if (fact is BlueprintItem blueprint)
		{
			PFLog.SmartConsole.Log("item " + fact.NameSafe() + " added");
			CheatsUnlock.CreateItem(blueprint);
		}
		else
		{
			if (!(fact is BlueprintUnitFact blueprint2))
			{
				return;
			}
			ObservableList<BaseUnitEntity> selectedUnits = Game.Instance.Controllers.SelectionCharacter.SelectedUnits;
			if (selectedUnits != null && selectedUnits.Count != 0)
			{
				foreach (BaseUnitEntity item in selectedUnits)
				{
					PFLog.SmartConsole.Log("fact " + fact.NameSafe() + " added to " + item.Name);
					item.AddFact(blueprint2);
				}
				return;
			}
			PFLog.SmartConsole.Log("No selected units");
		}
	}

	private static bool TryGetFactToAdd(out BlueprintMechanicEntityFact fact)
	{
		fact = null;
		if (TryGetFactFromBuffer(out fact))
		{
			PFLog.SmartConsole.Log("fact " + fact.NameSafe() + " taken from buffer");
			return true;
		}
		if (TryGetFactFromEditorSelection(out fact))
		{
			PFLog.SmartConsole.Log("fact " + fact.NameSafe() + " taken from editor selection");
			return true;
		}
		return false;
	}

	private static bool TryGetFactFromEditorSelection(out BlueprintMechanicEntityFact fact)
	{
		fact = null;
		return fact != null;
	}

	private static bool TryGetFactFromBuffer(out BlueprintMechanicEntityFact fact)
	{
		fact = null;
		string text = GUIUtility.systemCopyBuffer?.Trim();
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (text.StartsWith("add_feature ", StringComparison.OrdinalIgnoreCase))
		{
			string text2 = text;
			int length = "add_feature ".Length;
			text = text2.Substring(length, text2.Length - length);
		}
		else if (text.StartsWith("create_item ", StringComparison.OrdinalIgnoreCase))
		{
			string text2 = text;
			int length = "create_item ".Length;
			text = text2.Substring(length, text2.Length - length);
		}
		text = text.Trim();
		fact = Utilities.GetBlueprint<BlueprintMechanicEntityFact>(text);
		return fact != null;
	}

	[Cheat(Name = "detach_fact", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	[CheatTargetsUnit]
	public static void DetachFact(BlueprintUnitFact fact)
	{
		if (fact == null)
		{
			PFLog.SmartConsole.Log("Cannot find fact from given parameters");
		}
		else
		{
			Utilities.GetUnitUnderMouse()?.Facts.Remove(fact);
		}
	}

	[Cheat(Name = "attach_fact", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	[CheatTargetsUnit]
	public static void AttachFact(BlueprintUnitFact fact)
	{
		if (fact == null)
		{
			PFLog.SmartConsole.Log("Cannot find fact from given parameters");
		}
		else
		{
			(Utilities.GetUnitUnderMouse()?.AddFact(fact, null, new BuffDuration(null, BuffEndCondition.RemainAfterCombat, BuffExpireMoment.TurnStart)))?.AddSource(ConfigRoot.Instance.Cheats);
		}
	}

	[Cheat(Name = "summon", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SpawnEnemyUnderCursor(BlueprintUnit bp = null, BlueprintFaction factionBp = null, Vector3 position = default(Vector3))
	{
		Vector3 position2 = ((position != default(Vector3)) ? position : Game.Instance.Controllers.ClickEventsController.WorldPosition);
		if (bp == null)
		{
			bp = CheatRoot.Instance.Enemy;
		}
		PFLog.SmartConsole.Log("Summoning: " + Utilities.GetBlueprintPath(bp));
		BaseUnitEntity baseUnitEntity = Game.Instance.Controllers.EntitySpawner.SpawnUnit(bp, position2, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
		if (factionBp != null)
		{
			baseUnitEntity.Faction.Set(factionBp);
		}
	}

	[Cheat(Name = "activate_peril", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ActivatePeril(string parameters)
	{
		if (!int.TryParse(parameters, out var result))
		{
			PFLog.SmartConsole.Log("Cannot find peril index given parameters");
			return;
		}
		BlueprintPsykerRoot psykerRoot = ConfigRoot.Instance.PsykerRoot;
		if (result < 0 || psykerRoot.PerilsOfTheWarp.Length <= result)
		{
			PFLog.SmartConsole.Log($"Wrong index for peril. There are only {psykerRoot.PerilsOfTheWarp.Length} of them");
			return;
		}
		BaseUnitEntity unitUnderMouse = Utilities.GetUnitUnderMouse();
		if (unitUnderMouse == null)
		{
			PFLog.SmartConsole.Log("No unit under mouse to trigger peril");
		}
		else
		{
			RulePerformPsychicPhenomena.RunPsychicPhenomenaEffectOnTarget(unitUnderMouse, null, psykerRoot.PerilsOfTheWarp[result], isPerils: true);
		}
	}

	[Cheat(Name = "activate_phenomena", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ActivatePhenomena(string parameters)
	{
		if (!int.TryParse(parameters, out var result))
		{
			PFLog.SmartConsole.Log("Cannot find phenomena index given parameters");
			return;
		}
		BlueprintPsykerRoot psykerRoot = ConfigRoot.Instance.PsykerRoot;
		if (result < 0 || psykerRoot.PsychicPhenomena.Length <= result)
		{
			PFLog.SmartConsole.Log($"Wrong index for phenomena. There are only {psykerRoot.PsychicPhenomena.Length} of them");
			return;
		}
		BaseUnitEntity unitUnderMouse = Utilities.GetUnitUnderMouse();
		if (unitUnderMouse == null)
		{
			PFLog.SmartConsole.Log("No unit under mouse to trigger phenomena");
		}
		else
		{
			RulePerformPsychicPhenomena.RunPsychicPhenomenaEffectOnTarget(unitUnderMouse, null, psykerRoot.PsychicPhenomena[result], isPerils: false);
		}
	}

	private static void SpawnFromList(string[] guids, int number, bool nearPlayer, bool isExtra, bool roam = false)
	{
		for (int i = 0; i < number; i++)
		{
			int num = UnityEngine.Random.Range(0, guids.Length);
			Vector3 pos;
			if (nearPlayer)
			{
				Vector3 position = Game.Instance.Player.MainCharacter.Entity.Position;
				float num2 = UnityEngine.Random.Range(-10f, 10f);
				float num3 = UnityEngine.Random.Range(-10f, 10f);
				pos = new Vector3(position.x + num2, position.y, position.z + num3);
				pos = ObstacleAnalyzer.GetNearestNode(pos).position;
			}
			else
			{
				Bounds mechanicBounds = Game.Instance.CurrentlyLoadedArea.Bounds.MechanicBounds;
				float x = UnityEngine.Random.Range(mechanicBounds.min.x, mechanicBounds.max.x);
				float z = UnityEngine.Random.Range(mechanicBounds.min.z, mechanicBounds.max.z);
				pos = new Vector3(x, 0f, z);
				pos = ObstacleAnalyzer.GetNearestNode(pos).position;
			}
			BlueprintUnit blueprintUnit = BlueprintsDatabase.LoadById<BlueprintUnit>(guids[num]);
			PFLog.SmartConsole.Log("Summoning: " + Utilities.GetBlueprintPath(blueprintUnit));
			BaseUnitEntity baseUnitEntity = Game.Instance.Controllers.EntitySpawner.SpawnUnit(blueprintUnit, pos, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
			if (isExtra)
			{
				baseUnitEntity.MarkExtra();
			}
			if (roam)
			{
				UnitPartRoaming orCreate = baseUnitEntity.GetOrCreate<UnitPartRoaming>();
				orCreate.Settings = new RoamingUnitSettings();
				orCreate.Settings.Radius = 10f;
				orCreate.Settings.MinIdleTime = 1f;
				orCreate.Settings.MaxIdleTime = 5f;
			}
		}
	}

	[Cheat(Name = "spawn_units_dense", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SpawnUnitsDense(int number, bool roaming = false)
	{
		SpawnFromList(peacefulUnitsGuids, number, nearPlayer: true, isExtra: false, roaming);
	}

	[Cheat(Name = "spawn_units_sparse", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SpawnUnitsSparse(int number, bool roaming = false)
	{
		SpawnFromList(peacefulUnitsGuids, number, nearPlayer: false, isExtra: false, roaming);
	}

	[Cheat(Name = "spawn_extra_dense", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SpawnExtraDense(int number, bool roaming = false)
	{
		SpawnFromList(peacefulUnitsGuids, number, nearPlayer: true, isExtra: true, roaming);
	}

	[Cheat(Name = "spawn_extra_sparse", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SpawnExtraSparse(int number, bool roaming = false)
	{
		SpawnFromList(peacefulUnitsGuids, number, nearPlayer: false, isExtra: true, roaming);
	}

	[Cheat(Name = "spawn_enemies", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SpawnEnemies(int number)
	{
		SpawnFromList(enemyUnitsGuids, number, nearPlayer: true, isExtra: false);
	}

	public static IEnumerator SpawnTestCoroutine()
	{
		int i = 0;
		while (i < 10)
		{
			SpawnFromList(peacefulUnitsGuids, 10, nearPlayer: true, isExtra: true, roam: true);
			int num;
			for (int frame = 0; frame < 10; frame = num)
			{
				yield return null;
				num = frame + 1;
			}
			Profiler.enabled = true;
			for (int frame = 0; frame < 10; frame = num)
			{
				yield return null;
				num = frame + 1;
			}
			Profiler.enabled = false;
			PFLog.Default.Log($"Spawn step {i} complete");
			num = i + 1;
			i = num;
		}
		Profiler.logFile = "";
		Profiler.enabled = false;
		PFLog.Default.Log("ALL DONE!");
	}

	[Cheat(Name = "spawn_test", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SpawnTest()
	{
		Profiler.logFile = "profiler";
		Profiler.enableBinaryLog = true;
		Profiler.maxUsedMemory = int.MaxValue;
		MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(SpawnTestCoroutine());
	}

	[Cheat(Name = "set_action_points_yellow", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SetActionPointsYellow(int yellow)
	{
		MechanicEntity currentUnit = Game.Instance.Controllers.TurnController.CurrentUnit;
		if (currentUnit == null)
		{
			PFLog.Default.Log("No current unit in turn base");
			return;
		}
		PartUnitCombatState combatStateOptional = currentUnit.GetCombatStateOptional();
		if (combatStateOptional == null)
		{
			PFLog.Default.Log($"No combat state for unit {currentUnit}");
		}
		else
		{
			combatStateOptional.SetActionPoints(yellow, null);
		}
	}

	[Cheat(Name = "add_bonus_ability_usage", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void AddBonusAbilityUsage(int count = 1, int costBonus = -5, RestrictionsHolder restrictions = null, bool ingorePartAbilityRestrictions = false)
	{
		MechanicEntity currentUnit = Game.Instance.Controllers.TurnController.CurrentUnit;
		if (currentUnit == null)
		{
			PFLog.Default.Log("No current unit in turn base");
			return;
		}
		EntityFactSource source = new EntityFactSource(currentUnit);
		currentUnit.GetOrCreate<UnitPartBonusAbility>().AddBonusAbility(source, count, costBonus, restrictions.ToReference<RestrictionsHolder.Reference>(), ingorePartAbilityRestrictions);
	}

	private static void Iddqd(string parameters)
	{
		foreach (BaseUnitEntity selectedUnit in Game.Instance.Controllers.SelectionCharacter.SelectedUnits)
		{
			Buff(ConfigRoot.Instance.Cheats.Iddqd, force: false, selectedUnit);
		}
	}

	private static void FullBuffPlease(string parameters)
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			foreach (BlueprintBuff fullBuff in ConfigRoot.Instance.Cheats.FullBuffList)
			{
				GameHelper.ApplyBuff(item, fullBuff, 50.Rounds());
			}
		}
	}

	private static void Empowered(string parameters)
	{
		throw new NotImplementedException();
	}

	private static void Buff(BlueprintBuff buff, bool force, BaseUnitEntity unit)
	{
		if (!unit.Buffs.Contains(buff))
		{
			GameHelper.ApplyBuff(unit, buff);
		}
		else if (!force)
		{
			unit.Facts.Remove(buff);
		}
	}

	private static void DamageAbilityScore(string p)
	{
		DamageOrDrainAbilityScore(p, drain: false);
	}

	private static void DrainAbilityScore(string p)
	{
		DamageOrDrainAbilityScore(p, drain: true);
	}

	private static void DamageOrDrainAbilityScore(string p, bool drain)
	{
		string stat = Utilities.GetParamString(p, 1, "Missing ability score: str|dex|con|int|wis|cha").ToLowerInvariant();
		int? paramInt = Utilities.GetParamInt(p, 2, "Missing damage value");
		if (paramInt.HasValue)
		{
			if (paramInt.Value <= 0)
			{
				SmartConsole.Print("Damage value must be >= 1");
			}
			else if (!GetStatType(stat).HasValue)
			{
				SmartConsole.Print("Can't parse ability score, use one of these: str|dex|con|int|wis|cha");
			}
			else
			{
				SmartConsole.Print("Drain/Damage are dead mechanics, this command has no effect");
			}
		}
	}

	private static StatType? GetStatType(string stat)
	{
		return null;
	}

	private static string GetDifficulty()
	{
		int num = GetEncounterCr() - Game.Instance.Player.PartyLevel;
		if (num < 3)
		{
			return "Easy";
		}
		if (num < 5)
		{
			return "Hard";
		}
		return "Boss";
	}

	[Cheat(Name = "combat_text_debug", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CombatTextDebug(bool enable)
	{
		CombatTextDebugEnabled = enable;
	}

	[Cheat(Name = "crit_add")]
	public static void Cheat_AddCritical(BlueprintBodyPart damageBodyPart, int amount)
	{
		BaseUnitEntity unitForCheat = Utilities.GetUnitForCheat();
		unitForCheat.Health.AddCriticalEffectStages(damageBodyPart, amount, unitForCheat);
	}

	[Cheat(Name = "crit_remove")]
	public static void Cheat_RemoveCritical(BlueprintBodyPart damageBodyPart, int amount)
	{
		BaseUnitEntity unitForCheat = Utilities.GetUnitForCheat();
		unitForCheat.Health.RemoveCriticalEffectStages(damageBodyPart, amount, unitForCheat);
	}

	[Cheat(Name = "crit_remove_all")]
	public static void Cheat_RemoveCriticalAll()
	{
		BaseUnitEntity unitForCheat = Utilities.GetUnitForCheat();
		foreach (BlueprintBodyPart bodyPart in unitForCheat.BodyParts)
		{
			int criticalStage = unitForCheat.Health.GetCriticalStage(bodyPart);
			if (criticalStage > 0)
			{
				unitForCheat.Health.RemoveCriticalEffectStages(bodyPart, criticalStage, unitForCheat);
			}
		}
	}

	[Cheat(Name = "attach_buff", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	[CheatTargetsUnit]
	public static void AttachBuffTyped(BlueprintBuff buff)
	{
		Utilities.GetUnitUnderMouse()?.AddFact(buff, null, new BuffDuration(null, BuffEndCondition.RemainAfterCombat, BuffExpireMoment.TurnStart));
	}
}
