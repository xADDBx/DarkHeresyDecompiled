using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.Gameplay.Features.VariableInteractions;
using Kingmaker.UI.Sound;
using Kingmaker.View;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class VariativeInteractionView : View<VariativeInteractionVM>
{
	[Header("Prefabs")]
	[SerializeField]
	protected InteractionVariantView m_InteractionVariantViewPrefab;

	[SerializeField]
	protected NewInteractionVariantToggleView m_InteractionVariantToggleViewPrefab;

	[Header("Elements")]
	[SerializeField]
	protected WidgetList WidgetList;

	[SerializeField]
	protected RectTransform Viewport;

	[SerializeField]
	protected TMP_Text Title;

	[SerializeField]
	protected CanvasGroup Dots;

	[Header("Values")]
	[SerializeField]
	private float m_DefaultWidth = 220f;

	[SerializeField]
	private float m_WidthWithChance = 270f;

	[SerializeField]
	private float m_ToggleWidth = 320f;

	[SerializeField]
	private float m_AnimationDuration = 0.75f;

	[SerializeField]
	private Ease m_Ease = Ease.OutBounce;

	public void Awake()
	{
		base.gameObject.SetActive(value: false);
		Viewport.sizeDelta = Vector2.zero;
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		UISounds.Instance.Sounds.InteractionSounds.InteractionWindowOpen.Play();
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate), delegate
		{
			OnUpdateHandler();
		}).AddTo(this);
		OnUpdateHandler();
		Title.text = base.ViewModel.Title;
		Title.gameObject.SetActive(!string.IsNullOrEmpty(base.ViewModel.Title));
		List<MonoBehaviour> prefabs = new List<MonoBehaviour> { m_InteractionVariantToggleViewPrefab, m_InteractionVariantViewPrefab };
		WidgetList.DrawMultiEntries(base.ViewModel.Variants.EntitiesCollection, prefabs).AddTo(this);
		((RectTransform)base.transform).anchoredPosition = new Vector2(0f, 35f);
		RectTransform container = (RectTransform)WidgetList.Container;
		container.sizeDelta = new Vector2(GetContainerWidth(), container.sizeDelta.y);
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			Viewport.DOSizeDelta(container.sizeDelta, m_AnimationDuration).SetUpdate(isIndependentUpdate: true).SetEase(m_Ease);
			Dots.DOFade(1f, m_AnimationDuration).SetUpdate(isIndependentUpdate: true).SetEase(m_Ease);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		WidgetList.Clear();
		base.gameObject.SetActive(value: false);
		UISounds.Instance.Sounds.InteractionSounds.InteractionWindowClosed.Play();
		DOTween.Kill(Viewport);
		DOTween.Kill(Dots);
		Viewport.sizeDelta = Vector2.zero;
		Dots.alpha = 0f;
	}

	private float GetContainerWidth()
	{
		float result = m_DefaultWidth;
		if (base.ViewModel.VariativeType == VariativeType.ToggleGroup)
		{
			result = m_ToggleWidth;
		}
		else if (base.ViewModel.HasChance)
		{
			result = m_WidthWithChance;
		}
		return result;
	}

	private void OnUpdateHandler()
	{
		UpdatePosition();
		if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && !EventSystem.current.IsPointerOverGameObject())
		{
			base.ViewModel.Close();
		}
	}

	private void UpdatePosition()
	{
		if (!(CameraRig.Instance.Camera == null))
		{
			Vector2 vector = CameraRig.Instance.WorldToViewport(base.ViewModel.ObjectWorldPosition);
			RectTransform obj = (RectTransform)base.transform;
			Vector2 anchorMax = (obj.anchorMin = vector);
			obj.anchorMax = anchorMax;
		}
	}
}
