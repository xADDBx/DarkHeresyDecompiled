using Kingmaker.Controllers.TurnBased;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InspectPCView : View<InspectVM>, IAbilityTargetSelectionUIHandler, ISubscriber, ITurnBasedModeHandler
{
	[SerializeField]
	private InfoWindowPCView m_InfoWindow;

	private InfoWindowVM m_InfoWindowVM;

	protected override void OnBind()
	{
		EventBus.Subscribe(this).AddTo(this);
		base.ViewModel.Tooltip.Subscribe(delegate(TooltipBaseTemplate value)
		{
			m_InfoWindowVM?.Dispose();
			if (value == null)
			{
				m_InfoWindow.Hide();
			}
			else
			{
				m_InfoWindowVM = new InfoWindowVM(value, Close);
				m_InfoWindow.Bind(m_InfoWindowVM);
			}
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_InfoWindowVM?.Dispose();
		m_InfoWindowVM = null;
	}

	private void Close()
	{
		m_InfoWindow.Hide();
		Game.Instance.Player.UISettings.ShowInspect = false;
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		Close();
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		Close();
	}
}
