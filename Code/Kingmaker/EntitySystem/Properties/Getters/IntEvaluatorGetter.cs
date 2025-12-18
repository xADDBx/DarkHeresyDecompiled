using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("bf6e947766614dc0a865fa254370ef5a")]
public class IntEvaluatorGetter : IntPropertyGetter
{
	[ValidateNotNull]
	[SerializeReference]
	public IntEvaluator Evaluator;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return Evaluator?.GetCaption() ?? "<null>";
	}

	protected override int GetBaseValue()
	{
		return Evaluator?.GetValue() ?? 0;
	}
}
