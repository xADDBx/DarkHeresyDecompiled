using Owlcat.UI;
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

	[SerializeField]
	private ConsoleHintsWidget m_CommonHintsWidget;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

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

	protected override void OnUnbind()
	{
		m_NavigationBehaviour.UnFocusCurrentEntity();
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
		m_InputLayer = null;
		base.OnUnbind();
	}

	private void CreateInput()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = InputLayerName
		});
		BuildNavigationImpl(m_NavigationBehaviour);
		CreateInputImpl(m_InputLayer);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	protected virtual void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		m_CreateJoinBlock.BuildNavigationImpl(navigationBehaviour);
		m_LobbyBlock.BuildNavigationImpl(navigationBehaviour);
	}

	private void CreateInputImpl(InputLayer inputLayer)
	{
		m_CreateJoinBlock.CreateInputImpl(inputLayer, m_CommonHintsWidget);
		m_WaitingBlock.CreateInputImpl(inputLayer, m_CommonHintsWidget);
		m_LobbyBlock.CreateInputImpl(inputLayer, m_CommonHintsWidget);
	}
}
