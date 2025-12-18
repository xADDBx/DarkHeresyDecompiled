using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Facades;

public static class SelectionManagerFacade
{
	private static ISelectionManager s_SelectionManager;

	public static ISelectionManager SelectionManager => s_SelectionManager ?? (s_SelectionManager = Game.Instance.RootUIContext.SelectionManager);

	public static void SelectAll(IEnumerable<BaseUnitEntity> characters = null)
	{
		SelectionManager?.SelectAll(characters);
	}

	public static void UpdateSelectedUnits()
	{
		SelectionManager?.UpdateSelectedUnits();
	}

	public static void MultiSelect(IEnumerable<UnitEntityView> views, bool canAddToSelection = true)
	{
		SelectionManager?.MultiSelect(views, canAddToSelection);
	}

	public static bool IsSelected(AbstractUnitEntity unit)
	{
		return SelectionManager?.IsSelected(unit) ?? false;
	}

	public static void SelectUnit(UnitEntityView unit, bool single = true, bool sendSelectionEvent = true, bool ask = true)
	{
		SelectionManager?.SelectUnit(unit, single, sendSelectionEvent, ask);
	}

	public static void UnselectUnit(BaseUnitEntity data)
	{
		SelectionManager?.UnselectUnit(data);
	}

	public static void SwitchSelectionUnitInGroup(BaseUnitEntity data, bool canAddToSelection = true, bool force = false)
	{
		SelectionManager?.SwitchSelectionUnitInGroup(data, canAddToSelection, force);
	}

	public static void ForceCreateMarks()
	{
		SelectionManager?.ForceCreateMarks();
	}

	public static void Stop()
	{
		SelectionManager?.Stop();
	}

	public static void Hold()
	{
		SelectionManager?.Hold();
	}

	public static BaseUnitEntity GetNearestSelectedUnit(Vector3 point)
	{
		return SelectionManager?.GetNearestSelectedUnit(point);
	}
}
