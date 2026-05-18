using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.View;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.CutsceneCommands;

[ComponentName("Command/CommandTranslocateMapObject")]
[TypeId("da06247041cf477f8f07104d884df875")]
public class CommandTranslocateMapObject : CommandBase
{
	private static readonly LogChannel Logger = PFLog.Cutscene;

	[SerializeReference]
	public MapObjectEvaluator MapObject;

	[AllowedEntityType(typeof(LocatorView))]
	[SerializeField]
	public EntityReference TargetPosition;

	[SerializeField]
	public bool ChangeRotation;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		IEntityView entityView = TargetPosition.FindView();
		if (MapObject.TryGetValue(out var value) && entityView != null)
		{
			value.Position = entityView.ViewTransform.position;
			if (ChangeRotation)
			{
				value.View.ViewTransform.rotation = entityView.ViewTransform.rotation;
			}
			return CommandResult.Success;
		}
		return CommandResult.FailWithReport($"{this}: Failed to translocate {MapObject} to position position {TargetPosition}. Evaluator error");
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

	public override string GetCaption()
	{
		return "Teleport object (" + MapObject.NameSafe() + ")";
	}
}
