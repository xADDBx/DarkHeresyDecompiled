using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoWeaponAbilityPCView : CharInfoWeaponSetAbilityPCView
{
	[SerializeField]
	private CharInfoAbilityStatView m_AbilityStatPrefab;

	[SerializeField]
	private WidgetList m_WidgetList;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.Ability.Subscribe(delegate
		{
			m_WidgetList.DrawEntries(base.ViewModel.Stats, m_AbilityStatPrefab);
		}).AddTo(this);
	}
}
