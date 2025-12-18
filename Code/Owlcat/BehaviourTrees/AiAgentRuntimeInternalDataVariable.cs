using Owlcat.AI;

namespace Owlcat.BehaviourTrees;

public class AiAgentRuntimeInternalDataVariable : BlackboardVariable<AiAgentRuntimeInternalData>
{
	public override AiAgentRuntimeInternalData Value { get; set; }

	public override string ToString()
	{
		if (Value == null)
		{
			return "<null>";
		}
		return "Runtime Internal Data";
	}
}
