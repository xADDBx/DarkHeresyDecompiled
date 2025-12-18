using UnityEngine;

namespace Kingmaker.Code.UI.Common;

public abstract class BaseProgressBarSegment : MonoBehaviour
{
	public abstract void SetFill(bool isFilled, bool isMaxValue);
}
