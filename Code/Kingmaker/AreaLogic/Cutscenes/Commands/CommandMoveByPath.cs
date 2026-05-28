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
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Kingmaker.Visual.Animation.WeaponStyles;
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
	[SerializeField]
	private EntityReference Path;

	public WalkSpeedType Animation = WalkSpeedType.Walk;

	public bool OverrideSpeed;

	[ShowIf("OverrideSpeed")]
	public float SpeedMultiplier = 1f;

	public float PointsPerMeter = 20f;

	private const float AngularSpeedDuringPath = 720f;

	public override bool ShouldHaveControlledUnit => true;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (!Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Failed to find unit");
		}
		commandData.Unit = value;
		commandData.CommandHandle = null;
		IEntityView entityView = Path.FindView();
		VectorSpline vectorSpline = (entityView.GO ? entityView.GO.GetComponent<VectorSpline>() : null);
		if (vectorSpline == null)
		{
			return CommandResult.Fail("No spline path");
		}
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
			return CommandResult.Success;
		}
		int num2 = 0;
		Vector3 position = commandData.Unit.Position;
		float num3 = float.MaxValue;
		for (int j = 0; j < list.Count; j++)
		{
			float sqrMagnitude = (list[j] - position).sqrMagnitude;
			if (sqrMagnitude < num3)
			{
				num3 = sqrMagnitude;
				num2 = j;
			}
		}
		if (num2 > 0)
		{
			list.RemoveRange(0, num2);
			list.Insert(0, position);
		}
		UnitMoveToParams unitMoveToParams = new UnitMoveToParams(ForcedPath.Construct(list), commandData.EndPosition)
		{
			MovementType = ((Animation == WalkSpeedType.Sprint) ? WalkSpeedType.Walk : Animation)
		};
		commandData.CommandHandle = commandData.Unit.Commands.Run(unitMoveToParams);
		UnitMovementAgent movementAgent = commandData.Unit.View.MovementAgent;
		movementAgent.AvoidanceDisabled = true;
		if (OverrideSpeed && !Mathf.Approximately(SpeedMultiplier, 1f))
		{
			float? num4 = ReadAnimsetSpeed(commandData.Unit, unitMoveToParams.MovementType);
			if (num4.HasValue)
			{
				movementAgent.MaxSpeedOverride = num4.Value * SpeedMultiplier;
			}
		}
		movementAgent.OverridenAngularSpeed = 720f;
		return CommandResult.Success;
	}

	private static float? ReadAnimsetSpeed(AbstractUnitEntity unit, WalkSpeedType walkType)
	{
		UnitAnimationManager unitAnimationManager = unit.View?.AnimationManager;
		if (unitAnimationManager?.AnimationSet == null)
		{
			return null;
		}
		if (!(unitAnimationManager.AnimationSet.GetAction(UnitAnimationType.LocoMotion) is UnitAnimationActionLocomotion unitAnimationActionLocomotion))
		{
			return null;
		}
		WeaponStyleLocomotionData locomotionData = unitAnimationActionLocomotion.GetLocomotionData(unitAnimationManager.ActiveWeaponStyle);
		if (locomotionData == null)
		{
			return null;
		}
		return unitAnimationActionLocomotion.GetWalkingTypeData(locomotionData, walkType)?.Parameters?.Speed;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		if (!Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Failed to find unit");
		}
		IEntityView entityView = Path.FindView();
		VectorSpline vectorSpline = (entityView?.GO ? entityView.GO.GetComponent<VectorSpline>() : null);
		if (vectorSpline == null)
		{
			return CommandResult.Fail("No spline path");
		}
		List<Vector3> list = new List<Vector3>();
		int num = Mathf.Max(Mathf.CeilToInt(vectorSpline.Length * PointsPerMeter), 2);
		for (int i = 0; i < num; i++)
		{
			list.Add(vectorSpline.EvaluatePosition((float)i / (float)(num - 1)));
		}
		value.Translocate(list.LastItem(), null);
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (Game.Instance.Controllers.EntitySpawner.IsEntityInCreationQueue(commandData.Unit))
		{
			return false;
		}
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

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		UnitCommandHandle commandHandle = commandData.CommandHandle;
		if (commandHandle != null && !commandHandle.IsFinished)
		{
			commandHandle.Interrupt();
		}
		if (commandData.Unit?.View != null)
		{
			commandData.Unit.View.MovementAgent.MaxSpeedOverride = null;
			commandData.Unit.View.MovementAgent.OverridenAngularSpeed = null;
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		if (!Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Cant find unit");
		}
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.CommandHandle != null && commandData.EndPosition != Vector3.zero)
		{
			switch (commandData.CommandHandle?.Result)
			{
			case AbstractUnitCommand.ResultType.None:
				commandData.CommandHandle.Interrupt();
				break;
			case AbstractUnitCommand.ResultType.Fail:
			case AbstractUnitCommand.ResultType.Interrupt:
				value.Translocate(commandData.EndPosition, null);
				break;
			}
		}
		if (value.View != null)
		{
			value.View.MovementAgent.AvoidanceDisabled = false;
			value.View.MovementAgent.MaxSpeedOverride = null;
			value.View.MovementAgent.OverridenAngularSpeed = null;
			commandData.Unit?.View.MovementAgent.Blocker.BlockAtCurrentPosition();
		}
		if (Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			(value as UnitEntity)?.SnapToGrid();
		}
		return CommandResult.Success;
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
