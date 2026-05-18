using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandMarkUnitControlled")]
[TypeId("9d59dacab16c67d47ab0760668284391")]
public class CommandMarkUnitControlled : CommandBase
{
	private class Data
	{
		public bool IsFinished;

		public bool SkippedByPlayer;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public float UnmarkAfter;

	public override bool IsContinuous => UnmarkAfter <= 0f;

	public override bool ShouldHaveControlledUnit => true;

	public override bool TrySkip(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).SkippedByPlayer = true;
		return true;
	}

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		if (GetControlledUnit() == null)
		{
			CommandResult result = default(CommandResult);
			result.IsSuccess = false;
			result.ErrorMessage = "Cant find unit to mark";
			return result;
		}
		return CommandResult.Success;
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!Unit || !Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}

	public override bool TryPrepareForStop(CutscenePlayerData player)
	{
		if ((!player.GetCommandData<Data>(this).SkippedByPlayer && IsContinuous) || !IsFinished(player))
		{
			return false;
		}
		return StopPlaySignalIsReady(player);
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).IsFinished = true;
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
		Data commandData = player.GetCommandData<Data>(this);
		if (!commandData.SkippedByPlayer && IsContinuous)
		{
			return false;
		}
		return commandData.IsFinished;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).IsFinished |= time > (double)UnmarkAfter;
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		return "<b>Mark</b> " + ((!Unit) ? "???" : Unit?.GetCaptionShort()) + ((UnmarkAfter > 0f) ? (" for " + UnmarkAfter + " secs") : " indefinitely");
	}

	public override string GetWarning()
	{
		if ((bool)Unit && Unit.CanEvaluate())
		{
			return null;
		}
		return "No unit";
	}
}
