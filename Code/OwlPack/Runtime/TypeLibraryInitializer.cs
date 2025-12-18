using System;
using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Gameplay.Features.Channeling;
using Kingmaker.Gameplay.Features.Concentration;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace OwlPack.Runtime;

public static class TypeLibraryInitializer
{
	public static void Initialize()
	{
		Dictionary<string, Type> dictionary = new Dictionary<string, Type>
		{
			{
				"Kingmaker.Gameplay.Parts.PartAbilityModifiers+AppliedEntry, Code",
				typeof(PartAbilityModifiers.AddedEntry)
			},
			{
				"Kingmaker.Gameplay.Parts.PartChanneling, Code",
				typeof(PartChanneling)
			},
			{
				"Kingmaker.Gameplay.Parts.PartConcentration, Code",
				typeof(PartConcentration)
			},
			{
				"Kingmaker.Code.Gameplay.Parts.PartAdditionalCombatObjective, Code",
				typeof(PartAdditionalCombatObjectiveMapObject)
			},
			{
				"Kingmaker.UnitLogic.Parts.UnitPartInteractions, Code",
				typeof(PartUnitInteractions)
			},
			{
				"Kingmaker.Code.UI.MVVM.CharGenConfig+CharGenCompanionType, Code",
				typeof(CharGenCompanionType)
			},
			{
				"Kingmaker.Code.UI.MVVM.CharGenConfig+CharGenMode, Code",
				typeof(CharGenMode)
			},
			{
				"Kingmaker.Code.UI.MVVM.CombatEndReason, Code",
				typeof(CombatEndReason)
			},
			{
				"Kingmaker.Code.UI.MVVM.EquipSlotType, Code",
				typeof(EquipSlotType)
			},
			{
				"Kingmaker.Code.UI.MVVM.EquipSlotSubtype, Code",
				typeof(EquipSlotSubtype)
			},
			{
				"Kingmaker.Code.UI.MVVM.FullScreenUIType, Code",
				typeof(FullScreenUIType)
			},
			{
				"Kingmaker.Code.UI.MVVM.LootContext+LootWindowMode, Code",
				typeof(LootWindowMode)
			},
			{
				"Kingmaker.Code.UI.MVVM.NetLobbyErrorHandler+NetLobbyErrorType, Code",
				typeof(NetLobbyErrorType)
			},
			{
				"Kingmaker.Code.UI.MVVM.UIVendorHelper+SaleOptions, Code",
				typeof(SaleOptions)
			},
			{
				"Kingmaker.UI.Sound.UISounds+ButtonSoundsEnum, Code",
				typeof(ButtonSoundsEnum)
			},
			{
				"Kingmaker.Code.UI.MVVM.WarningNotificationFormat, Code",
				typeof(WarningNotificationFormat)
			},
			{
				"Kingmaker.Code.UI.MVVM.WarningNotificationType, Code",
				typeof(WarningNotificationType)
			},
			{
				"Kingmaker.Code.UI.MVVM.TooltipElement, Code",
				typeof(TooltipElement)
			},
			{
				"Kingmaker.Code.View.UI.UIUtilities.UIUtilityUnit+UnitFractionViewMode, Code",
				typeof(UnitFractionViewMode)
			},
			{
				"Kingmaker.Code.View.UI.UIUtilities.UIUtilityUnit+PortraitCombatSize, Code",
				typeof(PortraitCombatSize)
			}
		};
		if (dictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<string, Type> item in dictionary)
		{
			if (!TypeLibrary.OldNames.ContainsKey(item.Key))
			{
				TypeLibrary.OldNames.Add(item.Key, item.Value);
			}
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void InitializeInRuntime()
	{
		Initialize();
	}
}
