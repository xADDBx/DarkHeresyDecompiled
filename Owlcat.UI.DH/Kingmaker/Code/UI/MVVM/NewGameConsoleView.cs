using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NewGameConsoleView : NewGameBaseView
{
	[Header("Views")]
	[SerializeField]
	private NewGamePhaseStoryConsoleView m_NewGamePhaseStoryConsoleView;

	[SerializeField]
	private NewGamePhaseDifficultyConsoleView m_NewGamePhaseDifficultyConsoleView;

	[SerializeField]
	private ConsoleHintsWidget m_CommonHintsWidget;

	[SerializeField]
	private ConsoleHint m_ConfirmHint;

	[SerializeField]
	private ConsoleHint m_DeclineHint;

	[SerializeField]
	private ConsoleHint m_PrevHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	[SerializeField]
	private ConsoleHint m_SwitchOnOffDlcHint;

	[SerializeField]
	private ConsoleHint m_PurchaseHint;

	[SerializeField]
	private ConsoleHint m_InstallDlcHint;

	[SerializeField]
	private ConsoleHint m_DeleteDlcHint;

	[SerializeField]
	private ConsoleHint m_PlayPauseVideoHint;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private InputLayer m_GlossaryInputLayer;

	private GridConsoleNavigationBehaviour m_GlossaryNavigationBehavior;

	private readonly ReactiveProperty<bool> m_GlossaryMode = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasGlossary = new ReactiveProperty<bool>();

	private TooltipConfig m_TooltipConfig;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_NewGamePhaseDifficultyConsoleView.Initialize();
		m_Selector.Initialize();
		m_NewGamePhaseStoryConsoleView.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_NewGamePhaseDifficultyConsoleView.Bind(base.ViewModel.DifficultyVM);
		m_TooltipConfig.IsGlossary = true;
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		m_GlossaryNavigationBehavior = new GridConsoleNavigationBehaviour().AddTo(this);
		m_GlossaryNavigationBehavior.DeepestFocusAsObservable.Subscribe(OnGlossaryFocusedChanged).AddTo(this);
		CreateInput();
		UpdateNavigation();
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.ChangeTab, delegate
		{
			UpdateNavigation();
		}).AddTo(this);
		m_NewGamePhaseDifficultyConsoleView.ReactiveTooltipTemplate.Subscribe(delegate
		{
			CalculateGlossary();
		}).AddTo(this);
	}

	private void UpdateNavigation()
	{
		m_InputLayer.Unbind();
		m_NavigationBehaviour.Clear();
		m_InputLayer.Bind();
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "NewGame"
		});
		m_GlossaryInputLayer = m_GlossaryNavigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = "NewGameSettingsGlossary"
		});
		CreateInputImpl(m_InputLayer);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	private void CreateInputImpl(InputLayer inputLayer)
	{
		m_InputLayer.AddAxis(Scroll, 3, repeat: true).AddTo(this);
		m_ConfirmHint.SetLabel(UIStrings.Instance.CharGen.Next);
		m_DeclineHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnButtonBack();
		}, 9)).AddTo(this);
		m_DeclineHint.SetLabel(UIStrings.Instance.CharGen.Back);
		m_PrevHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnButtonBack();
		}, 14)).AddTo(this);
		m_CommonHintsWidget.BindHint(inputLayer.AddButton(ShowGlossary, 11, m_HasGlossary, InputActionEventType.ButtonJustReleased), UIStrings.Instance.Dialog.OpenGlossary).AddTo(this);
		m_GlossaryInputLayer.AddAxis(ScrollDescription, 3, repeat: true).AddTo(this);
		m_CommonHintsWidget.BindHint(m_GlossaryInputLayer.AddButton(delegate
		{
			CloseGlossary();
		}, 9, m_GlossaryMode), UIStrings.Instance.Dialog.CloseGlossary).AddTo(this);
		m_GlossaryInputLayer.AddButton(delegate
		{
			CloseGlossary();
		}, 11, m_GlossaryMode, InputActionEventType.ButtonJustReleased).AddTo(this);
		m_NewGamePhaseStoryConsoleView.CreateInputImpl(inputLayer, m_CommonHintsWidget, m_SwitchOnOffDlcHint, m_PurchaseHint, m_InstallDlcHint, m_DeleteDlcHint, m_PlayPauseVideoHint);
		m_NewGamePhaseDifficultyConsoleView.CreateInputImpl(inputLayer, m_CommonHintsWidget);
	}

	private void ShowGlossary(InputActionEventData data)
	{
		m_NavigationBehaviour.UnFocusCurrentEntity();
		(m_NavigationBehaviour.CurrentEntity as ExpandableElement)?.SetCustomLayer("On");
		m_GlossaryMode.Value = true;
		GamePad.Instance.PushLayer(m_GlossaryInputLayer).AddTo(this);
		CalculateGlossary();
		m_GlossaryNavigationBehavior.FocusOnFirstValidEntity();
	}

	private void CloseGlossary()
	{
		TooltipHelper.HideTooltip();
		m_GlossaryNavigationBehavior.UnFocusCurrentEntity();
		GamePad.Instance.PopLayer(m_GlossaryInputLayer);
		m_GlossaryMode.Value = false;
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	private void CalculateGlossary()
	{
		if (m_GlossaryNavigationBehavior != null)
		{
			m_GlossaryNavigationBehavior.Clear();
			List<IConsoleEntity> entities = m_NewGamePhaseDifficultyConsoleView.InfoView.GetNavigationBehaviour().Entities.Where((IConsoleEntity e) => e is FloatConsoleNavigationBehaviour).ToList();
			m_GlossaryNavigationBehavior.AddColumn(entities);
			m_HasGlossary.Value = m_GlossaryNavigationBehavior != null && m_GlossaryNavigationBehavior.Entities.Any();
			if (m_GlossaryMode.Value)
			{
				TooltipHelper.HideTooltip();
			}
		}
	}

	private void ShowTooltip()
	{
		IConsoleEntity value = m_GlossaryNavigationBehavior.DeepestFocusAsObservable.Value;
		MonoBehaviour component = (value as MonoBehaviour) ?? (value as IMonoBehaviour)?.MonoBehaviour;
		TooltipBaseTemplate template = (value as IHasTooltipTemplate)?.TooltipTemplate();
		component.ShowConsoleTooltip(template, m_GlossaryNavigationBehavior, m_TooltipConfig);
	}

	private void Scroll(InputActionEventData obj, float value)
	{
	}

	private void ScrollDescription(InputActionEventData obj, float value)
	{
		m_NewGamePhaseDifficultyConsoleView.Scroll(obj, value);
	}

	public void OnGlossaryFocusedChanged(IConsoleEntity entity)
	{
		MonoBehaviour monoBehaviour = ((!(entity is TMPLinkNavigationEntity tMPLinkNavigationEntity)) ? null : tMPLinkNavigationEntity.MonoBehaviour);
		MonoBehaviour monoBehaviour2 = monoBehaviour;
		if (monoBehaviour2 != null)
		{
			m_NewGamePhaseDifficultyConsoleView.InfoView.ScrollRectExtended.EnsureVisibleVertical(monoBehaviour2.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
		ShowTooltip();
	}
}
