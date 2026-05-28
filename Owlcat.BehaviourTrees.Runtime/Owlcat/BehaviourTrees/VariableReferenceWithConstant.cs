namespace Owlcat.BehaviourTrees;

public abstract class VariableReferenceWithConstant : VariableReference
{
	public bool IsConstant;
}
public abstract class VariableReferenceWithConstant<T> : VariableReferenceWithConstant
{
	public T Constant;

	protected TVariable GetRuntimeVariable<TVariable>(Blackboard blackboard) where TVariable : BlackboardVariable<T>, new()
	{
		if (!IsConstant)
		{
			return blackboard.GetVariable<TVariable>(Id);
		}
		return new TVariable
		{
			Value = Constant
		};
	}

	public override string ToString()
	{
		if (!IsConstant)
		{
			return Id;
		}
		return Constant.ToString();
	}
}
