using System;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[Serializable]
[TypeId("2dce40d34e7641a980af822fbe05fcc0")]
public abstract class BehaviourTreeNodeElement : BehaviourTreeElement
{
	public string Title;

	[HideInInspector]
	public Vector2 Position;

	[HideInInspector]
	public string ParentNodeName;

	[HideInInspector]
	public bool IsLogicalBranch;

	public abstract bool HasChild();

	public abstract bool HasChildren();

	public abstract BehaviourTreeNode CreateNode(Blackboard blackboard);

	public override string GetCaption()
	{
		return "Node: " + Title;
	}
}
public abstract class BehaviourTreeNodeElement<T> : BehaviourTreeNodeElement where T : BehaviourTreeNode
{
	protected abstract T CreateTypedNode(Blackboard blackboard);

	public override BehaviourTreeNode CreateNode(Blackboard blackboard)
	{
		return CreateTypedNode(blackboard);
	}

	private bool ImplementsInterface<TInterface>()
	{
		return typeof(T).GetInterface(typeof(TInterface).Name) != null;
	}

	public override bool HasChild()
	{
		return ImplementsInterface<IHasChildNode>();
	}

	public override bool HasChildren()
	{
		return ImplementsInterface<IHasChildrenNode>();
	}
}
