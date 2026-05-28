namespace Owlcat.BehaviourTrees;

public interface IBreakpointsProvider
{
	NodeBreakpointSetting GetGlobalSetting(BehaviourTreeNodeElement nodeElement);
}
