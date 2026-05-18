using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityModifiersView : BrickBaseView<BrickAbilityModifiersVM>
{
	[SerializeField]
	private TMP_Text m_HeaderText;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private AbilityAppliedModifierView m_ModifierPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		m_TextHelper = new AccessibilityTextHelper(m_HeaderText).AddTo(this);
		m_HeaderText.SetText(base.ViewModel.HeaderText);
		m_TextHelper.UpdateTextSize();
		m_WidgetList.DrawEntries(base.ViewModel.Modifiers, m_ModifierPrefab).AddTo(this);
	}
}
