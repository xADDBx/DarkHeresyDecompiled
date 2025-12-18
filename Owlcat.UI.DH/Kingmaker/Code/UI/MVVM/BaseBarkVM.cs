using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class BaseBarkVM : ViewModel
{
	private readonly ReactiveProperty<string> m_Text = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<bool> m_IsBarkActive = new ReactiveProperty<bool>(value: false);

	public ReadOnlyReactiveProperty<string> Text => m_Text;

	public ReadOnlyReactiveProperty<bool> IsBarkActive => m_IsBarkActive;

	public bool IsInCombat => GameUIState.Instance?.IsInCombat.Value ?? false;

	public void ShowBark(string text)
	{
		m_Text.Value = text;
		m_IsBarkActive.Value = true;
	}

	public void HideBark()
	{
		m_IsBarkActive.Value = false;
	}
}
