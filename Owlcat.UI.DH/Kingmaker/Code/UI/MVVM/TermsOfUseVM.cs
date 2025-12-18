using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TermsOfUseVM : ViewModel
{
	private readonly Action m_CloseAction;

	public readonly UITermsOfUseTexts TermsOfUseTexts;

	public bool TermsOfUseAccepted => TermsOfUse.TermsOfUseAccepted;

	public TermsOfUseVM(Action closeAction)
	{
		m_CloseAction = closeAction;
		TermsOfUseTexts = ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.TermsOfUseTexts;
	}

	public void TryCloseTermOfUse()
	{
		if (TermsOfUseAccepted)
		{
			TermsOfUseClose();
		}
		else
		{
			TermsOfUseDecline();
		}
	}

	public void TermsOfUseAccept()
	{
		TermsOfUse.AcceptTermOfUse();
		TermsOfUseClose();
	}

	public void TermsOfUseDecline()
	{
		string text = TermsOfUseTexts.AreYouReallyWantToDeclineAgreement;
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(text, DialogMessageBoxType.Dialog, OnDeclineDialogAnswer, null, UIStrings.Instance.SettingsUI.DialogYes, UIStrings.Instance.SettingsUI.DialogNo);
		});
	}

	private void OnDeclineDialogAnswer(DialogMessageBoxButton button)
	{
		if (button == DialogMessageBoxButton.Yes)
		{
			TermsOfUse.DeclineTermOfUse();
		}
	}

	public void TermsOfUseClose()
	{
		m_CloseAction?.Invoke();
	}

	public LocalizedString GetLicenceText()
	{
		if (!Game.Instance.IsControllerMouse)
		{
			return TermsOfUseTexts.LicenceConsole;
		}
		return TermsOfUseTexts.Licence;
	}
}
