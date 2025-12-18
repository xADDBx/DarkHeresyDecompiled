using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;
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
	private ConsoleHintsWidget m_CommonHintsWidget;

	[SerializeField]
	private ConsoleHint m_DeclineHint;

	[SerializeField]
	private ConsoleHint m_PurchaseHint;

	[SerializeField]
	private ConsoleHint m_PrevHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	[SerializeField]
	private ConsoleHint m_ApplyHintHint;

	[SerializeField]
	private ConsoleHint m_DefaultHint;

	[SerializeField]
	private ConsoleHint m_OpenModSettingsHint;

	[SerializeField]
	private ConsoleHint m_InstallDlcHint;

	[SerializeField]
	private ConsoleHint m_DeleteDlcHint;

	[SerializeField]
	private ConsoleHint m_PlayPauseVideoHint;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

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
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		CreateInput();
		UpdateNavigation();
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.ChangeTab, delegate
		{
			UpdateNavigation();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_Disposable?.Clear();
		m_Disposable?.Dispose();
		m_Disposable = null;
	}

	private void UpdateNavigation()
	{
		m_InputLayer.Unbind();
		m_NavigationBehaviour.Clear();
		if (base.ViewModel.SelectedMenuEntity.CurrentValue.DlcManagerTabVM == base.ViewModel.DlcsVM && !base.ViewModel.InGame)
		{
			m_NavigationBehaviour.SetEntitiesVertical(m_DlcManagerTabDlcsConsoleView.GetNavigationEntities());
			m_NavigationBehaviour.FocusOnFirstValidEntity();
			m_DlcManagerTabDlcsConsoleView.ScrollToTop();
			m_Disposable?.Clear();
			m_Disposable?.Add(m_NavigationBehaviour.Focus.Subscribe(m_DlcManagerTabDlcsConsoleView.ScrollList));
		}
		else if (base.ViewModel.SelectedMenuEntity.CurrentValue.DlcManagerTabVM == base.ViewModel.ModsVM && !base.ViewModel.IsConsole)
		{
			m_NavigationBehaviour.SetEntitiesVertical(m_DlcManagerTabModsConsoleView.GetNavigationEntities());
			m_NavigationBehaviour.FocusOnFirstValidEntity();
			m_DlcManagerTabModsConsoleView.ScrollToTop();
			m_Disposable?.Clear();
			m_Disposable?.Add(m_NavigationBehaviour.Focus.Subscribe(m_DlcManagerTabModsConsoleView.ScrollList));
		}
		else if (base.ViewModel.SelectedMenuEntity.CurrentValue.DlcManagerTabVM == base.ViewModel.SwitchOnDlcsVM && base.ViewModel.InGame)
		{
			m_NavigationBehaviour.SetEntitiesVertical(m_DlcManagerTabSwitchOnDlcsConsoleView.GetNavigationEntities());
			m_NavigationBehaviour.FocusOnFirstValidEntity();
			m_DlcManagerTabSwitchOnDlcsConsoleView.ScrollToTop();
			m_Disposable?.Clear();
			m_Disposable?.Add(m_NavigationBehaviour.Focus.Subscribe(m_DlcManagerTabSwitchOnDlcsConsoleView.ScrollList));
		}
		m_InputLayer.Bind();
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "DlcManager"
		});
		CreateInputImpl(m_InputLayer);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	private void CreateInputImpl(InputLayer inputLayer)
	{
		m_InputLayer.AddAxis(Scroll, 3, repeat: true).AddTo(this);
		m_DeclineHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnClose();
		}, 9)).AddTo(this);
		m_DeclineHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
		if (!base.ViewModel.IsConsole)
		{
			m_PrevHint.Bind(inputLayer.AddButton(delegate
			{
				m_Selector.OnPrev();
			}, 14)).AddTo(this);
			m_NextHint.Bind(inputLayer.AddButton(delegate
			{
				m_Selector.OnNext();
			}, 15)).AddTo(this);
			if (base.ViewModel.InGame)
			{
				m_ApplyHintHint.Bind(inputLayer.AddButton(delegate
				{
					base.ViewModel.CheckToReloadGame(null);
				}, 8, base.ViewModel.IsModsWindow.Or(base.ViewModel.IsSwitchOnDlcsWindow).And(base.ViewModel.SwitchOnDlcsVM?.NeedResave.Or(base.ViewModel.ModsVM?.NeedReload)).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustLongPressed)).AddTo(this);
				m_DefaultHint.Bind(inputLayer.AddButton(delegate
				{
					base.ViewModel.RestoreAllToPreviousState();
				}, 11, base.ViewModel.IsModsWindow.Or(base.ViewModel.IsSwitchOnDlcsWindow).And(base.ViewModel.SwitchOnDlcsVM?.NeedResave.Or(base.ViewModel.ModsVM?.NeedReload)).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustLongPressed)).AddTo(this);
			}
			else
			{
				m_ApplyHintHint.Bind(inputLayer.AddButton(delegate
				{
					base.ViewModel.CheckToReloadGame(null);
				}, 8, base.ViewModel.IsModsWindow.And(base.ViewModel.ModsVM.NeedReload).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustLongPressed)).AddTo(this);
				m_DefaultHint.Bind(inputLayer.AddButton(delegate
				{
					base.ViewModel.RestoreAllToPreviousState();
				}, 11, base.ViewModel.IsModsWindow.And(base.ViewModel.ModsVM.NeedReload).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustLongPressed)).AddTo(this);
			}
			m_ApplyHintHint.SetLabel(UIStrings.Instance.SettingsUI.Apply);
			m_DefaultHint.SetLabel(UIStrings.Instance.SettingsUI.Default);
		}
		if (!base.ViewModel.InGame)
		{
			m_DlcManagerTabDlcsConsoleView.CreateInputImpl(inputLayer, m_CommonHintsWidget, m_PurchaseHint, m_InstallDlcHint, m_DeleteDlcHint, m_PlayPauseVideoHint);
		}
		if (!base.ViewModel.IsConsole)
		{
			m_DlcManagerTabModsConsoleView.CreateInputImpl(inputLayer, m_CommonHintsWidget, m_OpenModSettingsHint, m_ModSettingsIsAvailable);
		}
		m_NavigationBehaviour.Focus.Subscribe(OnFocusEntity).AddTo(this);
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		if (m_NavigationBehaviour.Entities.Any())
		{
			if (entity is DlcManagerDlcEntityConsoleView dlcManagerDlcEntityConsoleView)
			{
				m_DeleteDlcHint.gameObject.SetActive(dlcManagerDlcEntityConsoleView.IsDlcCanBeDeleted);
			}
			if (entity is DlcManagerModEntityConsoleView dlcManagerModEntityConsoleView && !base.ViewModel.IsConsole)
			{
				m_ModSettingsIsAvailable.Value = dlcManagerModEntityConsoleView.GetAvailableSettings();
			}
		}
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		if (base.ViewModel.SelectedMenuEntity.CurrentValue.DlcManagerTabVM == base.ViewModel.DlcsVM && !base.ViewModel.InGame)
		{
			m_DlcManagerTabDlcsConsoleView.Scroll(obj, value);
		}
		else if (base.ViewModel.SelectedMenuEntity.CurrentValue.DlcManagerTabVM == base.ViewModel.ModsVM && !base.ViewModel.IsConsole)
		{
			m_DlcManagerTabModsConsoleView.Scroll(obj, value);
		}
		else if (base.ViewModel.SelectedMenuEntity.CurrentValue.DlcManagerTabVM == base.ViewModel.SwitchOnDlcsVM && base.ViewModel.InGame)
		{
			m_DlcManagerTabSwitchOnDlcsConsoleView.Scroll(obj, value);
		}
	}
}
