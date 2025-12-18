using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class TooltipBrickIconTextValueView : TooltipBrickCombatLogBaseView<TooltipBrickIconTextValueVM>
{
	[Space]
	[SerializeField]
	private TextMeshProUGUI m_ValueText;

	protected override void OnBind()
	{
		base.OnBind();
		m_ValueText.text = base.ViewModel.Value;
		TextHelper.AppendTexts(m_ValueText);
	}
}
