using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[Serializable]
public class BehaviourTreeSerializableData
{
	private IBehaviourTreeOwnerAsset m_OwnerAsset;

	[SerializeField]
	[SerializeReference]
	private List<BehaviourTreeNodeElement> m_Nodes = new List<BehaviourTreeNodeElement>();

	[SerializeField]
	[SerializeReference]
	private List<BehaviourTreeVariableElement> m_Variables = new List<BehaviourTreeVariableElement>();

	[SerializeField]
	[HideInInspector]
	private List<BehaviourTreeStickyNoteElement> m_StickyNotes = new List<BehaviourTreeStickyNoteElement>();

	public string Title => m_OwnerAsset?.GetTitle() ?? "<untitled>";

	public string AssetGuid => m_OwnerAsset.AssetGuid;

	public IBehaviourTreeOwnerAsset OwnerAsset => m_OwnerAsset;

	public IReadOnlyList<BehaviourTreeNodeElement> Nodes => m_Nodes;

	public IReadOnlyList<BehaviourTreeVariableElement> Variables => m_Variables;

	public IReadOnlyList<BehaviourTreeStickyNoteElement> StickyNotes => m_StickyNotes;

	public void SetOwnerAsset(IBehaviourTreeOwnerAsset ownerAsset)
	{
		m_OwnerAsset = ownerAsset;
		foreach (BehaviourTreeNodeElement node in Nodes)
		{
			node.BehaviourTree = this;
		}
		foreach (BehaviourTreeVariableElement variable in Variables)
		{
			variable.BehaviourTree = this;
		}
	}

	public void EnsureNodeGraphComponentExists()
	{
	}

	public void EnsureVariablesComponentExists()
	{
	}

	public RootNodeElement EnsureRootNodeExists()
	{
		RootNodeElement rootNodeElement = Nodes.FirstOrDefault((BehaviourTreeNodeElement element) => element is RootNodeElement) as RootNodeElement;
		if (rootNodeElement == null)
		{
			rootNodeElement = BehaviourTreeElement.CreateInstance<RootNodeElement>();
			rootNodeElement.Title = "Root";
			AddNode(rootNodeElement);
		}
		return rootNodeElement;
	}

	public bool AddNode(BehaviourTreeNodeElement node)
	{
		m_Nodes.Add(node);
		node.BehaviourTree = this;
		return true;
	}

	public bool RemoveNode(BehaviourTreeNodeElement node)
	{
		m_Nodes.Remove(node);
		node.BehaviourTree = this;
		return true;
	}

	public bool AddVariable(BehaviourTreeVariableElement variable)
	{
		m_Variables.Add(variable);
		variable.BehaviourTree = this;
		return true;
	}

	public bool RemoveVariable(BehaviourTreeVariableElement variable)
	{
		m_Variables.Remove(variable);
		return true;
	}

	public bool AddStickyNote(BehaviourTreeStickyNoteElement stickyNote)
	{
		m_StickyNotes.Add(stickyNote);
		stickyNote.BehaviourTree = this;
		return true;
	}

	public bool RemoveStickyNote(BehaviourTreeStickyNoteElement stickyNote)
	{
		m_StickyNotes.Remove(stickyNote);
		stickyNote.BehaviourTree = this;
		return true;
	}

	public static implicit operator bool(BehaviourTreeSerializableData o)
	{
		return o != null;
	}
}
