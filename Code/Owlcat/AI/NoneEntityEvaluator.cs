using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[ComponentName("AI/NoneEntityEvaluator")]
[TypeId("d2edffbafb7042abac9cc5c4ce84998e")]
public class NoneEntityEvaluator : EntityEvaluator
{
	public override string GetCaption()
	{
		return "None Entity";
	}

	protected override Entity GetValueInternal()
	{
		return null;
	}
}
