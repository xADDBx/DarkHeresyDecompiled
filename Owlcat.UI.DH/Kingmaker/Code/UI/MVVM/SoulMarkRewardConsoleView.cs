using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SoulMarkRewardConsoleView : SoulMarkRewardBaseView
{
	[Header("Console Part")]
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	[SerializeField]
	private Vector2 m_TooltipPivot = new Vector2(0f, 0.5f);

	[SerializeField]
	private RectTransform m_TooltipPlace;

	private readonly ReactiveProperty<bool> m_TooltipShown = new ReactiveProperty<bool>();

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private TooltipConfig m_TooltipConfig;

	private IConsoleHint m_DeclineBind;

	protected override void OnBind()
	{
		base.OnBind();
		m_TooltipConfig = new TooltipConfig
		{
			PriorityPivots = new List<Vector2> { m_TooltipPivot },
			TooltipPlace = m_TooltipPlace
		};
		CreateInput();
		EventBus.Subscribe(this).AddTo(this);
		GamePad.Instance.OnLayerPoped.Subscribe(OnCurrentInputLayerChanged).AddTo(this);
	}

	private void CreateInput()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "RewardWindow"
		});
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			CloseTooltip();
		}, 9, m_TooltipShown);
		m_DeclineBind = m_HintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Back);
		m_DeclineBind.AddTo(this);
		inputBindStruct.AddTo(this);
		InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(delegate
		{
			OnAccept();
		}, 8, m_TooltipShown.Not().ToReadOnlyReactiveProperty(initialValue: false));
		m_HintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Accept).AddTo(this);
		inputBindStruct2.AddTo(this);
		InputBindStruct inputBindStruct3 = m_InputLayer.AddButton(delegate
		{
			ToggleTooltip();
		}, 10, m_TooltipShown.Not().ToReadOnlyReactiveProperty(initialValue: false));
		m_HintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.Tooltips.ShowTooltipHint).AddTo(this);
		inputBindStruct3.AddTo(this);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	private void CloseTooltip()
	{
		if (m_TooltipShown.Value)
		{
			ToggleTooltip();
		}
	}

	private void OnAccept()
	{
		base.ViewModel.OnDeclinePressed();
	}

	private void ToggleTooltip()
	{
		m_TooltipShown.Value = !m_TooltipShown.Value;
		TooltipHelper.HideTooltip();
		if (m_TooltipShown.Value)
		{
			this.ShowConsoleTooltip(base.ViewModel.Tooltip, m_NavigationBehaviour, m_TooltipConfig);
		}
	}

	private void OnCurrentInputLayerChanged()
	{
		if (GamePad.Instance.CurrentInputLayer == m_InputLayer)
		{
			TooltipHelper.HideTooltip();
			m_TooltipShown.Value = false;
		}
	}
}
