using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.AI;

[ComponentName("AI/EntityIsNone")]
[TypeId("7c3c811189314c309029389fc514a45a")]
public class EntityIsNoneCondition : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public EntityEvaluator EntityEvaluator;

	protected override string GetConditionCaption()
	{
		return $"{EntityEvaluator} is None";
	}

	protected override bool CheckCondition()
	{
		return EntityEvaluator.GetValue() == null;
	}
}
