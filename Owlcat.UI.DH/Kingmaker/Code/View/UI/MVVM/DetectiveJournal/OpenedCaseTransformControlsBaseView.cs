using System;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI;
using Kingmaker.UI.Pointer;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class OpenedCaseTransformControlsBaseView : View<OpenedCaseTransformControlsVM>
{
	private enum ZoomType
	{
		ScrollBar,
		ZoomWheel
	}

	private class ZoomSoundCooldown : IDisposable
	{
		private readonly float m_CooldownDuration;

		private readonly CompositeDisposable m_TimerDisposable = new CompositeDisposable();

		private readonly UISound m_PositiveSound;

		private readonly UISound m_NegativeSound;

		private bool m_CanPlaySound = true;

		public ZoomSoundCooldown(ZoomType zoomType)
		{
			m_CooldownDuration = zoomType switch
			{
				ZoomType.ScrollBar => UIConfig.Instance.DetectiveConfig.ZoomSliderCooldown, 
				ZoomType.ZoomWheel => UIConfig.Instance.DetectiveConfig.ZoomWheelCooldown, 
				_ => 0.15f, 
			};
			m_PositiveSound = zoomType switch
			{
				ZoomType.ScrollBar => SystemSounds.Instance.Controls.ZoomInScroll, 
				ZoomType.ZoomWheel => SystemSounds.Instance.Controls.ZoomInWheel, 
				_ => UISounds.Instance.Sounds.DoNothingEvent, 
			};
			m_NegativeSound = zoomType switch
			{
				ZoomType.ScrollBar => SystemSounds.Instance.Controls.ZoomOutScroll, 
				ZoomType.ZoomWheel => SystemSounds.Instance.Controls.ZoomOutWheel, 
				_ => UISounds.Instance.Sounds.DoNothingEvent, 
			};
		}

		public bool TryPlaySound(float delta)
		{
			if (Math.Abs(delta) < UIConfig.Instance.DetectiveConfig.SoundScrollNormalizedThreshold)
			{
				return false;
			}
			if (!m_CanPlaySound)
			{
				return false;
			}
			UISound type = ((delta > 0f) ? m_PositiveSound : m_NegativeSound);
			UISounds.Instance.Play(type);
			StartCooldown();
			return true;
		}

		private void StartCooldown()
		{
			m_TimerDisposable.Clear();
			m_CanPlaySound = false;
			ObservableSubscribeExtensions.Subscribe(Observable.Timer(m_CooldownDuration.Seconds(), UnityTimeProvider.UpdateIgnoreTimeScale), delegate
			{
				m_CanPlaySound = true;
			}).AddTo(m_TimerDisposable);
		}

		public void Dispose()
		{
			m_TimerDisposable?.Dispose();
		}
	}

	[Header("Elements")]
	[SerializeField]
	protected RectTransform m_Transform;

	[SerializeField]
	protected RectTransform m_CaseCard;

	[SerializeField]
	private List<SliderInteraction> m_Sliders;

	[Header("Values")]
	[SerializeField]
	private float m_ZoomSpeed = 7f;

	[SerializeField]
	private float m_MoveToClueTime = 0.25f;

	private readonly ReactiveProperty<bool> m_InteractingWithSlider = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasPointerOverSlider = new ReactiveProperty<bool>();

	private Tweener m_MoveToClueTweener;

	private bool m_ResettingPosition;

	private ZoomSoundCooldown m_WheelSoundCooldown;

	private ZoomSoundCooldown m_SliderSoundCooldown;

	protected override void OnBind()
	{
		m_WheelSoundCooldown = new ZoomSoundCooldown(ZoomType.ZoomWheel).AddTo(this);
		m_SliderSoundCooldown = new ZoomSoundCooldown(ZoomType.ScrollBar).AddTo(this);
		m_Sliders.ForEach(delegate(SliderInteraction s)
		{
			s.Initialize(m_InteractingWithSlider, m_HasPointerOverSlider);
		});
		m_Transform.OnScrollAsObservable().Subscribe(OnScroll).AddTo(this);
		base.ViewModel.CurrentZoom.Subscribe(delegate(float value)
		{
			ZoomAtScreenPointViaPivot(CursorController.CursorPosition);
			m_Transform.localScale = Vector3.one * value;
			float slider01Value = base.ViewModel.CurrentZoom01;
			m_Sliders.ForEach(delegate(SliderInteraction s)
			{
				s.SetValueWithoutNotify(slider01Value);
			});
		}).AddTo(this);
		m_Sliders.ForEach(delegate(SliderInteraction s)
		{
			s.OnValueChangedAsObservable().Skip(1).Subscribe(MoveSlider)
				.AddTo(this);
		});
		EventBus.Subscribe(this).AddTo(this);
		m_Transform.pivot = Vector2.one * 0.5f;
	}

	private void OnScroll(PointerEventData eventData)
	{
		float currentZoom = base.ViewModel.CurrentZoom01;
		base.ViewModel.ChangeZoomBy((float)Math.Sign(eventData.scrollDelta.y) * m_ZoomSpeed);
		float value = base.ViewModel.ZoomTo01(base.ViewModel.CurrentZoom.CurrentValue + (float)Math.Sign(eventData.scrollDelta.y) * m_ZoomSpeed);
		value = Mathf.Clamp01(value);
		TryPlayScrollSound(value - currentZoom, ZoomType.ZoomWheel);
	}

	private void MoveSlider(float newSliderValue)
	{
		float delta = base.ViewModel.ZoomTo01(newSliderValue) - base.ViewModel.CurrentZoom01;
		base.ViewModel.SetZoomValue(newSliderValue);
		TryPlayScrollSound(delta, ZoomType.ScrollBar);
	}

	private bool TryPlayScrollSound(float delta, ZoomType type)
	{
		return ((ZoomSoundCooldown)(type switch
		{
			ZoomType.ScrollBar => m_SliderSoundCooldown, 
			ZoomType.ZoomWheel => m_WheelSoundCooldown, 
			_ => null, 
		}))?.TryPlaySound(delta) ?? false;
	}

	private void ZoomAtScreenPointViaPivot(Vector2 screenPoint)
	{
		Vector2 localPoint = Vector2.zero;
		if (!m_InteractingWithSlider.Value && !m_ResettingPosition)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Transform, screenPoint, UICamera.Instance, out localPoint);
		}
		Rect rect = m_Transform.rect;
		Vector2 vector = new Vector2(Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x), Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y));
		vector = new Vector2(Mathf.Clamp01(vector.x), Mathf.Clamp01(vector.y));
		Vector3 vector2 = m_Transform.TransformPoint(localPoint);
		m_Transform.pivot = vector;
		Vector3 vector3 = m_Transform.TransformPoint(Vector2.zero);
		m_Transform.position += vector2 - vector3;
		OnDrag(m_Transform.anchoredPosition);
	}

	protected void OnDrag(PointerEventData eventData)
	{
		OnDrag(eventData.delta);
	}

	private void OnDrag(Vector2 screenDelta)
	{
		DOTween.Kill(m_MoveToClueTweener);
		Vector2 sizeDelta = m_Transform.sizeDelta;
		Vector2 vector = -new Vector2(screenDelta.x / sizeDelta.x, screenDelta.y / sizeDelta.y) / base.ViewModel.CurrentZoom.CurrentValue;
		Vector2 pivot = m_Transform.pivot + vector;
		pivot.x = Mathf.Clamp01(pivot.x);
		pivot.y = Mathf.Clamp01(pivot.y);
		m_Transform.pivot = pivot;
		m_Transform.anchoredPosition = Vector2.zero;
	}

	public void HandleMoveToPosition(Vector3 worldPos)
	{
		Vector3 vector = m_Transform.InverseTransformPoint(worldPos);
		Vector2 endValue = m_Transform.pivot + ScreenPosToPivot(vector);
		DOTween.Kill(m_MoveToClueTweener);
		m_MoveToClueTweener = m_Transform.DOPivot(endValue, m_MoveToClueTime).SetUpdate(isIndependentUpdate: true);
	}

	protected void ResetPosition()
	{
		m_ResettingPosition = true;
		float initSliderValue = base.ViewModel.CurrentZoom01;
		Vector2 initPivot = m_Transform.pivot;
		Vector3 vector = m_Transform.InverseTransformPoint(m_CaseCard.position);
		Vector2 endValue = initPivot + ScreenPosToPivot(vector);
		DOTween.To(() => 0f, delegate(float x)
		{
			float zoomValue = Mathf.Lerp(initSliderValue, 0.5f, x);
			base.ViewModel.SetZoomValue(zoomValue);
			m_Transform.pivot = Vector2.Lerp(initPivot, endValue, x);
		}, 1f, m_MoveToClueTime).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			m_Transform.pivot = endValue;
			m_ResettingPosition = false;
		});
	}

	private Vector2 ScreenPosToPivot(Vector2 pos)
	{
		Vector2 sizeDelta = m_Transform.sizeDelta;
		return new Vector2(pos.x / sizeDelta.x, pos.y / sizeDelta.y);
	}
}
