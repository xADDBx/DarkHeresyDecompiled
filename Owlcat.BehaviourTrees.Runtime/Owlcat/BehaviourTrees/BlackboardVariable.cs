namespace Owlcat.BehaviourTrees;

public abstract class BlackboardVariable
{
	public string Key { get; set; }
}
public abstract class BlackboardVariable<T> : BlackboardVariable
{
	public virtual T Value { get; set; }

	public override string ToString()
	{
		return $"{base.Key}: {Value}";
	}
}
