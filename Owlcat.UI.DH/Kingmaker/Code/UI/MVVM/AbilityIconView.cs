using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class AbilityIconView : View<AbilityIconVM>
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TMP_Text m_Acronym;

	protected override void OnBind()
	{
		m_Icon.sprite = base.ViewModel.Icon;
		m_Icon.color = base.ViewModel.IconColor;
		m_Acronym.SetText(base.ViewModel.Acronym);
	}
}
