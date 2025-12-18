using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class InspectVM : ViewModel, IUnitClickUIHandler, ISubscriber, IShowInspectChanged
{
	protected readonly ReactiveProperty<TooltipBaseTemplate> m_Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> Tooltip => m_Tooltip;

	protected InspectVM()
	{
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		HideInspect();
	}

	public void HandleShowInspect(bool state)
	{
		if (!state)
		{
			HideInspect();
		}
	}

	protected virtual void HideInspect()
	{
		m_Tooltip.Value = null;
	}

	public void HandleUnitRightClick(AbstractUnitEntity baseUnitEntity)
	{
		if (!Game.Instance.IsControllerGamepad)
		{
			OnUnitInvoke(baseUnitEntity);
		}
	}

	public void HandleUnitConsoleInvoke(AbstractUnitEntity baseUnitEntity)
	{
		OnUnitInvoke(baseUnitEntity);
	}

	protected abstract void OnUnitInvoke(AbstractUnitEntity baseUnitEntity);
}
