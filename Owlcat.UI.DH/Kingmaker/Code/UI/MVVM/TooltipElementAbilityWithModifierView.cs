using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipElementAbilityWithModifierView : View<TooltipElementAbilityWithModifierVM>
{
	[SerializeField]
	private TMP_Text m_AbilityNameLabel;

	[SerializeField]
	private TMP_Text m_AbilityTagsLabel;

	[SerializeField]
	private TMP_Text m_AbilityCost;

	[SerializeField]
	private Image m_AbilityIcon;

	[SerializeField]
	private Image m_ModifierIcon;

	protected override void OnBind()
	{
		base.OnBind();
		m_AbilityNameLabel.text = base.ViewModel.Name;
		m_AbilityTagsLabel.text = base.ViewModel.Tags;
		m_AbilityCost.text = base.ViewModel.Cost;
		m_AbilityIcon.sprite = base.ViewModel.Icon;
		m_ModifierIcon.gameObject.SetActive(base.ViewModel.ModifierIcon != null);
		m_ModifierIcon.sprite = base.ViewModel.ModifierIcon;
		this.SetTooltip(base.ViewModel.Tooltip);
	}
}
