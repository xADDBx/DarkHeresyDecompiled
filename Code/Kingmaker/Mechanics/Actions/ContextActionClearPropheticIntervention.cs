using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Mechanics.Actions;

[TypeId("4017ffe5c10f497892af948b794a68b6")]
public class ContextActionClearPropheticIntervention : ContextAction
{
	protected override void RunAction()
	{
		((base.Context.Caster as UnitEntity)?.Parts.GetOptional<UnitPartPropheticIntervention>())?.Entries.Clear();
	}

	public override string GetCaption()
	{
		return "Activate Prophetic Intervention on target - resurrect it if it died last turn";
	}
}
