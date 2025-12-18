using System;
using Rewired;
using UnityEngine.Events;

namespace Owlcat.UI;

[Serializable]
public class ControlButtonEventHandler : UnityEvent<InputActionEventData>
{
	public Action<InputActionEventData> Action => base.Invoke;
}
