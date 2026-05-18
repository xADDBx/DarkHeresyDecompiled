using System;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Obsolete]
[TypeId("12ec6fe49f312884a9dd544e8af57ce7")]
public class CommandReevaluateParameters : CommandBase
{
	public override string GetCaption()
	{
		return "[HACK] Reevaluate parameters";
	}

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		ParametrizedContextSetter.ParameterEntry[] array = (player.ParameterSetter?.Parameters).EmptyIfNull();
		foreach (ParametrizedContextSetter.ParameterEntry parameterEntry in array)
		{
			player.Parameters.Params[parameterEntry.Name] = parameterEntry.GetValue();
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		OnRun(player, skipping: true);
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}
}
