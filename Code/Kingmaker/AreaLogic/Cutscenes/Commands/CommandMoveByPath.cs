using System.Collections.Generic;
using CatmullRomSplines;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[ComponentName("Command/CommandMoveByPath")]
[TypeId("09f4f8064a60e864cad32ee3f101819b")]
public class CommandMoveByPath : CommandBase
{
	private class Data
	{
		[CanBeNull]
		public AbstractUnitEntity Unit;

		[CanBeNull]
		public UnitCommandHandle CommandHandle;

		public Vector3 EndPosition;
	}

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[AllowedEntityType(typeof(CutscenePath))]
	[ValidateNotEmpty]
	public EntityReference Path;

	public WalkSpeedType Animation = WalkSpeedType.Walk;

	public bool OverrideSpeed;

	[ShowIf("OverrideSpeed")]
	public float Speed = 5f;

	public float PointsPerMeter = 20f;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit = Unit.GetValue();
		commandData.CommandHandle = null;
		IEntityViewBase entityViewBase = Path.FindView();
		VectorSpline vectorSpline = (entityViewBase.GO ? entityViewBase.GO.GetComponent<VectorSpline>() : null);
		if (commandData.Unit != null && (bool)vectorSpline)
		{
			List<Vector3> list = new List<Vector3>();
			int num = Mathf.Max(Mathf.CeilToInt(vectorSpline.Length * PointsPerMeter), 2);
			for (int i = 0; i < num; i++)
			{
				list.Add(vectorSpline.EvaluatePosition((float)i / (float)(num - 1)));
			}
			commandData.EndPosition = list.LastItem();
			if (skipping)
			{
				commandData.Unit.Translocate(commandData.EndPosition, null);
				return;
			}
			UnitMoveToParams cmdParams = new UnitMoveToParams(ForcedPath.Construct(list), commandData.EndPosition)
			{
				MovementType = ((Animation == WalkSpeedType.Sprint) ? WalkSpeedType.Walk : Animation),
				OverrideSpeed = (OverrideSpeed ? new float?(Speed) : null)
			};
			commandData.CommandHandle = commandData.Unit.Commands.Run(cmdParams);
			commandData.Unit.View.MovementAgent.AvoidanceDisabled = true;
		}
	}

	protected override void OnSkip(CutscenePlayerData player)
	{
		AbstractUnitEntity value = Unit.GetValue();
		IEntityViewBase entityViewBase = Path.FindView();
		VectorSpline vectorSpline = (entityViewBase?.GO ? entityViewBase.GO.GetComponent<VectorSpline>() : null);
		if (!(vectorSpline == null))
		{
			List<Vector3> list = new List<Vector3>();
			int num = Mathf.Max(Mathf.CeilToInt(vectorSpline.Length * PointsPerMeter), 2);
			for (int i = 0; i < num; i++)
			{
				list.Add(vectorSpline.EvaluatePosition((float)i / (float)(num - 1)));
			}
			value.Translocate(list.LastItem(), null);
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (IsUnitDisabled(commandData.Unit))
		{
			return true;
		}
		UnitCommandHandle commandHandle = commandData.CommandHandle;
		if (commandHandle != null && !commandHandle.IsFinished)
		{
			return false;
		}
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		UnitCommandHandle commandHandle = player.GetCommandData<Data>(this).CommandHandle;
		if (commandHandle != null && !commandHandle.IsFinished)
		{
			commandHandle.Interrupt();
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		AbstractUnitEntity unit = commandData.Unit;
		if (unit != null)
		{
			if (commandData.CommandHandle != null && commandData.EndPosition != Vector3.zero && commandData.CommandHandle.Result != AbstractUnitCommand.ResultType.Success)
			{
				unit.Translocate(commandData.EndPosition, null);
			}
			if ((bool)unit.View)
			{
				unit.View.MovementAgent.AvoidanceDisabled = false;
				commandData.Unit?.View.MovementAgent.Blocker.BlockAtCurrentPosition();
			}
			if (Game.Instance.Controllers.TurnController.TurnBasedModeActive)
			{
				(unit as UnitEntity)?.SnapToGrid();
			}
		}
	}

	private bool IsUnitDisabled(AbstractUnitEntity unit)
	{
		if (unit != null && !unit.IsDisposed && unit.LifeState.IsConscious && unit.IsInGame)
		{
			return !unit.IsInState;
		}
		return true;
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!Unit || !Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}

	public override string GetCaption()
	{
		return Unit?.GetCaptionShort() + " <b>move along</b> " + Path;
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
