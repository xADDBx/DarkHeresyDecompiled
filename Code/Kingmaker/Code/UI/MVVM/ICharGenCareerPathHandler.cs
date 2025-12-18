using JetBrains.Annotations;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Progression.Paths;

namespace Kingmaker.Code.UI.MVVM;

public interface ICharGenCareerPathHandler : ISubscriber
{
	void HandleCareerPath([NotNull] BlueprintCareerPath careerPath);
}
