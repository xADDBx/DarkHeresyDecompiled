using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;

namespace Kingmaker.UI.Events;

public interface ITooltipHandler : ISubscriber
{
	void HandleInfoRequest(TooltipBaseTemplate template, ConsoleNavigationBehaviour ownerNavigationBehaviour = null, bool shouldNotHideLittleTooltip = false);

	void HandleMultipleInfoRequest(IEnumerable<TooltipBaseTemplate> templates, ConsoleNavigationBehaviour ownerNavigationBehaviour = null);

	void HandleGlossaryInfoRequest(TooltipTemplateGlossary template, ConsoleNavigationBehaviour ownerNavigationBehaviour = null);

	void HandleHintRequest(HintData data, bool shouldShow);
}
