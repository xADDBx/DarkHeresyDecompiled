using System.Collections.Generic;
using Code.View.UI.Helpers;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharInfoStatusEffectsView : CharInfoComponentView<CharInfoStatusEffectsVM>
{
	[SerializeField]
	private TextMeshProUGUI m_StatusEffectsTitle;

	[SerializeField]
	private FadeAnimator m_NoStatusContainer;

	[SerializeField]
	private TextMeshProUGUI m_NoStatusEffectsLabel;

	[SerializeField]
	private ScrollRectExtended m_Scroll;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private CharInfoBuffGroupView m_GroupViewPrefab;

	private AccessibilityTextHelper m_TextHelper;

	protected IReadOnlyList<IBindable> Widgets => m_WidgetList.Entries;

	public override void Initialize()
	{
		base.Initialize();
		m_TextHelper = new AccessibilityTextHelper(m_StatusEffectsTitle, m_NoStatusEffectsLabel);
	}

	protected override void OnBind()
	{
		base.OnBind();
		SetupLabels();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_WidgetList.Clear();
		m_TextHelper.Dispose();
	}

	protected void EnsureScrollRect(RectTransform rectTransform)
	{
		m_Scroll.EnsureVisibleVertical(rectTransform);
	}

	private void SetupLabels()
	{
		m_StatusEffectsTitle.text = base.ViewModel.StatusEffectsTitleText;
		m_NoStatusEffectsLabel.text = base.ViewModel.NoEffectsText;
		m_TextHelper.UpdateTextSize();
	}

	protected override void RefreshView()
	{
		DrawEntities();
		DrawNoBuffsLabel();
		m_Scroll.ScrollToTop();
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.BuffGroups, m_GroupViewPrefab);
	}

	private void DrawNoBuffsLabel()
	{
		if (base.ViewModel.NoBuffs)
		{
			m_NoStatusContainer.AppearAnimation();
		}
		else
		{
			m_NoStatusContainer.DisappearAnimation();
		}
	}
}
