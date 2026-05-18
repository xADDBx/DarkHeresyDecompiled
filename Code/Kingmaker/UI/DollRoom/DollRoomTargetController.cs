using DG.Tweening;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.UnityExtensions;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.UI.DollRoom;

public class DollRoomTargetController : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IEndDragHandler, IDragHandler, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
{
	private const float MaxAngularSpeed = 360f;

	[SerializeField]
	private RawImage m_RawImage;

	[Space]
	[SerializeField]
	private float m_ResetRotationDuration = 0.25f;

	[SerializeField]
	private AnimationCurve m_ResetRotationCurve;

	private readonly ReactiveProperty<float> m_CurrentZoomNormalized = new ReactiveProperty<float>();

	private readonly ReactiveProperty<float> m_CurrentRotationAngle = new ReactiveProperty<float>();

	private readonly ReactiveProperty<bool> m_IsHoveredOver = new ReactiveProperty<bool>();

	private Transform m_TargetDoll;

	private PointerEventData m_EventData;

	private DollCamera m_DollCamera;

	private bool m_IsHovering;

	private bool m_IsDragging;

	private Tween m_ResetViewTween;

	private bool m_IsRotating;

	private float m_LastRotateTime;

	private float m_RotateStopDelay = 0.15f;

	private DollCamera DollCamera
	{
		get
		{
			if (!m_DollCamera)
			{
				return m_DollCamera = DollCamera.Current;
			}
			return m_DollCamera;
		}
	}

	public RawImage RawImage => m_RawImage;

	public ReadOnlyReactiveProperty<float> CurrentZoomNormalized => m_CurrentZoomNormalized;

	public ReadOnlyReactiveProperty<float> CurrentRotationAngle => m_CurrentRotationAngle;

	public ReadOnlyReactiveProperty<bool> IsHoveredOver => m_IsHoveredOver;

	public Vector2 GetRawImageSize()
	{
		RectTransform rectTransform = (RectTransform)m_RawImage.transform;
		if (rectTransform.rect.size.x == 0f)
		{
			return new Vector2(1920f, 1080f);
		}
		return rectTransform.rect.size;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDollCharacterDragUIHandler h)
		{
			h.StartDrag();
		});
		if ((bool)DollCamera && eventData != null && eventData.button == PointerEventData.InputButton.Right)
		{
			DollCamera.BeginZoom();
		}
		m_EventData = eventData;
		m_IsDragging = true;
		GameUIState.Instance.IsInventoryDollRotating = true;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		HandleDragEnd();
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			float amount = (0f - Mathf.Min(Mathf.Abs(eventData.delta.x * 0.4f), 360f * Time.unscaledDeltaTime)) * Mathf.Sign(eventData.delta.x);
			Rotate(amount);
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		Zoom(eventData.scrollDelta.y);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		m_IsHovering = true;
		UpdateIsHovered();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		m_IsHovering = false;
		UpdateIsHovered();
	}

	public void Zoom(float value)
	{
		if ((bool)DollCamera)
		{
			DollCamera.Zoom(value);
			UpdateCurrentZoom();
		}
	}

	public void Rotate(float amount)
	{
		if (!m_TargetDoll)
		{
			return;
		}
		if (Mathf.Abs(amount) > 0.01f)
		{
			if (!m_IsRotating)
			{
				m_IsRotating = true;
				ServiceWindowsSounds.Instance.Inventory.RotateDollStart.Play();
				ServiceWindowsSounds.Instance.Inventory.RotateDollLoopStart.Play();
			}
			m_LastRotateTime = Time.unscaledTime;
		}
		m_ResetViewTween?.Kill();
		m_TargetDoll.Rotate(Vector3.up, amount, Space.Self);
		m_CurrentRotationAngle.Value = m_TargetDoll.eulerAngles.y;
	}

	public void ZoomMin()
	{
		if ((bool)DollCamera)
		{
			DollCamera.ZoomMin();
			UpdateCurrentZoom();
		}
	}

	public void ZoomMax()
	{
		if ((bool)DollCamera)
		{
			DollCamera.ZoomMax();
			UpdateCurrentZoom();
		}
	}

	public void SetTarget(Transform targetPlaceholder)
	{
		m_TargetDoll = targetPlaceholder;
		m_TargetDoll.localRotation = Quaternion.identity;
		m_CurrentRotationAngle.Value = 0f;
	}

	public void ResetTargetView()
	{
		m_ResetViewTween?.Kill();
		m_ResetViewTween = m_TargetDoll.DOLocalRotate(Vector3.zero, m_ResetRotationDuration).SetEase(m_ResetRotationCurve).SetUpdate(isIndependentUpdate: true)
			.OnUpdate(delegate
			{
				m_CurrentRotationAngle.Value = m_TargetDoll.eulerAngles.y;
			});
		if ((bool)DollCamera)
		{
			DollCamera.ZoomMax();
			m_CurrentZoomNormalized.Value = 0f;
		}
	}

	private void HandleDragEnd()
	{
		m_EventData = null;
		EventBus.RaiseEvent(delegate(IDollCharacterDragUIHandler h)
		{
			h.EndDrag();
		});
		if ((bool)DollCamera)
		{
			DollCamera.EndZoom();
		}
		m_IsDragging = false;
		GameUIState.Instance.IsInventoryDollRotating = false;
		UpdateIsHovered();
	}

	private void UpdateCurrentZoom()
	{
		m_CurrentZoomNormalized.Value = 1f - DollCamera.ZoomNormalized;
	}

	private void UpdateIsHovered()
	{
		m_IsHoveredOver.Value = m_IsHovering || m_IsDragging;
	}

	private void Update()
	{
		if (m_EventData != null && (bool)DollCamera && m_EventData.button == PointerEventData.InputButton.Right)
		{
			Zoom(m_EventData.delta.y * 0.1f);
		}
		CheckDollRotation();
	}

	private void CheckDollRotation()
	{
		if (m_IsRotating && Time.unscaledTime - m_LastRotateTime > m_RotateStopDelay)
		{
			m_IsRotating = false;
			ServiceWindowsSounds.Instance.Inventory.RotateDollStop.Play();
			ServiceWindowsSounds.Instance.Inventory.RotateDollLoopStop.Play();
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (!ApplicationFocusEvents.DollRoomDisabled && m_EventData != null)
		{
			HandleDragEnd();
		}
	}

	private void OnDisable()
	{
		m_ResetViewTween?.Kill();
	}
}
