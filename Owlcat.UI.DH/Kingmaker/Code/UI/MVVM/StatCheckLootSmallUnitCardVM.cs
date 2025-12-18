using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class StatCheckLootSmallUnitCardVM : ViewModel
{
	private readonly ReactiveProperty<string> m_UnitName = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<Sprite> m_UnitPortrait = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<int> m_StatValue = new ReactiveProperty<int>(0);

	private readonly ReactiveProperty<bool> m_IsSelected = new ReactiveProperty<bool>(value: false);

	private readonly BaseUnitEntity m_UnitEntity;

	private readonly Action<BaseUnitEntity> m_UnitSelectedAction;

	public ReadOnlyReactiveProperty<string> UnitName => m_UnitName;

	public ReadOnlyReactiveProperty<Sprite> UnitPortrait => m_UnitPortrait;

	public ReadOnlyReactiveProperty<int> StatValue => m_StatValue;

	public ReadOnlyReactiveProperty<bool> IsSelected => m_IsSelected;

	public StatCheckLootSmallUnitCardVM(BaseUnitEntity unitEntity, StatType stat, Action<BaseUnitEntity> unitSelectedAction, bool isSelected)
	{
		m_UnitEntity = unitEntity;
		m_UnitSelectedAction = unitSelectedAction;
		m_UnitName.Value = unitEntity.Name;
		m_UnitPortrait.Value = unitEntity.Portrait.SmallPortrait;
		m_StatValue.Value = unitEntity.GetStatBaseValue(stat).Value;
		m_IsSelected.Value = isSelected;
	}

	public void SelectUnit()
	{
		m_UnitSelectedAction?.Invoke(m_UnitEntity);
	}
}
