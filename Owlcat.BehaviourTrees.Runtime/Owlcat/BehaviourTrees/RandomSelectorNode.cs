using System;
using System.Collections.Generic;

namespace Owlcat.BehaviourTrees;

public class RandomSelectorNode : CompositeNode
{
	private readonly List<int> m_Weights;

	private readonly int[] m_Order;

	private readonly List<(int Index, int Weight)> m_IndexedWeights = new List<(int, int)>();

	private int m_NextChildIndex;

	public int[] Order => m_Order;

	public RandomSelectorNode(List<int> weights)
	{
		m_Weights = new List<int>(weights);
		m_Order = new int[weights.Count];
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_NextChildIndex = 0;
		ShuffleChildren();
		return TryVisitNextChild();
	}

	public override NodeVisitResult BackwardVisit(NodeResult result)
	{
		return result switch
		{
			NodeResult.Success => NodeVisitResult.Success, 
			NodeResult.Running => NodeVisitResult.Running, 
			NodeResult.Failure => TryVisitNextChild(), 
			_ => throw new Exception($"Unknown result: {result}"), 
		};
	}

	private void ShuffleChildren()
	{
		m_IndexedWeights.Clear();
		int total = 0;
		for (int i = 0; i < m_Weights.Count; i++)
		{
			m_IndexedWeights.Add((i, m_Weights[i]));
			total += m_Weights[i];
		}
		for (int j = 0; j < m_Order.Length; j++)
		{
			m_Order[j] = SelectNextIndex(m_IndexedWeights, ref total);
		}
	}

	private int SelectNextIndex(List<(int Index, int Weight)> indexedWeights, ref int total)
	{
		int num = BehaviourTreeRandomProvider.Range(0, total);
		int i;
		for (i = 0; i < indexedWeights.Count - 1; i++)
		{
			int item = indexedWeights[i].Weight;
			if (num < item)
			{
				break;
			}
			num -= item;
		}
		total -= indexedWeights[i].Weight;
		int item2 = indexedWeights[i].Index;
		indexedWeights.RemoveAt(i);
		return item2;
	}

	private NodeVisitResult TryVisitNextChild()
	{
		while (m_NextChildIndex < base.Children.Count)
		{
			BehaviourTreeNode behaviourTreeNode = base.Children[m_Order[m_NextChildIndex]];
			m_NextChildIndex++;
			if (!behaviourTreeNode.IsDisabled)
			{
				return NodeVisitResult.GoForward(behaviourTreeNode);
			}
		}
		return NodeVisitResult.Failure;
	}
}
