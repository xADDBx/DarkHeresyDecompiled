namespace Owlcat.BehaviourTrees;

public class BehaviourTreeIterator
{
	private readonly BehaviourTreeRuntimeContext m_RuntimeContext;

	private readonly BehaviourTree m_BehaviourTree;

	private readonly NodeVisitCursor m_Cursor;

	private bool IsInterruptedByBreakpoint(NodeVisitCursor cursor)
	{
		if (BehaviourTreeConfig.BreakpointHandlingType == BreakpointHandlingType.Immediate)
		{
			return cursor.IsInterruptedByBreakpoint;
		}
		return false;
	}

	public BehaviourTreeIterator(BehaviourTreeRuntimeContext runtimeContext, BehaviourTree behaviourTree)
	{
		m_RuntimeContext = runtimeContext;
		m_BehaviourTree = behaviourTree;
		m_Cursor = new NodeVisitCursor(runtimeContext);
	}

	public void Tick()
	{
		bool flag = !m_Cursor.IsWaitingForRunningNode && !IsInterruptedByBreakpoint(m_Cursor);
		if (m_Cursor.IsWaitingForRunningNode)
		{
			m_BehaviourTree.OnTraversalTreeChanged(m_Cursor.RunningNode.BehaviourTree);
			if (IsInterruptedByBreakpoint(m_Cursor))
			{
				m_Cursor.ProcessBreakpoint();
			}
			if (m_BehaviourTree.AbortController.TryAbortByConditionNodes(m_Cursor))
			{
				flag = true;
			}
			else if (m_Cursor.VisitRunningNode() == NodeResult.Running)
			{
				m_RuntimeContext.Profiler.AddRunningNodeTick(m_BehaviourTree);
			}
			else
			{
				VisitAllPossible(m_Cursor);
			}
		}
		if (IsInterruptedByBreakpoint(m_Cursor))
		{
			m_Cursor.ProcessBreakpoint();
			VisitAllPossible(m_Cursor);
		}
		if (flag)
		{
			ResetNodeForNewPass(m_BehaviourTree);
			m_Cursor.ResetForNewPass(m_BehaviourTree.Root);
			m_BehaviourTree.OnTraversalTreeChanged(m_BehaviourTree);
			VisitAllPossible(m_Cursor);
		}
	}

	private void VisitAllPossible(NodeVisitCursor cursor)
	{
		do
		{
			DoNextVisitStep(cursor);
		}
		while (!IsInterruptedByBreakpoint(cursor) && (cursor.Direction != NodeVisitCursor.VisitDirection.Backward || cursor.NextNode != null));
	}

	private void DoNextVisitStep(NodeVisitCursor cursor)
	{
		BehaviourTreeNode nextNode = cursor.NextNode;
		if (cursor.Direction == NodeVisitCursor.VisitDirection.Forward)
		{
			cursor.ForwardVisit();
		}
		else if (cursor.Direction == NodeVisitCursor.VisitDirection.Backward)
		{
			cursor.BackwardVisit();
		}
		if (nextNode is SubTreeNode subTreeNode)
		{
			m_BehaviourTree.OnTraversalTreeChanged(subTreeNode.RuntimeBridge.BehaviourTree);
		}
	}

	private void ResetNodeForNewPass(BehaviourTree behaviourTree)
	{
		foreach (BehaviourTreeNode node in behaviourTree.Nodes)
		{
			node.DebugInformation.ResetForNewPass();
		}
		foreach (SubTreeNode subTreeNode in behaviourTree.SubTreeNodes)
		{
			ResetNodeForNewPass(subTreeNode.RuntimeBridge.BehaviourTree);
		}
	}

	public void Abort()
	{
		m_BehaviourTree.AbortController.Abort(m_Cursor);
	}
}
