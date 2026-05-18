using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties;

namespace Owlcat.BehaviourTrees;

public class PropertyCalculatorBlueprintEvaluatorVariable : PropertyCalculatorBlueprintVariable
{
	private readonly PropertyCalculatorBlueprintEvaluator m_Evaluator;

	private PropertyCalculatorBlueprint m_LastEvaluatedValue;

	public override PropertyCalculatorBlueprint Value
	{
		get
		{
			try
			{
				m_LastEvaluatedValue = m_Evaluator.GetValue();
			}
			catch (FailToEvaluateException)
			{
				m_LastEvaluatedValue = null;
			}
			return m_LastEvaluatedValue;
		}
		set
		{
			throw new Exception("Can't set value of PropertyCalculatorBlueprintEvaluatorVariable");
		}
	}

	public PropertyCalculatorBlueprintEvaluatorVariable(PropertyCalculatorBlueprintEvaluator evaluator)
	{
		m_Evaluator = evaluator;
	}

	public override string ToString()
	{
		return string.Format("{0}: {1} ('{2}')", base.Key, m_LastEvaluatedValue, (m_Evaluator == null) ? "null" : m_Evaluator.GetCaption());
	}
}
