using System;
using Cinemachine;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class BaseOvertipView<TViewModel> : View<TViewModel> where TViewModel : OvertipEntityVM
{
	private static Rect s_CanvasRect = new Rect(-0.2f, -0.2f, 1.2f, 1.2f);

	private bool m_IsOnScreen;

	private int m_LastScreenWidth = -1;

	private int m_LastScreenHeight = -1;

	protected RectTransform m_OwnRectTransform;

	private CanvasGroup m_CanvasGroup;

	private Rect m_ParentRect;

	private Vector3? m_CachedCanvasPosition;

	private Vector2 m_PositionCorrection;

	protected Vector2 m_PositionCorrectionFromView;

	private bool m_WasVisible;

	private IDisposable m_FlashlightVisibilityTimer;

	private CanvasGroup CanvasGroup
	{
		get
		{
			if (!(m_CanvasGroup == null))
			{
				return m_CanvasGroup;
			}
			return m_CanvasGroup = this.EnsureComponent<CanvasGroup>();
		}
	}

	protected abstract bool CheckVisibility { get; }

	protected virtual bool ForceUpdatePosition => false;

	protected Vector3 ViewportPosition { get; private set; }

	protected override void OnBind()
	{
		UpdateParentRect();
		SetCanvasGroupVisible(isVisible: false);
		m_OwnRectTransform = (RectTransform)base.transform;
		RectTransform ownRectTransform = m_OwnRectTransform;
		Vector2 anchorMin = (m_OwnRectTransform.anchorMax = Vector2.zero);
		ownRectTransform.anchorMin = anchorMin;
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate), delegate
		{
			InternalUpdate();
		}).AddTo(this);
		CinemachineCore.CameraUpdatedEvent.AddListener(OnCinemachineUpdate);
	}

	protected override void OnUnbind()
	{
		CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCinemachineUpdate);
		base.gameObject.name = base.gameObject.name + " [deactivated]";
		m_CachedCanvasPosition = null;
		if (m_CanvasGroup != null)
		{
			SetActiveInternal(isActive: false);
		}
	}

	protected virtual void UpdateActive(bool isActive)
	{
	}

	protected virtual void SetActiveInternal(bool isActive)
	{
		SetCanvasGroupVisible(isActive);
	}

	private void OnCinemachineUpdate(CinemachineBrain arg0)
	{
		InternalUpdate();
	}

	private void InternalUpdate()
	{
		if (base.ViewModel == null)
		{
			PFLog.UI.Error(base.gameObject.name + ": ViewModel == null, but View are still not Destroyed");
			return;
		}
		if (ForceUpdatePosition || m_FlashlightVisibilityTimer != null)
		{
			ViewportPosition = UIUtilityRect.GetNormalizedPositionInCamera(base.ViewModel.Position);
			UpdatePosition(ViewportPosition);
		}
		if (!CheckVisibility)
		{
			if ((!Game.Instance.Player.Flashlight.FlashlightInUse || !TryCreateFlashlightVisibilityTimer()) && m_FlashlightVisibilityTimer == null)
			{
				m_FlashlightVisibilityTimer?.Dispose();
				m_FlashlightVisibilityTimer = null;
				SetCanvasGroupVisible(isVisible: false);
				m_WasVisible = false;
			}
			return;
		}
		m_IsOnScreen = TryGetViewportPosition(out var viewportPos);
		ViewportPosition = viewportPos;
		bool flag = m_IsOnScreen && CheckVisibility;
		m_WasVisible = CheckVisibility;
		m_FlashlightVisibilityTimer?.Dispose();
		m_FlashlightVisibilityTimer = null;
		SetActiveInternal(flag);
		UpdateActive(flag);
		if (flag && !ForceUpdatePosition)
		{
			UpdatePosition(viewportPos);
		}
	}

	private bool TryCreateFlashlightVisibilityTimer()
	{
		if (!m_WasVisible || CheckVisibility || m_FlashlightVisibilityTimer != null)
		{
			return false;
		}
		float flashlightDelayTimer = UIConfig.Instance.ExplorationConfig.FlashlightDelayTimer;
		m_FlashlightVisibilityTimer = ObservableSubscribeExtensions.Subscribe(Observable.Timer(flashlightDelayTimer.Seconds(), UnityTimeProvider.Update), delegate
		{
			m_WasVisible = CheckVisibility;
			m_FlashlightVisibilityTimer = null;
		}).AddTo(this);
		return true;
	}

	private void SetCanvasGroupVisible(bool isVisible)
	{
		CanvasGroup.alpha = (isVisible ? 1f : 0f);
		CanvasGroup.blocksRaycasts = isVisible;
	}

	private void UpdateParentRect()
	{
		m_ParentRect = ((RectTransform)base.transform.parent.parent).rect;
		m_LastScreenWidth = Screen.width;
		m_LastScreenHeight = Screen.height;
	}

	private void UpdatePosition(Vector3 canvasPosition)
	{
		if (CanvasGroup.alpha < float.Epsilon)
		{
			return;
		}
		int num;
		if (Screen.width == m_LastScreenWidth)
		{
			num = ((Screen.height != m_LastScreenHeight) ? 1 : 0);
			if (num == 0)
			{
				goto IL_003c;
			}
		}
		else
		{
			num = 1;
		}
		UpdateParentRect();
		goto IL_003c;
		IL_003c:
		bool flag = !m_CachedCanvasPosition.HasValue || (m_CachedCanvasPosition.Value - canvasPosition).sqrMagnitude > 9.999999E-09f;
		if (num != 0 || flag)
		{
			m_CachedCanvasPosition = canvasPosition;
			m_PositionCorrection = m_PositionCorrectionFromView + new Vector2(0f, base.ViewModel.OvertipVerticalCorrection);
			Vector2 vector = UIUtilityRect.NormalizedToPixelPosition(m_ParentRect, canvasPosition);
			m_OwnRectTransform.anchoredPosition = vector + m_PositionCorrection;
		}
	}

	private bool TryGetViewportPosition(out Vector3 viewportPos)
	{
		viewportPos = UIUtilityRect.GetNormalizedPositionInCamera(base.ViewModel.Position);
		return s_CanvasRect.Contains(viewportPos);
	}

	private void OnEnable()
	{
		SetCanvasGroupVisible(isVisible: false);
	}
}
