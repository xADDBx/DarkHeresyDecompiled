using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Interfaces;

public interface ISelectionManager
{
	void SelectAll(IEnumerable<BaseUnitEntity> characters = null);

	void UpdateSelectedUnits();

	void MultiSelect(IEnumerable<UnitEntityView> views, bool canAddToSelection = true);

	bool IsSelected(AbstractUnitEntity unit);

	void SelectUnit(UnitEntityView unit, bool single = true, bool sendSelectionEvent = true, bool ask = true);

	void UnselectUnit(BaseUnitEntity data);

	void SwitchSelectionUnitInGroup(BaseUnitEntity data, bool canAddToSelection = true, bool force = false);

	BaseUnitEntity GetNearestSelectedUnit(Vector3 point);

	void ForceCreateMarks();

	void Stop();

	void Hold();
}
