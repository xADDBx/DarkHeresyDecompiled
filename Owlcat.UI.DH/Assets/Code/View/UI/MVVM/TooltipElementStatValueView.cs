using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Assets.Code.View.UI.MVVM;

public class TooltipElementStatValueView : View<TooltipElementStatValueVM>
{
	[SerializeField]
	private TextMeshProUGUI m_StatName;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	protected override void OnBind()
	{
		base.OnBind();
		m_StatName.text = base.ViewModel.StatName;
		m_Value.text = base.ViewModel.Value;
	}
}
