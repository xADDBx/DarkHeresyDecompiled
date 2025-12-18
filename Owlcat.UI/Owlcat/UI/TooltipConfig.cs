using System;
using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.UI;

[Serializable]
public struct TooltipConfig
{
	public InfoCallPCMethod InfoCallPCMethod;

	public InfoCallConsoleMethod InfoCallConsoleMethod;

	public bool IsGlossary;

	public bool IsEncyclopedia;

	public RectTransform TooltipPlace;

	public int MaxHeight;

	public int PreferredHeight;

	public int Width;

	[Tooltip("Relative To the owner\nX: 0 — Right | 1 — Left\nY: 0 — Top | 1 — Bottom")]
	public List<Vector2> PriorityPivots;

	public TooltipConfig(InfoCallPCMethod infoCallPCMethod = InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod infoCallConsoleMethod = InfoCallConsoleMethod.LongRightStickButton, bool isGlossary = false, bool isEncyclopedia = false, RectTransform tooltipPlace = null, int maxHeight = 0, int preferredHeight = 0, int width = 0, List<Vector2> priorityPivots = null)
	{
		InfoCallPCMethod = infoCallPCMethod;
		InfoCallConsoleMethod = infoCallConsoleMethod;
		IsGlossary = isGlossary;
		IsEncyclopedia = isEncyclopedia;
		TooltipPlace = tooltipPlace;
		MaxHeight = maxHeight;
		PreferredHeight = preferredHeight;
		Width = width;
		PriorityPivots = priorityPivots;
	}
}
