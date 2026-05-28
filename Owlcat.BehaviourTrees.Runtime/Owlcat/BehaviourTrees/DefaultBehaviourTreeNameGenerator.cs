using System;

namespace Owlcat.BehaviourTrees;

public class DefaultBehaviourTreeNameGenerator : IBehaviourTreeNameGenerator
{
	public string GenerateName(string typeName)
	{
		return $"${typeName}${Guid.NewGuid()}";
	}
}
