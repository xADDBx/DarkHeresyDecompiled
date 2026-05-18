using System.Collections.Generic;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoWeaponsBlockPCView : CharInfoComponentView<CharInfoWeaponsBlockVM>
{
	[SerializeField]
	private WidgetList m_WidgetListFirstWeaponSet;

	[SerializeField]
	private WidgetList m_WidgetListSecondWeaponSet;

	[SerializeField]
	private CharInfoWeaponSetPCView m_WeaponSetViewPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		DrawEntities();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_WidgetListFirstWeaponSet.Clear();
		m_WidgetListSecondWeaponSet.Clear();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		DrawEntities();
	}

	private void DrawEntities()
	{
		m_WidgetListFirstWeaponSet.Clear();
		m_WidgetListSecondWeaponSet.Clear();
		for (int i = 0; i < Mathf.Min(base.ViewModel.WeaponSets.Count, 2); i++)
		{
			WidgetList obj = ((i == 0) ? m_WidgetListFirstWeaponSet : m_WidgetListSecondWeaponSet);
			IEnumerable<CharInfoWeaponSetViewData> datas = BuildEntries(base.ViewModel.WeaponSets[i]);
			obj.DrawEntries(datas, m_WeaponSetViewPrefab);
		}
	}

	private static IEnumerable<CharInfoWeaponSetViewData> BuildEntries(CharInfoWeaponSetVM vm)
	{
		List<CharInfoWeaponSetViewData> list = new List<CharInfoWeaponSetViewData>
		{
			new CharInfoWeaponSetViewData(vm, isPrimary: true)
		};
		if (vm == null || vm.Secondary?.CanBeFakeItem.CurrentValue != true || vm == null || vm.Primary?.ItemWeapon?.Blueprint.IsTwoHanded != true)
		{
			list.Add(new CharInfoWeaponSetViewData(vm, isPrimary: false));
		}
		return list;
	}
}
