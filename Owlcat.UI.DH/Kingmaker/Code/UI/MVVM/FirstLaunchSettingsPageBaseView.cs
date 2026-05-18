using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class FirstLaunchSettingsPageBaseView<TViewModel> : View<TViewModel> where TViewModel : FirstLaunchSettingsPageVM
{
	protected IConsoleEntity[] m_AdditionalEntities;

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}
}
