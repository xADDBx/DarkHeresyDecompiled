namespace Owlcat.BehaviourTrees;

public static class BehaviourTreeRandomProvider
{
	private static IBehaviourTreeRandomProvider s_Provider = new DefaultBehaviourTreeRandomProvider();

	public static float value => s_Provider.value;

	public static int Range(int minInclusive, int maxExclusive)
	{
		return s_Provider.Range(minInclusive, maxExclusive);
	}

	public static void SetProvider(IBehaviourTreeRandomProvider provider)
	{
		s_Provider = provider;
	}
}
