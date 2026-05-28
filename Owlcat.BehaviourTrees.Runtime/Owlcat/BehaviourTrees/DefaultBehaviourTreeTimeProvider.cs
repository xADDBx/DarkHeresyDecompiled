using UnityEngine;

namespace Owlcat.BehaviourTrees;

public class DefaultBehaviourTreeTimeProvider : IBehaviourTreeTimeProvider
{
	public float Time => UnityEngine.Time.time;

	public float DeltaTime => UnityEngine.Time.deltaTime;
}
