using Kingmaker;
using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public class SimulationBehaviourTreeTimeProvider : IBehaviourTreeTimeProvider
{
	public float Time => (float)Game.Instance.Player.RealTime.TotalSeconds;

	public float DeltaTime => Game.Instance.Controllers.TimeController.GameDeltaTime;
}
