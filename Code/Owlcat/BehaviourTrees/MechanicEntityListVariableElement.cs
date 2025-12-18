using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Runtime/MechanicEntityList", typeof(List<MechanicEntity>))]
[TypeId("704871e848cd401087da0883a1f6cbd5")]
public class MechanicEntityListVariableElement : BehaviourTreeVariableElement<List<MechanicEntity>>
{
	public override BlackboardVariable CreateVariable()
	{
		return new MechanicEntityListVariable();
	}
}
