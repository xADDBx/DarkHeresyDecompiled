using System;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickTimer : ITooltipBrick
{
	private readonly Func<string> m_TextFunc;

	private readonly bool m_ShowIcon;

	public TooltipBrickTimer(Func<string> textFunc, bool showIcon = false)
	{
		m_TextFunc = textFunc;
		m_ShowIcon = showIcon;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickTimerVM(m_TextFunc, m_ShowIcon);
	}
}
