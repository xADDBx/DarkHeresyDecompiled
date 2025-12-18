using System;
using Kingmaker.EntitySystem.Entities;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BookEventCharacterVM : ViewModel
{
	public readonly Sprite Portrait;

	public readonly BaseUnitEntity Unit;

	private readonly Action<BaseUnitEntity> m_OnChooseUnit;

	public BookEventCharacterVM(BaseUnitEntity unit, Action<BaseUnitEntity> onChooseUnit)
	{
		Unit = unit;
		Portrait = Unit?.Portrait.SmallPortrait;
		m_OnChooseUnit = onChooseUnit;
	}

	public void OnChooseUnit(bool value)
	{
		m_OnChooseUnit(value ? Unit : null);
	}
}
