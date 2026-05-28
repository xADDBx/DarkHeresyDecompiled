using System;
using System.Collections.Generic;

namespace Owlcat.BehaviourTrees;

public class BehaviourTree
{
	private readonly BehaviourTreeRuntimeContext m_RuntimeContext;

	private readonly BehaviourTreeIterator m_Iterator;

	public RootNode Root { get; set; }

	public List<BehaviourTreeNode> Nodes { get; } = new List<BehaviourTreeNode>();


	public Blackboard Blackboard { get; }

	public List<SubTreeNode> SubTreeNodes { get; } = new List<SubTreeNode>();


	public AbortController AbortController { get; }

	public event Action Ticked;

	public event Action<BehaviourTree> TraversalTreeChange;

	public BehaviourTree(BehaviourTreeRuntimeContext runtimeContext, Blackboard blackboard)
	{
		m_RuntimeContext = runtimeContext;
		Blackboard = blackboard;
		m_Iterator = new BehaviourTreeIterator(runtimeContext, this);
		AbortController = new AbortController(this);
	}

	public void Tick()
	{
		m_RuntimeContext.Profiler.BeginTick(this);
		m_Iterator.Tick();
		m_RuntimeContext.Profiler.EndTick(this);
		this.Ticked?.Invoke();
	}

	public void OnTraversalTreeChanged(BehaviourTree behaviourTree)
	{
		this.TraversalTreeChange?.Invoke(behaviourTree);
	}

	public void Abort()
	{
		m_Iterator.Abort();
	}
}
