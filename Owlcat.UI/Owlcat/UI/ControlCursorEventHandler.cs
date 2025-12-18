using System;
using Rewired;
using UnityEngine;
using UnityEngine.Events;

namespace Owlcat.UI;

[Serializable]
public class ControlCursorEventHandler : UnityEvent<InputActionEventData, Vector2>
{
	public Action<InputActionEventData, Vector2> Action => base.Invoke;
}
