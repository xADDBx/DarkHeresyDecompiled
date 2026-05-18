using Kingmaker.EntitySystem.Properties;

namespace Owlcat.BehaviourTrees;

public class SetCalculatorFromBlueprintNode : BehaviourTreeNode
{
	private readonly PropertyCalculatorBlueprintVariable m_Variable;

	private readonly PropertyCalculatorBlueprint m_Blueprint;

	public SetCalculatorFromBlueprintNode(PropertyCalculatorBlueprintVariable variable, PropertyCalculatorBlueprint blueprint)
	{
		m_Variable = variable;
		m_Blueprint = blueprint;
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_Variable.Value = m_Blueprint;
		if (m_Blueprint == null)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.Success;
	}
}
