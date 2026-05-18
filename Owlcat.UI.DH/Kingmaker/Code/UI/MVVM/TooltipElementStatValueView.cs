using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipElementStatValueView : View<TooltipElementStatValueVM>
{
	[SerializeField]
	private TMP_Text m_StatName;

	[SerializeField]
	private TMP_Text m_Value;

	protected override void OnBind()
	{
		base.OnBind();
		m_StatName.text = base.ViewModel.StatName;
		m_Value.text = base.ViewModel.Value;
	}
}
