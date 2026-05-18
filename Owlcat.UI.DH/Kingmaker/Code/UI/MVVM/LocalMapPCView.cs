using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities.Base;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapPCView : LocalMapBaseView
{
	[Space]
	[SerializeField]
	private LocalMapRaycastTarget m_RaycastTarget;

	[Header("Right Buttons PC")]
	[SerializeField]
	private OwlcatMultiButton m_ZoomPlusButton;

	[SerializeField]
	private OwlcatMultiButton m_CenterOnRogueTraderButton;

	[SerializeField]
	private OwlcatMultiButton m_ZoomMinusButton;

	protected override void OnBind()
	{
		base.OnBind();
		ObservableSubscribeExtensions.Subscribe(m_ZoomPlusButton.OnLeftClick.AsObservable(), delegate
		{
			SetMapScale(1f);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ZoomMinusButton.OnLeftClick.AsObservable(), delegate
		{
			SetMapScale(-1f);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_CenterOnRogueTraderButton.OnLeftClick.AsObservable(), delegate
		{
			FindRogueTrader(smooth: true);
		}).AddTo(this);
		m_CenterOnRogueTraderButton.SetHint(UIStrings.Instance.LocalMapTexts.CenterOnRogueTrader).AddTo(this);
		m_ZoomPlusButton.SetHint(UIStrings.Instance.LocalMapTexts.ZoomMapPlus).AddTo(this);
		m_ZoomMinusButton.SetHint(UIStrings.Instance.LocalMapTexts.ZoomMapMinus).AddTo(this);
		m_RaycastTarget.Initialize(OnDrag, OnPointerClick, OnScroll);
	}

	protected override void InteractableRightButtons()
	{
		base.InteractableRightButtons();
		Vector3 localScale = m_Image.rectTransform.localScale;
		OwlcatMultiButton centerOnRogueTraderButton = m_CenterOnRogueTraderButton;
		OwlcatMultiButton zoomMinusButton = m_ZoomMinusButton;
		bool flag2 = (m_MinZoom.Value = localScale.x > base.ZoomMin && localScale.y > base.ZoomMin);
		bool interactable = (zoomMinusButton.Interactable = flag2);
		centerOnRogueTraderButton.Interactable = interactable;
		OwlcatMultiButton zoomPlusButton = m_ZoomPlusButton;
		interactable = (m_MaxZoom.Value = localScale.x < base.ZoomMax && localScale.y < base.ZoomMax);
		zoomPlusButton.Interactable = interactable;
	}

	public void OnDrag(PointerEventData eventData)
	{
		try
		{
			switch (eventData.button)
			{
			case PointerEventData.InputButton.Left:
			{
				Entity entity = eventData.pointerPress.GetComponent<LocalMapMarkerPCView>()?.GetEntity();
				base.ViewModel.OnClick(GetViewportPos(eventData), state: true, entity);
				break;
			}
			case PointerEventData.InputButton.Middle:
				UpdateMapPosition(eventData.delta);
				break;
			}
		}
		catch (Exception arg)
		{
			PFLog.UI.Error($"NullReferenceException {arg}");
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Middle)
		{
			Entity entity = eventData.pointerPress.GetComponent<LocalMapMarkerPCView>()?.GetEntity();
			base.ViewModel.OnClick(GetViewportPos(eventData), eventData.button == PointerEventData.InputButton.Left, entity);
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		SetMapScale((eventData.scrollDelta.y > 0f) ? 1 : (-1));
	}
}
