using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class GameCommandWithSynchronized : GameCommand, IOwlPackable<GameCommandWithSynchronized>
{
	protected bool m_IsSynchronized;

	public override bool IsSynchronized => m_IsSynchronized;

	public override void AfterDeserialization()
	{
		base.AfterDeserialization();
		m_IsSynchronized = true;
	}
}
