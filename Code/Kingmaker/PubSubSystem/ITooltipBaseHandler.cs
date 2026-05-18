using System.Collections.Generic;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface ITooltipBaseHandler : ISubscriber
{
	void HandleTooltipRequest(TooltipData data, bool shouldNotHideLittleTooltip = false, bool showScrollbar = false);

	void HandleComparativeTooltipRequest(Transform source, IEnumerable<TooltipData> data, bool showScrollbar = false);

	void HandleComparativeTooltipRequest(Transform source, IEnumerable<TooltipData> mainData, IEnumerable<TooltipData> compareData, bool showScrollbar = false);
}
