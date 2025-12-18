using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SettingEntityKeyBindingConsoleView : SettingsEntityWithValueConsoleView<SettingEntityKeyBindingVM>
{
	[SerializeField]
	private OwlcatMultiButton m_BindingButton1;

	[SerializeField]
	private OwlcatMultiButton m_BindingButton2;

	[SerializeField]
	private TextMeshProUGUI m_BindingText1;

	[SerializeField]
	private TextMeshProUGUI m_BindingText2;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_BindingButton1.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.OpenBindingDialogVM(0);
		}));
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_BindingButton2.OnConfirmClickAsObservable(), delegate
		{
			base.ViewModel.OpenBindingDialogVM(1);
		}));
	}

	private void SetupBindingButton(OwlcatMultiButton button, TextMeshProUGUI buttonText, string text)
	{
		bool flag = string.IsNullOrEmpty(text);
		button.SetActiveLayer(flag ? "Off" : "On");
		buttonText.text = (flag ? "---" : text);
	}

	public override void OnModificationChanged(string reason, bool allowed = true)
	{
		base.OnModificationChanged(reason, allowed);
		m_BindingButton1.Interactable = allowed;
		m_BindingButton2.Interactable = allowed;
	}

	public override bool HandleLeft()
	{
		_ = base.ViewModel.ModificationAllowed.CurrentValue;
		return false;
	}

	public override bool HandleRight()
	{
		_ = base.ViewModel.ModificationAllowed.CurrentValue;
		return false;
	}
}
