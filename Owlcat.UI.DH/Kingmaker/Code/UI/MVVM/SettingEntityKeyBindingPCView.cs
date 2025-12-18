using Kingmaker.Settings.Entities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SettingEntityKeyBindingPCView : SettingsEntityWithValueView<SettingEntityKeyBindingVM>
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
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_BindingButton1.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenBindingDialogVM(0);
		}));
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_BindingButton2.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenBindingDialogVM(1);
		}));
		AddDisposable(base.ViewModel.TempBindingValue1.Subscribe(delegate(KeyBindingData data)
		{
			SetupBindingButton(m_BindingButton1, m_BindingText1, data.GetPrettyString());
		}));
		AddDisposable(base.ViewModel.TempBindingValue2.Subscribe(delegate(KeyBindingData data)
		{
			SetupBindingButton(m_BindingButton2, m_BindingText2, data.GetPrettyString());
		}));
		SubscribeNotAllowedSelectable(m_BindingButton1);
		SubscribeNotAllowedSelectable(m_BindingButton2);
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
		SetNotAllowedModificationHint(m_BindingButton1);
		SetNotAllowedModificationHint(m_BindingButton2);
	}

	public override bool HandleLeft()
	{
		return false;
	}

	public override bool HandleRight()
	{
		return false;
	}
}
