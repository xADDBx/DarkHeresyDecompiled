using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Owlcat.UI;

[AddComponentMenu("UnifiedCursor")]
public class UnifiedCursor : UIBehaviour
{
	private Vector2 m_Position;

	private Mouse m_NativeMouse;

	private Mouse m_VirtualMouse;

	private Vector2 m_LastStickValue;

	private double m_LastStickTime;

	[SerializeField]
	[Min(0f)]
	private float m_Sensitivity = 800f;

	[SerializeField]
	private InputActionProperty m_Move;

	[SerializeField]
	private InputActionProperty m_Scroll;

	[SerializeField]
	private InputActionProperty m_Left;

	[SerializeField]
	private InputActionProperty m_Right;

	[SerializeField]
	private InputActionProperty m_Middle;

	[SerializeField]
	private InputActionProperty m_Forward;

	[SerializeField]
	private InputActionProperty m_Back;

	public Vector2 Position
	{
		get
		{
			return m_Position;
		}
		set
		{
			SetPosition(value);
		}
	}

	public bool HasNativeMouse => m_NativeMouse != null;

	protected override void OnEnable()
	{
		if (TryGetSystemMouse(ref m_NativeMouse))
		{
			SetPosition(m_NativeMouse.position.value, warpNativeCursor: false);
		}
		else
		{
			SetPosition(new Vector2(Screen.width, Screen.height) / 2f);
		}
		Setup(m_Left, install: true);
		Setup(m_Right, install: true);
		Setup(m_Middle, install: true);
		Setup(m_Forward, install: true);
		Setup(m_Back, install: true);
		m_Move.action?.Enable();
		m_Scroll.action?.Enable();
		InputSystem.onAfterUpdate += OnAfterInputUpdate;
	}

	protected override void OnDisable()
	{
		InputSystem.onAfterUpdate -= OnAfterInputUpdate;
		if (m_VirtualMouse != null && m_VirtualMouse.enabled)
		{
			InputSystem.DisableDevice(m_VirtualMouse);
		}
		if (m_NativeMouse != null)
		{
			m_NativeMouse.MakeCurrent();
		}
		Setup(m_Left, install: false);
		Setup(m_Right, install: false);
		Setup(m_Middle, install: false);
		Setup(m_Forward, install: false);
		Setup(m_Back, install: false);
		m_Move.action?.Disable();
		m_Scroll.action?.Disable();
	}

	protected override void OnDestroy()
	{
		if (m_VirtualMouse != null && m_VirtualMouse.added)
		{
			InputSystem.RemoveDevice(m_VirtualMouse);
		}
	}

	private void Setup(InputActionProperty property, bool install)
	{
		if (property.action != null)
		{
			if (install)
			{
				property.action.started += OnButtonCallback;
				property.action.canceled += OnButtonCallback;
				property.action.Enable();
			}
			else
			{
				property.action.started -= OnButtonCallback;
				property.action.canceled -= OnButtonCallback;
				property.action.Disable();
			}
		}
	}

	private void OnAfterInputUpdate()
	{
	}

	private void Update()
	{
		if (TryGetSystemMouse(ref m_NativeMouse))
		{
			Vector2 value = m_NativeMouse.position.value;
			if (!Approximately(value, m_Position))
			{
				m_NativeMouse.MakeCurrent();
				SetPosition(value, warpNativeCursor: false);
			}
		}
		if (m_Move.action == null || !m_Move.action.enabled)
		{
			return;
		}
		Vector2 vector = m_Move.action.ReadValue<Vector2>();
		if (ApproximatelyZero(vector))
		{
			m_LastStickValue = default(Vector2);
			m_LastStickTime = 0.0;
		}
		else if (TryGetVirtualMouse(ref m_VirtualMouse))
		{
			double currentTime = InputState.currentTime;
			if (ApproximatelyZero(m_LastStickValue))
			{
				m_LastStickTime = currentTime;
			}
			float num = (float)(currentTime - m_LastStickTime);
			Vector2 vector2 = PowMagnitude(vector, 2f) * (m_Sensitivity * num);
			Vector2 vector3 = Round(m_Position + vector2);
			if (!Approximately(vector3, m_Position))
			{
				m_VirtualMouse.MakeCurrent();
				SetPosition(vector3);
			}
			m_LastStickTime = currentTime;
			m_LastStickValue = vector;
		}
	}

