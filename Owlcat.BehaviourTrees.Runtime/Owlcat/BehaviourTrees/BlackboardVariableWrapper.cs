namespace Owlcat.BehaviourTrees;

public class BlackboardVariableWrapper
{
	public readonly string Key;

	public readonly BlackboardVariable Variable;

	public BlackboardVariableWrapper(string key, BlackboardVariable variable)
	{
		Key = key;
		Variable = variable;
	}
}
