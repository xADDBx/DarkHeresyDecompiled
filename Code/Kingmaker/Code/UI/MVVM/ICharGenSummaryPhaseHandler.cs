using JetBrains.Annotations;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface ICharGenSummaryPhaseHandler : ISubscriber
{
	void HandleSetName([NotNull] string name);
}
