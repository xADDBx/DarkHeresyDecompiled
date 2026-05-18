using System.Collections.Generic;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarConvertedConsoleView : View<ActionBarConvertedVM>, IClickMechanicActionBarSlotHandler, ISubscriber
{
	[SerializeField]
	private ActionBarBaseSlotView m_SlotView;

	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	protected RectTransform m_TooltipPlace;

	[SerializeField]
	private int ColumnsCount = 2;

	private bool m_IsInit;

	private readonly List<ActionBarBaseSlotView> m_Slots = new List<ActionBarBaseSlotView>();

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
		m_Visibility.SetVisible(visible: true);
	}

	protected override void OnUnbind()
	{
		m_Slots.ForEach(WidgetFactory.DisposeWidget);
		m_Slots.Clear();
		m_ShowTooltip = false;
		m_Visibility.SetVisible(visible: false);
	}

	private void CreateInput()
	{
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

	public void HandleClickMechanicActionBarSlot(MechanicActionBarSlot ability)
	{
		base.ViewModel.Close();
	}
}
