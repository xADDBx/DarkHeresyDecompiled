using System;
using System.Linq;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.View;
using Owlcat.UI.Commands;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kingmaker.Code.UI.MVVM;

public class InputController : InputControllerBase
{
	private Vector2 m_KeyboardCameraMove;

	private Vector2 m_KeyboardCameraOrbit;

	public InputController()
	{
		BindKeyboard();
	}

	private void BindKeyboard()
	{
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		Bind(uIKeybindGeneralSettings.CameraRotateLeft, delegate
		{
			OnCameraRotate(Vector2.right);
		});
		Bind(uIKeybindGeneralSettings.CameraRotateRight, delegate
		{
			OnCameraRotate(Vector2.left);
		});
		Bind(uIKeybindGeneralSettings.CameraLeft, delegate
		{
			OnCameraMove(Vector2.left);
		});
		Bind(uIKeybindGeneralSettings.CameraRight, delegate
		{
			OnCameraMove(Vector2.right);
		});
		Bind(uIKeybindGeneralSettings.CameraUp, delegate
		{
			OnCameraMove(Vector2.up);
		});
		Bind(uIKeybindGeneralSettings.CameraDown, delegate
		{
			OnCameraMove(Vector2.down);
		});
	}

	private void Bind(UISettingsEntityKeyBinding binding, Action action)
	{
		if (!(binding == null))
		{
			Game.Instance.Keyboard.Bind(binding.name, action).AddTo(ref m_Bag);
		}
	}

	private void OnCameraMove(Vector2 direction)
	{
		m_KeyboardCameraMove += direction;
	}

	private void OnCameraRotate(Vector2 direction)
	{
		m_KeyboardCameraOrbit += direction;
	}

	protected override InputContext GetInputContext()
	{
		InputContext result = default(InputContext);
		result.Unit = Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value;
		result.Cursor = Game.Instance.CursorController;
		result.CameraRig = CameraRig.Instance;
		return result;
	}

	protected override InputFrame GetInputFrame()
	{
		InputFrame result = default(InputFrame);
		if (!CommandLayerStack.Current.Layers.Any((CommandLayer x) => x.Mode == CommandLayerMode.Modal))
		{
			result.CameraMove = m_KeyboardCameraMove;
			result.CameraOrbit = m_KeyboardCameraOrbit;
			Gamepad current = Gamepad.current;
			if (current != null)
			{
				result.LeftStick += current.leftStick.ReadValue();
				result.RightStick += current.rightStick.ReadValue();
				if (current.rightStickButton.wasPressedThisFrame)
				{
					Game.Instance.CursorController.SetActive(!Game.Instance.CursorController.IsCursorActive);
				}
			}
		}
		m_KeyboardCameraMove = default(Vector2);
		m_KeyboardCameraOrbit = default(Vector2);
		return result;
	}

	protected override ModeType GetInputMode()
	{
		InputContext inputContext = GetInputContext();
		if (inputContext.CameraRig != null && Game.Instance.CursorController.IsCursorActive)
		{
			return ModeType.FreeCamera;
		}
		if (inputContext.Unit != null && !Game.Instance.CursorController.IsCursorActive)
		{
			return ModeType.FreeMovement;
		}
		return ModeType.None;
	}
}
