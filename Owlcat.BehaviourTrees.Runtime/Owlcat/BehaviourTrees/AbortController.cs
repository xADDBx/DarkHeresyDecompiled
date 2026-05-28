using System;
using System.Collections.Generic;

namespace Owlcat.BehaviourTrees;

public class AbortController
{
	private readonly BehaviourTree m_BehaviourTree;

	private readonly List<ConditionNode> m_AbortNodes = new List<ConditionNode>();

	private readonly Dictionary<(ConditionNode, TaskNode), NodeRelation> m_NodeRelations = new Dictionary<(ConditionNode, TaskNode), NodeRelation>();

	internal IReadOnlyList<ConditionNode> AbortNodes => m_AbortNodes;

	public AbortController(BehaviourTree behaviourTree)
	{
		m_BehaviourTree = behaviourTree;
	}

	public void Initialize()
	{
		FillDepthsRecursive(m_BehaviourTree.Root, 0);
		foreach (BehaviourTreeNode node in m_BehaviourTree.Nodes)
		{
			if (!(node is ConditionNode conditionNode))
			{
				if (node is SubTreeNode subTreeNode)
				{
					m_AbortNodes.AddRange(subTreeNode.RuntimeBridge.BehaviourTree.AbortController.AbortNodes);
				}
			}
			else if (conditionNode.AbortType != 0)
			{
				m_AbortNodes.Add(conditionNode);
			}
		}
	}

	private void FillDepthsRecursive(BehaviourTreeNode node, int depth)
	{
		node.Depth = depth;
		if (node is IHasChildNode hasChildNode)
		{
			FillDepthsRecursive(hasChildNode.Child, depth + 1);
			return;
		}
		if (node is IHasChildrenNode hasChildrenNode)
		{
			{
				foreach (BehaviourTreeNode child in hasChildrenNode.Children)
				{
					FillDepthsRecursive(child, depth + 1);
				}
				return;
			}
		}
		if (node is SubTreeNode subTreeNode)
		{
			FillDepthsRecursive(subTreeNode.RuntimeBridge.BehaviourTree.Root, depth + 1);
		}
	}

	public bool TryAbortByConditionNodes(NodeVisitCursor cursor)
	{
		bool num = HasAbort(cursor.RunningNode);
		if (num)
		{
			cursor.AbortRunningNode();
		}
		return num;
	}

	private bool HasAbort(TaskNode runningNode)
	{
		foreach (ConditionNode abortNode in m_AbortNodes)
		{
			if (!m_NodeRelations.TryGetValue((abortNode, runningNode), out var value))
			{
				value = GetNodeRelation(abortNode, runningNode);
				m_NodeRelations[(abortNode, runningNode)] = value;
			}
			if (((abortNode.AbortType == AbortType.Self && value == NodeRelation.Self) || (abortNode.AbortType == AbortType.LowerPriority && value == NodeRelation.LowerPriority) || (abortNode.AbortType == AbortType.Both && value != NodeRelation.Unrelated)) && abortNode.IsPassed())
			{
				return true;
			}
		}
		return false;
	}

	private NodeRelation GetNodeRelation(BehaviourTreeNode abortNode, BehaviourTreeNode runningNode)
	{
		if (abortNode == runningNode)
		{
			throw new Exception($"Abort node '{abortNode}' is the same as running node");
		}
		var (behaviourTreeNode, item, item2) = GetNearestCommonParent(abortNode, runningNode);
		if (behaviourTreeNode == abortNode)
		{
			return NodeRelation.Self;
		}
		if (behaviourTreeNode == runningNode)
		{
			throw new Exception($"Running '{runningNode}' node should be a leaf, but it is parent of abort node '{abortNode}'");
		}
		if (behaviourTreeNode is CompositeNode compositeNode)
		{
			int num = compositeNode.Children.IndexOf(item);
			int num2 = compositeNode.Children.IndexOf(item2);
			if (num < num2)
			{
				return NodeRelation.LowerPriority;
			}
		}
		return NodeRelation.Unrelated;
	}

	private (BehaviourTreeNode parent, BehaviourTreeNode branch1, BehaviourTreeNode branch2) GetNearestCommonParent(BehaviourTreeNode node1, BehaviourTreeNode node2)
	{
		while (node1.Depth > node2.Depth)
		{
			node1 = node1.Parent;
		}
		while (node2.Depth > node1.Depth)
		{
			node2 = node2.Parent;
		}
		BehaviourTreeNode item = null;
		BehaviourTreeNode item2 = null;
		while (node1 != node2)
		{
			item = node1;
			item2 = node2;
			node1 = node1.Parent;
			node2 = node2.Parent;
		}
		if (node1 == null)
		{
			throw new Exception($"No common parent found for '{node1}' and '{node2}'");
		}
		return (parent: node1, branch1: item, branch2: item2);
	}

	public void Abort(NodeVisitCursor cursor)
	{
		if (cursor.IsWaitingForRunningNode)
		{
			cursor.AbortRunningNode();
		}
	}
}
