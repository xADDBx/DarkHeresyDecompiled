using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Owlcat.UI;
using R3;
using Rewired;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyCreateJoinPartConsoleView : NetLobbyCreateJoinPartBaseView
{
	[Header("Console Part")]
	[SerializeField]
	private ConsoleInputField m_ConsoleInputField;

	[SerializeField]
	private ConsoleHint m_SelectRegionHint;

	[SerializeField]
	private ConsoleHint m_PasteLobbyIdHint;

	[SerializeField]
	private ConsoleHint m_ShowLobbyCodeHint;

	[SerializeField]
	private ConsoleHint m_EnterLobbyIdHint;

	[SerializeField]
	private ConsoleHint m_JoinableUserTypesHint;

	[SerializeField]
	private ConsoleHint m_InvitableUserTypesHint;

	[SerializeField]
	private OwlcatMultiButton m_CreateBlockFocusButton;

	[SerializeField]
	private OwlcatMultiButton m_JoinBlockFocusButton;

	[SerializeField]
	private TextMeshProUGUI m_CreateLobbyLabel;

	private readonly ReactiveProperty<bool> m_InputFieldIsFocused = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CreateBlockIsFocused = new ReactiveProperty<bool>();

	private InputLayer m_InputLayer;

	protected override void OnBind()
	{
		base.OnBind();
		m_ConsoleInputField.Bind(string.Empty, delegate
		{
		});
		m_CreateLobbyLabel.text = UIStrings.Instance.NetLobbyTexts.CreateLobby;
		GamePad.Instance.OnLayerPoped.Subscribe(OnCurrentInputLayerPopped).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_CreateBlockIsFocused.Value = false;
		m_InputFieldIsFocused.Value = false;
		base.OnUnbind();
	}

	public void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		m_InputLayer = inputLayer;
		hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnClose();
		}, 9, m_InputFieldIsFocused.Not().And(IsInCreateJoinPart).ToReadOnlyReactiveProperty(initialValue: false)), UIStrings.Instance.CharGen.Back).AddTo(this);
		hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			ActivateDeactivateInputField(state: false);
		}, 9, m_InputFieldIsFocused.And(IsInCreateJoinPart).ToReadOnlyReactiveProperty(initialValue: false)), UIStrings.Instance.SettingsUI.Cancel).AddTo(this);
		hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.ShowNetLobbyTutorial();
		}, 19, base.ViewModel.IsAnyTutorialBlocks.And(IsInCreateJoinPart).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustLongPressed), UIStrings.Instance.NetLobbyTexts.HowToPlay).AddTo(this);
		m_SelectRegionHint.Bind(inputLayer.AddButton(delegate
		{
			m_RegionDropdown.SetState(value: true);
		}, 17, m_InputFieldIsFocused.Not().And(IsInCreateJoinPart).ToReadOnlyReactiveProperty(initialValue: false))).AddTo(this);
		m_SelectRegionHint.SetLabel(UIStrings.Instance.NetLobbyTexts.SelectRegion);
		m_ShowLobbyCodeHint.Bind(inputLayer.AddButton(delegate
		{
			ShowLobbyCode.Value = !ShowLobbyCode.Value;
		}, 16, IsInCreateJoinPart, InputActionEventType.ButtonJustLongPressed)).AddTo(this);
		m_ShowLobbyCodeHint.SetLabel(UIStrings.Instance.NetLobbyTexts.ShowLobbyCode);
		ShowLobbyCode.Subscribe(delegate(bool state)
		{
			m_ShowLobbyCodeHint.SetLabel(state ? UIStrings.Instance.NetLobbyTexts.HideLobbyCode : UIStrings.Instance.NetLobbyTexts.ShowLobbyCode);
		}).AddTo(this);
		m_PasteLobbyIdHint.Bind(inputLayer.AddButton(delegate
		{
			m_ConsoleInputField.Text = base.ViewModel.GetCopiedLobbyId();
		}, 16, IsInCreateJoinPart, InputActionEventType.ButtonJustReleased)).AddTo(this);
		m_PasteLobbyIdHint.SetLabel(UIStrings.Instance.NetLobbyTexts.PasteLobbyId);
		m_EnterLobbyIdHint.Bind(inputLayer.AddButton(delegate
		{
			ActivateDeactivateInputField(state: true);
		}, 10, m_InputFieldIsFocused.Not().And(IsInCreateJoinPart).And(m_CreateBlockIsFocused.Not())
			.ToReadOnlyReactiveProperty(initialValue: false))).AddTo(this);
		m_EnterLobbyIdHint.SetLabel(UIStrings.Instance.NetLobbyTexts.JoinLobbyCodePlaceholder);
		m_JoinableUserTypesHint.Bind(inputLayer.AddButton(delegate
		{
			m_JoinableUserTypesDropdown.SetState(value: true);
		}, 10, m_InputFieldIsFocused.Not().And(IsInCreateJoinPart).And(m_CreateBlockIsFocused)
			.ToReadOnlyReactiveProperty(initialValue: false))).AddTo(this);
		m_InvitableUserTypesHint.Bind(inputLayer.AddButton(delegate
		{
			m_InvitableUserTypesDropdown.SetState(value: true);
		}, 11, m_InputFieldIsFocused.Not().And(IsInCreateJoinPart).And(m_CreateBlockIsFocused)
			.ToReadOnlyReactiveProperty(initialValue: false))).AddTo(this);
		hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.CreateLobby();
		}, 8, m_InputFieldIsFocused.Not().And(IsInCreateJoinPart).And(m_CreateBlockIsFocused)
			.ToReadOnlyReactiveProperty(initialValue: false)), UIStrings.Instance.NetLobbyTexts.CreateLobby).AddTo(this);
		hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			if (m_InputFieldIsFocused.Value)
			{
				ActivateDeactivateInputField(state: false);
			}
			base.ViewModel.JoinLobby();
		}, 8, ReadyToJoin.And(IsInCreateJoinPart).And(m_CreateBlockIsFocused.Not()).ToReadOnlyReactiveProperty(initialValue: false)), UIStrings.Instance.NetLobbyTexts.JoinLobby).AddTo(this);
	}

	public void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		List<IConsoleEntity> entities = new List<IConsoleEntity> { m_CreateBlockFocusButton, m_JoinBlockFocusButton };
		navigationBehaviour.AddRow(entities);
		IsInCreateJoinPart.Subscribe(delegate(bool state)
		{
			if (state)
			{
				DelayedInvoker.InvokeInFrames(delegate
				{
					navigationBehaviour.FocusOnEntityManual(m_CreateBlockFocusButton);
				}, 1);
			}
			else
			{
				navigationBehaviour.UnFocusCurrentEntity();
			}
		}).AddTo(this);
		navigationBehaviour.Focus.Subscribe(OnFocusEntity).AddTo(this);
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		m_CreateBlockIsFocused.Value = entity as OwlcatMultiButton == m_CreateBlockFocusButton && IsInCreateJoinPart.Value;
	}

	private void ActivateDeactivateInputField(bool state)
	{
		if (state)
		{
			m_ConsoleInputField.Select();
		}
		else
		{
			m_ConsoleInputField.Abort();
		}
		m_InputFieldIsFocused.Value = state;
	}

	private void OnCurrentInputLayerPopped()
	{
		if (GamePad.Instance.CurrentInputLayer == m_InputLayer)
		{
			ActivateDeactivateInputField(state: false);
		}
	}
}
