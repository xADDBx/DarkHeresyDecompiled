using System.Collections.Generic;
using Owlcat.UI;
using R3;
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

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private List<CharInfoWeaponSetPCView> m_WeaponSets = new List<CharInfoWeaponSetPCView>();

	protected override void OnBind()
	{
		base.OnBind();
		DrawEntities();
		UpdateNavigation();
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
		UpdateNavigation();
	}

	private void DrawEntities()
	{
		foreach (CharInfoWeaponSetPCView weaponSet in m_WeaponSets)
		{
			Object.Destroy(weaponSet.gameObject);
		}
		m_WeaponSets.Clear();
		for (int i = 0; i < Mathf.Min(base.ViewModel.WeaponSets.Count, 2); i++)
		{
			Transform parent = ((i == 0) ? m_WidgetListFirstWeaponSet.transform : m_WidgetListSecondWeaponSet.transform);
			CreateAndAddWeaponSet(base.ViewModel.WeaponSets[i], parent);
		}
	}

	private void CreateAndAddWeaponSet(CharInfoWeaponSetVM data, Transform parent)
	{
		CharInfoWeaponSetPCView charInfoWeaponSetPCView = Object.Instantiate(m_WeaponSetViewPrefab, parent);
		charInfoWeaponSetPCView.Initialize(isPrimaryHand: true, data == null);
		charInfoWeaponSetPCView.Bind(data);
		m_WeaponSets.Add(charInfoWeaponSetPCView);
		if (data == null || data.Secondary?.CanBeFakeItem.CurrentValue != true || data == null || data.Primary?.ItemWeapon?.Blueprint.IsTwoHanded != true)
		{
			CharInfoWeaponSetPCView charInfoWeaponSetPCView2 = Object.Instantiate(m_WeaponSetViewPrefab, parent);
			charInfoWeaponSetPCView2.Initialize(isPrimaryHand: false, data == null);
			charInfoWeaponSetPCView2.Bind(data);
			m_WeaponSets.Add(charInfoWeaponSetPCView2);
		}
	}

	private void UpdateNavigation()
	{
		if (m_NavigationBehaviour == null)
		{
			m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		}
		m_NavigationBehaviour.Clear();
	}

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		return m_NavigationBehaviour;
	}
}
