using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.Random;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Serializable]
[ComponentName("Command/CommandUnitPlayCutsceneAnimation")]
[TypeId("5d582208451380241b50fe71a9d94298")]
public sealed class CommandUnitPlayCutsceneAnimation : CommandBase
{
	public class Data
	{
		internal bool Started;

		internal bool Finished;

		internal float FiniteLoopDuration;

		internal bool SkippedByPlayer;

		internal bool IsFirstRun = true;

		internal UnitAnimationAction Action;

		internal AnimationActionHandle Handle;

		internal AnimationClipWrapper CutsceneClipWrapper;

		[CanBeNull]
		internal AbstractUnitEntity Unit;
	}

	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	[Obsolete]
	[HideInInspector]
	private AnimationClipWrapper m_CutsceneClipWrapper;

	[SerializeField]
	[ValidateNotNull]
	private AnimationClipWrapperLink m_CutsceneClipWrapperLink;

	[SerializeField]
	[InfoBox("If true, animation would not start until previous one finishes")]
	private bool m_WaitForCurrentAnimation;

	[SerializeField]
	[FormerlySerializedAs("m_DontReturnToIdle")]
	[InfoBox("If true, command would finish a bit before animation, allowing next animation to blend smoothly")]
	private bool m_ExitBeforeDone;

	[SerializeField]
	private float m_CrossfadeIn = 0.1f;

	[SerializeField]
	private float m_CrossfadeOut = 0.1f;

	[SerializeField]
	private float m_AnimationSpeed = 1f;

	[SerializeField]
	private bool m_LockRotation;

	[SerializeField]
	private bool m_MarkUnit;

	[SerializeField]
	private bool m_OnlyIfIdle;

	[SerializeField]
	private bool m_FiniteLoop;

	[SerializeField]
	private bool m_RandomizeStart;

	[SerializeField]
	[ShowIf("m_FiniteLoop")]
	private float m_FiniteLoopDuration;

	[SerializeField]
	[ShowIf("m_FiniteLoop")]
	private bool m_FiniteLoopRandomDuration;

	[SerializeField]
	[ShowIf("m_FiniteLoopRandomDuration")]
	private float m_FiniteLoorDurationMax;

	[SerializeField]
	[HideIf("IsContinuous")]
	[Tooltip("Timeout in case something breaks, forces this command to stop after this many seconds")]
	private float m_Timeout = 20f;

	[SerializeField]
	private bool m_UseAvatarMask;

	[SerializeField]
	[ShowIf("m_UseAvatarMask")]
	private AvatarMask m_AvatarMask;

	[SerializeField]
	private bool m_RerunBeforeReleaseAnimation;

	private bool? m_CutsceneClipIsLooping;

	public override bool IsContinuous
	{
		get
		{
			if (ClipIsLooping)
			{
				return !m_FiniteLoop;
			}
			return false;
		}
	}

