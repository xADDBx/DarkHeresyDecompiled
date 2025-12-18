using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Runtime/RuntimeInternalData", typeof(AiAgentRuntimeInternalData))]
[TypeId("e473c0954e754205a3c46e83954a8a91")]
public class AiAgentRuntimeInternalDataVariableElement : BehaviourTreeVariableElement<AiAgentRuntimeInternalData>
{
	public override BlackboardVariable CreateVariable()
	{
		return new AiAgentRuntimeInternalDataVariable();
	}
}
