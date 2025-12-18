using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public interface ISetTooltipHandler : ISubscriber
{
	void SetTooltip(TooltipBaseTemplate template);
}
