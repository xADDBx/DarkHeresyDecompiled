using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Owlcat.BehaviourTrees;

public class EntityVariable : BlackboardVariable<MechanicEntity>
{
	[CanBeNull]
	public override MechanicEntity Value { get; set; }

	public override string ToString()
	{
		return base.Key + ": " + (Value?.ToString() ?? "<null>");
	}
}
