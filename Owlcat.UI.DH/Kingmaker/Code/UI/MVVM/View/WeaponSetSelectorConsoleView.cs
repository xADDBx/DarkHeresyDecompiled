using System.Collections.Generic;
using System.Linq;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class WeaponSetSelectorConsoleView : WeaponSetSelectorPCView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private Coroutine m_AddHintsCo;

	protected override void OnBind()
	{
		base.OnBind();
		CreateNavigation();
	}

	protected override void OnUnbind()
	{
		if (m_AddHintsCo != null)
		{
			StopCoroutine(m_AddHintsCo);
		}
		base.OnUnbind();
	}

	private void CreateNavigation()
	{
		if (m_NavigationBehaviour == null)
		{
			m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		}
		else
		{
			m_NavigationBehaviour.Clear();
		}
		foreach (WeaponSetBaseView weaponSetView in m_WeaponSetViews)
		{
			if (weaponSetView is WeaponSetConsoleView weaponSetConsoleView)
			{
				m_NavigationBehaviour.AddRow(weaponSetConsoleView.GetNavigationEntities());
			}
		}
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		if (m_NavigationBehaviour == null)
		{
			CreateNavigation();
		}
		return m_NavigationBehaviour?.Entities.ToList();
	}
}
