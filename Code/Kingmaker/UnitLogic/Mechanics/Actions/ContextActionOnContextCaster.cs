using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.ContextContract;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("5f950c181b3157a4486fcd36b702b702")]
[ReadsContext(new ContextField[] { ContextField.Caster })]
[SetsContext(ContextField.Target, Availability.Definitely)]
public class ContextActionOnContextCaster : ContextAction
{
	public ActionList Actions;

	public override string GetCaption()
	{
		return "Run actions on Caster of context";
	}

	protected override void RunAction()
	{
		MechanicEntity caster = base.Context.Caster;
		if (caster != null)
		{
			using (base.Context.PushTarget(caster))
			{
				Actions.Run();
			}
		}
	}
}
