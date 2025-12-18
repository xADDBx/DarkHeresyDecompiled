using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface ICharGenTextureSelectorTabChangeHandler : ISubscriber
{
	void HandleTextureSelectorTabChange(CharGenAppearancePageComponent type, int tabIndex);
}
