using System;
using System.Collections.Generic;
using System.Linq;

namespace Owlcat.BehaviourTrees;

public class BehaviourTreeRuntimeToBlueprintBridge
{
	private readonly BiDictionary<BehaviourTreeNode, BehaviourTreeNodeElement> m_NodeToElementMap = new BiDictionary<BehaviourTreeNode, BehaviourTreeNodeElement>();

	private readonly Dictionary<BlackboardVariable, BehaviourTreeVariableElement> m_VariableToElementMap = new Dictionary<BlackboardVariable, BehaviourTreeVariableElement>();

	public BehaviourTreeRuntimeContext RuntimeContext { get; }

	public BehaviourTreeSerializableData BehaviourTreeData { get; }

	public BehaviourTree BehaviourTree { get; private set; }

	public BehaviourTreeRuntimeToBlueprintBridge(BehaviourTreeRuntimeContext runtimeContext, BehaviourTreeSerializableData behaviourTreeDataData)
	{
		RuntimeContext = runtimeContext;
		BehaviourTreeData = behaviourTreeDataData;
		Blackboard blackboard = BuildBlackboard(BehaviourTreeData);
		BuildTree(blackboard);
	}

	public BehaviourTreeRuntimeToBlueprintBridge(BehaviourTreeRuntimeContext runtimeContext, ParameterizedBehaviourTree parameterizedBehaviourTree)
	{
		RuntimeContext = runtimeContext;
		BehaviourTreeData = parameterizedBehaviourTree.BehaviourTree.Get();
		Blackboard blackboard = BuildParameterizedBlackboard(parameterizedBehaviourTree);
		BuildTree(blackboard);
	}

	private BehaviourTreeRuntimeToBlueprintBridge(BehaviourTreeRuntimeContext runtimeContext, BehaviourTreeSerializableData behaviourTreeDataData, Blackboard inheritedBlackboard)
	{
		RuntimeContext = runtimeContext;
		BehaviourTreeData = behaviourTreeDataData;
		Blackboard blackboard = BuildInheritedBlackboard(BehaviourTreeData, inheritedBlackboard);
		BuildTree(blackboard);
	}

	private BehaviourTreeRuntimeToBlueprintBridge(BehaviourTreeRuntimeContext runtimeContext, BehaviourTreeSerializableData behaviourTreeDataData, Blackboard inheritedBlackboard, VariableMappingContainer mappingContainer)
	{
		RuntimeContext = runtimeContext;
		BehaviourTreeData = behaviourTreeDataData;
		Blackboard blackboard = BuildMappedBlackboard(BehaviourTreeData, inheritedBlackboard, mappingContainer);
		BuildTree(blackboard);
	}

	public BehaviourTreeNode GetNode(BehaviourTreeNodeElement nodeElement)
	{
		return m_NodeToElementMap.GetKey(nodeElement);
	}

	public bool TryGetNode(BehaviourTreeNodeElement nodeElement, out BehaviourTreeNode node)
	{
		return m_NodeToElementMap.TryGetKey(nodeElement, out node);
	}

	public BehaviourTreeNode GetNodeFromAllHierarchy(BehaviourTreeNodeElement nodeElement)
	{
		if (TryGetNodeFromAllHierarchy(nodeElement, out var node))
		{
			return node;
		}
		throw new Exception("Can't get node for element: '" + nodeElement.Title + "'");
	}

	public bool TryGetNodeFromAllHierarchy(BehaviourTreeNodeElement nodeElement, out BehaviourTreeNode node)
	{
		if (TryGetNode(nodeElement, out node))
		{
			return true;
		}
		foreach (SubTreeNode subTreeNode in BehaviourTree.SubTreeNodes)
		{
			if (subTreeNode.RuntimeBridge.TryGetNodeFromAllHierarchy(nodeElement, out node))
			{
				return true;
			}
		}
		return false;
	}

	public BehaviourTreeNodeElement GetNodeElement(BehaviourTreeNode node)
	{
		return m_NodeToElementMap.GetValue(node);
	}

	public bool TryGetNodeElement(BehaviourTreeNode node, out BehaviourTreeNodeElement nodeElement)
	{
		return m_NodeToElementMap.TryGetValue(node, out nodeElement);
	}

	public BehaviourTreeNodeElement GetNodeElementFromAllHierarchy(BehaviourTreeNode node)
	{
		if (TryGetNodeElementFromAllHierarchy(node, out var nodeElement))
		{
			return nodeElement;
		}
		throw new Exception("Can't get node element for node: '" + node.Title + "'");
	}

	public bool TryGetNodeElementFromAllHierarchy(BehaviourTreeNode node, out BehaviourTreeNodeElement nodeElement)
	{
		if (TryGetNodeElement(node, out nodeElement))
		{
			return true;
		}
		foreach (SubTreeNode subTreeNode in BehaviourTree.SubTreeNodes)
		{
			if (subTreeNode.RuntimeBridge.TryGetNodeElementFromAllHierarchy(node, out nodeElement))
			{
				return true;
			}
		}
		return false;
	}

