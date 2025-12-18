using Kingmaker.Code.UI.MVVM;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code.View.UI.MVVM;

public class TooltipElementAbilityWithModifierView : View<TooltipElementAbilityWithModifierVM>
{
	[SerializeField]
	private TextMeshProUGUI m_AbilityNameLabel;

	[SerializeField]
	private TextMeshProUGUI m_AbilityTagsLabel;

	[SerializeField]
	private TextMeshProUGUI m_AbilityCost;

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
