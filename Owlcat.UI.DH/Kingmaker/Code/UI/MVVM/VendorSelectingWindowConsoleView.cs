using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VendorSelectingWindowConsoleView : VendorSelectingWindowBaseView, ICullFocusHandler, ISubscriber
{
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private ReactiveProperty<bool> m_CanConfirm = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowTooltip = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasTooltip = new ReactiveProperty<bool>();

	private VendorInfoFactionReputationItemConsoleView m_CurrentSelectedFaction;

	private IConsoleEntity m_CulledFocus;

	protected override void OnBind()
	{
		base.OnBind();
		CreateNavigation();
		m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnEntityFocused).AddTo(this);
		m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(HandleTooltip).AddTo(this);
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip.Value = !m_ShowTooltip.Value;
		HandleTooltip(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Vendor Selecting Window Console View"
		});
		m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnDeclineClick();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow).AddTo(this);
		m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			m_CurrentSelectedFaction.TryTrade();
		}, 8, m_CanConfirm), UIStrings.Instance.Vendor.Trade).AddTo(this);
		m_HintsWidget.BindHint(m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased), UIStrings.Instance.CommonTexts.Information).AddTo(this);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	private void HandleTooltip(IConsoleEntity entity)
	{
		TooltipHelper.HideTooltip();
		if (entity == null)
		{
			m_HasTooltip.Value = false;
			return;
		}
		MonoBehaviour monoBehaviour = (entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour;
		if (monoBehaviour == null)
		{
			m_HasTooltip.Value = false;
		}
		else if (entity is IHasTooltipTemplate hasTooltipTemplate)
		{
			m_HasTooltip.Value = hasTooltipTemplate.TooltipTemplate() != null;
			if (m_ShowTooltip.Value)
			{
				monoBehaviour.ShowConsoleTooltip(hasTooltipTemplate.TooltipTemplate(), m_NavigationBehaviour);
			}
		}
		else
		{
			m_HasTooltip.Value = false;
		}
	}

	public void CreateNavigation()
	{
		if (m_NavigationBehaviour == null)
		{
			m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		}
		else
		{
			m_NavigationBehaviour.Clear();
		}
		GridConsoleNavigationBehaviour entity = new GridConsoleNavigationBehaviour();
		foreach (IBindable entry in m_WidgetList.Entries)
		{
			if (entry is VendorInfoFactionReputationItemConsoleView vendorInfoFactionReputationItemConsoleView)
			{
				m_NavigationBehaviour.AddEntityHorizontal(vendorInfoFactionReputationItemConsoleView.GetNavigation());
			}
		}
		m_NavigationBehaviour.AddEntityGrid(entity);
		m_NavigationBehaviour.FocusOnFirstValidEntity();
		CreateInput();
	}

	private void OnDeclineClick()
	{
		TooltipHelper.HideTooltip();
		if (m_HasTooltip.Value && m_ShowTooltip.Value)
		{
			m_ShowTooltip.Value = false;
		}
		else
		{
			OnCloseClick();
		}
	}

	protected override void Close()
	{
		base.Close();
		TooltipHelper.HideTooltip();
		m_ShowTooltip.Value = false;
		GamePad.Instance.PopLayer(m_InputLayer);
	}

	private void OnEntityFocused(IConsoleEntity currentFocus)
	{
		m_CurrentSelectedFaction = currentFocus as VendorInfoFactionReputationItemConsoleView;
		m_CanConfirm.Value = m_CurrentSelectedFaction != null && m_CurrentSelectedFaction.HasVendors;
	}

	public void HandleRemoveFocus()
	{
		m_CulledFocus = m_NavigationBehaviour.DeepestNestedFocus;
		m_NavigationBehaviour.UnFocusCurrentEntity();
	}

	public void HandleRestoreFocus()
	{
		if (m_CulledFocus != null)
		{
			m_NavigationBehaviour.FocusOnEntityManual(m_CulledFocus);
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
		}
		m_CulledFocus = null;
	}
}
