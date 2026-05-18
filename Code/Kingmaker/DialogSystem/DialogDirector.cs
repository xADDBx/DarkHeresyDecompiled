using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.QA;
using Kingmaker.View;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.DialogSystem;

public class DialogDirector
{
	[CanBeNull]
	private DialogContext m_Context;

	private BlueprintCue m_CurrentCue;

	private FogOfWarRevealerSettings m_RevealedSpeaker;

	private UnitAnimationActionHandle m_CurrentSpeakerAnimation;

	private float m_FirstSpeakerReturnOrientation;

	private static CameraRig CameraRig => CameraRig.Instance;

	public void SetUpDialogScene([NotNull] DialogContext dialogContext)
	{
		m_Context = dialogContext;
		if (m_Context.FirstSpeaker != null && m_Context.Initiator != null)
		{
			m_FirstSpeakerReturnOrientation = m_Context.FirstSpeaker.DesiredOrientation;
			if (m_Context.Dialog.TurnFirstSpeaker)
			{
				TurnUnit(m_Context.FirstSpeaker, null);
			}
		}
		TryMoveCamera();
		if (m_Context.Dialog.TurnPlayer)
		{
			TurnUnit(Game.Instance.Player.MainCharacterEntity, null);
			TurnUnit(m_Context.Initiator, null);
		}
	}

	public void ResetDialogScene()
	{
		ObjectExtensions.Or(CameraRig, null)?.SetWorldOffset(Vector2.zero);
		if (m_Context == null)
		{
			return;
		}
		if (m_Context.FirstSpeaker != null && m_Context.Initiator != null)
		{
			m_Context.FirstSpeaker.DesiredOrientation = m_FirstSpeakerReturnOrientation;
		}
		List<BaseUnitEntity> list = m_Context.InvolvedUnits.ToList();
		Clear();
		foreach (BaseUnitEntity item in list)
		{
			item.StopLookAt();
			StopDialogAnimations(item);
			CutsceneControlledUnit.UpdateActiveCutscene(item);
		}
	}

	public void HandleExitedCue(BlueprintCue cue)
	{
		ReleaseAnimation();
	}

	public void HandleEnteringCue(BlueprintCue cue)
	{
		if (m_CurrentCue != cue)
		{
			TurnOffSpeakerHighlight();
		}
		m_CurrentCue = cue;
		if (m_CurrentCue == null || m_Context == null)
		{
			return;
		}
		BaseUnitEntity currentSpeaker = m_Context.CurrentSpeaker;
		PlayAnimation(currentSpeaker, m_CurrentCue);
		m_RevealedSpeaker = null;
		if (!m_CurrentCue.Speaker.NotRevealInFoW && currentSpeaker != null && currentSpeaker.IsInGame)
		{
			IUnitEntityView view = currentSpeaker.View;
			if (view != null)
			{
				FogOfWarRevealerSettings fogOfWarRevealer = view.FogOfWarRevealer;
				if (!fogOfWarRevealer || !fogOfWarRevealer.enabled)
				{
					m_RevealedSpeaker = view.gameObject.AddComponent<FogOfWarRevealerSettings>();
					m_RevealedSpeaker.Enable();
					m_RevealedSpeaker.DefaultRadius = false;
					m_RevealedSpeaker.Radius = 1f;
					FogOfWarControllerData.AddRevealer(m_RevealedSpeaker.transform);
				}
			}
		}
		if (m_CurrentCue.TurnSpeaker)
		{
			TurnUnit(currentSpeaker, m_CurrentCue.Listener);
		}
		TryMoveCamera();
	}

	public void TryMoveCamera()
	{
		if (m_Context == null || Game.Instance?.CurrentlyLoadedArea == null || CameraRig == null)
		{
			return;
		}
		BaseUnitEntity currentSpeaker = m_Context.CurrentSpeaker;
		if (currentSpeaker != null && m_CurrentCue?.Speaker != null && m_CurrentCue.Speaker.MoveCamera)
		{
			DialogRoot dialogRoot = ConfigRoot.Instance?.Dialog;
			if (dialogRoot != null)
			{
				CameraRig.SetWorldOffset(dialogRoot.DialogCameraCorrection);
				float cameraOffsetBySize = dialogRoot.GetCameraOffsetBySize(currentSpeaker.Size);
				Vector3 position = currentSpeaker.Position + Vector3.up * cameraOffsetBySize;
				CameraRig.ScrollTo(position);
			}
		}
	}

