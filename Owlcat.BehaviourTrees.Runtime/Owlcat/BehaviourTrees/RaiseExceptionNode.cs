using System;

namespace Owlcat.BehaviourTrees;

public class RaiseExceptionNode : BehaviourTreeNode
{
	private readonly string m_Message;

	public RaiseExceptionNode(string message)
	{
		m_Message = message;
	}

	public override NodeVisitResult ForwardVisit()
	{
		throw new Exception(m_Message);
	}
}
