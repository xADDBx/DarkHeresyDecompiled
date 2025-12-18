using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarConvertedConsoleView : View<ActionBarConvertedVM>, IClickMechanicActionBarSlotHandler, ISubscriber
{
	[SerializeField]
	private ActionBarBaseSlotView m_SlotView;

	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	[SerializeField]
	protected RectTransform m_TooltipPlace;

	[SerializeField]
	private int ColumnsCount = 2;

	private bool m_IsInit;

	private readonly List<ActionBarBaseSlotView> m_Slots = new List<ActionBarBaseSlotView>();

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private bool m_ShowTooltip;

	private readonly ReactiveProperty<bool> m_HasTooltip = new ReactiveProperty<bool>();

	private VisibilityController m_Visibility;

	private void Awake()
	{
		m_Visibility = VisibilityController.Control(base.gameObject);
		m_Visibility.SetVisible(visible: false);
	}

	protected override void OnBind()
	{
		TryFindConsoleHintWidget();
		CreateInput();
		foreach (ActionBarSlotVM slot in base.ViewModel.Slots)
		{
			ActionBarBaseSlotView widget = WidgetFactory.GetWidget(m_SlotView);
			widget.Initialize();
			widget.transform.SetParent(m_Container, worldPositionStays: false);
			widget.Bind(slot);
			m_Slots.Add(widget);
		}
		EventBus.Subscribe(this).AddTo(this);
		SetConsoleEntities();
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
		m_NavigationBehaviour.FocusOnFirstValidEntity();
		m_Visibility.SetVisible(visible: true);
	}

	protected override void OnUnbind()
	{
		m_Slots.ForEach(WidgetFactory.DisposeWidget);
		m_Slots.Clear();
		GamePad.Instance.PopLayer(m_InputLayer);
		m_NavigationBehaviour.UnFocusCurrentEntity();
		m_ShowTooltip = false;
		m_Visibility.SetVisible(visible: false);
	}

	private void SetConsoleEntities()
	{
		m_NavigationBehaviour.Clear();
		List<IConsoleNavigationEntity> list = m_Slots.Select((ActionBarBaseSlotView x) => (IConsoleNavigationEntity)x).ToList();
		m_NavigationBehaviour.AddRow(list.GetRange(0, ColumnsCount));
		m_NavigationBehaviour.AddRow(list.GetRange(ColumnsCount, list.Count - ColumnsCount));
	}

	private void CreateInput()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour(null, null, new Vector2Int(1, 0));
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "ActionBarConvertedConsoleView"
		});
		if (!(m_HintsWidget == null))
		{
			InputBindStruct inputBindStruct = m_InputLayer.AddButton(OnDecline, 9);
			m_HintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Cancel, ConsoleHintsWidget.HintPosition.Left).AddTo(this);
			inputBindStruct.AddTo(this);
			InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased);
			m_HintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Information, ConsoleHintsWidget.HintPosition.Left).AddTo(this);
			inputBindStruct2.AddTo(this);
			m_NavigationBehaviour.Focus.Subscribe(OnFocusEntity).AddTo(this);
		}
	}

	private void OnDecline(InputActionEventData data)
	{
		base.ViewModel.Close();
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		TooltipBaseTemplate tooltipBaseTemplate = (entity as IHasTooltipTemplate)?.TooltipTemplate();
		m_HasTooltip.Value = tooltipBaseTemplate != null;
		if (m_ShowTooltip)
		{
			((entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour).ShowTooltip(tooltipBaseTemplate, new TooltipConfig
			{
				TooltipPlace = m_TooltipPlace,
				PriorityPivots = new List<Vector2>
				{
					new Vector2(0.5f, 0f)
				}
			});
		}
		else
		{
			TooltipHelper.HideTooltip();
		}
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip = !RootUIContext.Instance.TooltipIsShown;
		OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
	}

	public void HandleClickMechanicActionBarSlot(MechanicActionBarSlot ability)
	{
		base.ViewModel.Close();
	}

	private void TryFindConsoleHintWidget()
	{
		if (!(m_HintsWidget != null))
		{
			ConsoleHintWidgetContainer componentInParent = GetComponentInParent<ConsoleHintWidgetContainer>();
			if ((bool)componentInParent)
			{
				m_HintsWidget = componentInParent.GetConsoleHintWidget();
			}
		}
	}
}
