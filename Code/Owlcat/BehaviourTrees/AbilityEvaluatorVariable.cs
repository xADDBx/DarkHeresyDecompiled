using System;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.AI;

namespace Owlcat.BehaviourTrees;

public class AbilityEvaluatorVariable : AbilityVariable
{
	private readonly AbilityEvaluator _evaluator;

	private AbilityData _lastEvaluatedValue;

	public override AbilityData Value
	{
		get
		{
			try
			{
				_lastEvaluatedValue = _evaluator.GetValue();
			}
			catch (FailToEvaluateException)
			{
				_lastEvaluatedValue = null;
			}
			return _lastEvaluatedValue;
		}
		set
		{
			throw new NotSupportedException("Can't set value of AbilityEvaluatorVariable");
		}
	}

	public AbilityEvaluatorVariable(AbilityEvaluator evaluator)
	{
		_evaluator = evaluator;
	}

	public override string ToString()
	{
		return string.Format("{0}: {1} ('{2}')", base.Key, _lastEvaluatedValue, (_evaluator == null) ? "null" : _evaluator.GetCaption());
	}
}
