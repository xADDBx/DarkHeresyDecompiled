using Cinemachine;
using Kingmaker.Code.View.UI.UIUtilities;
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

	private RectTransform m_OwnRectTransform;

	private CanvasGroup m_CanvasGroup;

	private Rect m_ParentRect;

	private Vector3? m_CachedCanvasPosition;

	private Vector2 m_PositionCorrection;

	protected Vector2 PositionCorrectionFromView;

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
		if (ForceUpdatePosition)
		{
			ViewportPosition = UIUtilityRect.GetNormalizedPositionInCamera(base.ViewModel.Position);
			UpdatePosition(ViewportPosition);
		}
		if (!CheckVisibility)
		{
			SetCanvasGroupVisible(isVisible: false);
			return;
		}
		m_IsOnScreen = TryGetViewportPosition(out var viewportPos);
		ViewportPosition = viewportPos;
		bool flag = m_IsOnScreen && CheckVisibility;
		SetActiveInternal(flag);
		UpdateActive(flag);
		if (flag && !ForceUpdatePosition)
		{
			UpdatePosition(viewportPos);
		}
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
			m_PositionCorrection = PositionCorrectionFromView + new Vector2(0f, base.ViewModel.OvertipVerticalCorrection);
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
