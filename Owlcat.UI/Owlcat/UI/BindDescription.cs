using System;
using R3;
using Rewired;

namespace Owlcat.UI;

public class BindDescription
{
	public Action<InputActionEventData> ActionHandler;

	public Action<InputActionEventData> HintsHandler;

	public InputActionEventType EventType;

	public int ActionId;

	public ReadOnlyReactiveProperty<bool> Enabled;

	public string Group;

	public int BindId;

	public bool Bound;

	public bool Debug;

	public void Handler(InputActionEventData data)
	{
		ActionHandler?.Invoke(data);
		HintsHandler?.Invoke(data);
	}
}
