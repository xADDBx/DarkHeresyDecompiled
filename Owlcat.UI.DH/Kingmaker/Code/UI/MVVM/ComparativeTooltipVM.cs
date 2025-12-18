using System.Collections.Generic;
using System.Linq;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ComparativeTooltipVM : ViewModel
{
	public readonly List<TooltipVM> TooltipVms = new List<TooltipVM>();

	public TooltipVM MainTooltip => TooltipVms.LastOrDefault();

	public TooltipVM FirstCompareTooltip => TooltipVms.FirstOrDefault();

	public RectTransform MainOwnerTransform => MainTooltip?.OwnerTransform;

	public RectTransform ComparativeOwnerTransform => FirstCompareTooltip?.OwnerTransform;

	public ComparativeTooltipVM(IEnumerable<TooltipData> tooltipsData, bool showScrollbar)
	{
		foreach (TooltipData tooltipsDatum in tooltipsData)
		{
			TooltipVM item = new TooltipVM(tooltipsDatum, isComparative: true, shouldNotHideLittleTooltip: false, showScrollbar).AddTo(this);
			TooltipVms.Add(item);
		}
	}
}
