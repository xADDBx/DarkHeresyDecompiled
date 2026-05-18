using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Networking;
using Kingmaker.Networking.Hash;
using Kingmaker.Networking.Serialization;
using Kingmaker.Plugins.CoopDesyncAnalyzer.Attributes;
using Kingmaker.Signals;
using StateHasher.Core;

namespace Kingmaker.StateHasher;

public readonly ref struct StateHasherContext
{
	private readonly GameStateSerializationContext m_Context;

	private StateHasherContext(GameStateSerializationContext context)
	{
		m_Context = context;
		RandomState.Instance.Refresh();
	}

	[HashCommitRoot]
	public HashableState GetHashableState()
	{
		HashableState result = default(HashableState);
		result.player = Game.Instance.Player;
		result.sceneEntitiesState = Game.Instance.State.CrossSceneState;
		result.areaPersistentState = Game.Instance.State.LoadedAreaState;
		result.randomState = RandomState.Instance;
		result.signalService = SignalService.Instance.State;
		return result;
	}

	public void Dispose()
	{
		m_Context.Dispose();
		RecursiveReferences.Reset();
	}

	public static StateHasherContext Request()
	{
		return new StateHasherContext(ContextData<GameStateSerializationContext>.Request());
	}
}
