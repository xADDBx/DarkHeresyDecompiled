using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarPartWeaponsConsoleView : View<ActionBarPartWeaponsVM>
{
	[SerializeField]
	private ActionBarWeaponSetConsoleView m_CurrentWeaponSet;

	[SerializeField]
	private ConsoleHint m_ChangeSetSurfaceHint;

	[SerializeField]
	private ConsoleHint m_ChangeSetCombatHint;

	[SerializeField]
	private ConsoleHint m_ChangeSetQuickAccessHint;

	protected override void OnBind()
	{
		base.ViewModel.CurrentSet.Subscribe(m_CurrentWeaponSet.Bind).AddTo(this);
	}

	private void ChangeWeaponSet(InputActionEventData data)
	{
		UISounds.Instance.Sounds.ActionBar.WeaponListOpen.Play();
		base.ViewModel.ChangeWeaponSet();
	}

	public void AddInput(InputLayer inputLayer)
	{
		ConsoleHint consoleHint;
		InputActionEventType eventType;
		switch (inputLayer.ContextName)
		{
		case "SurfaceMainInputLayer":
			consoleHint = m_ChangeSetSurfaceHint;
			eventType = InputActionEventType.ButtonJustLongPressed;
			break;
		case "SurfaceCombatInputLayer":
			consoleHint = m_ChangeSetCombatHint;
			eventType = InputActionEventType.ButtonJustLongPressed;
			break;
		case "SurfaceActionBarPartQuickAccessConsoleView":
			consoleHint = m_ChangeSetQuickAccessHint;
			eventType = InputActionEventType.ButtonJustReleased;
			break;
		default:
			consoleHint = m_ChangeSetSurfaceHint;
			eventType = InputActionEventType.ButtonJustLongPressed;
			break;
		}
		consoleHint.Bind(inputLayer.AddButton(ChangeWeaponSet, 10, base.ViewModel.CanSwitchSets, eventType)).AddTo(this);
	}

	protected override void OnUnbind()
	{
	}
}