	private void BuildTree(Blackboard blackboard)
	{
		BehaviourTree = new BehaviourTree(RuntimeContext, blackboard);
		Dictionary<string, BehaviourTreeNodeElement> namesMap = BuildNodesList(blackboard, BehaviourTree);
		InitializeRootNode();
		BuildHierarchy(namesMap);
		OrderChildrenNodes();
		BuildSubTrees(blackboard);
		BehaviourTree.AbortController.Initialize();
	}

	private Blackboard BuildBlackboard(BehaviourTreeSerializableData behaviourTreeData)
	{
		Blackboard blackboard = new Blackboard();
		foreach (BehaviourTreeVariableElement variable in behaviourTreeData.Variables)
		{
			BlackboardVariable blackboardVariable = variable.CreateVariable();
			blackboardVariable.Key = variable.Key;
			blackboard.AddVariable(variable.Id, blackboardVariable);
			m_VariableToElementMap.Add(blackboardVariable, variable);
		}
		return blackboard;
	}

	private Blackboard BuildParameterizedBlackboard(ParameterizedBehaviourTree parameterizedBehaviourTree)
	{
		Blackboard blackboard = new Blackboard();
		BehaviourTreeSerializableData behaviourTreeSerializableData = parameterizedBehaviourTree.BehaviourTree.Get();
		BehaviourTreeVariableElement[] variables = parameterizedBehaviourTree.Variables;
		VariableMappingContainer mappingContainer = parameterizedBehaviourTree.MappingContainer;
		foreach (BehaviourTreeVariableElement variable in behaviourTreeSerializableData.Variables)
		{
			BlackboardVariable blackboardVariable = variable.CreateVariable();
			blackboardVariable.Key = variable.Key;
			if (mappingContainer != null && mappingContainer.TryGet(variable.Id, out var mapping) && !string.IsNullOrEmpty(mapping.SuperTreeVariableId))
			{
				BehaviourTreeVariableElement behaviourTreeVariableElement = variables.FirstOrDefault((BehaviourTreeVariableElement v) => v.Id == mapping.SuperTreeVariableId);
				if (behaviourTreeVariableElement != null)
				{
					BlackboardVariable blackboardVariable2 = behaviourTreeVariableElement.CreateVariable();
					if (!blackboardVariable.GetType().IsAssignableFrom(blackboardVariable2.GetType()))
					{
						throw new Exception("Failed to map variable. SubTree '" + behaviourTreeSerializableData.Title + "', Variable '" + variable.Key + "'");
					}
					blackboardVariable = blackboardVariable2;
				}
				else
				{
					BTLog.Log("Variable was not mapped. SubTree '" + behaviourTreeSerializableData.Title + "', Variable '" + variable.Key + "'");
				}
			}
			blackboard.AddVariable(variable.Id, blackboardVariable, variable.Key);
			m_VariableToElementMap.Add(blackboardVariable, variable);
		}
		return blackboard;
	}

	private Blackboard BuildInheritedBlackboard(BehaviourTreeSerializableData behaviourTreeData, Blackboard inheritedBlackboard)
	{
		Blackboard blackboard = new Blackboard();
		foreach (BehaviourTreeVariableElement variable in behaviourTreeData.Variables)
		{
			BlackboardVariable blackboardVariable = variable.CreateVariable();
			blackboardVariable.Key = variable.Key;
			foreach (BlackboardVariable variable2 in inheritedBlackboard.Variables)
			{
				if (blackboardVariable.Key == variable2.Key && blackboardVariable.GetType().IsAssignableFrom(variable2.GetType()))
				{
					blackboardVariable = variable2;
					break;
				}
			}
			blackboard.AddVariable(variable.Id, blackboardVariable);
			m_VariableToElementMap.Add(blackboardVariable, variable);
		}
		return blackboard;
	}

	private Blackboard BuildMappedBlackboard(BehaviourTreeSerializableData behaviourTreeData, Blackboard inheritedBlackboard, VariableMappingContainer mappingContainer)
	{
		Blackboard blackboard = new Blackboard();
		foreach (BehaviourTreeVariableElement variable2 in behaviourTreeData.Variables)
		{
			BlackboardVariable blackboardVariable = variable2.CreateVariable();
			blackboardVariable.Key = variable2.Key;
			if (mappingContainer.TryGet(variable2.Id, out var mapping) && !string.IsNullOrEmpty(mapping.SuperTreeVariableId))
			{
				BlackboardVariable variable = inheritedBlackboard.GetVariable(mapping.SuperTreeVariableId);
				if (variable == null || !blackboardVariable.GetType().IsAssignableFrom(variable.GetType()))
				{
					throw new Exception("Failed to map variable. SubTree '" + behaviourTreeData.Title + "', Variable '" + variable2.Key + "'");
				}
				blackboardVariable = variable;
			}
			blackboard.AddVariable(variable2.Id, blackboardVariable, variable2.Key);
			m_VariableToElementMap.Add(blackboardVariable, variable2);
		}
		return blackboard;
	}

