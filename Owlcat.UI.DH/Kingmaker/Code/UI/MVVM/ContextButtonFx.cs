using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class ContextButtonFx : MonoBehaviour
{
	public abstract void DoHovered(bool value);

	public abstract void DoBlink();
}
