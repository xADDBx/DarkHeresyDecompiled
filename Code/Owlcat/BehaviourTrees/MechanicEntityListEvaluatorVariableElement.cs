using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Owlcat.AI;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Evaluators/MechanicEntityList Evaluator", typeof(List<MechanicEntity>))]
[TypeId("61e84ff06a1e4ff2ade13798f1880e64")]
public class MechanicEntityListEvaluatorVariableElement : BehaviourTreeEvaluatorVariableElement<MechanicEntityListEvaluator>
{
	public override BlackboardVariable CreateVariable()
	{
		return new MechanicEntityListEvaluatorVariable(Evaluator);
	}
}
