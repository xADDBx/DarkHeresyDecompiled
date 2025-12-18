using Kingmaker.GameModes;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class ChangeGameModeCommand : GameCommand, IOwlPackable<ChangeGameModeCommand>
{
	public enum ActionType
	{
		Start,
		Stop
	}

	public readonly ActionType Action;

	public readonly GameModeType GameMode;

	protected ChangeGameModeCommand(ActionType action, GameModeType gameMode)
	{
		Action = action;
		GameMode = gameMode;
	}

	protected ChangeGameModeCommand(OwlPackConstructorParameter _)
	{
	}
}
