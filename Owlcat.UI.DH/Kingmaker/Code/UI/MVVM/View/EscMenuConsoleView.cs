using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using Rewired;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class EscMenuConsoleView : EscMenuBaseView, INetInviteHandler, ISubscriber
{
	[Header("Console Buttons")]
	[SerializeField]
	private OwlcatButton m_QuickSave;

	[SerializeField]
	private OwlcatButton m_QuickLoad;

	[Header("Console Buttons Labels")]
	[SerializeField]
	private TextMeshProUGUI m_QuickSaveButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_QuickLoadButtonLabel;

	protected override void OnBind()
	{
		m_QuickSave.OnConfirmClickAsObservable().Subscribe(base.ViewModel.OnQuickSave).AddTo(this);
		m_QuickLoad.OnConfirmClickAsObservable().Subscribe(OnQuickLoadConfirm).AddTo(this);
		GamePad.Instance.OnLayerPushed.Subscribe(OnCurrentInputLayerChanged).AddTo(this);
		m_QuickSave.SetInteractable(base.ViewModel.IsSavingAllowed);
		base.OnBind();
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void UpdateInteractableButtonsImpl()
	{
		base.UpdateInteractableButtonsImpl();
		m_QuickSave.SetInteractable(base.ViewModel.IsSavingAllowed);
	}

	protected override void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		base.BuildNavigationImpl(navigationBehaviour);
		navigationBehaviour.InsertVertical(0, m_QuickSave);
		navigationBehaviour.InsertVertical(1, m_QuickLoad);
		navigationBehaviour.FocusOnFirstValidEntity();
	}

	protected override void CreateInputImpl(InputLayer inputLayer)
	{
		base.CreateInputImpl(inputLayer);
		inputLayer.AddButton(delegate
		{
			base.ViewModel.OnClose();
		}, 16, InputActionEventType.ButtonJustReleased).AddTo(this);
		inputLayer.AddButton(delegate
		{
			if (!Input.GetKey(KeyCode.Escape))
			{
				base.ViewModel.OnClose();
			}
		}, 9).AddTo(this);
	}

	protected override void SetButtonsTexts()
	{
		base.SetButtonsTexts();
		m_QuickSaveButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuQuickSave;
		m_QuickLoadButtonLabel.text = UIStrings.Instance.EscapeMenu.EscMenuQuickLoad;
	}

	private void OnCurrentInputLayerChanged()
	{
		GamePad instance = GamePad.Instance;
		if (instance.CurrentInputLayer != InputLayer && !(instance.CurrentInputLayer.ContextName == BugReportBaseView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == BugReportDrawingView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == "BugReportDuplicatesViewInput") && !(instance.CurrentInputLayer.ContextName == OwlcatDropdown.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == OwlcatInputField.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == CrossPlatformConsoleVirtualKeyboard.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == MessageBoxConsoleView.InputLayerName) && !(instance.CurrentInputLayer.ContextName == NetLobbyConsoleView.InputLayerName))
		{
			instance.PopLayer(InputLayer);
			instance.PushLayer(InputLayer);
		}
	}

	private void OnQuickLoadConfirm()
	{
		UtilityMessageBox.ShowMessageBox(UIStrings.Instance.SaveLoadTexts.ConfirmQuickLoad, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
		{
			if (button == DialogMessageBoxButton.Yes)
			{
				base.ViewModel.OnQuickLoad();
			}
		});
	}

	public void HandleInvite(Action<bool> callback)
	{
	}

	public void HandleInviteAccepted(bool accepted)
	{
		PFLog.Net.Log($"ACCEPTED INVITE: {accepted}");
		if (accepted)
		{
			base.ViewModel.OnClose();
		}
	}
}