	public void Clear()
	{
		ReleaseAnimation();
		TurnOffSpeakerHighlight();
		m_CurrentCue = null;
		m_Context = null;
	}

	private void TurnUnit([CanBeNull] BaseUnitEntity unit, [CanBeNull] BlueprintUnit listener)
	{
		if (m_Context == null || unit == null || !unit.IsInGame)
		{
			return;
		}
		float listenerRange = ConfigRoot.Instance.Dialog.ListenerRange;
		BaseUnitEntity baseUnitEntity = null;
		if (listener != null)
		{
			baseUnitEntity = Game.Instance.EntityPools.AllBaseUnits.Where((BaseUnitEntity u) => u.Blueprint == listener).Nearest(unit.Position);
			if (baseUnitEntity != null && unit.DistanceTo(baseUnitEntity) > listenerRange)
			{
				baseUnitEntity = null;
			}
		}
		if (baseUnitEntity != null)
		{
			TurnUnitToLookAt(unit, baseUnitEntity.Position, changeOrientation: false);
		}
		else if (unit.IsDirectlyControllable)
		{
			Vector3 point = m_Context.FirstSpeaker?.Position ?? m_Context.DialogPosition;
			TurnUnitToLookAt(unit, point, changeOrientation: true);
		}
		else
		{
			BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
			BaseUnitEntity baseUnitEntity2 = ((unit.DistanceTo(mainCharacterEntity) <= listenerRange || m_Context.Initiator == null) ? mainCharacterEntity : m_Context.Initiator);
			TurnUnitToLookAt(unit, baseUnitEntity2.Position, changeOrientation: true);
		}
	}

	private static void TurnUnitToLookAt(BaseUnitEntity unit, Vector3 point, bool changeOrientation)
	{
		if (changeOrientation)
		{
			unit.StopLookAt();
			unit.TurnTo(point);
		}
		else
		{
			unit.LookAt(point + LookAtIKController.EyeShift);
		}
	}

	private void PlayAnimation([CanBeNull] BaseUnitEntity unit, BlueprintCue cue)
	{
		if (m_Context != null)
		{
			DialogAnimation animationType = cue.Animation;
			if (unit?.View != null && !(unit.View.AnimationManager == null) && animationType != 0 && !unit.View.AnimationManager.TryExecute(UnitAnimationType.Dialog, delegate(UnitAnimationActionHandle h)
			{
				h.Variant = (int)animationType;
				h.AnimationClipWrapper = ((animationType == DialogAnimation.Custom) ? cue.CustomAnimation : null);
			}, out m_CurrentSpeakerAnimation))
			{
				PFLog.Default.ErrorWithReport($"DialogCueAnimation is missing (dialog: {m_Context.Dialog.name}, unit: {unit})");
			}
		}
	}

	private void ReleaseAnimation()
	{
		if (m_CurrentSpeakerAnimation != null)
		{
			m_CurrentSpeakerAnimation.Release();
			m_CurrentSpeakerAnimation = null;
		}
	}

	private void TurnOffSpeakerHighlight()
	{
		if (m_RevealedSpeaker != null)
		{
			FogOfWarControllerData.RemoveRevealer(m_RevealedSpeaker.transform);
			Object.Destroy(m_RevealedSpeaker);
			m_RevealedSpeaker = null;
		}
	}

	private static void StopDialogAnimations(BaseUnitEntity unit)
	{
		IReadOnlyList<AnimationActionHandle> readOnlyList = ((unit.View != null && unit.View.AnimationManager != null) ? unit.View.AnimationManager.ActiveActions : null);
		if (readOnlyList == null)
		{
			return;
		}
		foreach (UnitAnimationActionHandle item in readOnlyList)
		{
			if (item.Action.Type == UnitAnimationType.Dialog)
			{
				item.Release();
			}
		}
	}
}
