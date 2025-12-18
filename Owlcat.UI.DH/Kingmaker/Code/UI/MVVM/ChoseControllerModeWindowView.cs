using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using Rewired;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ChoseControllerModeWindowView : View<GamepadConnectDisconnectVM>
{
	[SerializeField]
	private TextMeshProUGUI m_HintLabel;

	private InputLayer m_InputLayer;

	protected override void OnBind()
	{
		m_HintLabel.text = UIStrings.Instance.ControllerModeTexts.PressAnyKeyText;
		base.gameObject.SetActive(value: true);
		GamePad.Instance.PushLayer(GetInputLayer()).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate), delegate
		{
			OnLateUpdate();
		}).AddTo(this);
		base.ViewModel.GamepadDisconnected.Subscribe(base.ViewModel.SetKeyboardMode).AddTo(this);
	}

	protected override void OnUnbind()
	{
		GamePad.Instance.PopLayer(m_InputLayer);
		base.gameObject.SetActive(value: false);
	}

	private InputLayer GetInputLayer()
	{
		InputLayer inputLayer = new InputLayer();
		inputLayer.ContextName = "ChoseControllerModeWindowView";
		inputLayer.AddButton(SetGamepadMode, 8);
		inputLayer.AddButton(SetGamepadMode, 9);
		inputLayer.AddButton(SetGamepadMode, 10);
		inputLayer.AddButton(SetGamepadMode, 11);
		inputLayer.AddButton(SetGamepadMode, 16);
		inputLayer.AddButton(SetGamepadMode, 17);
		inputLayer.AddButton(SetGamepadMode, 12);
		inputLayer.AddButton(SetGamepadMode, 14);
		inputLayer.AddButton(SetGamepadMode, 13);
		inputLayer.AddButton(SetGamepadMode, 15);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 7);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 4);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 5);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 6);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 18);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 19);
		inputLayer.AddAxis(delegate(InputActionEventData eventData, float __)
		{
			SetGamepadMode(eventData);
		}, 0);
		inputLayer.AddAxis(delegate(InputActionEventData eventData, float __)
		{
			SetGamepadMode(eventData);
		}, 1);
		inputLayer.AddAxis(delegate(InputActionEventData eventData, float __)
		{
			SetGamepadMode(eventData);
		}, 2);
		inputLayer.AddAxis(delegate(InputActionEventData eventData, float __)
		{
			SetGamepadMode(eventData);
		}, 3);
		return inputLayer;
	}

	private void SetGamepadMode(InputActionEventData eventData)
	{
		if (eventData.IsCurrentInputSource(ControllerType.Joystick))
		{
			base.ViewModel.SetGamepadMode();
		}
	}

	private void OnLateUpdate()
	{
		if (UtilityKeyboard.IsAnyKeyboardKeyDown())
		{
			base.ViewModel.SetKeyboardMode();
		}
	}
}
