using R3;

namespace Kingmaker.Code.UI.MVVM;

public interface ICharInfoCanHookDecline
{
	ReadOnlyReactiveProperty<bool> GetCanHookDeclineProperty();
}
