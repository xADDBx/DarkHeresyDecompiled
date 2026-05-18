using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyCreateJoinPartConsoleView : NetLobbyCreateJoinPartBaseView
{
	[Header("Console Part")]
	[SerializeField]
	private ConsoleInputField m_ConsoleInputField;

	[SerializeField]
	private HintView m_SelectRegionHint;

	[SerializeField]
	private HintView m_PasteLobbyIdHint;

	[SerializeField]
	private HintView m_ShowLobbyCodeHint;

	[SerializeField]
	private HintView m_EnterLobbyIdHint;

	[SerializeField]
	private HintView m_JoinableUserTypesHint;

	[SerializeField]
	private HintView m_InvitableUserTypesHint;

	[SerializeField]
	private OwlcatMultiButton m_CreateBlockFocusButton;

	[SerializeField]
	private OwlcatMultiButton m_JoinBlockFocusButton;

	[SerializeField]
	private TextMeshProUGUI m_CreateLobbyLabel;

	private readonly ReactiveProperty<bool> m_InputFieldIsFocused = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CreateBlockIsFocused = new ReactiveProperty<bool>();

	protected override void OnBind()
	{
		base.OnBind();
		m_ConsoleInputField.Bind(string.Empty, delegate
		{
		});
		m_CreateLobbyLabel.text = UIStrings.Instance.NetLobbyTexts.CreateLobby;
	}

	protected override void OnUnbind()
	{
		m_CreateBlockIsFocused.Value = false;
		m_InputFieldIsFocused.Value = false;
		base.OnUnbind();
	}

	public void CreateInputImpl()
	{
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
}
