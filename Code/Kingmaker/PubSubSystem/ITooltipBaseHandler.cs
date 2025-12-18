using System.Collections.Generic;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;

namespace Kingmaker.PubSubSystem;

public interface ITooltipBaseHandler : ISubscriber
{
	void HandleTooltipRequest(TooltipData data, bool shouldNotHideLittleTooltip = false, bool showScrollbar = false);

	void HandleComparativeTooltipRequest(IEnumerable<TooltipData> data, bool showScrollbar = false);
}
