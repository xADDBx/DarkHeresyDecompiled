namespace Owlcat.BehaviourTrees;

public class IsSetAbilityNode : ConditionNode
{
	private readonly AbilityVariable _ability;

	private readonly bool _invert;

	public IsSetAbilityNode(AbortType abortType, AbilityVariable ability, bool invert)
		: base(abortType)
	{
		bool invert2 = invert;
		_ability = ability;
		_invert = invert2;
	}

	public override bool IsPassed()
	{
		return (_ability.Value != null) ^ _invert;
	}
}
