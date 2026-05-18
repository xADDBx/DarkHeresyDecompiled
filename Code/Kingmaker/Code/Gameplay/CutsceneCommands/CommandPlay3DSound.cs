using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.CutsceneCommands;

[ComponentName("Command/CommandPlay3DSound")]
[TypeId("5672f1ec661644db9432df7dcfd69141")]
public class CommandPlay3DSound : CommandBase
{
	private class Data
	{
		public string? SoundName;

		public EntityReference? SoundSourceObject;

		public bool SetSex;

		public bool SetRace;

		public bool SetCurrentSpeaker;
	}

	[Tooltip("Ak event name from Wwise library")]
	public string? SoundName;

	[SerializeReference]
	public MapObjectEvaluator? SoundSourceObject;

	[Tooltip("Sets Ak switch on player's Sex")]
	public bool SetSex;

	[Tooltip("Sets Ak switch on player's Race")]
	public bool SetRace;

	[Tooltip("Sets SoundSourceObject as current dialog speaker")]
	public bool SetCurrentSpeaker;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.SoundName = SoundName;
		commandData.SoundSourceObject = SoundSourceObject?.GetValue();
		commandData.SetSex = SetSex;
		commandData.SetRace = SetRace;
		commandData.SetCurrentSpeaker = SetCurrentSpeaker;
		Play3DSound.Play(commandData.SoundName, commandData.SoundSourceObject, commandData.SetSex, commandData.SetRace, commandData.SetCurrentSpeaker, this);
		return CommandResult.Success;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return OnRun(player, skipping: true);
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
}
