namespace Owlcat.BehaviourTrees;

public static class VariableReferenceExtensions
{
	public static T GetOptionalRuntimeVariable<T>(this VariableReference variableReference, Blackboard blackboard) where T : BlackboardVariable
	{
		if (!string.IsNullOrEmpty(variableReference?.Id))
		{
			return blackboard.GetVariable<T>(variableReference.Id);
		}
		return null;
	}
}