	private void OnButtonCallback(InputAction.CallbackContext context)
	{
		MouseButton? mouseButton = null;
		if (context.action == m_Left.action)
		{
			mouseButton = MouseButton.Left;
		}
		else if (context.action == m_Right.action)
		{
			mouseButton = MouseButton.Right;
		}
		else if (context.action == m_Middle.action)
		{
			mouseButton = MouseButton.Middle;
		}
		else if (context.action == m_Forward.action)
		{
			mouseButton = MouseButton.Forward;
		}
		else if (context.action == m_Back.action)
		{
			mouseButton = MouseButton.Back;
		}
		if (mouseButton.HasValue)
		{
			SetButton(mouseButton.Value, context.control.IsPressed());
		}
	}

	private void SetPosition(Vector2 value, bool warpNativeCursor = true)
	{
		Vector2 vector = Round(value);
		Vector2 state = vector - m_Position;
		m_Position = vector;
		if (TryGetVirtualMouse(ref m_VirtualMouse))
		{
			InputState.Change(m_VirtualMouse.position, vector);
			InputState.Change(m_VirtualMouse.delta, state);
		}
		if (TryGetSystemMouse(ref m_NativeMouse))
		{
			InputState.Change(m_NativeMouse.position, vector);
			InputState.Change(m_NativeMouse.delta, state);
			if (warpNativeCursor)
			{
				m_NativeMouse.WarpCursorPosition(m_Position);
			}
		}
	}

	private void SetButton(MouseButton button, bool isPressed)
	{
		if (TryGetVirtualMouse(ref m_VirtualMouse))
		{
			m_VirtualMouse.MakeCurrent();
			m_VirtualMouse.CopyState<MouseState>(out var state);
			state.WithButton(button, isPressed);
			InputState.Change(m_VirtualMouse, state);
		}
	}

	private void SetScroll(Vector2 value)
	{
		if (TryGetVirtualMouse(ref m_VirtualMouse) && m_VirtualMouse.scroll.value != value)
		{
			m_VirtualMouse.MakeCurrent();
			InputState.Change(m_VirtualMouse.scroll, value);
		}
	}

	private static Vector2 Round(Vector2 value)
	{
		return new Vector2(Mathf.Round(value.x), Mathf.Round(value.y));
	}

	private static Vector2 PowMagnitude(Vector2 value, float p, float dz = 0.05f)
	{
		float num = Mathf.InverseLerp(dz, 1f, value.magnitude);
		if (Mathf.Approximately(num, 0f))
		{
			return Vector2.zero;
		}
		value *= Mathf.Pow(num, p) / num;
		return value;
	}

	private static bool ApproximatelyZero(Vector2 value)
	{
		if (Mathf.Approximately(value.x, 0f))
		{
			return Mathf.Approximately(value.y, 0f);
		}
		return false;
	}

	private static bool Approximately(Vector2 v1, Vector2 v2)
	{
		if (Mathf.Approximately(v1.x, v2.x))
		{
			return Mathf.Approximately(v1.y, v2.y);
		}
		return false;
	}

	private static bool TryGetSystemMouse(ref Mouse mouse)
	{
		if (mouse == null)
		{
			foreach (InputDevice device in InputSystem.devices)
			{
				if (device.native && device is Mouse mouse2)
				{
					mouse = mouse2;
					break;
				}
			}
		}
		if (mouse != null)
		{
			return mouse.added;
		}
		return false;
	}

	private static bool TryGetVirtualMouse(ref Mouse mouse)
	{
		if (mouse == null)
		{
			mouse = InputSystem.AddDevice<Mouse>("UnifiedCursor");
		}
		if (!mouse.added)
		{
			InputSystem.AddDevice(mouse);
		}
		if (!mouse.enabled)
		{
			InputSystem.EnableDevice(mouse);
		}
		if (mouse != null && mouse.added)
		{
			return mouse.enabled;
		}
		return false;
	}
}
