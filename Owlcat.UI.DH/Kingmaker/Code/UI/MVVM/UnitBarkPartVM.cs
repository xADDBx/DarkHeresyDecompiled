using System;
using Kingmaker.EntitySystem.Entities;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class UnitBarkPartVM : BaseBarkVM
{
	private readonly ReactiveProperty<bool> m_IsUnitOnScreen = new ReactiveProperty<bool>();

	private BaseUnitEntity m_Unit;

	private IDisposable m_UnitVisibility;

	public string name = "";

	public ReadOnlyReactiveProperty<bool> IsUnitOnScreen => m_IsUnitOnScreen;

	public UnitBarkPartVM()
	{
		name = UnityEngine.Random.Range(0, 1000).ToString();
	}

	protected override void OnDispose()
	{
		m_Unit = null;
		m_IsUnitOnScreen.Value = false;
	}

	public void SetUnitData(BaseUnitEntity unit)
	{
		HideBark();
		m_Unit = unit;
		m_UnitVisibility?.Dispose();
		m_UnitVisibility = null;
		if (unit != null)
		{
			UnitUIStateHolder.Instance.GetOrCreateUnitState(unit).IsVisibleForPlayer.Subscribe(delegate(bool val)
			{
				m_IsUnitOnScreen.Value = val;
			}).AddTo(this);
		}
	}
}
