using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public sealed class BrickBuffGroupsView : BrickBaseView<BrickBuffGroupsVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private WidgetList m_WidgetList;

	[Header("Views")]
	[SerializeField]
	private BrickBuffGroupView m_BuffGroupPrefab;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Title).AddTo(this);
		}
		m_Title.text = base.ViewModel.TitleText;
		m_WidgetList.DrawEntries(base.ViewModel.Groups, m_BuffGroupPrefab).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}
}
