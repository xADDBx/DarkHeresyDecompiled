using System;
using System.Collections.Generic;
using Kingmaker.Code.EntitySystem.Entities;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.GameCommands;
using Kingmaker.Gameplay.Features.Channeling;
using Kingmaker.Gameplay.Features.Concentration;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects;
using Kingmaker.View.Mechanics;
using Kingmaker.View.Spawners;
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
				"Kingmaker.Framework.DetectiveSystem.DetectiveSystem+AddendumState, Code",
				typeof(AddendumState)
			},
			{
				"Kingmaker.Framework.DetectiveSystem.DetectiveSystem+CaseItemState, Code",
				typeof(CaseItemState)
			},
			{
				"Kingmaker.Framework.DetectiveSystem.DetectiveSystem+CaseState, Code",
				typeof(CaseState)
			},
			{
				"Kingmaker.Framework.DetectiveSystem.DetectiveSystem+ClueState, Code",
				typeof(ClueState)
			},
			{
				"Kingmaker.Framework.DetectiveSystem.DetectiveSystem+ConclusionState, Code",
				typeof(ConclusionState)
			},
			{
				"Kingmaker.Code.EntitySystem.Entities.ViewBasedPartRef`2, Code",
				typeof(EntityPartAbstractRef<, >)
			},
			{
				"Kingmaker.View.MapObjects.DroppedLoot+EntityData, Code",
				typeof(DroppedLootEntity)
			},
			{
				"Kingmaker.View.MapObjects.DynamicMapObjectView+EntityData, Code",
				typeof(DynamicMapObjectEntity)
			},
			{
				"Kingmaker.View.Spawners.EntityGroupView+UnitGroupData, Code",
				typeof(EntityGroupEntity)
			},
			{
				"Kingmaker.View.Mechanics.MapObjectGroupView+MechanicGroupData, Code",
				typeof(MapObjectGroupEntity)
			},
			{
				"Kingmaker.View.Mechanics.TrapObjectGroupView+MechanicGroupData, Code",
				typeof(TrapObjectGroupEntity)
			},
			{
				"Kingmaker.View.Spawners.UnitGroupView+UnitGroupData, Code",
				typeof(UnitGroupEntity)
			},
			{
				"Kingmaker.View.Spawners.CompanionSpawner+MyData, Code",
				typeof(CompanionSpawnerEntity)
			},
			{
				"Kingmaker.View.Spawners.UnitSpawnerBase+MyData, Code",
				typeof(UnitSpawnerEntity)
			},
			{
				"Kingmaker.View.Mechanics.AreaEffectGroupView+MechanicGroupData, Code",
				typeof(AreaEffectGroupEntity)
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
				"Kingmaker.GameCommands.SelectionEntry, Code",
				typeof(SelectionFeature)
			},
			{
				"Kingmaker.View.MapObjects.DroppedLoot+EntityPartBreathOfMoney, Code",
				typeof(EntityPartBreathOfMoney)
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
			},
			{
				"Kingmaker.Code.UI.MVVM.FeedbackPopupItem, Code",
				typeof(FeedbackPopupItem)
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
