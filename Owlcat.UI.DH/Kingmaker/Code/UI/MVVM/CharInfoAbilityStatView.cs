using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAbilityStatView : View<CharInfoAbilityStatVM>
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	protected override void OnBind()
	{
		base.OnBind();
		m_Value.text = base.ViewModel.AbilityStatValue;
		m_Icon.gameObject.SetActive(base.ViewModel.AbilityStatIcon != null);
		m_Icon.sprite = base.ViewModel.AbilityStatIcon;
		this.SetTooltip(base.ViewModel.AbilityStatTooltip);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
	}
}
