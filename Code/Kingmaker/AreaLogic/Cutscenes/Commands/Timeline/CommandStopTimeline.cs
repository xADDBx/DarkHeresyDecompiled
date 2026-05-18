using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine.Playables;

namespace Kingmaker.AreaLogic.Cutscenes.Commands.Timeline;

[TypeId("47e7be2aa27c3e8449426a446e0c0497")]
public class CommandStopTimeline : CommandBase
{
	[ValidateNotNull]
	[AllowedEntityType(typeof(DirectorAdapter))]
	[UsedImplicitly]
	public EntityReference Director;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		DirectorAdapter directorAdapter = Director.FindView() as DirectorAdapter;
		if (!directorAdapter)
		{
			PFLog.Cutscene.Warning($"Command {this} in {player.Cutscene}: cannot find DirectorAdapter {Director}");
			return CommandResult.Success;
		}
		foreach (CommandPlayTimeline item in player.Cutscene.AllCommandsOfType<CommandPlayTimeline>())
		{
			CommandPlayTimeline.Data commandData = player.GetCommandData<CommandPlayTimeline.Data>(item);
			if (!(commandData.Adapter != directorAdapter))
			{
				PlayableDirector director = commandData.Director;
				if ((bool)director && director.playableGraph.IsValid())
				{
					director.Stop();
				}
				directorAdapter.Stop();
				break;
			}
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
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

	public override string GetCaption()
	{
		return "Stop timeline " + Director.EntityNameInEditor;
	}
}
