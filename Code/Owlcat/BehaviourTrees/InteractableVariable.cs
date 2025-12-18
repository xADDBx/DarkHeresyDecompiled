using JetBrains.Annotations;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;

namespace Owlcat.BehaviourTrees;

public class InteractableVariable : BlackboardVariable<InteractionAction>
{
	[CanBeNull]
	public override InteractionAction Value { get; set; }

	public override string ToString()
	{
		return base.Key + ": " + (Value.Or(null)?.gameObject.ToString() ?? "<null>");
	}
}
