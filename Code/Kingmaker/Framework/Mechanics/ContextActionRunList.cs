using System;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.Mechanics;

[Serializable]
[TypeId("258db95777fd4bac98d336956530c17e")]
public sealed class ContextActionRunList : ContextAction
{
	[ValidateNotNull]
	public BpRef<ContextActionsList> List;

	public override string GetCaption()
	{
		return $"Run actions list {List}";
	}

	protected override void RunAction()
	{
		List.Blueprint.Run();
	}
}
