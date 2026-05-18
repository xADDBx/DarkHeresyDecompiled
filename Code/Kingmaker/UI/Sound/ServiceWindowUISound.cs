using UnityEngine;

namespace Kingmaker.UI.Sound;

public abstract class ServiceWindowUISound
{
	[field: SerializeField]
	public UISound Open { get; private set; }

	[field: SerializeField]
	public UISound Close { get; private set; }

	[field: SerializeField]
	public UISound SwitchTo { get; private set; }
}