	private bool ClipIsLooping
	{
		get
		{
			bool valueOrDefault = m_CutsceneClipIsLooping.GetValueOrDefault();
			if (!m_CutsceneClipIsLooping.HasValue)
			{
				valueOrDefault = GetIsLooping();
				m_CutsceneClipIsLooping = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public AbstractUnitEvaluator UnitEvaluator => m_Unit;

	public override bool ShouldHaveControlledUnit => m_MarkUnit;

	private bool GetIsLooping()
	{
		AnimationClipWrapper animationClipWrapper = m_CutsceneClipWrapperLink?.Load();
		if (animationClipWrapper != null)
		{
			return animationClipWrapper.IsLooping;
		}
		return false;
	}

	public void Preload()
	{
		m_CutsceneClipWrapperLink.Load();
	}

	public override bool TrySkip(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).SkippedByPlayer = true;
		return true;
	}

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Finished = false;
		commandData.Started = false;
		if (!m_Unit.TryGetValue(out var value))
		{
			commandData.Finished = true;
			return CommandResult.Fail("Failed to find unit");
		}
		commandData.Unit = value;
		commandData.FiniteLoopDuration = (m_FiniteLoopRandomDuration ? player.Random.Range(m_FiniteLoopDuration, m_FiniteLoorDurationMax) : m_FiniteLoopDuration);
		if (skipping)
		{
			commandData.Finished = true;
			commandData.Handle?.Release();
			return CommandResult.Success;
		}
		return PlayAnimation(commandData, player);
	}

	private CommandResult PlayAnimation(Data data, CutscenePlayerData player)
	{
		AbstractUnitEntity unit = data.Unit;
		if (unit == null || !unit.IsInGame)
		{
			data.Finished = true;
			data.Handle?.Release();
			return CommandResult.Success;
		}
		if (unit.LifeState.IsDead)
		{
			data.Finished = true;
			data.Handle?.Release();
			return CommandResult.Success;
		}
		if (data.CutsceneClipWrapper == null)
		{
			data.CutsceneClipWrapper = m_CutsceneClipWrapperLink.Load();
			if (!data.CutsceneClipWrapper)
			{
				data.Finished = true;
				return CommandResult.Fail($"No clip found for {this} in {player.Cutscene}");
			}
		}
		IAbstractUnitEntityView view = unit.View;
		UnitAnimationManager animationManager = view.AnimationManager;
		if (!animationManager)
		{
			data.Finished = true;
			return CommandResult.FailWithReport($"{view}: no animator");
		}
		if (!data.Started && !data.Finished && (!m_OnlyIfIdle || animationManager.CanRunIdleAction()))
		{
			data.Action = UnitAnimationActionClip.Create(data.CutsceneClipWrapper, "PlayAnimation");
			if (m_UseAvatarMask)
			{
				data.Action.AvatarMasks = new List<AvatarMask> { m_AvatarMask };
			}
			data.Action.TransitionIn = m_CrossfadeIn;
			data.Action.TransitionOut = m_CrossfadeOut;
			data.Action.ExecutionMode = (m_WaitForCurrentAnimation ? ExecutionMode.Sequenced : ExecutionMode.Interrupted);
			animationManager.TryExecute(data.Action, delegate(AnimationActionHandle handle)
			{
				handle.PreventsRotation = m_LockRotation;
				handle.HasCrossfadePriority = true;
				handle.SpeedScale = m_AnimationSpeed;
			}, out data.Handle);
			if (m_RandomizeStart && data.IsFirstRun)
			{
				data.Handle?.ActiveAnimation?.SetTime(PFStatefulRandom.Visuals.Animation.value * data.CutsceneClipWrapper.Length);
			}
			data.Started = true;
			data.IsFirstRun = false;
		}
		if (data.Started)
		{
			if (data.Handle.IsReleased)
			{
				data.Finished = true;
			}
			else if ((!IsContinuous || data.SkippedByPlayer) && m_ExitBeforeDone && data.Handle.GetTime() >= data.CutsceneClipWrapper.Length - m_CrossfadeOut - RealTimeController.SystemStepDurationSeconds * 3f)
			{
				data.Finished = true;
			}
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<Data>(this).Finished;
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData == null)
		{
			return CommandResult.Success;
		}
		if (m_FiniteLoop || !commandData.Finished || (!m_ExitBeforeDone && commandData.Handle != null && !commandData.Handle.IsFinished))
		{
			commandData.Handle?.Release();
			if (commandData.Handle?.Action != null)
			{
				UnityEngine.Object.Destroy(commandData.Handle.Action);
			}
		}
		if (!m_Unit.TryGetValue(out var value))
		{
			return CommandResult.Fail("Unit not found");
		}
		if (value.LifeState.IsDead)
		{
			commandData.Finished = true;
			return CommandResult.Success;
		}
		if (value.View == null)
		{
			return CommandResult.Fail("Unit view is null");
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (m_FiniteLoop)
		{
			float num = commandData.FiniteLoopDuration;
			if (m_ExitBeforeDone)
			{
				num -= m_CrossfadeOut;
			}
			if (time > (double)num)
			{
				commandData.Finished = true;
			}
		}
		PlayAnimation(commandData, player);
		if ((commandData.SkippedByPlayer || !IsContinuous) && time > (double)m_Timeout)
		{
			commandData.Finished = true;
		}
		if (!commandData.SkippedByPlayer && IsContinuous && m_RerunBeforeReleaseAnimation && commandData.Handle.IsReleased)
		{
			return OnRun(player, skipping: false);
		}
		return CommandResult.Success;
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if ((commandData.SkippedByPlayer || !IsContinuous) && !commandData.Finished)
		{
			return Stop(player);
		}
		return CommandResult.Success;
	}

	public override bool TryPrepareForStop(CutscenePlayerData player)
	{
		if ((!player.GetCommandData<Data>(this).SkippedByPlayer && IsContinuous) || !IsFinished(player))
		{
			return false;
		}
		return StopPlaySignalIsReady(player);
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!m_MarkUnit || m_Unit == null || !m_Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}

	public override string GetCaption()
	{
		string text = string.Empty;
		if (m_FiniteLoop)
		{
			text = ((!m_FiniteLoopRandomDuration) ? $" {m_FiniteLoopDuration} secs" : $" {m_FiniteLoopDuration}-{m_FiniteLoorDurationMax} secs");
		}
		return (m_Unit?.GetCaptionShort() ?? "(none)") + " <b>animation</b> " + (m_CutsceneClipWrapperLink?.LoadAsset()?.AnimationClip.name ?? "(none)") + text;
	}
}
