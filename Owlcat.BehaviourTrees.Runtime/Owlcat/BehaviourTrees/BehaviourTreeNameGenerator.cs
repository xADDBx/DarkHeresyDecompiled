namespace Owlcat.BehaviourTrees;

public class BehaviourTreeNameGenerator
{
	private static IBehaviourTreeNameGenerator s_Generator = new DefaultBehaviourTreeNameGenerator();

	public static void SetGenerator(IBehaviourTreeNameGenerator generator)
	{
		s_Generator = generator;
	}

	public static string GenerateName(string typeName)
	{
		return s_Generator.GenerateName(typeName);
	}
}