	public BehaviourTreeVariableElement GetVariableElement(BlackboardVariable variable)
	{
		if (m_VariableToElementMap.TryGetValue(variable, out var value))
		{
			return value;
		}
		throw new Exception($"Can't get variable element for variable: '{variable}'.");
	}

	private Dictionary<string, BehaviourTreeNodeElement> BuildNodesList(Blackboard blackboard, BehaviourTree behaviourTree)
	{
		Dictionary<string, BehaviourTreeNodeElement> dictionary = new Dictionary<string, BehaviourTreeNodeElement>();
		foreach (BehaviourTreeNodeElement node in BehaviourTreeData.Nodes)
		{
			BehaviourTreeNode behaviourTreeNode = node.CreateNode(blackboard);
			behaviourTreeNode.BehaviourTree = behaviourTree;
			behaviourTreeNode.NodeElement = node;
			BehaviourTree.Nodes.Add(behaviourTreeNode);
			m_NodeToElementMap.Add(behaviourTreeNode, node);
			dictionary.Add(node.name, node);
			if (behaviourTreeNode is SubTreeNode item)
			{
				BehaviourTree.SubTreeNodes.Add(item);
			}
		}
		return dictionary;
	}

	private void InitializeRootNode()
	{
		BehaviourTree.Root = BehaviourTree.Nodes.FirstOrDefault((BehaviourTreeNode node) => node is RootNode) as RootNode;
		if (BehaviourTree.Root == null)
		{
			BTLog.Error($"BehaviourTree '{BehaviourTreeData}' does not have a root node");
		}
	}

	private void BuildHierarchy(Dictionary<string, BehaviourTreeNodeElement> namesMap)
	{
		foreach (BehaviourTreeNode node in BehaviourTree.Nodes)
		{
			BehaviourTreeNodeElement value = m_NodeToElementMap.GetValue(node);
			if (!string.IsNullOrEmpty(value.ParentNodeName))
			{
				if (namesMap.TryGetValue(value.ParentNodeName, out var value2))
				{
					BehaviourTreeNode key = m_NodeToElementMap.GetKey(value2);
					SetParent(key, node);
					continue;
				}
				BTLog.Error("Node '" + value.Title + "' has a parent '" + value.ParentNodeName + "' that does not exist");
			}
		}
	}

	private void SetParent(BehaviourTreeNode parent, BehaviourTreeNode child)
	{
		child.Parent = parent;
		if (parent is IHasChildrenNode hasChildrenNode)
		{
			hasChildrenNode.Children.Add(child);
		}
		else if (parent is IHasChildNode hasChildNode)
		{
			hasChildNode.Child = child;
		}
		else
		{
			BTLog.Error($"Unexpected parent type: {parent.GetType()} for node '{child.Title}'");
		}
	}

	private void OrderChildrenNodes()
	{
		foreach (BehaviourTreeNode node in BehaviourTree.Nodes)
		{
			if (node is IHasChildrenNode hasChildrenNode)
			{
				hasChildrenNode.Children.Sort(Comparison);
			}
		}
		int Comparison(BehaviourTreeNode node1, BehaviourTreeNode node2)
		{
			return m_NodeToElementMap.GetValue(node1).Position.x.CompareTo(m_NodeToElementMap.GetValue(node2).Position.x);
		}
	}

	private void BuildSubTrees(Blackboard blackboard)
	{
		foreach (SubTreeNode subTreeNode2 in BehaviourTree.SubTreeNodes)
		{
			if (!(m_NodeToElementMap.GetValue(subTreeNode2) is SubTreeNodeElement subTreeNodeElement) || subTreeNodeElement.SubTree.IsEmpty())
			{
				throw new Exception($"SubTreeNode '{subTreeNode2.Title}' does not have a sub tree in BehaviourTree '{BehaviourTreeData}'");
			}
			BehaviourTreeSerializableData behaviourTreeDataData = subTreeNodeElement.SubTree.Get();
			SubTreeNode subTreeNode = subTreeNode2;
			subTreeNode.RuntimeBridge = subTreeNodeElement.InheritanceMode switch
			{
				SubTreeVariableInheritanceMode.VariableMapping => new BehaviourTreeRuntimeToBlueprintBridge(RuntimeContext, behaviourTreeDataData, blackboard, subTreeNodeElement.MappingContainer), 
				SubTreeVariableInheritanceMode.KeyAndType => new BehaviourTreeRuntimeToBlueprintBridge(RuntimeContext, behaviourTreeDataData, blackboard), 
				_ => new BehaviourTreeRuntimeToBlueprintBridge(RuntimeContext, behaviourTreeDataData), 
			};
			subTreeNode2.RuntimeBridge.BehaviourTree.Root.Parent = subTreeNode2;
		}
	}

	public void FetchAllHierarchyNodes(List<BehaviourTreeNode> nodes)
	{
		foreach (BehaviourTreeNode node in BehaviourTree.Nodes)
		{
			nodes.Add(node);
		}
		foreach (SubTreeNode subTreeNode in BehaviourTree.SubTreeNodes)
		{
			subTreeNode.RuntimeBridge.FetchAllHierarchyNodes(nodes);
		}
	}
}
