using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.UI;
using R3;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class MainMenuButtonsConsoleView : MainMenuButtonsView<ContextMenuEntityConsoleView>, ISavesUpdatedHandler, ISubscriber
{
	[Space]
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	[SerializeField]
	private ConsoleHint m_LicenseHint;

	[SerializeField]
	private OwlcatMultiButton m_FirstGlossaryFocus;

	[SerializeField]
	private OwlcatMultiButton m_SecondGlossaryFocus;

	[Header("XBox")]
	[SerializeField]
	protected GameObject m_XBoxGamerGroup;

	[SerializeField]
	protected TextMeshProUGUI m_XBoxGamerTagText;

	[SerializeField]
	protected RawImage m_XBoxGamerRawImage;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_LinkNavigation;

	private InputLayer m_LinkInputLayer;

	private readonly ReactiveProperty<bool> m_InputEnabled = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasLinks = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsLinkMode = new ReactiveProperty<bool>();

	[Header("Navigation")]
	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_Parameters;

	private FloatConsoleNavigationBehaviour NavigationBehaviour { get; set; }

	protected override void OnBind()
	{
		base.OnBind();
		m_XBoxGamerGroup.gameObject.SetActive(value: false);
		BuildNavigation();
	}

	protected override void OnUnbind()
	{
		NavigationBehaviour.Clear();
		NavigationBehaviour = null;
		m_InputLayer = null;
		m_LinkNavigation.Clear();
		m_LinkNavigation = null;
		m_LinkInputLayer = null;
		m_HintsWidget.Dispose();
	}

	private void BuildNavigation()
	{
		NavigationBehaviour = new FloatConsoleNavigationBehaviour(m_Parameters);
		NavigationBehaviour.AddTo(this);
		List<ContextMenuEntityConsoleView> list = new List<ContextMenuEntityConsoleView> { m_ContinueView, m_NewGameView, m_LoadView, m_OptionsView, m_CreditView, m_AddonsView };
		if (BuildModeUtility.IsCoopEnabled)
		{
			list.Add(m_NetView);
		}
		if (base.ViewModel.ExitEnabled)
		{
			list.Add(m_ExitView);
		}
		NavigationBehaviour.AddEntities(list);
		NavigationBehaviour.FocusOnEntityManual(m_ContinueView.IsValid() ? m_ContinueView : m_NewGameView);
		m_InputLayer = GetInputLayer();
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
		DelayedInvoker.InvokeInFrames(CalculateGlossary, 3);
		EventBus.Subscribe(this).AddTo(this);
		m_InputEnabled.Value = true;
	}

	public void OnSaveListUpdated()
	{
	}

	private InputLayer GetInputLayer()
	{
		InputLayer inputLayer = NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "MainMenuSideBarInputContext"
		});
		inputLayer.AddButton(OnConfirmClick, 8, m_InputEnabled).AddTo(this);
		if (BuildModeUtility.IsDevelopment)
		{
			inputLayer.AddButton(OnStreamSaves, 17, m_InputEnabled).AddTo(this);
		}
		m_HintsWidget.BindHint(inputLayer.AddButton(EnterLinks, 11, m_HasLinks.And(m_InputEnabled).ToReadOnlyReactiveProperty(initialValue: false)), "", ConsoleHintsWidget.HintPosition.Left).AddTo(this);
		return inputLayer;
	}

	private async void OnStreamSaves(InputActionEventData obj)
	{
		await base.ViewModel.OnStreamSaves();
	}

	private void CalculateGlossary()
	{
		m_LinkNavigation = new GridConsoleNavigationBehaviour(null, null, Vector2Int.one, lineGrid: true).AddTo(this);
		m_LinkInputLayer = m_LinkNavigation.GetInputLayer(new InputLayer
		{
			ContextName = "MainMenuLinks"
		});
		m_HintsWidget.BindHint(m_LinkInputLayer.AddButton(ExitLinks, 9, m_IsLinkMode), UIStrings.Instance.CommonTexts.Back, ConsoleHintsWidget.HintPosition.Left).AddTo(this);
		m_LinkInputLayer.AddButton(ExitLinks, 11, m_IsLinkMode).AddTo(this);
		m_HintsWidget.BindHint(m_LinkInputLayer.AddButton(delegate
		{
		}, 8, m_IsLinkMode), UIStrings.Instance.CommonTexts.Select, ConsoleHintsWidget.HintPosition.Left).AddTo(this);
	}

	private void OnClickLink(string key)
	{
		Application.OpenURL(key);
	}

	private void OnFocusLink(string key)
	{
	}

	private void EnterLinks(InputActionEventData data)
	{
		if (m_InputEnabled.Value)
		{
			NavigationBehaviour.UnFocusCurrentEntity();
			GamePad.Instance.PushLayer(m_LinkInputLayer);
			m_LinkNavigation.FocusOnFirstValidEntity();
			m_IsLinkMode.Value = true;
		}
	}

	private void ExitLinks(InputActionEventData data)
	{
		m_LinkNavigation.UnFocusCurrentEntity();
		GamePad.Instance.PopLayer(m_LinkInputLayer);
		m_IsLinkMode.Value = false;
		NavigationBehaviour.FocusOnCurrentEntity();
	}

	private void OnConfirmClick(InputActionEventData obj)
	{
		if (NavigationBehaviour.CurrentEntity is ContextMenuEntityConsoleView)
		{
			(NavigationBehaviour.CurrentEntity as IConfirmClickHandler)?.OnConfirmClick();
		}
	}
}
