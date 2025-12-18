using System;
using Rewired;
using UnityEngine.Events;

namespace Owlcat.UI;

[Serializable]
public class ControlAxisEventHandler : UnityEvent<InputActionEventData, float>
{
	public Action<InputActionEventData, float> Action => base.Invoke;
}
