using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.BehaviourTrees;

[AllowedOn(typeof(BlueprintBehaviourTree))]
[ComponentName("AI/BehaviourTreeNodeGraphComponent")]
[TypeId("f924d235b989440d8c6ccb972b10cdb7")]
public class BehaviourTreeNodeGraphComponent : BlueprintComponent
{
	[SerializeReference]
	public List<BehaviourTreeNodeElement> Nodes = new List<BehaviourTreeNodeElement>();

	public void Add(BehaviourTreeNodeElement node)
	{
		Nodes.Add(node);
	}

	public void Remove(BehaviourTreeNodeElement node)
	{
		Nodes.Remove(node);
	}
}
