using Rewired;
using UnityEngine;

namespace Owlcat.UI.Commands.Rewired;

public class RewiredInputEvent : InputEvent
{
	private static readonly RewiredInputEvent m_Instance = new RewiredInputEvent();

	private string m_Binding;

	private float m_Axis;

	private Vector2 m_Axis2D;

	public static bool TryCreate(Keyboard keyboard, KeyCode keyCode, out InputEvent result)
	{
		string bindingNonAlloc = RewiredUtility.GetBindingNonAlloc(keyboard, keyCode);
		if (bindingNonAlloc == null)
		{
			result = null;
			return false;
		}
		result = Create(bindingNonAlloc);
		return true;
	}

	public static bool TryCreate(InputActionEventData actionEventData, out InputEvent result)
	{
		string bindingNonAlloc = RewiredUtility.GetBindingNonAlloc(actionEventData);
		if (bindingNonAlloc == null)
		{
			result = null;
			return false;
		}
		result = Create(bindingNonAlloc);
		return true;
	}

	public static InputEvent Create(string binding)
	{
		return Create(binding, 0f, Vector2.zero);
	}

	public static InputEvent Create(string binding, float axis)
	{
		return Create(binding, axis, Vector2.zero);
	}

	public static InputEvent Create(string binding, Vector2 axis2d)
	{
		return Create(binding, 0f, axis2d);
	}

	public static InputEvent Create(string binding, float axis, Vector2 axis2d)
	{
		m_Instance.Consumed = false;
		m_Instance.Bubbling = false;
		m_Instance.m_Binding = binding;
		m_Instance.m_Axis = axis;
		m_Instance.m_Axis2D = axis2d;
		return m_Instance;
	}

	private RewiredInputEvent()
	{
	}

	public override float GetAxis()
	{
		return m_Axis;
	}

	public override Vector2 GetAxis2D()
	{
		return m_Axis2D;
	}

	public override bool IsTrigger(string binding)
	{
		return InputEvent.Contains(binding, m_Binding);
	}

	public override string ToString()
	{
		return "[RewiredInputEvent " + m_Binding + "]";
	}
}
