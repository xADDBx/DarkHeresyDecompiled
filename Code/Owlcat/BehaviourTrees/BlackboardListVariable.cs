using System.Collections.Generic;
using JetBrains.Annotations;

namespace Owlcat.BehaviourTrees;

public class BlackboardListVariable<T> : BlackboardVariable<List<T>>
{
	[CanBeNull]
	public override List<T> Value { get; set; }

	public override string ToString()
	{
		return base.Key + ": " + (Value?.ToString() ?? "<null>");
	}
}
