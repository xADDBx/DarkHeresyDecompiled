using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerConsoleView : DlcManagerBaseView
{
	[Header("Views")]
	[SerializeField]
	private DlcManagerTabDlcsConsoleView m_DlcManagerTabDlcsConsoleView;

	[SerializeField]
	private DlcManagerTabModsConsoleView m_DlcManagerTabModsConsoleView;

	[SerializeField]
	private DlcManagerTabSwitchOnDlcsConsoleView m_DlcManagerTabSwitchOnDlcsConsoleView;

	[SerializeField]
	private HintView m_DeclineHint;

	[SerializeField]
	private HintView m_PurchaseHint;

	[SerializeField]
	private HintView m_PrevHint;

	[SerializeField]
	private HintView m_NextHint;

	[SerializeField]
	private HintView m_ApplyHintHint;

	[SerializeField]
	private HintView m_DefaultHint;

	[SerializeField]
	private HintView m_OpenModSettingsHint;

	[SerializeField]
	private HintView m_InstallDlcHint;

	[SerializeField]
	private HintView m_DeleteDlcHint;

	[SerializeField]
	private HintView m_PlayPauseVideoHint;

	private CompositeDisposable m_Disposable = new CompositeDisposable();

	private readonly ReactiveProperty<bool> m_ModSettingsIsAvailable = new ReactiveProperty<bool>();

	protected override void InitializeImpl()
	{
		if (!base.ViewModel.InGame)
		{
			m_DlcManagerTabDlcsConsoleView.Initialize();
		}
		else
		{
			m_DlcManagerTabSwitchOnDlcsConsoleView.Initialize();
		}
		if (!base.ViewModel.IsConsole)
		{
			m_DlcManagerTabModsConsoleView.Initialize();
		}
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_Disposable = new CompositeDisposable();
		if (!base.ViewModel.InGame)
		{
			m_DlcManagerTabDlcsConsoleView.Bind(base.ViewModel.DlcsVM);
		}
		else
		{
			m_DlcManagerTabSwitchOnDlcsConsoleView.Bind(base.ViewModel.SwitchOnDlcsVM);
		}
		if (!base.ViewModel.IsConsole)
		{
			m_DlcManagerTabModsConsoleView.Bind(base.ViewModel.ModsVM);
		}
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_Disposable?.Clear();
		m_Disposable?.Dispose();
		m_Disposable = null;
	}

	private void CreateInputImpl()
	{
	}

	private void Scroll(float value)
	{
		if (base.ViewModel.SelectedMenuEntity.CurrentValue.DlcManagerTabVM == base.ViewModel.DlcsVM && !base.ViewModel.InGame)
		{
			m_DlcManagerTabDlcsConsoleView.Scroll(value);
		}
		else if (base.ViewModel.SelectedMenuEntity.CurrentValue.DlcManagerTabVM == base.ViewModel.ModsVM && !base.ViewModel.IsConsole)
		{
			m_DlcManagerTabModsConsoleView.Scroll(value);
		}
		else if (base.ViewModel.SelectedMenuEntity.CurrentValue.DlcManagerTabVM == base.ViewModel.SwitchOnDlcsVM && base.ViewModel.InGame)
		{
			m_DlcManagerTabSwitchOnDlcsConsoleView.Scroll(value);
		}
	}
}
