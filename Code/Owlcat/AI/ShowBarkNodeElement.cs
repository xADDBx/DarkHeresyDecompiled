using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Owlcat.BehaviourTrees;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.AI;

[NodeMenuItem("Add Node/Actions/Show Bark", "Show Bark")]
[TypeId("eb8a2069b9d7499a8c3b45a383401c60")]
public class ShowBarkNodeElement : BehaviourTreeNodeElement<ShowBarkNode>
{
	[LocalizedStringParam(Kind = "ask")]
	public LocalizedString BarkString;

	public ShowBarkNode.BarkDurationType DurationType;

	[ShowIf("UseCustomDuration")]
	public float CustomDuration;

	private bool UseCustomDuration => DurationType == ShowBarkNode.BarkDurationType.CustomDuration;

	protected override ShowBarkNode CreateTypedNode(Blackboard blackboard)
	{
		return new ShowBarkNode(blackboard.GetAgentVariable(), BarkString, DurationType, CustomDuration);
	}
}
