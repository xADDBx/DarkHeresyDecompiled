using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/Prevent Snap To Grid")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("43929aeb3a512b342a36fb375edcbb35")]
public class PreventSnapToGrid : GameAction
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override string GetCaption()
	{
		return "Prevent Snap To Grid";
	}

	protected override void RunAction()
	{
		AbstractUnitEntity value = Unit.GetValue();
		if (value == null)
		{
			PFLog.Default.Error("PreventSnapToGrid: Unit is null!");
			return;
		}
		value.GetOrCreate<PartPreventSnapToGrid>().ShouldPreventSnapToGrid = true;
		PFLog.Default.Log("PreventSnapToGrid: Set flag to prevent SnapToGrid for " + value.View.name);
	}
}
