using Kingmaker.Localization;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsEntityHeaderVM : VirtualListElementVMBase
{
	public readonly LocalizedString Tittle;

	private readonly ReactiveCommand<Unit> m_LanguageChanged = new ReactiveCommand<Unit>();

	public Observable<Unit> LanguageChanged => m_LanguageChanged;

	public SettingsEntityHeaderVM(LocalizedString tittle)
	{
		Tittle = tittle;
	}

	public void UpdateLocalization()
	{
		m_LanguageChanged.Execute();
	}

	protected override void DisposeImplementation()
	{
	}
}
