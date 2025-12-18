using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DialogSystemAnswerPCView : DialogSystemAnswerBaseView
{
	[SerializeField]
	protected TextMeshProUGUI m_NextOrEndBindingText;

	[SerializeField]
	protected TextMeshProUGUI m_DialogChoice1BindingText;

	private static string NextOrEndBindingName => "NextOrEnd";

	private static string DialogChoice1BindingName => "DialogChoice1";

	protected override void OnBind()
	{
		base.OnBind();
		Game.Instance.Keyboard.Bind(NextOrEndBindingName, ButtonClickWithSound).AddTo(this);
		Game.Instance.Keyboard.Bind(DialogChoice1BindingName, ButtonClickWithSound).AddTo(this);
		m_NextOrEndBindingText.text = "[" + GetBindingString(NextOrEndBindingName) + "]";
		m_DialogChoice1BindingText.text = GetBindingString(DialogChoice1BindingName);
		m_Button.OnLeftClickAsObservable().Subscribe(ButtonClickWithSound).AddTo(this);
		UISounds.Instance.SetHoverSound(m_Button, ButtonSoundsEnum.MajorPaperSound);
		UISounds.Instance.SetClickSound(m_Button, ButtonSoundsEnum.NoSound);
	}

	private void ButtonClickWithSound()
	{
		UISounds.Instance.Play(UISounds.Instance.Sounds.Buttons.MajorPaperButtonClick, isButton: true);
		base.ViewModel.OnChooseAnswer();
	}

	private string GetBindingString(string bindingName)
	{
		KeyboardAccess keyboard = Game.Instance.Keyboard;
		return UIKeyboardTexts.Instance.GetStringByBinding(keyboard.GetBindingByName(bindingName));
	}
}
