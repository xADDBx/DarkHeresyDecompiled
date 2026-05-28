using System;

namespace Owlcat.BehaviourTrees;

public class NodeVisitResult
{
	public NodeResult Result { get; private set; }

	public BehaviourTreeNode ForwardNode { get; private set; }

	public static NodeVisitResult Success { get; } = new NodeVisitResult
	{
		Result = NodeResult.Success
	};


	public static NodeVisitResult Failure { get; } = new NodeVisitResult
	{
		Result = NodeResult.Failure
	};


	public static NodeVisitResult Running { get; } = new NodeVisitResult
	{
		Result = NodeResult.Running
	};


	private static NodeVisitResult Forward { get; } = new NodeVisitResult();


	private NodeVisitResult()
	{
	}

	public static NodeVisitResult GoForward(BehaviourTreeNode childNode)
	{
		Forward.ForwardNode = childNode;
		return Forward;
	}

	public static NodeVisitResult GoBackward(NodeResult result)
	{
		return result switch
		{
			NodeResult.Success => Success, 
			NodeResult.Failure => Failure, 
			NodeResult.Running => Running, 
			_ => throw new Exception($"Unknown result: {result}"), 
		};
	}

	public override string ToString()
	{
		if (ForwardNode != null)
		{
			return $"Forward: {ForwardNode}";
		}
		return $"Backward: {Result}";
	}
}
