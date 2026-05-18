using System.Collections.Generic;
using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public sealed class BrickBuffGroupView : BrickBaseView<BrickBuffGroupVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_GroupName;

	[SerializeField]
	private TMP_Text m_GroupHintText;

	[SerializeField]
	private TMP_Text m_NoEffectsText;

	[SerializeField]
	private Image m_GroupIcon;

	[SerializeField]
	private GameObject m_EmptyState;

	[SerializeField]
	private WidgetList m_WidgetList;

	[Header("Views")]
	[SerializeField]
	private BuffListItemView m_BuffViewPrefab;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_GroupName, m_GroupHintText, m_NoEffectsText).AddTo(this);
		}
		m_GroupName.SetText(base.ViewModel.GroupName);
		m_NoEffectsText.SetText(base.ViewModel.NoEffectsText);
		m_GroupIcon.sprite = base.ViewModel.GroupIcon;
		base.ViewModel.Buffs.Subscribe(HandleBuffsChanged).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}

	private void HandleBuffsChanged(IReadOnlyList<BrickBuffVM> buffs)
	{
		bool flag = buffs.Count < 1;
		m_EmptyState.SetActive(flag);
		SetHint(!flag);
		m_WidgetList.Clear();
		m_WidgetList.DrawEntries(buffs, m_BuffViewPrefab).AddTo(this);
	}

	private void SetHint(bool showHint)
	{
		bool active = !string.IsNullOrEmpty(base.ViewModel.GroupHint) && showHint;
		m_GroupHintText.gameObject.SetActive(active);
		m_GroupHintText.SetText(base.ViewModel.GroupHint);
	}
}
