using System;
using Kingmaker.Utility.Attributes;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public sealed class DetailedBuffsBlockView : BuffsBlockView
{
	private const char MultiplicationSign = '×';

	[SerializeField]
	private MonoBehaviour m_TooltipSource;

	[ShowIf("m_HasDotStacksText")]
	[SerializeField]
	private RectTransform m_LayoutGroup;

	[Space]
	[ShowIf("m_HasDotStacksText")]
	[SerializeField]
	private TMP_Text m_DotStacksText;

	[ShowIf("m_HasDotStacksText")]
	[SerializeField]
	private RectTransform m_DotEffectsContainer;

	[ShowIf("m_HasDotStacksText")]
	[SerializeField]
	private float m_DotEffectsWidthPadding;

	[ShowIf("m_HasDotStacksText")]
	[SerializeField]
	private float m_DotEffectsNoTextWidth;

	[SerializeField]
	private bool m_HasDotStacksText;

	private IDisposable m_Tooltip;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.BuffsTooltip.Subscribe(HandleTooltipChanged).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_Tooltip?.Dispose();
		m_Tooltip = null;
	}

	protected override void HandleStatusEffectsChanged(StatusEffectsUIData data)
	{
		base.HandleStatusEffectsChanged(data);
		UpdateDOTStacksText();
	}

	protected override void HandleCriticalEffectsChanged(CriticalEffectsUIData data)
	{
		base.HandleCriticalEffectsChanged(data);
		UpdateDOTStacksText();
	}

	protected override void HandleDOTEffectsChanged(DOTEffectsUIData data)
	{
		base.HandleDOTEffectsChanged(data);
		UpdateDOTStacksText();
	}

	private void HandleTooltipChanged(TooltipBaseTemplate tooltipTemplate)
	{
		m_Tooltip?.Dispose();
		m_Tooltip = null;
		if (tooltipTemplate != null)
		{
			m_Tooltip = m_TooltipSource.SetTooltip(tooltipTemplate);
		}
	}

	private void UpdateDOTStacksText()
	{
		if (m_HasDotStacksText)
		{
			int count = base.ViewModel.DOTEffects.CurrentValue.DotEffects.Count;
			bool flag = base.ViewModel.StatusEffects.CurrentValue.Count > 0;
			bool flag2 = base.ViewModel.CriticalEffects.CurrentValue.Count > 0;
			int num = ((base.ViewModel.DOTEffects.CurrentValue.DotEffects.Count > 0) ? base.ViewModel.DOTEffects.CurrentValue.DotEffects[0].rank : 0);
			bool flag3 = count == 1 && !(flag && flag2) && num < 10;
			if (flag3)
			{
				m_DotStacksText.SetText($"{'×'}{num}");
			}
			m_DotStacksText.gameObject.SetActive(flag3);
			UpdateDOTWidgetWidth(flag3);
		}
	}

	private void UpdateDOTWidgetWidth(bool showCounter)
	{
		float size = (showCounter ? (m_DotStacksText.preferredWidth + m_DotEffectsWidthPadding) : m_DotEffectsNoTextWidth);
		m_DotEffectsContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
		LayoutRebuilder.MarkLayoutForRebuild(m_LayoutGroup);
	}

	[ContextMenu("Update DOT Widget width")]
	private void EditorUpdateDOTWidgetWidth()
	{
		UpdateDOTWidgetWidth(m_DotStacksText.gameObject.activeSelf);
	}
}
