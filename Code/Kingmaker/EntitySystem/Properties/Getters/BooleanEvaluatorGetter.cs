using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("d28824856aaa4e0abb1de4b12c31bcf6")]
public class BooleanEvaluatorGetter : BoolPropertyGetter
{
	[ValidateNotNull]
	[SerializeReference]
	public BooleanEvaluator Evaluator;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return Evaluator?.GetCaption() ?? "<null>";
	}

	protected override bool GetBaseValue()
	{
		return Evaluator?.GetValue() ?? false;
	}
}
