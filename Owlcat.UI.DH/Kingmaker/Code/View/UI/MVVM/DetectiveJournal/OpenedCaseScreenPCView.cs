using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class OpenedCaseScreenPCView : OpenedCaseScreenBaseView
{
	[Header("PC")]
	[SerializeField]
	private OwlcatMultiButton m_BackButton;

	[SerializeField]
	private TMP_Text m_BackButtonLabel;

	protected override void OnBind()
	{
		base.OnBind();
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close).AddTo(this);
		m_BackButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Close).AddTo(this);
		m_BackButtonLabel.text = UIStrings.Instance.CommonTexts.Back.Text;
		m_BackButton.Interactable = base.ViewModel.CanCloseCase;
		if (!base.ViewModel.CanCloseCase)
		{
			m_BackButton.SetHint(UIStrings.Instance.DetectiveJournal.CannotCloseCaseHint.Text).AddTo(this);
		}
	}
}
