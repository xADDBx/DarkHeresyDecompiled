using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public class IterateOverListNode<T> : BehaviourTreeNode, IHasChildNode
{
	private readonly BlackboardListVariable<T> m_List;

	private readonly BlackboardVariable<T> m_Current;

	private readonly IterationExitCondition m_ExitCondition;

	private readonly bool m_RandomOrder;

	private readonly int m_NumberOfIterations;

	private List<T> m_CachedList;

	private List<T> m_ShuffledList;

	private int m_CurrentIndex;

	private int m_ActualNumberOfIterations;

	private List<T> ActualList
	{
		get
		{
			if (!m_RandomOrder)
			{
				return m_CachedList;
			}
			return m_ShuffledList;
		}
	}

	public BehaviourTreeNode Child { get; set; }

	public IterateOverListNode(BlackboardListVariable<T> list, BlackboardVariable<T> current, IterationExitCondition exitCondition, bool randomOrder, int numberOfIterations)
	{
		m_List = list;
		m_Current = current;
		m_ExitCondition = exitCondition;
		m_RandomOrder = randomOrder;
		m_NumberOfIterations = numberOfIterations;
	}

	public override NodeVisitResult ForwardVisit()
	{
		m_CurrentIndex = 0;
		m_CachedList = new List<T>();
		m_CachedList.AddRange(m_List.Value);
		m_ActualNumberOfIterations = ((m_NumberOfIterations > 0 && m_NumberOfIterations < m_List.Value.Count) ? m_NumberOfIterations : m_List.Value.Count);
		if (m_RandomOrder)
		{
			m_ShuffledList = new List<T>();
			m_ShuffledList.AddRange(m_CachedList);
			m_ShuffledList.Shuffle(PFStatefulRandom.Mechanics);
		}
		return ProcessNextElement();
	}

	public override NodeVisitResult BackwardVisit(NodeResult result)
	{
		if (result == NodeResult.Running)
		{
			return NodeVisitResult.Running;
		}
		m_CurrentIndex++;
		if (m_CurrentIndex >= m_ActualNumberOfIterations)
		{
			return NodeVisitResult.GoBackward(result);
		}
		switch (m_ExitCondition)
		{
		case IterationExitCondition.WhenAllProcessed:
			return ProcessNextElement();
		case IterationExitCondition.OnFirstSuccess:
			if (result != NodeResult.Success)
			{
				return ProcessNextElement();
			}
			return NodeVisitResult.Success;
		case IterationExitCondition.OnFirstFailure:
			if (result != NodeResult.Failure)
			{
				return ProcessNextElement();
			}
			return NodeVisitResult.Failure;
		default:
			throw new Exception($"Unknown result: {result}");
		}
	}

	private NodeVisitResult ProcessNextElement()
	{
		if (!m_CachedList.SequenceEqual(m_List.Value))
		{
			throw new InvalidOperationException("Collection was modified");
		}
		m_Current.Value = ActualList[m_CurrentIndex];
		if (Child.IsDisabled)
		{
			return NodeVisitResult.Failure;
		}
		return NodeVisitResult.GoForward(Child);
	}
}
