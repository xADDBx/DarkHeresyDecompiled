namespace Owlcat.BehaviourTrees;

public interface IBehaviourTreeTimeProvider
{
	float Time { get; }

	float DeltaTime { get; }
}
