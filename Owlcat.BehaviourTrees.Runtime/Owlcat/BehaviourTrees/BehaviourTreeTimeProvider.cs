namespace Owlcat.BehaviourTrees;

public static class BehaviourTreeTimeProvider
{
	private static IBehaviourTreeTimeProvider s_Provider = new DefaultBehaviourTreeTimeProvider();

	public static float Time => s_Provider.Time;

	public static float DeltaTime => s_Provider.DeltaTime;

	public static void SetProvider(IBehaviourTreeTimeProvider provider)
	{
		s_Provider = provider;
	}
}
