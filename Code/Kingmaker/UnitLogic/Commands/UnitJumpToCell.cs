using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Controllers;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

public class UnitJumpToCell : UnitCommand<UnitJumpToCellParams>
{
	private AnimationActionHandle m_Handle;

	private Vector3 m_TransitionVector;

	public override bool IsMoveUnit => true;

	public override bool ShouldTurnToTarget => false;

	public UnitJumpToCell([NotNull] UnitJumpToCellParams @params)
		: base(@params)
	{
	}

	public override void Tick()
	{
		TimeController timeController = Game.Instance.Controllers.TimeController;
		base.TimeSinceStart += timeController.DeltaTime;
		using (ProfileScope.New("OnTick"))
		{
			OnTick();
		}
	}

	protected override ResultType OnAction()
	{
		return ResultType.None;
	}

	public override void OnRun()
	{
		base.OnRun();
		base.Executor.View.MovementAgent.Blocker.Unblock();
		base.Executor.View.MovementAgent.Blocker.BlockAt(base.ForcedPath.vectorPath.Last());
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (!base.Params.ProvokeAttackOfOpportunity)
		{
			base.Executor.Features.DoNotProvokeAttacksOfOpportunity.Retain();
		}
		StartAnimation();
		List<Vector3> vectorPath = base.ForcedPath.vectorPath;
		m_TransitionVector = vectorPath[vectorPath.Count - 1] - base.ForcedPath.vectorPath[0];
	}

	protected override void OnTick()
	{
		base.OnTick();
		float num = Mathf.Clamp01((base.TimeSinceStart - base.Params.StartJumpTime) / (base.Params.EndJumpTime - base.Params.StartJumpTime));
		base.Executor.Position = base.ForcedPath.vectorPath[0] + m_TransitionVector * num;
		if (base.TimeSinceStart > base.Params.TotalTransitionTime)
		{
			ForceFinish(ResultType.Success);
		}
	}

	private void StartAnimation()
	{
		AnimationClipWrapper animationClipWrapper = base.Params.JumpAnimationLink?.Load();
		if (!(animationClipWrapper != null))
		{
			return;
		}
		AnimationManager maybeAnimationManager = base.Executor.MaybeAnimationManager;
		if ((object)maybeAnimationManager != null)
		{
			UnitAnimationActionClip animationAction = UnitAnimationActionClip.Create(animationClipWrapper, "StartAnimation");
			maybeAnimationManager.TryExecute(animationAction, delegate(AnimationActionHandle h)
			{
				h.AnimationLayer = UnitAnimationLayerType.Actions;
			}, out m_Handle);
		}
	}

	protected override void OnEnded()
	{
		base.Executor.Position = base.ForcedPath.vectorPath.Last();
		if (!base.Params.ProvokeAttackOfOpportunity)
		{
			base.Executor.Features.DoNotProvokeAttacksOfOpportunity.Release();
		}
		base.OnEnded();
	}
}
