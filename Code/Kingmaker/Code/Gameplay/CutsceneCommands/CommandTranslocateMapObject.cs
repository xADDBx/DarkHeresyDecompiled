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

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		IEntityViewBase entityViewBase = TargetPosition.FindView();
		if (MapObject.TryGetValue(out var value) && entityViewBase != null)
		{
			value.Position = entityViewBase.ViewTransform.position;
			if (ChangeRotation)
			{
				value.View.ViewTransform.rotation = entityViewBase.ViewTransform.rotation;
			}
		}
		else
		{
			Logger.Error($"{this}: Failed to translocate {MapObject} to position position {TargetPosition}. Evaluator error");
		}
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
		OnRun(player, skipping: true);
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
