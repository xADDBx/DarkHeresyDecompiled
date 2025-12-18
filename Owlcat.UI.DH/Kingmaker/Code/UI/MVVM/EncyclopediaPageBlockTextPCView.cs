using System.Collections.Generic;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaPageBlockTextPCView : EncyclopediaPageBlockPCView<EncyclopediaPageBlockTextVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private float m_DefaultFontSize = 21f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 21f;

	protected override void OnBind()
	{
		base.OnBind();
		m_Text.text = base.ViewModel.Text;
		m_Text.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.LeftMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: true)).AddTo(this);
		m_Text.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
	}

	protected override void OnUnbind()
	{
		m_Text.text = string.Empty;
		base.OnUnbind();
	}

	public override List<TextMeshProUGUI> GetLinksTexts()
	{
		return new List<TextMeshProUGUI> { m_Text };
	}
}
