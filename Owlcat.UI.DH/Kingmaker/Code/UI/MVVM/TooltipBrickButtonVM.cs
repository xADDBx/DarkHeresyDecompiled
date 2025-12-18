using System;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickButtonVM : TooltipBaseBrickVM
{
	public readonly string Text;

	private readonly Action m_Callback;

	public TooltipBrickButtonVM(Action callback, string text)
	{
		m_Callback = callback;
		Text = text;
	}

	public void OnClick()
	{
		m_Callback?.Invoke();
	}
}
