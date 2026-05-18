using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class BrickIconTextValueView : BrickCombatLogBaseView<BrickIconTextValueVM>
{
	[Space]
	[SerializeField]
	private TextMeshProUGUI m_ValueText;

	protected override void OnBind()
	{
		base.OnBind();
		m_ValueText.text = base.ViewModel.Value;
		m_TextHelper.AppendTexts(m_ValueText);
	}
}
