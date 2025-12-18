using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Code.UI.MVVM;

public interface ICharGenSelectItemHandler : ISubscriber
{
	void HandleSelectItem(FeatureGroup featureGroup, BlueprintFeature blueprintFeature);
}
