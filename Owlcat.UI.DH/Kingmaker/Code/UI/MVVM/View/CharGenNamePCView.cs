using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenNamePCView : CharGenNameBaseView
{
	[Header("PC")]
	[SerializeField]
	private PCInputField m_InputField;

	[SerializeField]
	private TMP_Text m_InputButtonText;

	[SerializeField]
	private OwlcatMultiButton m_SetRandomNameButton;

	protected override void OnBind()
	{
		base.OnBind();
		m_InputField.Bind(base.ViewModel.CurrentDisplayName.CurrentValue, base.ViewModel.SetName).AddTo(this);
		base.ViewModel.CurrentDisplayName.Subscribe(delegate(string text)
		{
			m_InputButtonText.text = text;
			m_InputField.SetText(text);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_SetRandomNameButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.SetRandomName();
		}).AddTo(this);
		m_SetRandomNameButton.SetHint(UIStrings.Instance.CharGen.SetRandomName).AddTo(this);
		CheckCoopButtons(base.ViewModel.IsMainCharacter.CurrentValue);
		base.ViewModel.CheckCoopControls.Subscribe(CheckCoopButtons).AddTo(this);
	}

	private void CheckCoopButtons(bool isMainCharacter)
	{
		m_SetRandomNameButton.Interactable = isMainCharacter;
	}
}
