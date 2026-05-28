namespace Owlcat.BehaviourTrees;

public class InverterNode : DecoratorNode
{
	protected override NodeResult Decorate(NodeResult result)
	{
		return result switch
		{
			NodeResult.Running => NodeResult.Running, 
			NodeResult.Success => NodeResult.Failure, 
			_ => NodeResult.Success, 
		};
	}
}
