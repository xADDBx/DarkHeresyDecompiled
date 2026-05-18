using JetBrains.Annotations;
using Kingmaker.View.MapObjects;

namespace Owlcat.BehaviourTrees;

public class InteractableVariable : BlackboardVariable<InteractionActionPart>
{
	[CanBeNull]
	public override InteractionActionPart Value { get; set; }

	public override string ToString()
	{
		return base.Key + ": " + (Value?.ToString() ?? "<null>");
	}
}
