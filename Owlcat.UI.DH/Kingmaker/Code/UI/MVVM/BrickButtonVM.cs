using System;

namespace Kingmaker.Code.UI.MVVM;

public class BrickButtonVM : TooltipBrickVM
{
	public readonly string Text;

	private readonly Action m_Callback;

	public BrickButtonVM(Action callback, string text)
	{
		m_Callback = callback;
		Text = text;
	}

	public void OnClick()
	{
		m_Callback?.Invoke();
	}
}
