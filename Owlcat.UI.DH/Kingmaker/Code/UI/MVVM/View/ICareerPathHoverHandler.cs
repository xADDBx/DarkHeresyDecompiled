using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Progression.Paths;

namespace Kingmaker.Code.UI.MVVM.View;

public interface ICareerPathHoverHandler : ISubscriber
{
	void HandleHoverStart(BlueprintCareerPath careerPath);

	void HandleHoverStop();
}
