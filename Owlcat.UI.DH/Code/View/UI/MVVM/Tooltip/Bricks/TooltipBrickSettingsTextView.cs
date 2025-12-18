using Code.View.UI.Helpers;
using Kingmaker.Code.UI.MVVM;
using R3;
using TMPro;
using UnityEngine;

namespace Code.View.UI.MVVM.Tooltip.Bricks;

public class TooltipBrickSettingsTextView : TooltipBaseBrickView<TooltipBrickSettingsTextVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_Text;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		base.OnBind();
		m_TextHelper = new AccessibilityTextHelper(m_Text).AddTo(this);
		m_Text.text = base.ViewModel.Text;
		m_TextHelper.UpdateTextSize();
	}
}
