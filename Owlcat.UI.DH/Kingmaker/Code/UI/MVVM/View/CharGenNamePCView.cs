using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenNamePCView : CharGenNameBaseView
{
	[SerializeField]
	private OwlcatButton m_SetNameButton;

	[SerializeField]
	private TextMeshProUGUI m_SetNameLabel;

	[SerializeField]
	private OwlcatButton m_SetRandomNameButton;

	protected override void OnBind()
	{
		base.OnBind();
		m_SetNameLabel.text = UIStrings.Instance.CharGen.EditName;
		ObservableSubscribeExtensions.Subscribe(m_SetNameButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ShowChangeNameMessageBox();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_SetRandomNameButton.OnLeftClickAsObservable(), delegate
		{
			GenerateRandomName();
		}).AddTo(this);
		m_SetRandomNameButton.SetHint(UIStrings.Instance.CharGen.SetRandomName).AddTo(this);
		CheckCoopButtons(base.ViewModel.IsMainCharacter.CurrentValue);
		base.ViewModel.CheckCoopControls.Subscribe(CheckCoopButtons).AddTo(this);
	}

	private void CheckCoopButtons(bool isMainCharacter)
	{
		m_SetNameButton.gameObject.SetActive(isMainCharacter);
		m_SetRandomNameButton.gameObject.SetActive(isMainCharacter);
	}
}
