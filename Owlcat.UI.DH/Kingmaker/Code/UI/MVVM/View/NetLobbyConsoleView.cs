using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyConsoleView : NetLobbyBaseView
{
	[Header("Console Part")]
	[SerializeField]
	private NetLobbyCreateJoinPartConsoleView m_CreateJoinBlock;

	[SerializeField]
	private NetLobbyWaitingPartConsoleView m_WaitingBlock;

	[SerializeField]
	private NetLobbyLobbyPartConsoleView m_LobbyBlock;

	[SerializeField]
	private NetLobbyTutorialPartConsoleView m_TutorialBlock;

	public static readonly string InputLayerName = "NetLobby";

	private NetLobbySaveSlotCollectionConsoleView SlotCollectionView => m_SlotCollectionView as NetLobbySaveSlotCollectionConsoleView;

	public override void Initialize()
	{
		base.Initialize();
		m_CreateJoinBlock.Initialize();
		m_WaitingBlock.Initialize();
		m_LobbyBlock.Initialize();
		m_TutorialBlock.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_CreateJoinBlock.Bind(base.ViewModel);
		m_WaitingBlock.Bind(base.ViewModel);
		m_LobbyBlock.Bind(base.ViewModel);
		base.ViewModel.SaveSlotCollectionVm.Subscribe(SlotCollectionView.Bind).AddTo(this);
		CreateInput();
		base.ViewModel.NetLobbyTutorialPartVM.Subscribe(m_TutorialBlock.Bind).AddTo(this);
	}

	private void CreateInput()
	{
	}
}
