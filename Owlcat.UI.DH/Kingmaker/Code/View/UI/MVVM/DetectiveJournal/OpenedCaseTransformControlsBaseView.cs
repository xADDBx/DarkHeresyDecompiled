using System;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class OpenedCaseTransformControlsBaseView : View<OpenedCaseTransformControlsVM>
{
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

	private const float SoundScrollThreshold = 0.005f;

	protected override void OnBind()
	{
		m_Sliders.ForEach(delegate(SliderInteraction s)
		{
			s.Initialize(m_InteractingWithSlider, m_HasPointerOverSlider);
		});
		m_Transform.OnScrollAsObservable().Subscribe(OnScroll).AddTo(this);
		base.ViewModel.CurrentZoom.Subscribe(delegate(float value)
		{
			ZoomAtScreenPointViaPivot(Input.mousePosition);
			m_Transform.localScale = Vector3.one * value;
			float slider01Value = base.ViewModel.CurrentZoom01;
			m_Sliders.ForEach(delegate(SliderInteraction s)
			{
				s.SetValueWithoutNotify(slider01Value);
			});
		}).AddTo(this);
		m_Sliders.ForEach(delegate(SliderInteraction s)
		{
			s.OnValueChangedAsObservable().Subscribe(MoveSlider).AddTo(this);
		});
		EventBus.Subscribe(this).AddTo(this);
		m_Transform.pivot = Vector2.one * 0.5f;
	}

	private void OnScroll(PointerEventData eventData)
	{
		float currentZoom = base.ViewModel.CurrentZoom01;
		base.ViewModel.ChangeZoomBy((float)Math.Sign(eventData.scrollDelta.y) * m_ZoomSpeed);
		PlayScrollSound(Mathf.Clamp01(base.ViewModel.ZoomTo01(base.ViewModel.CurrentZoom.CurrentValue + (float)Math.Sign(eventData.scrollDelta.y) * m_ZoomSpeed)) - currentZoom);
	}

	private void MoveSlider(float newSliderValue)
	{
		float delta = base.ViewModel.ZoomTo01(newSliderValue) - base.ViewModel.CurrentZoom01;
		base.ViewModel.SetValueFromSlider(newSliderValue);
		PlayScrollSound(delta);
	}

	private static void PlayScrollSound(float delta)
	{
		if (!(delta > 0.005f))
		{
			if (delta < -0.005f)
			{
				UISounds.Instance.Play(UISounds.Instance.Sounds.Common.ZoomOutScroll);
			}
		}
		else
		{
			UISounds.Instance.Play(UISounds.Instance.Sounds.Common.ZoomInScroll);
		}
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
			float valueFromSlider = Mathf.Lerp(initSliderValue, 0.5f, x);
			base.ViewModel.SetValueFromSlider(valueFromSlider);
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
