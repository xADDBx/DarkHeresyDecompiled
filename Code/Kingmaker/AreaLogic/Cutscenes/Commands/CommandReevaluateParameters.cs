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

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		ParametrizedContextSetter.ParameterEntry[] array = (player.ParameterSetter?.Parameters).EmptyIfNull();
		foreach (ParametrizedContextSetter.ParameterEntry parameterEntry in array)
		{
			player.Parameters.Params[parameterEntry.Name] = parameterEntry.GetValue();
		}
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
		OnRun(player, skipping: true);
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}
}
