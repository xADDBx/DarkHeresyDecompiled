using System.Collections.Generic;
using System.Linq;
using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public static class SquadControlNodeValidationHelper
{
	public static List<string> Validate(BehaviourTreeNodeElement squadControlNode, Dictionary<string, List<BehaviourTreeNodeElement>> childrenLookup)
	{
		List<string> list = new List<string>();
		List<BehaviourTreeNodeElement> list2 = (from c in GetDirectChildren(squadControlNode, childrenLookup)
			orderby c.Position.x
			select c).ToList();
		if (list2.Count != 3)
		{
			list.Add($"SquadControlNode must have exactly 3 children (planning, synchronous, individual), but has {list2.Count}");
			return list;
		}
		ValidatePlanningBranch(list2[0], childrenLookup, list);
		ValidateSynchronousBranch(list2[1], childrenLookup, list);
		return list;
	}

	public static List<string> ValidateAll(IEnumerable<BehaviourTreeNodeElement> allNodes)
	{
		List<string> list = new List<string>();
		IReadOnlyList<BehaviourTreeNodeElement> obj = (allNodes as IReadOnlyList<BehaviourTreeNodeElement>) ?? allNodes.ToList();
		Dictionary<string, List<BehaviourTreeNodeElement>> childrenLookup = BuildChildrenLookup(obj);
		foreach (BehaviourTreeNodeElement item in obj)
		{
			if (item is SquadControlNodeElement)
			{
				list.AddRange(Validate(item, childrenLookup));
			}
		}
		return list;
	}

	private static Dictionary<string, List<BehaviourTreeNodeElement>> BuildChildrenLookup(IReadOnlyList<BehaviourTreeNodeElement> allNodes)
	{
		Dictionary<string, List<BehaviourTreeNodeElement>> dictionary = new Dictionary<string, List<BehaviourTreeNodeElement>>();
		foreach (BehaviourTreeNodeElement allNode in allNodes)
		{
			if (!string.IsNullOrEmpty(allNode.ParentNodeName))
			{
				if (!dictionary.TryGetValue(allNode.ParentNodeName, out var value))
				{
					value = new List<BehaviourTreeNodeElement>();
					dictionary[allNode.ParentNodeName] = value;
				}
				value.Add(allNode);
			}
		}
		return dictionary;
	}

	private static void ValidatePlanningBranch(BehaviourTreeNodeElement branchRoot, Dictionary<string, List<BehaviourTreeNodeElement>> childrenLookup, List<string> errors)
	{
		foreach (BehaviourTreeNodeElement allDescendant in GetAllDescendants(branchRoot, childrenLookup))
		{
			if (IsActionNode(allDescendant))
			{
				errors.Add("Planning branch must not contain action nodes: found '" + (allDescendant.Title ?? allDescendant.GetType().Name) + "'");
			}
		}
	}

	private static void ValidateSynchronousBranch(BehaviourTreeNodeElement branchRoot, Dictionary<string, List<BehaviourTreeNodeElement>> childrenLookup, List<string> errors)
	{
		foreach (BehaviourTreeNodeElement allDescendant in GetAllDescendants(branchRoot, childrenLookup))
		{
			if (IsActionNode(allDescendant) && !IsMovementNode(allDescendant))
			{
				errors.Add("Synchronous branch must not contain action nodes other than movement: found '" + (allDescendant.Title ?? allDescendant.GetType().Name) + "'");
			}
		}
	}

	private static bool IsActionNode(BehaviourTreeNodeElement node)
	{
		if (!(node is UseAbilityOnEntityNodeElement) && !(node is UseAbilityOnGraphNodeNodeElement) && !(node is UseAbilityOnSelfNodeElement) && !(node is UseAbilityOnBodyPartNodeElement) && !(node is InteractNodeElement) && !(node is EndTurnNodeElement) && !(node is MoveToGraphNodeNodeElement) && !(node is MoveToEntityNodeElement))
		{
			return node is MoveToInteractableNodeElement;
		}
		return true;
	}

	private static bool IsMovementNode(BehaviourTreeNodeElement node)
	{
		if (!(node is MoveToGraphNodeNodeElement) && !(node is MoveToEntityNodeElement))
		{
			return node is MoveToInteractableNodeElement;
		}
		return true;
	}

	private static List<BehaviourTreeNodeElement> GetDirectChildren(BehaviourTreeNodeElement parent, Dictionary<string, List<BehaviourTreeNodeElement>> childrenLookup)
	{
		if (!childrenLookup.TryGetValue(parent.name, out var value))
		{
			return new List<BehaviourTreeNodeElement>();
		}
		return value;
	}

	private static IEnumerable<BehaviourTreeNodeElement> GetAllDescendants(BehaviourTreeNodeElement root, Dictionary<string, List<BehaviourTreeNodeElement>> childrenLookup)
	{
		Stack<BehaviourTreeNodeElement> stack = new Stack<BehaviourTreeNodeElement>();
		foreach (BehaviourTreeNodeElement directChild in GetDirectChildren(root, childrenLookup))
		{
			stack.Push(directChild);
		}
		while (stack.Count > 0)
		{
			BehaviourTreeNodeElement current = stack.Pop();
			yield return current;
			foreach (BehaviourTreeNodeElement directChild2 in GetDirectChildren(current, childrenLookup))
			{
				stack.Push(directChild2);
			}
		}
	}
}
