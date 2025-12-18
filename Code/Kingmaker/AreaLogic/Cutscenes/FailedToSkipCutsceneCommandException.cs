using System;

namespace Kingmaker.AreaLogic.Cutscenes;

public class FailedToSkipCutsceneCommandException : CutsceneCommandException
{
	public FailedToSkipCutsceneCommandException(CutscenePlayerData player, CommandBase command, Exception e)
		: base(player, command, CutsceneCommandException.GetMessage(player, command, e, "Skip"), e)
	{
	}
}
