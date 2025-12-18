using UnityEngine;

namespace Kingmaker.Code.UI.Common;

public abstract class BaseProgressBar<T> : MonoBehaviour
{
	public abstract void SetLimits(T min, T max);

	public abstract void SetValue(T value);
}
