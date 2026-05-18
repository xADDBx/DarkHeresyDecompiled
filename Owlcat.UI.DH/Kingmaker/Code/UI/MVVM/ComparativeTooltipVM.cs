using System.Collections.Generic;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ComparativeTooltipVM : ViewModel
{
	private readonly List<TooltipVM> m_MainTooltips = new List<TooltipVM>();

	private readonly List<TooltipVM> m_CompareTooltips = new List<TooltipVM>();

	public readonly Transform Source;

	public IReadOnlyList<TooltipVM> MainTooltips => m_MainTooltips;

	public IReadOnlyList<TooltipVM> CompareTooltips => m_CompareTooltips;

	public TooltipVM FirstMainTooltip
	{
		get
		{
			if (m_MainTooltips.Count <= 0)
			{
				return null;
			}
			return m_MainTooltips[0];
		}
	}

	public ComparativeTooltipVM(Transform source, IEnumerable<TooltipData> mainTooltipsData, IEnumerable<TooltipData> compareTooltipsData, bool showScrollbar)
	{
		Source = source;
		foreach (TooltipData compareTooltipsDatum in compareTooltipsData)
		{
			TooltipVM item = new TooltipVM(compareTooltipsDatum, isComparative: true, shouldNotHideLittleTooltip: false, showScrollbar).AddTo(this);
			m_CompareTooltips.Add(item);
		}
		foreach (TooltipData mainTooltipsDatum in mainTooltipsData)
		{
			TooltipVM item2 = new TooltipVM(mainTooltipsDatum, isComparative: true, shouldNotHideLittleTooltip: false, showScrollbar).AddTo(this);
			m_MainTooltips.Add(item2);
		}
	}
}
