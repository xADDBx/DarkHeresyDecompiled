using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public interface ILineTarget
{
	RectTransform RectTransform { get; }

	ClueDotsPositions DotsPositions { get; }

	List<LineDirectionData> Directions { get; }
}
