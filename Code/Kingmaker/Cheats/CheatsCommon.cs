using System;
using System.Collections.Generic;
using Core.Cheats;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.Localization.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Settings;
using Kingmaker.UI.InputSystems;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Effects.ParticleSumEmitter;
using UnityEngine;
using UnityHeapEx;

namespace Kingmaker.Cheats;

public class CheatsCommon
{
	public class CheatedDice : IGlobalRulebookHandler<RuleRollD100>, IRulebookHandler<RuleRollD100>, ISubscriber, IGlobalRulebookSubscriber
	{
		public static readonly CheatedDice Singletone = new CheatedDice();

		public int Additive;

		private bool m_IsSubscribed;

		public void OnEventAboutToTrigger(RuleRollD100 evt)
		{
		}

		public void OnEventDidTrigger(RuleRollD100 evt)
		{
			if (evt.Initiator.IsPlayerFaction)
			{
				evt.Override(Additive, null);
			}
		}

		public void Subscribe()
		{
			if (!m_IsSubscribed)
			{
				EventBus.Subscribe(Singletone);
				m_IsSubscribed = true;
			}
		}

		public void Unsubscribe()
		{
			if (m_IsSubscribed)
			{
				EventBus.Unsubscribe(Singletone);
				m_IsSubscribed = false;
			}
		}
	}

	private static bool s_HudHidden;

	private static Canvas[] s_HiddenCanvases;

	private static int s_Grade = 1;

	private static readonly int[] Grades = new int[3] { -999, 0, 999 };

	private static readonly System.Random s_Rnd = new System.Random(42);

	[Cheat(Name = "random_enc", Description = "When false, random encounters on global map are disabled")]
	public static bool RandomEncounters { get; set; } = true;


	[Cheat(Name = "send_unity_events", Description = "When true, send unity analytic events as normal game does")]
	public static bool SendAnalyticEvents { get; set; }

	[Cheat(Name = "ignore_encumbrance", Description = "When true, encumbrance is always Light")]
	public static bool IgnoreEncumbrance { get; set; }

	public static void RegisterCheats(KeyboardAccess keyboard)
	{
		CheatsTransfer.RegisterCommands(keyboard);
		CheatsUnlock.RegisterCommands(keyboard);
		CheatsStats.RegisterCommands();
		CheatsPooling.RegisterCommands(keyboard);
		CheatsTime.RegisterCommands(keyboard);
		CheatsDebug.RegisterCommands(keyboard);
		CheatsItems.RegisterCommands(keyboard);
		CheatsCombat.RegisterCommands(keyboard);
		CheatsSaves.RegisterCommands(keyboard);
		CheatsRomance.RegisterCheats();
		if (BuildModeUtility.CheatsEnabled)
		{
			keyboard.Bind("Action", delegate
			{
				CheatsHelper.Run("action @mouseover");
			});
			keyboard.Bind("ChecksFail", delegate
			{
				CheatsHelper.Run("checks_fail");
			});
			keyboard.Bind("ChecksSuccess", delegate
			{
				CheatsHelper.Run("checks_success");
			});
			keyboard.Bind("RandomEncounterStatusSwitch", delegate
			{
				CheatsHelper.Run("random_encounter_status_switch");
			});
			keyboard.Bind("StatCoercion", delegate
			{
				CheatsHelper.Run("stat_coercion");
			});
			keyboard.Bind("SwitchHighlightCovers", delegate
			{
				CheatsHelper.Run("switch_highlight_covers");
			});
			keyboard.Bind("ShowDebugBubble", delegate
			{
				CheatsHelper.Run("emperor_open_my_eyes");
			});
			SmartConsole.RegisterCommand("produce_exception", CheatsDebug.ProduceException);
			SmartConsole.RegisterCommand("gain_xp", GainExperience);
			SmartConsole.RegisterCommand("remove_untyped_ac", RemoveUntypedAC);
			SmartConsole.RegisterCommand("set_locale", SetLocale);
			SmartConsole.RegisterCommand("dumpmem", delegate
			{
				HeapDump.DoStuff();
			});
			SmartConsole.RegisterCommand("shuffle_party", ShuffleParty);
			SmartConsole.RegisterCommand("reload_area", delegate
			{
				Game.Instance.ReloadArea();
			});
			SmartConsole.RegisterCommand("ui_toggle", ToggleHUD);
			SmartConsole.RegisterCommand("item_dialog", ItemDialog);
			SmartConsole.RegisterCommand("hardware_detect", TestHardwareDetect);
			SmartConsole.RegisterCommand("show_titles", ShowTitles);
			SmartConsole.RegisterCommand("show_net", ShowNet);
		}
	}

	private static void ShowNet(string parameters)
	{
		EventBus.RaiseEvent(delegate(INetLobbyRequest h)
		{
			h.HandleNetLobbyRequest();
		});
	}

