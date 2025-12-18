using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface ICharGenChangePhaseHandler : ISubscriber
{
	void HandlePhaseChange(CharGenPhaseType phaseType);
}
