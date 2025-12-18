using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface ICharGenAttributesPhaseHandler : ISubscriber
{
	void HandleTryAdvanceStat(StatType statType, bool advance);
}