	private static void ItemDialog(string parameters)
	{
		foreach (ItemEntity item in Game.Instance.Player.MainCharacterEntity.Inventory)
		{
			if (item.Blueprint.ComponentsArray.TryFind((BlueprintComponent x) => x is ItemDialog, out var result) && result is ItemDialog itemDialog)
			{
				itemDialog.StartDialog();
			}
		}
	}

	private static void ToggleHUD(string parameters)
	{
		s_HudHidden = !s_HudHidden;
		if (s_HudHidden)
		{
			s_HiddenCanvases = UnityEngine.Object.FindObjectsOfType<Canvas>();
		}
		Canvas[] array = s_HiddenCanvases;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(!s_HudHidden);
		}
	}

	[Cheat(Name = "action", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ExecuteAction(MechanicEntity target)
	{
		if (target != null)
		{
			using (MechanicsContext mechanicsContext = MechanicsContext.Claim(CheatRoot.Instance, Game.Instance.DefaultUnit))
			{
				using (mechanicsContext.SetScope(target, null))
				{
					ConfigRoot.Instance.Cheats.TestAction.Run();
					return;
				}
			}
		}
		ConfigRoot.Instance.Cheats.TestAction.Run();
	}

	[Cheat(Name = "random_encounter_status_switch", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void RandomEncounterStatusSwitch()
	{
		RandomEncounters = !RandomEncounters;
		string text = "Random encounter is " + (RandomEncounters ? "enabled" : "disabled");
		PFLog.Default.Log(text);
		UtilityMessageBox.SendWarning(text);
	}

	[Cheat(Name = "checks_fail", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ChecksFail()
	{
		s_Grade = Math.Max(0, s_Grade - 1);
		SetPlayerDice(Grades[s_Grade]);
	}

	[Cheat(Name = "checks_success", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ChecksSuccess()
	{
		s_Grade = Math.Min(Grades.Length - 1, s_Grade + 1);
		SetPlayerDice(Grades[s_Grade]);
	}

	[Cheat(Name = "set_dice", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SetPlayerDice(int value)
	{
		CheatedDice.Singletone.Subscribe();
		CheatedDice.Singletone.Additive = Math.Max(1, value);
		PFLog.Default.Log("Dice was cheated with " + CheatedDice.Singletone.Additive);
	}

	[Cheat(Name = "release_dice", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ReleasePlayerDice()
	{
		CheatedDice.Singletone.Unsubscribe();
	}

	private static void GainExperience(string parameters)
	{
		int? paramInt = Utilities.GetParamInt(parameters, 1, "Can't parse xp amount");
		if (paramInt.HasValue)
		{
			Game.Instance.Player.GainPartyExperience(paramInt.Value);
		}
	}

	private static void RemoveUntypedAC(string parameters)
	{
		PFLog.SmartConsole.Log("Not implemented");
	}

	private static void SetLocale(string parameters)
	{
		try
		{
			string paramString = Utilities.GetParamString(parameters, 1, "Locale not specified");
			Locale locale = (Locale)Enum.Parse(typeof(Locale), paramString, ignoreCase: true);
			if (Enum.IsDefined(typeof(Locale), locale))
			{
				LocalizationManager.Instance.CurrentLocale = locale;
			}
		}
		catch (Exception)
		{
		}
	}

	private static AlignmentAxis? ParseSoulMark(string p, bool logError = true)
	{
		AlignmentAxis value;
		switch (p)
		{
		case "tor":
			value = AlignmentAxis.Torian;
			break;
		case "mon":
			value = AlignmentAxis.Monodominance;
			break;
		case "xen":
			value = AlignmentAxis.Xenophilia;
			break;
		case "xan":
			value = AlignmentAxis.Xanthite;
			break;
		default:
			if (logError)
			{
				SmartConsole.Print("Can't parse alignment, use one of these: tor|mon|xen|xan");
			}
			return null;
		}
		return value;
	}

	[Cheat(Name = "show_alignment_ranks", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static string ShowAlignmentRanks()
	{
		int mainCharacterAlignmentMark = AlignmentShiftExtension.GetMainCharacterAlignmentMark(AlignmentAxis.Torian);
		int mainCharacterAlignmentMark2 = AlignmentShiftExtension.GetMainCharacterAlignmentMark(AlignmentAxis.Monodominance);
		int mainCharacterAlignmentMark3 = AlignmentShiftExtension.GetMainCharacterAlignmentMark(AlignmentAxis.Xenophilia);
		int mainCharacterAlignmentMark4 = AlignmentShiftExtension.GetMainCharacterAlignmentMark(AlignmentAxis.Xanthite);
		return "Torian: " + mainCharacterAlignmentMark + ", Monodominance: " + mainCharacterAlignmentMark2 + ", Xanthite: " + mainCharacterAlignmentMark3 + ", Xenophilia " + mainCharacterAlignmentMark4;
	}

	[Cheat(Name = "shift_alignment", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ShiftAlignmentMark(string alignmentMark, int value)
	{
		AlignmentAxis? alignmentAxis = ParseSoulMark(alignmentMark);
		if (alignmentAxis.HasValue)
		{
			AlignmentShiftExtension.ApplyShiftToMainCharacter(new AlignmentShift
			{
				Axis = alignmentAxis.Value,
				Value = value
			});
		}
	}

	[Cheat(Name = "shift_alignment_xen", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ShiftXenophilia(int value)
	{
		ShiftAlignment(AlignmentAxis.Xenophilia, value);
	}

	[Cheat(Name = "shift_alignment_mon", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ShiftMonodominance(int value)
	{
		ShiftAlignment(AlignmentAxis.Monodominance, value);
	}

	[Cheat(Name = "shift_alignment_xan", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ShiftXanthite(int value)
	{
		ShiftAlignment(AlignmentAxis.Xanthite, value);
	}

	[Cheat(Name = "shift_alignment_tor", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ShiftTorian(int value)
	{
		ShiftAlignment(AlignmentAxis.Torian, value);
	}

	private static void ShiftAlignment(AlignmentAxis axis, int value)
	{
		AlignmentShiftExtension.ApplyShiftToMainCharacter(new AlignmentShift
		{
			Axis = axis,
			Value = value
		}, ConfigRoot.Instance.Cheats);
	}

	[Cheat(Name = "change_localization", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ChangeLocalization(Locale locale)
	{
		if (locale == LocalizationManager.Instance.CurrentLocale)
		{
			return;
		}
		SettingsController.Instance.Sync();
		SettingsController.Instance.ConfirmAllTempValues();
		SettingsController.Instance.SaveAll();
		UISettingsRoot.Instance.UIGameMainSettings.Localization.SetIndexValueAndConfirm((int)locale);
		Game.Instance.RootUIContext.ResetUI(delegate
		{
			EventBus.RaiseEvent(delegate(ISettingsUIHandler h)
			{
				h.HandleOpenSettings(Game.IsInMainMenu);
			});
		});
	}

	[Cheat(Name = "emperor_open_my_eyes", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ShowDebugBubble()
	{
		Game.Instance.RootUIContext.SwitchDebugBlueprintsInformationShow();
		if (Game.Instance.RootUIContext.GetDebugBlueprintsInformationShow())
		{
			EventBus.RaiseEvent(delegate(IDebugInformationUIHandler h)
			{
				h.HandleShowDebugBubble();
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(IDebugInformationUIHandler h)
			{
				h.HandleHideDebugBubble();
			});
		}
	}

	private static void ShuffleParty(string parameters)
	{
		List<BaseUnitEntity> party = Game.Instance.Player.Party;
		int num = party.Count;
		while (num > 1)
		{
			int index = s_Rnd.Next(0, num) % num;
			num--;
			BaseUnitEntity value = party[index];
			party[index] = party[num];
			party[num] = value;
		}
	}

	private static void TestHardwareDetect(string parameters)
	{
		switch (HardwareConfigDetect.GetConfigIndex())
		{
		case HardwareConfigDetect.HardwareLevel.Low:
			Debug.Log("Test hardware detected: LOW");
			break;
		case HardwareConfigDetect.HardwareLevel.Medium:
			Debug.Log("Test hardware detected: MEDIUM");
			break;
		case HardwareConfigDetect.HardwareLevel.High:
			Debug.Log("Test hardware detected: HIGH");
			break;
		default:
			Debug.Log("Test hardware detected: UNKNOWN");
			break;
		}
	}

	private static void ShowTitles(string parameters)
	{
		EventBus.RaiseEvent(delegate(IEndGameTitlesUIHandler h)
		{
			h.HandleShowEndGameTitles(returnToMainMenu: false);
		});
	}

	[Cheat(Name = "clean_space", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CleanSpace()
	{
		foreach (Entity entity2 in Game.Instance.EntityPools.Entities)
		{
			if (entity2 is BaseUnitEntity baseUnitEntity && baseUnitEntity.LifeState.IsFinallyDead)
			{
				Game.Instance.Controllers.EntityDestroyer.Destroy(baseUnitEntity);
			}
			if (entity2 is DroppedLoot.EntityData entity)
			{
				Game.Instance.Controllers.EntityDestroyer.Destroy(entity);
			}
		}
		FxHelper.DestroyAll();
		MonoSingleton<ParticleSystemCustomSubEmitterDelegate>.Instance.ClearAllParticles();
	}
}
