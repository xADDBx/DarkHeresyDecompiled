using System;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

[Serializable]
public class ClueLineData
{
	public RectTransform DotFrom { get; private set; }

	public RectTransform DotTo { get; private set; }

	public ClueLineData((RectTransform, RectTransform) fromToDots)
	{
		(DotFrom, DotTo) = fromToDots;
	}
}
