using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Framework.ContextContract;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("c93350feff15464a95621a9ff2eb4a69")]
[ReadsContext(new ContextField[]
{
	ContextField.Caster,
	ContextField.Owner,
	ContextField.Fact
})]
public class AddBuffRemovedContextActions : UnitFactComponentDelegate, IBuffRemoved
{
	public ActionList Actions;

	public void OnRemoved()
	{
		base.Fact.RunActionInContext(Actions);
	}
}
