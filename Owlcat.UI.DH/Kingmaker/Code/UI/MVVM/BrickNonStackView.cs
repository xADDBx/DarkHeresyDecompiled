using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickNonStackView : BrickBaseView<BrickNonStackVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Header;

	[SerializeField]
	private WidgetList m_WidgetList;

	[Header("Views")]
	[SerializeField]
	private NonStackEntityView m_EntityViewPrefab;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Header).AddTo(this);
		}
		base.OnBind();
		m_Header.text = UIStrings.Instance.Tooltips.NonStackHeaderLabel;
		m_WidgetList.DrawEntries(base.ViewModel.Entities, m_EntityViewPrefab).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}
}
