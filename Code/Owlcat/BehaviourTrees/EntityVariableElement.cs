using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

[VariableTypeName("Runtime/MechanicEntity", typeof(MechanicEntity))]
[TypeId("d85a8f95e6a4425d9ee5f4c943f5dfa5")]
public class EntityVariableElement : BehaviourTreeVariableElement<MechanicEntity>
{
	public override BlackboardVariable CreateVariable()
	{
		return new EntityVariable();
	}
}
