using System.Collections.Generic;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyLobbyPartConsoleView : NetLobbyLobbyPartBaseView
{
	[Header("Console Part")]
	[SerializeField]
	private List<NetLobbyPlayerConsoleView> m_PlayerList;

	[SerializeField]
	private HintView m_ShowHideLobbyCodeHint;

	[SerializeField]
	private HintView m_CopyLobbyCodeHint;

	[SerializeField]
	private OwlcatButton m_SavePartFocusButton;

	[SerializeField]
	private NetLobbyDlcListConsoleView m_DlcListConsoleView;

	private readonly ReactiveProperty<bool> m_SaveSlotIsFocused = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_EpicGamesIsFocused = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_GamersTagsMode = new ReactiveProperty<bool>();

	public override void Initialize()
	{
		base.Initialize();
		m_DlcListConsoleView.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		for (int i = 0; i < m_PlayerList.Count; i++)
		{
			m_PlayerList[i].Bind(base.ViewModel.PlayerVms[i]);
		}
		base.ViewModel.DlcListVM.Subscribe(m_DlcListConsoleView.Bind).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_SaveSlotIsFocused.Value = false;
		base.OnUnbind();
	}

	public void CreateInputImpl()
	{
	}

	private void AddGamersTagsInput()
	{
	}

	private void ShowGamersTagsMode()
	{
	}

	private void CloseGamersTagsMode()
	{
	}

	private void SetGamersTagsNavigation()
	{
	}

	private void Launch()
	{
		base.ViewModel.Launch();
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		m_SaveSlotIsFocused.Value = entity as OwlcatButton == m_SavePartFocusButton && IsInLobbyPart.Value;
		m_EpicGamesIsFocused.Value = entity as OwlcatButton == m_ConnectEpicGamesToSteam && IsInLobbyPart.Value;
	}
}
