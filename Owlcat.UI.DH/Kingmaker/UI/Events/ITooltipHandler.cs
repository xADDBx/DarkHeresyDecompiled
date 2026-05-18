using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;

namespace Kingmaker.UI.Events;

public interface ITooltipHandler : ISubscriber
{
	void HandleInfoRequest(TooltipBaseTemplate template, bool shouldNotHideLittleTooltip = false);

	void HandleMultipleInfoRequest(IEnumerable<TooltipBaseTemplate> templates);

	void HandleGlossaryInfoRequest(TooltipTemplateGlossary template);

	void HandleHintRequest(HintData data, bool shouldShow);
}
