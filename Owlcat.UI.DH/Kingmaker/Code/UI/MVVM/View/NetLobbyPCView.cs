using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyPCView : NetLobbyBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private OwlcatButton m_CloseButton;

	[Space]
	[SerializeField]
	private NetLobbyCreateJoinPartPCView m_CreateJoinBlock;

	[SerializeField]
	private NetLobbyWaitingPartPCView m_WaitingBlock;

	[SerializeField]
	private NetLobbyLobbyPartPCView m_LobbyBlock;

	[SerializeField]
	private NetLobbyTutorialPartPCView m_TutorialBlock;

	[SerializeField]
	private OwlcatButton[] m_TutorialButtons;

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
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, ButtonSoundsEnum.PlastickSound);
		m_CloseButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.OnClose).AddTo(this);
		m_TutorialButtons.ForEach(delegate(OwlcatButton b)
		{
			b.gameObject.SetActive(base.ViewModel.IsAnyTutorialBlocks.CurrentValue);
			b.OnLeftClickAsObservable().Subscribe(base.ViewModel.ShowNetLobbyTutorial).AddTo(this);
			b.SetHint(UIStrings.Instance.NetLobbyTexts.HowToPlay).AddTo(this);
		});
		EscHotkeyManager.Instance.Subscribe(delegate
		{
			if (!base.ViewModel.NetLobbyTutorialOnScreen.CurrentValue)
			{
				base.ViewModel.OnClose();
			}
		}).AddTo(this);
		base.OnBind();
		base.ViewModel.SaveSlotCollectionVm.Subscribe(m_SlotCollectionView.Bind).AddTo(this);
		m_CreateJoinBlock.Bind(base.ViewModel);
		m_WaitingBlock.Bind(base.ViewModel);
		m_LobbyBlock.Bind(base.ViewModel);
		base.ViewModel.NetLobbyTutorialPartVM.Subscribe(m_TutorialBlock.Bind).AddTo(this);
		base.ViewModel.NetLobbyTutorialOnScreen.Subscribe(delegate(bool value)
		{
			m_CloseButton.gameObject.SetActive(!value);
		}).AddTo(this);
	}
}
