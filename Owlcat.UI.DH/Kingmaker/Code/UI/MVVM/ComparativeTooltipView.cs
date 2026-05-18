using System;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class ComparativeTooltipView : View<ComparativeTooltipVM>
{
	[Serializable]
	private struct LayoutsPair
	{
		public RectTransform VerticalLayout;

		public RectTransform HorizontalLayout;
	}

	[SerializeField]
	private HorizontalOrVerticalLayoutGroup m_Layout;

	[SerializeField]
	private RectTransform m_TooltipContainer;

	[SerializeField]
	private TooltipBaseView m_TooltipView;

	[SerializeField]
	private LayoutsPair m_MainLayouts;

	[SerializeField]
	private LayoutsPair m_ComparativeLayouts;

	private readonly List<TooltipBaseView> m_Widgets = new List<TooltipBaseView>();

	private CanvasGroup m_CanvasGroup;

	private Tweener m_ShowTween;

	private bool m_IsShowed;

	private bool m_IsInit;

	private RectTransform m_MainTooltipContainer;

	private RectTransform m_ComparativeTooltipContainer;

	private CanvasGroup CanvasGroup => m_CanvasGroup ?? (m_CanvasGroup = base.gameObject.EnsureComponent<CanvasGroup>());

	protected bool UseVerticalMainLayout
	{
		get
		{
			if (base.ViewModel.MainTooltips.Count > 1)
			{
				return base.ViewModel.CompareTooltips.Count > 0;
			}
			return false;
		}
	}

	protected bool UseVerticalComparativeLayout => base.ViewModel.CompareTooltips.Count > 1;

	public void Awake()
	{
		if (!m_IsInit)
		{
			base.gameObject.SetActive(value: false);
			m_IsInit = true;
		}
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_MainTooltipContainer = GetTooltipsContainer(isMain: true);
		m_ComparativeTooltipContainer = GetTooltipsContainer(isMain: false);
		foreach (TooltipVM mainTooltip in base.ViewModel.MainTooltips)
		{
			CreateTooltip(mainTooltip, m_MainTooltipContainer);
		}
		foreach (TooltipVM compareTooltip in base.ViewModel.CompareTooltips)
		{
			CreateTooltip(compareTooltip, m_ComparativeTooltipContainer);
		}
		base.gameObject.SetActive(value: true);
		CanvasGroup.alpha = 0f;
		DelayedInvoker.InvokeInTime(delegate
		{
			Show(base.ViewModel.Source);
		}, 0.2f).AddTo(this);
	}

	private void CreateTooltip(TooltipVM tooltipVM, RectTransform parentContainer)
	{
		TooltipBaseView widget = WidgetFactory.GetWidget(m_TooltipView);
		widget.transform.SetParent(parentContainer, worldPositionStays: false);
		widget.Bind(tooltipVM);
		m_Widgets.Add(widget);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		if (m_IsShowed)
		{
			ModalWindowsSounds.Instance.Tooltip.Hide.Play();
		}
		m_IsShowed = false;
		m_Widgets.ForEach(WidgetFactory.DisposeWidget);
		m_Widgets.Clear();
		CanvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: false);
		m_ShowTween?.Kill();
		m_ShowTween = null;
		m_MainTooltipContainer = null;
		m_ComparativeTooltipContainer = null;
	}

	private void Show(Transform source, List<Vector2> forcedPivots = null)
	{
		UIUtilityRect.SetPopupWindowPosition(m_TooltipContainer, source, Vector2.zero, forcedPivots ?? base.ViewModel.FirstMainTooltip.PriorityPivots);
		UpdateContainersOrder(source);
		m_ShowTween = CanvasGroup.DOFade(1f, 0.2f).OnComplete(delegate
		{
			ModalWindowsSounds.Instance.Tooltip.Show.Play();
			m_IsShowed = true;
		}).SetUpdate(isIndependentUpdate: true);
	}

	private void UpdateContainersOrder(Transform source)
	{
		bool reverseArrangement = m_TooltipContainer.InverseTransformPoint(source.position).x > 0f;
		m_Layout.reverseArrangement = reverseArrangement;
	}

	private RectTransform GetTooltipsContainer(bool isMain)
	{
		if (isMain)
		{
			if (!UseVerticalMainLayout)
			{
				return m_MainLayouts.HorizontalLayout;
			}
			return m_MainLayouts.VerticalLayout;
		}
		if (!UseVerticalComparativeLayout)
		{
			return m_ComparativeLayouts.HorizontalLayout;
		}
		return m_ComparativeLayouts.VerticalLayout;
	}
}
