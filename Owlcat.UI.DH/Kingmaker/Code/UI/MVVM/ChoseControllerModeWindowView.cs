using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ChoseControllerModeWindowView : View<GamepadConnectDisconnectVM>
{
	[SerializeField]
	private TextMeshProUGUI m_HintLabel;

	protected override void OnBind()
	{
		m_HintLabel.text = UIStrings.Instance.ControllerModeTexts.PressAnyKeyText;
		base.gameObject.SetActive(value: true);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate), delegate
		{
			OnLateUpdate();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}

	private void GetInputLayer()
	{
	}

	private void SetGamepadMode()
	{
	}

	private void OnLateUpdate()
	{
		if (UtilityKeyboard.IsAnyKeyboardKeyDown())
		{
			base.ViewModel.SetKeyboardMode();
		}
	}
}
