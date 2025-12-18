using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabBaseVM : ViewModel
{
	private readonly ReactiveProperty<bool> m_IsEnabled = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> IsEnabled => m_IsEnabled;

	protected DlcManagerTabBaseVM()
	{
		m_IsEnabled.Value = false;
	}

	public virtual bool SetEnabled(bool value, bool? direction = null)
	{
		m_IsEnabled.Value = value;
		return true;
	}
}
