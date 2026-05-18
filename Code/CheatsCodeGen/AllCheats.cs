using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Cheats;
using Kingmaker.Achievements;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.BarkBanters;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Cheats;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.Gameplay.Controllers;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.MovePrediction;
using Kingmaker.Controllers.Net;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators.BarkBanters;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Formations;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.Framework.GlobalEffectSystem;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Gameplay.Features.Vendor;
using Kingmaker.Localization;
using Kingmaker.Localization.Enums;
using Kingmaker.Modding;
using Kingmaker.Networking;
using Kingmaker.QA;
using Kingmaker.QA.Arbiter.GameCore;
using Kingmaker.QA.Arbiter.GameCore.StaticPerformanceChecker;
using Kingmaker.Replay;
using Kingmaker.Settings;
using Kingmaker.Settings.Graphics;
using Kingmaker.Tutorial;
using Kingmaker.UI.InputSystems;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.Particles.ForcedCulling;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace CheatsCodeGen;

public static class AllCheats
{
	public static readonly List<CheatMethodInfoInternal> Methods = new List<CheatMethodInfoInternal>
	{
		new CheatMethodInfoInternal(new Action<string>(AchievementsManager.ListAchievements), "void ListAchievements(string param = \\\"\\\")", "list_achievements", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "param",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CutsceneCheats.SkipCutscene), "void SkipCutscene()", "skip_cutscene", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CutsceneCheats.SkipBark), "void SkipBark()", "skip_bark", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<BlueprintCase>(DetectiveSystem.Cheat_OpenCase), "void Cheat_OpenCase(BlueprintCase blueprint)", "ds_open_case", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "blueprint",
				Type = "Kingmaker.Framework.DetectiveSystem.BlueprintCase",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintCase>(DetectiveSystem.Cheat_OpenCaseWhole), "void Cheat_OpenCaseWhole(BlueprintCase blueprint)", "ds_open_case_whole", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "blueprint",
				Type = "Kingmaker.Framework.DetectiveSystem.BlueprintCase",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintCase>(DetectiveSystem.Cheat_ReopenCase), "void Cheat_ReopenCase(BlueprintCase blueprint)", "ds_reopen_case", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "blueprint",
				Type = "Kingmaker.Framework.DetectiveSystem.BlueprintCase",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintCase>(DetectiveSystem.Cheat_CloseCase), "void Cheat_CloseCase(BlueprintCase blueprint)", "ds_close_case", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "blueprint",
				Type = "Kingmaker.Framework.DetectiveSystem.BlueprintCase",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintClue, bool>(DetectiveSystem.Cheat_AddClue), "void Cheat_AddClue(BlueprintClue blueprint, bool withAllAddendums = false)", "ds_add_clue", "", "", ExecutionPolicy.All, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "blueprint",
				Type = "Kingmaker.Framework.DetectiveSystem.BlueprintClue",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "withAllAddendums",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintClue>(DetectiveSystem.Cheat_AddClueWithAllAddendums), "void Cheat_AddClueWithAllAddendums(BlueprintClue blueprint)", "ds_add_clue_with_all_addendums", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "blueprint",
				Type = "Kingmaker.Framework.DetectiveSystem.BlueprintClue",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintClue>(DetectiveSystem.Cheat_RemoveClue), "void Cheat_RemoveClue(BlueprintClue blueprint)", "ds_remove_clue", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "blueprint",
				Type = "Kingmaker.Framework.DetectiveSystem.BlueprintClue",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintClueAddendum>(DetectiveSystem.Cheat_AddAddendum), "void Cheat_AddAddendum(BlueprintClueAddendum blueprint)", "ds_add_addendum", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "blueprint",
				Type = "Kingmaker.Framework.DetectiveSystem.BlueprintClueAddendum",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintClueAddendum>(DetectiveSystem.Cheat_RemoveAddendum), "void Cheat_RemoveAddendum(BlueprintClueAddendum blueprint)", "ds_remove_addendum", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "blueprint",
				Type = "Kingmaker.Framework.DetectiveSystem.BlueprintClueAddendum",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintClue>(DetectiveSystem.Cheat_StudyClue), "void Cheat_StudyClue(BlueprintClue clue)", "ds_study_clue", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "clue",
				Type = "Kingmaker.Framework.DetectiveSystem.BlueprintClue",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<bool>(DetectiveSystem.Cheat_SetMoveToClue), "void Cheat_SetMoveToClue(bool shouldMove)", "ds_move_to_new_clue", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "shouldMove",
				Type = "System.Boolean",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintGlobalEffect, float>(GlobalEffectCheats.SetGlobalEffect), "void SetGlobalEffect(BlueprintGlobalEffect effect, float weight)", "set_global_effect", "", "", ExecutionPolicy.All, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "effect",
				Type = "Kingmaker.Framework.GlobalEffectSystem.BlueprintGlobalEffect",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "weight",
				Type = "System.Single",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintGlobalEffect>(GlobalEffectCheats.RemoveGlobalEffect), "void RemoveGlobalEffect(BlueprintGlobalEffect effect)", "remove_global_effect", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "effect",
				Type = "Kingmaker.Framework.GlobalEffectSystem.BlueprintGlobalEffect",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(InteractionHelper.AddMeltaCharge), "void AddMeltaCharge(int count)", "add_melta_charge", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "count",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string, int>(InteractionHelper.AddItem), "void AddItem(string blueprintName, int count = 1)", "add_item", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "blueprintName",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "count",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string, bool>(InteractionDoorPart.ToggleDoor), "void ToggleDoor(string name, bool newState)", "toggle_door", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "name",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "newState",
				Type = "System.Boolean",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<bool>(VOTextHelper.SetExportToVoiceOver), "void SetExportToVoiceOver(bool isOn)", "set_export_to_voice_over", "Включить тест работы текстов в режиме ВО", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "isOn",
				Type = "System.Boolean",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Func<string>(StatProfiler.Arm), "string Arm()", "stats_profiler_arm", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Func<string>(StatProfiler.Dump), "string Dump()", "stats_profiler_dump", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Func<string>(StatProfiler.DumpCacheability), "string DumpCacheability()", "stat_cacheability_dump", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Action(OwlcatModificationsManager.CheatReloadData), "void CheatReloadData()", "reload_modifications_data", "Reload data for all modifications", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatNetInit), "void CheatNetInit()", "net_init", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatNetHash), "void CheatNetHash()", "net_hash", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatNetManager.CheatJoinNet), "void CheatJoinNet(string roomName = null)", "net_join", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "roomName",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatStopNet), "void CheatStopNet()", "net_stop", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatPrintPlayers), "void CheatPrintPlayers()", "net_players", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatPrintRegions), "void CheatPrintRegions()", "net_regions", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatNetManager.CheatSetRegion), "void CheatSetRegion(string region)", "net_set_region", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "region",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatInvite), "void CheatInvite()", "net_invite", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatDesync), "void CheatDesync(int playerIndex = -1)", "net_desync", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "playerIndex",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatState), "void CheatState()", "net_state", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatThreadSleep), "void CheatThreadSleep(int timeMs = 1)", "thread_sleep", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "timeMs",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatCheckMath), "void CheatCheckMath()", "net_check_math", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<bool>(CheatNetManager.CheatSetRoomOpen), "void CheatSetRoomOpen(bool isOpen = true)", "net_set_open", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "isOpen",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<bool>(CheatNetManager.CheatIsRoomOpen), "void CheatIsRoomOpen(bool isOpen = true)", "net_is_open", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "isOpen",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatPath), "void CheatPath()", "net_path", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatNetManager.CheatRunFsmTrigger), "void CheatRunFsmTrigger(string triggerName)", "net_fsm", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "triggerName",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatTestLoadingProcessCommandsLogic), "void CheatTestLoadingProcessCommandsLogic(int count = 100)", "net_test_cmd", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "count",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatSendAvatarAlwaysViaPhoton), "void CheatSendAvatarAlwaysViaPhoton()", "net_avatar", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatClearAvatarCache), "void CheatClearAvatarCache()", "net_av_clear", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatAllowRunWithOnePlayer), "void CheatAllowRunWithOnePlayer()", "net_allow_one", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatSetMaxPacketSize), "void CheatSetMaxPacketSize(int packetSizeKb)", "net_packet", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "packetSizeKb",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatSetStreamsCount), "void CheatSetStreamsCount(int cnt)", "net_streams", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "cnt",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<bool>(CheatNetManager.CheatSlow), "void CheatSlow(bool activate = true)", "net_slow", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "activate",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatPortraitManagerClearGuidMapping), "void CheatPortraitManagerClearGuidMapping()", "net_clear_portraits", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatNetManager.CheatPauseDataSending), "void CheatPauseDataSending()", "net_pause_send", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<bool, bool>(CheatNetManager.CheatNetSimulationSetIncomingLag), "void CheatNetSimulationSetIncomingLag(bool enabled, bool reset = false)", "net_sim", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "enabled",
				Type = "System.Boolean",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "reset",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetLag), "void CheatNetSimulationSetLag(int lag)", "net_sim_lag", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "lag",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetIncomingLag), "void CheatNetSimulationSetIncomingLag(int lag)", "net_sim_inlag", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "lag",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetOutgoingLag), "void CheatNetSimulationSetOutgoingLag(int lag)", "net_sim_outlag", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "lag",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetJitter), "void CheatNetSimulationSetJitter(int jit)", "net_sim_jit", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "jit",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetIncomingJitter), "void CheatNetSimulationSetIncomingJitter(int jit)", "net_sim_injit", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "jit",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetOutgoingJitter), "void CheatNetSimulationSetOutgoingJitter(int jit)", "net_sim_outjit", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "jit",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetLossPercentage), "void CheatNetSimulationSetLossPercentage(int loss)", "net_sim_loss", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "loss",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetIncomingLossPercentage), "void CheatNetSimulationSetIncomingLossPercentage(int loss)", "net_sim_inloss", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "loss",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatNetManager.CheatNetSimulationSetOutgoingLossPercentage), "void CheatNetSimulationSetOutgoingLossPercentage(int loss)", "net_sim_outloss", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "loss",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(PhotonManager.MaxPlayersCheat), "void MaxPlayersCheat(int value = 6)", "net_max_players", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(SyncNetManager.ForceDefaultDesyncDetectionStrategy), "void ForceDefaultDesyncDetectionStrategy()", "net_desync_default", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<float, int, int>(Kingmaker.QA.Arbiter.GameCore.Cheats.ArbiterRunStaticPerformanceTest), "void ArbiterRunStaticPerformanceTest(float step = 10, int type = 0, int iVSync = -1)", "arbiter_spt", "Run Arbiter Static Performance Test. <step> - jump interval for camera ([10]), <type> type of algorithm ([0],1,2), <vsync> - ([-1]/0/1)", "", ExecutionPolicy.PlayMode, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "step",
				Type = "System.Single",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "type",
				Type = "System.Int32",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "iVSync",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(Kingmaker.QA.Arbiter.GameCore.Cheats.DisableClouds), "void DisableClouds()", "clouds_disable", "Отключить облака", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(Kingmaker.QA.Arbiter.GameCore.Cheats.EnableClouds), "void EnableClouds()", "clouds_enable", "Включить облака", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<bool>(Kingmaker.QA.Arbiter.GameCore.Cheats.DisableFog), "void DisableFog(bool disable = true)", "fog_disable", "Отключить туманных объемы", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "disable",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<bool>(Kingmaker.QA.Arbiter.GameCore.Cheats.EnableFog), "void EnableFog(bool disable = true)", "fog_enable", "Включить туманных объемы", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "disable",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<bool>(Kingmaker.QA.Arbiter.GameCore.Cheats.DisableWind), "void DisableWind(bool disable = true)", "wind_disable", "Отключить ветер", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "disable",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(Kingmaker.QA.Arbiter.GameCore.Cheats.EnableWind), "void EnableWind()", "wind_enable", "Включить ветер", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(Kingmaker.QA.Arbiter.GameCore.Cheats.DisableFow), "void DisableFow()", "fow_disable", "Отключить туман войны", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(Kingmaker.QA.Arbiter.GameCore.Cheats.EnableFow), "void EnableFow()", "fow_enable", "Включить туман войны", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(Kingmaker.QA.Arbiter.GameCore.Cheats.DisableFx), "void DisableFx()", "fx_disable", "Отключить fx", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(Kingmaker.QA.Arbiter.GameCore.Cheats.EnableFx), "void EnableFx()", "fx_enable", "Включить fx", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(Kingmaker.QA.Arbiter.GameCore.Cheats.HideUnits), "void HideUnits()", "units_hide", "Скрыть юниты", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(Kingmaker.QA.Arbiter.GameCore.Cheats.ShowUnits), "void ShowUnits()", "units_show", "Показать юниты", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(Kingmaker.QA.Arbiter.GameCore.Cheats.HideUi), "void HideUi()", "ui_hide", "Скрыть UI", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(Kingmaker.QA.Arbiter.GameCore.Cheats.ShowUi), "void ShowUi()", "ui_show", "Показать UI", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<Vector3, string, float>(Kingmaker.QA.Arbiter.GameCore.StaticPerformanceChecker.Cheats.MoveCameraToPoint), "void MoveCameraToPoint(Vector3 position, string rotation, float zoom = 4)", "arbiter_spt_view", "Set camera position with rotation and zoom <x;y;z> <rotation=up|right|down|left> <zoom=4>", "", ExecutionPolicy.All, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "position",
				Type = "UnityEngine.Vector3",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "rotation",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "zoom",
				Type = "System.Single",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<Vector3, float, float>(Kingmaker.QA.Arbiter.GameCore.StaticPerformanceChecker.Cheats.MoveCameraToPoint), "void MoveCameraToPoint(Vector3 position, float rotation, float zoom = 4)", "set_camera", "Move camera to position with rotation and zoom <x;y;z> <0-360> <zoom=4>", "", ExecutionPolicy.All, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "position",
				Type = "UnityEngine.Vector3",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "rotation",
				Type = "System.Single",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "zoom",
				Type = "System.Single",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<Vector3>(Kingmaker.QA.Arbiter.GameCore.StaticPerformanceChecker.Cheats.MoveArbiterRevealerToPoint), "void MoveArbiterRevealerToPoint(Vector3 position)", "revealer_teleport", "Move revealer to position <x;y;z>", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "position",
				Type = "UnityEngine.Vector3",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(OwlcatProtocol.OwlcatProtocolHandler), "void OwlcatProtocolHandler(string message)", "owlcat_protocol", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "message",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, int, int>(StackTraceSpamDetector.StackTraceSpamDetectionEnable), "void StackTraceSpamDetectionEnable(int frameSize, int count = 5, int cooldown = 30000)", "spam_stacktrace_detection", "", "", ExecutionPolicy.All, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "frameSize",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "count",
				Type = "System.Int32",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "cooldown",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(Replay.SetStateSkipFrames), "void SetStateSkipFrames(int n)", "replay_skip", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "n",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(Replay.GetStateSkipFrames), "void GetStateSkipFrames()", "replay_skip_get", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Func<string, string, string>(Replay.CreateReplayStart), "string CreateReplayStart(string replayName, string saveName = null)", "replay_create_start", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "replayName",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "saveName",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "string"),
		new CheatMethodInfoInternal(new Action(Replay.CreateReplayCancel), "void CreateReplayCancel()", "replay_create_cancel", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(Replay.CreateReplayStop), "void CreateReplayStop()", "replay_create_stop", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string, Action, bool>(Replay.PlayReplay), "void PlayReplay(string replayName, Action callbackAfterStart = null, bool popupOnEnd = false)", "replay_play", "", "", ExecutionPolicy.PlayMode, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "replayName",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "callbackAfterStart",
				Type = "System.Action",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "popupOnEnd",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(Replay.CancelReplay), "void CancelReplay()", "replay_cancel", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(Replay.NeedToSaveState.TurnOn), "void TurnOn()", "replay_log_on", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(Replay.NeedToSaveState.TurnOff), "void TurnOff()", "replay_log_off", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(TexturesQualityController.EnableTextureQualityLoweringToReduceMemoryUsage), "void EnableTextureQualityLoweringToReduceMemoryUsage()", "enable_texture_quality_lowering_to_reduce_memory_usage", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(TexturesQualityController.DisableTextureQualityLoweringToReduceMemoryUsage), "void DisableTextureQualityLoweringToReduceMemoryUsage()", "disable_texture_quality_lowering_to_reduce_memory_usage", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int, string>(TexturesQualityController.TexturesMipmapLevelController.CheatSetMipMapLevel), "void CheatSetMipMapLevel(int level, string groupName = null)", "set_mipmap_level", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "level",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "groupName",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(SettingsRoot.DeletePlayerPrefs), "void DeletePlayerPrefs()", "clear_prefs", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Func<string, string>(TutorialCheats.ShowTutorial), "string ShowTutorial(string blueprint)", "tutorial_start", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "blueprint",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "string"),
		new CheatMethodInfoInternal(new Action(TutorialSystem.UnBanAll), "void UnBanAll()", "tutorial_unban", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(MemoryUsageHelper.DumpMemoryStats), "void DumpMemoryStats()", "memory_stats_dump", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string, string>(Screenshot.Capture), "void Capture(string path = null, string name = null)", "screenshot", "capture screenshot", "", ExecutionPolicy.All, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "path",
				Type = "System.String",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "name",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<double>(VideoPlayerHelper.CheatSeekVideo), "void CheatSeekVideo(double seekTime)", "seek_video", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "seekTime",
				Type = "System.Double",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Func<string, bool, Task>(CheatsHelper.Exec), "Task Exec(string path, bool silent = false)", "exec", "", "", ExecutionPolicy.All, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "path",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "silent",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "Task"),
		new CheatMethodInfoInternal(new Action<string>(CheatsBots.ConsoleClockworkStart), "void ConsoleClockworkStart(string scenario)", "clockwork_start", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "scenario",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsBots.ConsoleClockworkStop), "void ConsoleClockworkStop()", "clockwork_stop", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Func<string>(CheatsBots.ConsoleClockworkScenarioList), "string ConsoleClockworkScenarioList()", "clockwork_list_scenarios", "", "", ExecutionPolicy.All, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Func<string>(CheatsBots.ConsoleClockworkStatus), "string ConsoleClockworkStatus()", "clockwork_status", "", "", ExecutionPolicy.All, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Action<bool>(CheatsBugReportUI.BugReportOpenSingleMode), "void BugReportOpenSingleMode(bool showBugReportOnly = true)", "bugreportopensinglemode", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "showBugReportOnly",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsBugReportUI.BugReportHotkeyOpen), "void BugReportHotkeyOpen()", "bugreporthotkeyopen", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsBugReportUI.BugReportShow), "void BugReportShow()", "bugreportshow", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsBugReportUI.BugReportHide), "void BugReportHide()", "bugreporthide", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsBugReportUI.BugReportFeature), "void BugReportFeature()", "bugreportfeature", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsBugReportUI.BugReportCrushDump), "void BugReportCrushDump()", "bugreportcrushdump", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsBugReportUI.BugReportException), "void BugReportException()", "bugreportexception", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsBugReportUI.BugReportErrorMessages), "void BugReportErrorMessages()", "bugreporterrormessages", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<MechanicEntity>(CheatsCombat.KillAll), "void KillAll(MechanicEntity entity)", "kill_all", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "entity",
				Type = "Kingmaker.EntitySystem.Entities.MechanicEntity",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BaseUnitEntity>(CheatsCombat.Kill), "void Kill(BaseUnitEntity unit)", "kill", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "unit",
				Type = "Kingmaker.EntitySystem.Entities.BaseUnitEntity",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<MechanicEntity, bool>(CheatsCombat.Damage), "void Damage(MechanicEntity entity, bool tryToKill = false)", "damage", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "entity",
				Type = "Kingmaker.EntitySystem.Entities.MechanicEntity",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "tryToKill",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<MechanicEntity>(CheatsCombat.Heal), "void Heal(MechanicEntity entity)", "heal", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "entity",
				Type = "Kingmaker.EntitySystem.Entities.MechanicEntity",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsCombat.ListCombatUnits), "void ListCombatUnits()", "list_combat_units", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsCombat.ListCombatBuffs), "void ListCombatBuffs()", "list_combat_buffs", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<BaseUnitEntity>(CheatsCombat.ListBuffs), "void ListBuffs(BaseUnitEntity targetUnit)", "list_buffs", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "targetUnit",
				Type = "Kingmaker.EntitySystem.Entities.BaseUnitEntity",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsCombat.ListBuffsUnderMouse), "void ListBuffsUnderMouse()", "list_buffs_mouse", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<BaseUnitEntity>(CheatsCombat.DetachAllBuffs), "void DetachAllBuffs(BaseUnitEntity unit)", "detach_all_buffs", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "unit",
				Type = "Kingmaker.EntitySystem.Entities.BaseUnitEntity",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsCombat.RestAll), "void RestAll()", "rest_all", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsCombat.SetMP100), "void SetMP100()", "setmp100", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnitFact>(CheatsCombat.DetachFact), "void DetachFact(BlueprintUnitFact fact)", "detach_fact", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "fact",
				Type = "Kingmaker.Blueprints.Facts.BlueprintUnitFact",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnitFact>(CheatsCombat.AttachFact), "void AttachFact(BlueprintUnitFact fact)", "attach_fact", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "fact",
				Type = "Kingmaker.Blueprints.Facts.BlueprintUnitFact",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnit, BlueprintFaction, Vector3>(CheatsCombat.SpawnEnemyUnderCursor), "void SpawnEnemyUnderCursor(BlueprintUnit bp = null, BlueprintFaction factionBp = null, Vector3 position = default(Vector3))", "summon", "", "", ExecutionPolicy.PlayMode, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "bp",
				Type = "Kingmaker.Blueprints.BlueprintUnit",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "factionBp",
				Type = "Kingmaker.Blueprints.BlueprintFaction",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "position",
				Type = "UnityEngine.Vector3",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsCombat.ActivatePeril), "void ActivatePeril(string parameters)", "activate_peril", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "parameters",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsCombat.ActivatePhenomena), "void ActivatePhenomena(string parameters)", "activate_phenomena", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "parameters",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, bool>(CheatsCombat.SpawnUnitsDense), "void SpawnUnitsDense(int number, bool roaming = false)", "spawn_units_dense", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "number",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "roaming",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, bool>(CheatsCombat.SpawnUnitsSparse), "void SpawnUnitsSparse(int number, bool roaming = false)", "spawn_units_sparse", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "number",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "roaming",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, bool>(CheatsCombat.SpawnExtraDense), "void SpawnExtraDense(int number, bool roaming = false)", "spawn_extra_dense", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "number",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "roaming",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, bool>(CheatsCombat.SpawnExtraSparse), "void SpawnExtraSparse(int number, bool roaming = false)", "spawn_extra_sparse", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "number",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "roaming",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsCombat.SpawnEnemies), "void SpawnEnemies(int number)", "spawn_enemies", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "number",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsCombat.SpawnTest), "void SpawnTest()", "spawn_test", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsCombat.SetActionPointsYellow), "void SetActionPointsYellow(int yellow)", "set_action_points_yellow", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "yellow",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, int, RestrictionsHolder, bool>(CheatsCombat.AddBonusAbilityUsage), "void AddBonusAbilityUsage(int count = 1, int costBonus = -5, RestrictionsHolder restrictions = null, bool ingorePartAbilityRestrictions = false)", "add_bonus_ability_usage", "", "", ExecutionPolicy.PlayMode, new CheatParameter[4]
		{
			new CheatParameter
			{
				Name = "count",
				Type = "System.Int32",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "costBonus",
				Type = "System.Int32",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "restrictions",
				Type = "Kingmaker.Blueprints.RestrictionsHolder",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "ingorePartAbilityRestrictions",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<bool>(CheatsCombat.CombatTextDebug), "void CombatTextDebug(bool enable)", "combat_text_debug", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "enable",
				Type = "System.Boolean",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintBodyPart, int>(CheatsCombat.Cheat_AddCritical), "void Cheat_AddCritical(BlueprintBodyPart damageBodyPart, int amount)", "crit_add", "", "", ExecutionPolicy.All, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "damageBodyPart",
				Type = "Kingmaker.Code.Gameplay.Blueprints.BlueprintBodyPart",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "amount",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintBodyPart, int>(CheatsCombat.Cheat_RemoveCritical), "void Cheat_RemoveCritical(BlueprintBodyPart damageBodyPart, int amount)", "crit_remove", "", "", ExecutionPolicy.All, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "damageBodyPart",
				Type = "Kingmaker.Code.Gameplay.Blueprints.BlueprintBodyPart",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "amount",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsCombat.Cheat_RemoveCriticalAll), "void Cheat_RemoveCriticalAll()", "crit_remove_all", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<BlueprintBuff>(CheatsCombat.AttachBuffTyped), "void AttachBuffTyped(BlueprintBuff buff)", "attach_buff", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "buff",
				Type = "Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<MechanicEntity>(CheatsCommon.ExecuteAction), "void ExecuteAction(MechanicEntity target)", "action", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "target",
				Type = "Kingmaker.EntitySystem.Entities.MechanicEntity",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.RandomEncounterStatusSwitch), "void RandomEncounterStatusSwitch()", "random_encounter_status_switch", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsCommon.SetAreaCR), "void SetAreaCR(int value)", "area_cr_set", "Set area CR override to value", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.ClearAreaCR), "void ClearAreaCR()", "area_cr_clear", "Clear area CR override", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.ChecksFail), "void ChecksFail()", "checks_fail", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.ChecksSuccess), "void ChecksSuccess()", "checks_success", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsCommon.SetPlayerDice), "void SetPlayerDice(int value)", "set_dice", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.ReleasePlayerDice), "void ReleasePlayerDice()", "release_dice", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Func<string>(CheatsCommon.ShowAlignmentRanks), "string ShowAlignmentRanks()", "show_alignment_ranks", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Action<string, int>(CheatsCommon.ShiftAlignmentMark), "void ShiftAlignmentMark(string alignmentMark, int value)", "shift_alignment", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "alignmentMark",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsCommon.ShiftXenophilia), "void ShiftXenophilia(int value)", "shift_alignment_xen", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsCommon.ShiftMonodominance), "void ShiftMonodominance(int value)", "shift_alignment_mon", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsCommon.ShiftXanthite), "void ShiftXanthite(int value)", "shift_alignment_xan", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsCommon.ShiftTorian), "void ShiftTorian(int value)", "shift_alignment_tor", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<Locale>(CheatsCommon.ChangeLocalization), "void ChangeLocalization(Locale locale)", "change_localization", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "locale",
				Type = "Kingmaker.Localization.Enums.Locale",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.ShowDebugBubble), "void ShowDebugBubble()", "emperor_open_my_eyes", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.ShowStringDebugMode), "void ShowStringDebugMode()", "lectitio_divinitatus", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.CopyStringDebugName), "void CopyStringDebugName()", "copy_string_debug_name", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.CleanSpace), "void CleanSpace()", "clean_space", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.SetGenderM), "void SetGenderM()", "gender_set_m", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsCommon.SetGenderF), "void SetGenderF()", "gender_set_f", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsContextData.RandomEncounterStatusSwitch), "void RandomEncounterStatusSwitch()", "break_context_data", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<BlueprintCutscene>(CheatsCutscenes.StopCutscenes), "void StopCutscenes(BlueprintCutscene cutscene)", "stop_cutscene", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "cutscene",
				Type = "Kingmaker.Code.Framework.CutsceneSystem.BlueprintCutscene",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintCutscene>(CheatsCutscenes.PlayCutscene), "void PlayCutscene(BlueprintCutscene cutscene)", "play_cutscene", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "cutscene",
				Type = "Kingmaker.Code.Framework.CutsceneSystem.BlueprintCutscene",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.DoRunOutOfMemory), "void DoRunOutOfMemory()", "alloc_crash", "Allocate memory until Unity crashes", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.DoGC), "void DoGC()", "gc", "Call GC.Collect()", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.DoGCUUA), "void DoGCUUA()", "gc_uua", "Call GC.Collect() and UnloadUnusedAssets", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.MemStats), "void MemStats()", "memstats", "Show allocated memory amounts", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.Quit), "void Quit()", "quit", "Call SystemUtil.ApplicationQuit()", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.QuitForce), "void QuitForce()", "quit_force", "Call Application.Quit(1)", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.WwiseProfilerCapture), "void WwiseProfilerCapture()", "wwise_profile", "Launch Wwise profiler session", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ReloadUI), "void ReloadUI()", "reload_ui", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ReloadUIScene), "void ReloadUIScene()", "reload_ui_scene", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ToggleUIFx), "void ToggleUIFx()", "toggle_uifx", "Toggle monitor postprocess volume", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ResetWidgetStash), "void ResetWidgetStash()", "reset_widget_stash", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ChangeUINextPlatform), "void ChangeUINextPlatform()", "change_ui_next_platform", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ChangeUIPrevPlatform), "void ChangeUIPrevPlatform()", "change_ui_prev_platform", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsDebug.SetCameraPosition), "void SetCameraPosition(string parameters)", "set_camera_pos", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "parameters",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.CrashGame), "void CrashGame()", "debug_crash", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ExceptionGame), "void ExceptionGame()", "debug_exception", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ExceptionDBZ), "void ExceptionDBZ()", "test_exception_dbz", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ExceptionNRE), "void ExceptionNRE()", "test_exception_nre", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ExceptionAIOB), "void ExceptionAIOB()", "test_exception_aiob", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ReturnToMainMenu), "void ReturnToMainMenu()", "return_to_main_menu", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.LogDisposables), "void LogDisposables()", "log_disposables", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.TakeSnapshot), "void TakeSnapshot()", "snapshot", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.TakeSnapshotFull), "void TakeSnapshotFull()", "snapshot_full", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.TakeSnapshotNativeOnly), "void TakeSnapshotNativeOnly()", "snapshot_objects", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<bool>(CheatsDebug.FreezeOutsideCameraAll), "void FreezeOutsideCameraAll(bool value)", "freeze_outside_camera_all", "Sets FreezeOutsideCamera to the given value on every unit not in the MainCharacter party roster", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Boolean",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, int, int>(CheatsDebug.DebugExceptionSpam), "void DebugExceptionSpam(int count = 5, int depth = 10, int interval = 10)", "debug_spam_exceptions", "", "", ExecutionPolicy.PlayMode, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "count",
				Type = "System.Int32",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "depth",
				Type = "System.Int32",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "interval",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.DebugOffThread), "void DebugOffThread()", "debug_spam_start_in_outer_thread", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string, int, int>(CheatsDebug.DebugStartSpam), "void DebugStartSpam(string spamType = \\\"exceptions\\\", int intervalMs = 10, int depth = 10)", "debug_start_spam", "", "", ExecutionPolicy.PlayMode, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "spamType",
				Type = "System.String",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "intervalMs",
				Type = "System.Int32",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "depth",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.DebugStopSpam), "void DebugStopSpam()", "debug_stop_spam", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.DisableLogging), "void DisableLogging()", "disable_logging", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.EnableLogging), "void EnableLogging()", "enable_logging", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsDebug.ClearBugReportPrefs), "void ClearBugReportPrefs()", "reset_bugreport_prefs", "to make a call BugReport`s tutor", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsExportCharacter.ExportCharacter), "void ExportCharacter(string preset)", "export_character", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "preset",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsFinalArenaPlague.FinalArenaPlagueOff), "void FinalArenaPlagueOff()", "off_finalarenaplagueoff", "Disable final arena plague", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsFlashlight.ToggleFlashlight), "void ToggleFlashlight()", "flashlight_toggle", "Переключает состояние фонарика главного персонажа", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsFlashlight.FlashlightOn), "void FlashlightOn()", "flashlight_on", "Включает фонарик главного персонажа", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsFlashlight.FlashlightOff), "void FlashlightOff()", "flashlight_off", "Выключает фонарик главного персонажа", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Func<string>(CheatsGraphics.ToggleStochasticSSR), "string ToggleStochasticSSR()", "gl_togglestochasticssr", "Toggle stochastic SSR algorithm in PostProcessing_Global volume", "", ExecutionPolicy.All, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Func<string, string>(CheatsGraphics.DisableKeyword), "string DisableKeyword(string keyword)", "gl_disablekeyword", "Disables shader keyword in all materials in all loaded scenes", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "keyword",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "string"),
		new CheatMethodInfoInternal(new Func<string, float, string>(CheatsGraphics.SetMaterialsFloat), "string SetMaterialsFloat(string name = \\\"\\\", float value = -1)", "gl_setmaterialsfloat", "Sets float value in all materials in all loaded scenes", "", ExecutionPolicy.All, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "name",
				Type = "System.String",
				HasDefaultValue = true
			},
			new CheatParameter
			{
				Name = "value",
				Type = "System.Single",
				HasDefaultValue = true
			}
		}, "string"),
		new CheatMethodInfoInternal(new Func<string>(CheatsGraphics.ToggleSRPBatching), "string ToggleSRPBatching()", "gl_togglesrpbatching", "", "", ExecutionPolicy.All, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Action(CheatsItems.AoeLoot), "void AoeLoot()", "aoe_loot", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Func<string>(CheatsSaves.ListSaves), "string ListSaves()", "list_saves", "", "", ExecutionPolicy.All, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Action<string>(CheatsSaves.RemoteLoadGame), "void RemoteLoadGame(string path)", "load_remote", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "path",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsSaves.SaveGame), "void SaveGame(string name)", "save", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "name",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsSaves.SaveGameAuto), "void SaveGameAuto()", "save_auto", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsSaves.LoadGame), "void LoadGame(string name)", "load", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "name",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsSaves.DeleteSaveGame), "void DeleteSaveGame(string name)", "delsave", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "name",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsSaves.CopySaves), "void CopySaves(string filter)", "copy_saves", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "filter",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsSaves.ImportSaves), "void ImportSaves(string folder = null)", "import_saves", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "folder",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckAthletics), "void SkillCheckAthletics(int difficulty = 0)", "skill_check_athletics", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckAwareness), "void SkillCheckAwareness(int difficulty = 0)", "skill_check_awareness", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckDemolition), "void SkillCheckDemolition(int difficulty = 0)", "skill_check_demolition", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckDiplomacy), "void SkillCheckDiplomacy(int difficulty = 0)", "skill_check_diplomacy", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckInterrogation), "void SkillCheckInterrogation(int difficulty = 0)", "skill_check_interrogation", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckIntimidation), "void SkillCheckIntimidation(int difficulty = 0)", "skill_check_intimidation", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckLoreHeresy), "void SkillCheckLoreHeresy(int difficulty = 0)", "skill_check_lore_heresy", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckLoreXenos), "void SkillCheckLoreXenos(int difficulty = 0)", "skill_check_lore_xenos", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckLoreWarp), "void SkillCheckLoreWarp(int difficulty = 0)", "skill_check_lore_warp", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckMedicae), "void SkillCheckMedicae(int difficulty = 0)", "skill_check_medicae", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckMettle), "void SkillCheckMettle(int difficulty = 0)", "skill_check_mettle", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckMobility), "void SkillCheckMobility(int difficulty = 0)", "skill_check_mobility", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckReflexes), "void SkillCheckReflexes(int difficulty = 0)", "skill_check_reflexes", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckResistance), "void SkillCheckResistance(int difficulty = 0)", "skill_check_resistance", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckSleightOfHand), "void SkillCheckSleightOfHand(int difficulty = 0)", "skill_check_sleight_of_hand", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckTechUse), "void SkillCheckTechUse(int difficulty = 0)", "skill_check_tech_use", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckTenacity), "void SkillCheckTenacity(int difficulty = 0)", "skill_check_tenacity", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsSkillCheck.SkillCheckWits), "void SkillCheckWits(int difficulty = 0)", "skill_check_wits", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "difficulty",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(CheatsTime.SkipHours), "void SkipHours(int hours)", "skip_hours", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "hours",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsTime.TimeScaleUp), "void TimeScaleUp()", "time_scale_up", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(CheatsTime.TimeScaleDown), "void TimeScaleDown()", "time_scale_down", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Func<string>(CheatsTime.GetTime), "string GetTime()", "get_time", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Action<BlueprintAreaEnterPoint>(CheatsTransfer.Tp2Loc), "void Tp2Loc(BlueprintAreaEnterPoint enterPoint)", "tp2loc", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "enterPoint",
				Type = "Kingmaker.Blueprints.Area.BlueprintAreaEnterPoint",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintArea>(CheatsTransfer.TeleportArea), "void TeleportArea(BlueprintArea area)", "teleport_area", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "area",
				Type = "Kingmaker.Blueprints.Area.BlueprintArea",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsTransfer.ListLocs), "void ListLocs(string nameSubstring)", "list_locs", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "nameSubstring",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Func<string, string>(CheatsTransfer.EnableLocsAlias), "string EnableLocsAlias(string arg)", "locs_alias", "Enable/disable commands tp2loc_*location*", "True, true, false", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "arg",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "string"),
		new CheatMethodInfoInternal(new Action<Vector3, string>(CheatsTransfer.LocalTeleport), "void LocalTeleport(Vector3 tpPosition, string selectedUnits)", "local_teleport", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "tpPosition",
				Type = "UnityEngine.Vector3",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "selectedUnits",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int, int, int>(CheatsTransfer.DebugTeleport), "void DebugTeleport(int x, int y, int z)", "debug_teleport", "Teleport party to scene position. Coordinates match Transform Position / Position Overlay (F11).", "100 0 50", ExecutionPolicy.PlayMode, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "x",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "y",
				Type = "System.Int32",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "z",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Func<string>(CheatsTransfer.GetPosition), "string GetPosition()", "get_position", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Action(CheatsTransfer.ListGamePresets), "void ListGamePresets()", "list_game_presets", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<BlueprintAreaPreset>(CheatsTransfer.StartNewGame), "void StartNewGame(BlueprintAreaPreset preset)", "new_game", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "preset",
				Type = "Kingmaker.Blueprints.Area.BlueprintAreaPreset",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<string>(CheatsUnlock.CheckFlag), "void CheckFlag(string flag = null)", "check_flag", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "flag",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintQuestObjective>(CheatsUnlock.CompleteObjective), "void CompleteObjective(BlueprintQuestObjective targetObjective)", "objective_complete", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "targetObjective",
				Type = "Kingmaker.Blueprints.Quests.BlueprintQuestObjective",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintItem, int>(CheatsUnlock.CreateItem), "void CreateItem(BlueprintItem blueprint, int quantity = 1)", "create_item", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "blueprint",
				Type = "Kingmaker.Blueprints.Items.BlueprintItem",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "quantity",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnitFact>(CheatsUnlock.AddFeature), "void AddFeature(BlueprintUnitFact blueprint)", "add_feature", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "blueprint",
				Type = "Kingmaker.Blueprints.Facts.BlueprintUnitFact",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnitFact>(CheatsUnlock.RemoveFeature), "void RemoveFeature(BlueprintUnitFact blueprint)", "remove_feature", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "blueprint",
				Type = "Kingmaker.Blueprints.Facts.BlueprintUnitFact",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnit>(CheatsUnlock.RemoveCompanionFromRoster), "void RemoveCompanionFromRoster(BlueprintUnit unit)", "remove_companion_from_roster", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "unit",
				Type = "Kingmaker.Blueprints.BlueprintUnit",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnit>(CheatsUnlock.DetachCompanion), "void DetachCompanion(BlueprintUnit unit)", "detach_companion", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "unit",
				Type = "Kingmaker.Blueprints.BlueprintUnit",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnit>(CheatsUnlock.UnrecruitCompanion), "void UnrecruitCompanion(BlueprintUnit unit)", "unrecruit_companion", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "unit",
				Type = "Kingmaker.Blueprints.BlueprintUnit",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(CheatsUnlock.ChangeParty), "void ChangeParty()", "change_party", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnlockableFlag>(CheatsUnlock.LockFlagTyped), "void LockFlagTyped(BlueprintUnlockableFlag flag)", "lock_flag", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "flag",
				Type = "Kingmaker.Blueprints.BlueprintUnlockableFlag",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnlockableFlag, int>(CheatsUnlock.UnlockFlagTyped), "void UnlockFlagTyped(BlueprintUnlockableFlag flag, int value = 0)", "unlock_flag", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "flag",
				Type = "Kingmaker.Blueprints.BlueprintUnlockableFlag",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintQuestObjective>(CheatsUnlock.ObjectiveGive), "void ObjectiveGive(BlueprintQuestObjective objective)", "objective_give", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "objective",
				Type = "Kingmaker.Blueprints.Quests.BlueprintQuestObjective",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintQuestObjective>(CheatsUnlock.ObjectiveFail), "void ObjectiveFail(BlueprintQuestObjective objective)", "objective_fail", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "objective",
				Type = "Kingmaker.Blueprints.Quests.BlueprintQuestObjective",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintEtude>(CheatsUnlock.EtudeStart), "void EtudeStart(BlueprintEtude etude)", "etude_start", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "etude",
				Type = "Kingmaker.AreaLogic.Etudes.BlueprintEtude",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintEtude>(CheatsUnlock.EtudeComplete), "void EtudeComplete(BlueprintEtude etude)", "etude_complete", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "etude",
				Type = "Kingmaker.AreaLogic.Etudes.BlueprintEtude",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintEtude>(CheatsUnlock.EtudeUncomplete), "void EtudeUncomplete(BlueprintEtude etude)", "etude_uncomplete", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "etude",
				Type = "Kingmaker.AreaLogic.Etudes.BlueprintEtude",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintDialog>(CheatsUnlock.DialogForce), "void DialogForce(BlueprintDialog dialog)", "dialog_force", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "dialog",
				Type = "Kingmaker.DialogSystem.Blueprints.BlueprintDialog",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(DlcCheats.RefreshAllDLCStatuses), "void RefreshAllDLCStatuses()", "refresh_all_dlc_statuses", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<BlueprintDlc>(DlcCheats.SetDlcEnabled), "void SetDlcEnabled(BlueprintDlc dlc)", "set_dlc_enabled", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "dlc",
				Type = "Kingmaker.DLC.BlueprintDlc",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintDlc>(DlcCheats.SetDlcDisabled), "void SetDlcDisabled(BlueprintDlc dlc)", "set_dlc_disabled", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "dlc",
				Type = "Kingmaker.DLC.BlueprintDlc",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Func<BlueprintDlc, string>(DlcCheats.CheckDlcStatus), "string CheckDlcStatus(BlueprintDlc dlc)", "check_dlc_status", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "dlc",
				Type = "Kingmaker.DLC.BlueprintDlc",
				HasDefaultValue = false
			}
		}, "string"),
		new CheatMethodInfoInternal(new Action(DlcCheats.CheckAllDlcStatuses), "void CheckAllDlcStatuses()", "check_all_dlc_statuses", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(DlcCheats.SetAllDlcEnabled), "void SetAllDlcEnabled()", "set_all_dlc_enabled", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(DlcCheats.SetAllDlcDisabled), "void SetAllDlcDisabled()", "set_all_dlc_disabled", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int>(MoraleCheats.MoraleAdd), "void MoraleAdd(int value = 1)", "morale_add", "Increases or decrease morale by specified value (default = 1)", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(MoraleCheats.MoralePrintGroups), "void MoralePrintGroups()", "morale_print_groups", "Prints all morale groups in combat.", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Func<string, object>(StateExplorer.GetObject), "object GetObject(string path)", "game_data", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "path",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "object"),
		new CheatMethodInfoInternal(new Action(EtudeBracketForbidOpenShipInventory.ChangeStarshipAccess), "void ChangeStarshipAccess()", "change_ship_access", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(InteractionHighlightController.SwitchHighlightCovers), "void SwitchHighlightCovers()", "switch_highlight_covers", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<bool>(MovePredictionController.SetActive), "void SetActive(bool value)", "net_move", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Boolean",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(StateSerializationController.Collect_State), "void Collect_State()", "collect_state", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(StateSerializationController.Save_State), "void Save_State()", "save_state", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(StateSerializationController.Apply_State), "void Apply_State()", "apply_state", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int>(PsychicPhenomenaController.HealVeil), "void HealVeil(int value = 1)", "veil_heal", "Heal veil by specified value (default = 1)", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(PsychicPhenomenaController.DamageVeil), "void DamageVeil(int value = 1)", "veil_damage", "Damage veil by specified value (default = 1)", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(PsychicPhenomenaController.HealVeilAll), "void HealVeilAll()", "veil_heal_all", "Heal all veil damage", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<int>(TimeSpeedController.ForceSpeedMode), "void ForceSpeedMode(int speedMode = -1)", "net_speed_mode", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "speedMode",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<bool>(TimeSpeedController.AutoSpeed.AutoSpeedCheat), "void AutoSpeedCheat(bool auto = false)", "net_auto_speed", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "auto",
				Type = "System.Boolean",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<float>(TimeSpeedController.AutoSpeed.SlowCheat), "void SlowCheat(float value = 0.99)", "net_speed_slow", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "value",
				Type = "System.Single",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(TurnController.TryEndPlayerTurnStatic), "void TryEndPlayerTurnStatic()", "end_turn", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(UnitForceMoveController.Debug_Force_Move), "void Debug_Force_Move()", "debug_force_move", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<BlueprintBarkBanterList>(ListBarkBanterEvaluator.Debug_Show_Banter_List), "void Debug_Show_Banter_List(BlueprintBarkBanterList list)", "debug_show_banter_list", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "list",
				Type = "Kingmaker.BarkBanters.BlueprintBarkBanterList",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<ReputationType, int>(ReputationHelper.Cheat_AddReputationAll), "void Cheat_AddReputationAll(ReputationType reputationType, int value)", "add_reputation_all", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "reputationType",
				Type = "Kingmaker.Gameplay.Features.Reputation.ReputationType",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<FactionType, ReputationType, int>(ReputationHelper.Cheat_AddReputation), "void Cheat_AddReputation(FactionType faction, ReputationType reputationType, int value)", "add_reputation", "", "", ExecutionPolicy.PlayMode, new CheatParameter[3]
		{
			new CheatParameter
			{
				Name = "faction",
				Type = "Kingmaker.Gameplay.Features.Reputation.FactionType",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "reputationType",
				Type = "Kingmaker.Gameplay.Features.Reputation.ReputationType",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "value",
				Type = "System.Int32",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<int>(VendorHelper.AddMoney), "void AddMoney(int amount = 1000)", "add_money", "Add money to player (default = 1000)", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "amount",
				Type = "System.Int32",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(UnitGroupAttackFactionsValidator.Validate), "void Validate()", "validate_unit_groups", "", "", ExecutionPolicy.All, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(PartyAutoFormationHelper.UpdateAutoFormation), "void UpdateAutoFormation()", "update_auto_formation", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(UnitHelper.CheatRespecUnit), "void CheatRespecUnit()", "respec", "Respec selected unit", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Func<bool>(InputLog.LogInput), "bool LogInput()", "log_key_input", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "bool"),
		new CheatMethodInfoInternal(new Action<float>(InputLog.LogCurrentInput), "void LogCurrentInput(float delay = 5)", "log_current_input_state", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "delay",
				Type = "System.Single",
				HasDefaultValue = true
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<float, float>(CameraRig.StartShakeCheat), "void StartShakeCheat(float amplitude, float speed)", "start_shake", "", "", ExecutionPolicy.PlayMode, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "amplitude",
				Type = "System.Single",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "speed",
				Type = "System.Single",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action<BlueprintUnitAsksList>(UnitAsksManager.OverridePlayerAsks), "void OverridePlayerAsks(BlueprintUnitAsksList asksList)", "overrideplayerasks", "", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "asksList",
				Type = "Kingmaker.Visual.Sound.BlueprintUnitAsksList",
				HasDefaultValue = false
			}
		}, "void")
	};

	public static readonly List<CheatPropertyInfoInternal> Properties = new List<CheatPropertyInfoInternal>
	{
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => IgnorePrerequisites.IgnorePrerequisitesAlways),
			Setter = (Action<bool>)delegate(bool value)
			{
				IgnorePrerequisites.IgnorePrerequisitesAlways = value;
			}
		}, "ignore_prereq", "When true, prerequisites will be ignored", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => EditorSafeThreading.AsyncSaves),
			Setter = (Action<bool>)delegate(bool value)
			{
				EditorSafeThreading.AsyncSaves = value;
			}
		}, "enable_editor_async_save", "", "", ExecutionPolicy.PlayMode, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => SaveManager.AllowSaveInCutscenesAndDialogs),
			Setter = (Action<bool>)delegate(bool value)
			{
				SaveManager.AllowSaveInCutscenesAndDialogs = value;
			}
		}, "allow_save_in_cutscenes_and_dialogs", "", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => SteamSavesReplicator.ForceFullDownload),
			Setter = (Action<bool>)delegate(bool value)
			{
				SteamSavesReplicator.ForceFullDownload = value;
			}
		}, "steam_force_full_download", "", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<float>)(() => CheatsAnimation.SpeedForce),
			Setter = (Action<float>)delegate(float value)
			{
				CheatsAnimation.SpeedForce = value;
			}
		}, "am_forcespeed", "Set to override movement speed for player characters, in feet per standard action. Set to 0 to revert to default speed", "", ExecutionPolicy.All, "float"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<int>)(() => CheatsAnimation.MoveType),
			Setter = (Action<int>)delegate(int value)
			{
				CheatsAnimation.MoveType = value;
			}
		}, "am_movetype", "Set to override default move type for player characters.\n0 = charge\n1 = walk\n2 = run\n3 = crouch\n", "", ExecutionPolicy.All, "int"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CheatsAnimation.SpeedLock),
			Setter = (Action<bool>)delegate(bool value)
			{
				CheatsAnimation.SpeedLock = value;
			}
		}, "am_speedlock", "When true, all player characters move with the same speed out of combat.\nWhen false, everyone uses their own default speed.", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CheatsCommon.RandomEncounters),
			Setter = (Action<bool>)delegate(bool value)
			{
				CheatsCommon.RandomEncounters = value;
			}
		}, "random_enc", "When false, random encounters on global map are disabled", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CheatsCommon.SendAnalyticEvents),
			Setter = (Action<bool>)delegate(bool value)
			{
				CheatsCommon.SendAnalyticEvents = value;
			}
		}, "send_unity_events", "When true, send unity analytic events as normal game does", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CheatsCommon.IgnoreEncumbrance),
			Setter = (Action<bool>)delegate(bool value)
			{
				CheatsCommon.IgnoreEncumbrance = value;
			}
		}, "ignore_encumbrance", "When true, encumbrance is always Light", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CheatsDebug.DrawFps),
			Setter = (Action<bool>)delegate(bool value)
			{
				CheatsDebug.DrawFps = value;
			}
		}, "draw_fps", "When false, FPS Counter is disabled", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CheatsDebug.DrawCutscenes),
			Setter = (Action<bool>)delegate(bool value)
			{
				CheatsDebug.DrawCutscenes = value;
			}
		}, "draw_cutscenes", "When false, Cutscenes debug info is disabled", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CheatsDebug.DrawSpaceCombatDebugDecals),
			Setter = (Action<bool>)delegate(bool value)
			{
				CheatsDebug.DrawSpaceCombatDebugDecals = value;
			}
		}, "draw_space_combat_debug_decals", "When false, space combat debug decals are disabled", "", ExecutionPolicy.PlayMode, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CheatsJira.QaMode),
			Setter = (Action<bool>)delegate(bool value)
			{
				CheatsJira.QaMode = value;
			}
		}, "qa_mode", "Set to true to see all exceptions as a message box", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CheatsSaves.ForceJsonSaves),
			Setter = (Action<bool>)delegate(bool value)
			{
				CheatsSaves.ForceJsonSaves = value;
			}
		}, "force_json_saves", "", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<int>)(() => CheatsSoundRagdoll.SoundRagdollDebug),
			Setter = (Action<int>)delegate(int value)
			{
				CheatsSoundRagdoll.SoundRagdollDebug = value;
			}
		}, "sound_ragdoll", "0 = off, 1 = receiver logs (play/block reasons), 2 = verbose (+ every bone collision from sender)", "", ExecutionPolicy.All, "int"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => MoraleCheats.MoraleDisableChanges),
			Setter = (Action<bool>)delegate(bool value)
			{
				MoraleCheats.MoraleDisableChanges = value;
			}
		}, "morale_disable_changes", "", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => PointerController.PointerDebug),
			Setter = (Action<bool>)delegate(bool value)
			{
				PointerController.PointerDebug = value;
			}
		}, "pointer_debug", "", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<float>)(() => SlowMoController.SlowMoFactor),
			Setter = (Action<float>)delegate(float value)
			{
				SlowMoController.SlowMoFactor = value;
			}
		}, "SlowMoFactor", "", "", ExecutionPolicy.All, "float"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<float>)(() => UnitJumpAsideDodgeParams.Speed),
			Setter = (Action<float>)delegate(float value)
			{
				UnitJumpAsideDodgeParams.Speed = value;
			}
		}, "jump_aside_dodge_speed", "", "", ExecutionPolicy.All, "float"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CameraRig.DebugCameraScroll),
			Setter = (Action<bool>)delegate(bool value)
			{
				CameraRig.DebugCameraScroll = value;
			}
		}, "debug_camera_scroll", "", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<float>)(() => CameraRig.ConsoleScrollMod),
			Setter = (Action<float>)delegate(float value)
			{
				CameraRig.ConsoleScrollMod = value;
			}
		}, "camera_scroll_mod", "", "", ExecutionPolicy.All, "float"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<float>)(() => CameraRig.ConsoleRotationMod),
			Setter = (Action<float>)delegate(float value)
			{
				CameraRig.ConsoleRotationMod = value;
			}
		}, "camera_rotation_mod", "", "", ExecutionPolicy.All, "float"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => CameraRig.IsShakingCheat),
			Setter = (Action<bool>)delegate(bool value)
			{
				CameraRig.IsShakingCheat = value;
			}
		}, "is_shaking", "", "", ExecutionPolicy.PlayMode, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => UnitMovementAgent.FallbackToRayCast),
			Setter = (Action<bool>)delegate(bool value)
			{
				UnitMovementAgent.FallbackToRayCast = value;
			}
		}, "movement_use_raycast", "", "", ExecutionPolicy.All, "bool"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => ForcedCullingService.CheatDisabled),
			Setter = (Action<bool>)delegate(bool value)
			{
				ForcedCullingService.CheatDisabled = value;
			}
		}, "forced_culling_disabled", "", "", ExecutionPolicy.All, "bool")
	};

	public static readonly List<(ArgumentConverter.ConvertDelegate, int)> ArgConverters = new List<(ArgumentConverter.ConvertDelegate, int)>
	{
		(CheatArgConverters.UnitConverter, 0),
		(CheatArgConverters.MechanicEntityConverter, 0),
		(CheatArgConverters.Vector3Converter, 0),
		(Utilities.BlueprintConverter, 0)
	};

	public static readonly List<(ArgumentConverter.PreprocessDelegate, int)> ArgPreprocessors = new List<(ArgumentConverter.PreprocessDelegate, int)>
	{
		(CheatArgPreprocessors.SelectedUnits, 0),
		(CheatArgPreprocessors.Mouseover, 0),
		(CheatArgPreprocessors.Cursor, 0)
	};
}
