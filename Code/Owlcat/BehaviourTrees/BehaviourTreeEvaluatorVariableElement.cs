using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[TypeId("489938ab623f44c789bd9b08f7e3abfb")]
public abstract class BehaviourTreeEvaluatorVariableElement : BehaviourTreeVariableElement
{
	protected abstract Evaluator GetEvaluator();

	public override string GetCaption()
	{
		return GetEvaluator()?.GetCaption() ?? "<None>";
	}
}
[TypeId("489938ab623f44c789bd9b08f7e3abfb")]
public abstract class BehaviourTreeEvaluatorVariableElement<TEvaluator> : BehaviourTreeEvaluatorVariableElement where TEvaluator : Evaluator
{
	[SerializeReference]
	public TEvaluator Evaluator;

	protected override Evaluator GetEvaluator()
	{
		return Evaluator;
	}
}
