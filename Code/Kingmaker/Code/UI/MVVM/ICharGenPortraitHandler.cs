using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface ICharGenPortraitHandler : ISubscriber
{
	void HandleSetPortrait([NotNull] BlueprintPortrait blueprintPortrait);
}
