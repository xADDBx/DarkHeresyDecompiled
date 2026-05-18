using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarPartWeaponsConsoleView : View<ActionBarPartWeaponsVM>
{
	[SerializeField]
	private ActionBarWeaponSetConsoleView m_CurrentWeaponSet;

	[SerializeField]
	private HintView m_ChangeSetSurfaceHint;

	[SerializeField]
	private HintView m_ChangeSetCombatHint;

	[SerializeField]
	private HintView m_ChangeSetQuickAccessHint;

	protected override void OnBind()
	{
		base.ViewModel.CurrentSet.Subscribe(m_CurrentWeaponSet.Bind).AddTo(this);
	}

	private void ChangeWeaponSet()
	{
		CombatSounds.Instance.ActionBar.WeaponListOpen.Play();
		base.ViewModel.ChangeWeaponSet();
	}

	public void AddInput()
	{
	}
}
