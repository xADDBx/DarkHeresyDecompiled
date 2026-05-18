using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandMarkOnPlatform")]
[TypeId("ee8188ea95c9412da826a7d098a3885e")]
public class CommandMarkOnPlatform : CommandBase
{
	private class Data
	{
		[CanBeNull]
		public AbstractUnitEntity Unit;

		public PlatformObjectEntity Platform;
	}

	[AllowedEntityType(typeof(PlatformObjectView))]
	[ValidateNotEmpty]
	public EntityReference PlatformReference;

	[SerializeReference]
	public AbstractUnitEvaluator UnitEvaluator;

	public override bool IsContinuous => true;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		if (!UnitEvaluator.TryGetValue(out var value))
		{
			return CommandResult.Fail("Failed to find unit");
		}
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit = value;
		commandData.Platform = PlatformReference.FindData() as PlatformObjectEntity;
		commandData.Unit.GetOrCreate<EntityPartStayOnPlatform>().SetOnPlatform(commandData.Platform);
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).Unit?.GetOrCreate<EntityPartStayOnPlatform>().ReleaseFromPlatform();
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}

	public override string GetCaption()
	{
		return "Mark " + UnitEvaluator?.GetCaptionShort() + " <b>on platform</b>";
	}
}
