using R3;

namespace Kingmaker.Code.UI.MVVM;

public interface ICharInfoCanHookConfirm
{
	ReadOnlyReactiveProperty<bool> GetCanHookConfirmProperty();
}
