using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility.Locator;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Utility;

public class MouseHoverBlueprintSystem : IService, ITooltipBaseHandler, ISubscriber, IDisposable
{
	public static MouseHoverBlueprintSystem Instance => Services.GetInstance<MouseHoverBlueprintSystem>();

	public string UnderMouseBlueprintName { get; private set; } = string.Empty;


	public TooltipData TooltipData { get; private set; }

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public MouseHoverBlueprintSystem()
	{
		EventBus.Subscribe(this);
	}

	public void HandleTooltipRequest(TooltipData data, bool shouldNotHideLittleTooltip, bool showScrollbar)
	{
		if (data != null)
		{
			HandleTooltipCreated(data);
		}
		else
		{
			HandleTooltipDeleted();
		}
	}

	public void HandleComparativeTooltipRequest(Transform source, IEnumerable<TooltipData> data, bool showScrollbar = false)
	{
		HandleTooltipRequest(data?.LastOrDefault(), shouldNotHideLittleTooltip: false, showScrollbar);
	}

	public void HandleComparativeTooltipRequest(Transform source, IEnumerable<TooltipData> mainData, IEnumerable<TooltipData> compareData, bool showScrollbar = false)
	{
		HandleComparativeTooltipRequest(source, mainData);
	}

	private void HandleTooltipCreated(TooltipData tooltipData)
	{
		UnderMouseBlueprintName = Game.Instance.BugReportContext.GetTooltipData(tooltipData);
		if (!string.IsNullOrWhiteSpace(UnderMouseBlueprintName))
		{
			TooltipData = tooltipData;
		}
	}

	private void HandleTooltipDeleted()
	{
		TooltipData = null;
		UnderMouseBlueprintName = string.Empty;
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(this);
	}
}
