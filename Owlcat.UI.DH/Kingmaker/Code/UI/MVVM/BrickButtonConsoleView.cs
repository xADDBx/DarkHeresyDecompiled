using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickButtonConsoleView : BrickButtonView, IConsoleTooltipBrick
{
	[SerializeField]
	private OwlcatMultiButton m_ConsoleButton;

	private readonly ReactiveProperty<bool> m_IsFocused = new ReactiveProperty<bool>();

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Text).AddTo(this);
		}
		m_IsFocused.Value = false;
		m_ConsoleButton.OnFocusAsObservable().Subscribe(delegate(bool value)
		{
			m_IsFocused.Value = value;
		}).AddTo(this);
		m_Text.text = base.ViewModel.Text;
		m_TextHelper.UpdateTextSize();
	}

	public IConsoleEntity GetConsoleEntity()
	{
		return m_ConsoleButton;
	}
}
