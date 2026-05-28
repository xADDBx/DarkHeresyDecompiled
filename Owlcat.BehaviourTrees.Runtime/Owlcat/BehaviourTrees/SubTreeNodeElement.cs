using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[NodeMenuItem("Add Node/Flow Control/Sub Tree", "Sub Tree")]
[TypeId("e3f41ce707834a83a9527f7b04e32a12")]
public class SubTreeNodeElement : BehaviourTreeNodeElement<SubTreeNode>
{
	public BehaviourTreeReference SubTree;

	public SubTreeVariableInheritanceMode InheritanceMode;

	public VariableMappingContainer MappingContainer;

	protected override SubTreeNode CreateTypedNode(Blackboard blackboard)
	{
		return new SubTreeNode();
	}

	public void UpdateMappings()
	{
		MappingContainer.Update(SubTree?.Get());
	}
}
