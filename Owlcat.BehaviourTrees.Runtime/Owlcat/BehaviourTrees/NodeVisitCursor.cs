using System;

namespace Owlcat.BehaviourTrees;

public class NodeVisitCursor
{
	public enum VisitDirection
	{
		Forward,
		Backward
	}

	private readonly BehaviourTreeRuntimeContext m_RuntimeContext;

	private NodeResult m_LastResult;

	public VisitDirection Direction { get; private set; }

	public BehaviourTreeNode NextNode { get; private set; }

	public TaskNode RunningNode { get; private set; }

	public bool IsInterruptedByBreakpoint { get; private set; }

	public bool IsWaitingForRunningNode => RunningNode != null;

	public NodeVisitCursor(BehaviourTreeRuntimeContext runtimeContext)
	{
		m_RuntimeContext = runtimeContext;
	}

	public void ResetForNewPass(BehaviourTreeNode startNode)
	{
		NextNode = startNode;
		Direction = VisitDirection.Forward;
		RunningNode = null;
	}

	public void ProcessBreakpoint()
	{
		IsInterruptedByBreakpoint = false;
	}

	public NodeResult VisitRunningNode()
	{
		DebugBreakIfRequired(RunningNode, NodeBreakpointSetting.EveryRunningTick);
		try
		{
			m_RuntimeContext.Profiler.BeginVisit(RunningNode);
			m_LastResult = RunningNode.RunningVisit();
			m_RuntimeContext.Profiler.EndVisit(RunningNode);
		}
		catch (Exception exception)
		{
			m_RuntimeContext.NodeErrorsRuntimeController.HandleException(RunningNode, exception);
			m_LastResult = NodeResult.Failure;
			throw;
		}
		finally
		{
			if (m_LastResult != 0)
			{
				RunningNode.DebugInformation.Result = m_LastResult;
				Direction = VisitDirection.Backward;
				NextNode = RunningNode.Parent;
				RunningNode = null;
			}
		}
		return m_LastResult;
	}

	public void AbortRunningNode()
	{
		RunningNode.Abort();
		RunningNode = null;
	}

	private void DebugBreakIfRequired(BehaviourTreeNode node, NodeBreakpointSetting breakpointSetting)
	{
	}

	public void ForwardVisit()
	{
		DebugBreakIfRequired(NextNode, NodeBreakpointSetting.ForwardVisit);
		NodeVisitResult nodeVisitResult;
		try
		{
			m_RuntimeContext.Profiler.BeginVisit(NextNode);
			nodeVisitResult = NextNode.ForwardVisit();
			m_RuntimeContext.Profiler.EndVisit(NextNode);
		}
		catch (Exception exception)
		{
			m_RuntimeContext.NodeErrorsRuntimeController.HandleException(NextNode, exception);
			throw;
		}
		if (nodeVisitResult.ForwardNode == null && nodeVisitResult.Result == NodeResult.Running)
		{
			if (!(NextNode is TaskNode runningNode))
			{
				throw new Exception($"Only TaskNode can return Running result. {NextNode} is not a TaskNode.");
			}
			RunningNode = runningNode;
		}
		HandleVisitResult(nodeVisitResult);
	}

	public void BackwardVisit()
	{
		DebugBreakIfRequired(NextNode, NodeBreakpointSetting.BackwardVisit);
		NodeVisitResult visitResult;
		try
		{
			visitResult = NextNode.BackwardVisit(m_LastResult);
		}
		catch (Exception exception)
		{
			m_RuntimeContext.NodeErrorsRuntimeController.HandleException(NextNode, exception);
			throw;
		}
		HandleVisitResult(visitResult);
	}

	private void HandleVisitResult(NodeVisitResult visitResult)
	{
		if (visitResult.ForwardNode == null)
		{
			m_LastResult = visitResult.Result;
			Direction = VisitDirection.Backward;
			NextNode.DebugInformation.Result = visitResult.Result;
			NextNode = NextNode.Parent;
		}
		else
		{
			Direction = VisitDirection.Forward;
			NextNode = visitResult.ForwardNode;
		}
	}

	public override string ToString()
	{
		return $"{Direction} {NextNode} {RunningNode}";
	}
}
