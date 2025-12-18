using System;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickButton : ITooltipBrick
{
	private readonly Action m_Callback;

	private readonly string m_Text;

	public TooltipBrickButton(Action callback, string text)
	{
		m_Callback = callback;
		m_Text = text;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickButtonVM(m_Callback, m_Text);
	}
}
