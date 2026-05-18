using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenAppearancePhaseDetailedConsoleView : CharGenAppearancePhaseDetailedView
{
	[Header("Console")]
	[SerializeField]
	private HintView m_SelectHint;

	[SerializeField]
	private HintView m_SwitchNavigationHint;

	private readonly ReactiveProperty<ActivePhaseNavigation> m_ActivePhaseNavigation = new ReactiveProperty<ActivePhaseNavigation>(ActivePhaseNavigation.Content);

	private readonly ReactiveProperty<bool> m_CanConfirm = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanDecline = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanFunc01 = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanSwitchNavigation = new ReactiveProperty<bool>(value: true);

	private bool m_ShouldSelectLastContentEntity;

	public void AddInput()
	{
	}
}
