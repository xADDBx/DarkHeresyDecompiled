using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Mechanics.Actions;

[Serializable]
[TypeId("349edff1d36a4e919a09a5d034008676")]
public class ContextActionOnContextOwner : ContextAction
{
	public ActionList Actions;

	public override string GetCaption()
	{
		return "Run actions on Owner of context";
	}

	protected override void RunAction()
	{
		MechanicEntity maybeOwner = base.Context.MaybeOwner;
		if (maybeOwner != null)
		{
			using (base.Context.SetScope(maybeOwner, null))
			{
				Actions.Run();
			}
		}
	}
}
