using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.Common;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.DetectiveJournal;
using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.ServiceWindows;

[ViewFactoryPolicy(ViewFactoryPolicyFlag.DontReparent, null)]
public class ServiceWindowsPanelPCView : ServiceWindowsPanelBaseView
{
	[Header("Elements")]
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[Header("Menu")]
	[SerializeField]
	private MenuBaseView m_MenuView;

	[Header("Views")]
	[SerializeField]
	private InventoryPCView m_InventoryPCView;

	[SerializeField]
	private CharInfoBaseView m_CharInfoBaseView;

	[SerializeField]
	private JournalPCView m_JournalView;

	[SerializeField]
	private FactionReputationView m_ReputationView;

	[SerializeField]
	private DetectiveJournalBaseView m_DetectiveJournalView;

	[SerializeField]
	private EncyclopediaPCView m_EncyclopediaView;

	[SerializeField]
	private LocalMapBaseView m_LocalMapBase;

	public void Awake()
	{
		m_InventoryPCView.Initialize();
		m_CharInfoBaseView.Initialize();
		m_JournalView.Initialize();
		m_ReputationView.Initialize();
		m_LocalMapBase.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_MenuView.Bind(base.ViewModel.MenuVM);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
		base.ViewModel.LockedWindowType.Subscribe(delegate(ServiceWindowsType value)
		{
			m_CloseButton.Interactable = value == ServiceWindowsType.None;
		}).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close).AddTo(this);
	}

	protected override void OnShowTransitionCompleted()
	{
		base.ViewModel.InventoryVM.Subscribe(m_InventoryPCView.Bind).AddTo(this);
		base.ViewModel.CharInfoVM.Subscribe(m_CharInfoBaseView.Bind).AddTo(this);
		base.ViewModel.JournalVM.Subscribe(m_JournalView.Bind).AddTo(this);
		base.ViewModel.ReputationVM.Subscribe(m_ReputationView.Bind).AddTo(this);
		base.ViewModel.DetectiveJournalVM.Subscribe(m_DetectiveJournalView.Bind).AddTo(this);
		base.ViewModel.EncyclopediaVM.Subscribe(m_EncyclopediaView.Bind).AddTo(this);
		base.ViewModel.LocalMapVM.Subscribe(m_LocalMapBase.Bind).AddTo(this);
	}
}
