using System;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class StatCheckLootUnitCardVM : ViewModel
{
	private readonly ReactiveProperty<string> m_UnitName = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<Sprite> m_UnitPortrait = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<string> m_StatName = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<int> m_StatValue = new ReactiveProperty<int>(0);

	private readonly BaseUnitEntity m_UnitEntity;

	private readonly StatType m_StatType;

	private readonly Action<BaseUnitEntity, StatType> m_CheckStatAction;

	private readonly Action<BaseUnitEntity, StatType> m_SwitchUnitAction;

	public ReadOnlyReactiveProperty<string> UnitName => m_UnitName;

	public ReadOnlyReactiveProperty<Sprite> UnitPortrait => m_UnitPortrait;

	public ReadOnlyReactiveProperty<string> StatName => m_StatName;

	public ReadOnlyReactiveProperty<int> StatValue => m_StatValue;

	public bool IsSelected { get; private set; }

	public StatCheckLootUnitCardVM(BaseUnitEntity unitEntity, StatType stat, Action<BaseUnitEntity, StatType> checkStatAction, Action<BaseUnitEntity, StatType> switchUnitAction)
	{
		m_UnitEntity = unitEntity;
		m_StatType = stat;
		m_CheckStatAction = checkStatAction;
		m_SwitchUnitAction = switchUnitAction;
		m_UnitName.Value = unitEntity.Name;
		m_UnitPortrait.Value = unitEntity.Portrait.SmallPortrait;
		m_StatName.Value = UIUtilityText.GetStatShortName(stat);
		m_StatValue.Value = unitEntity.GetStatBaseValue(stat).Value;
	}

	public void SetSelected(bool value)
	{
		IsSelected = value;
	}

	public void CheckStat()
	{
		m_CheckStatAction?.Invoke(m_UnitEntity, m_StatType);
	}

	public void SwitchUnit()
	{
		m_SwitchUnitAction?.Invoke(m_UnitEntity, m_StatType);
	}
}
