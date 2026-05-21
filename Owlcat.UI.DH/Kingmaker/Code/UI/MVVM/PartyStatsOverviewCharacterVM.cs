using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class PartyStatsOverviewCharacterVM : ViewModel
{
	public readonly Sprite Portrait;

	public readonly string Name;

	public readonly int StatValue;

	public PartyStatsOverviewCharacterVM(BaseUnitEntity unit, StatType stat)
	{
		Portrait = unit.Portrait?.SmallPortrait;
		Name = unit.CharacterName;
		StatValue = unit.Actor.GetStat(stat, null, default(StatContext), ".ctor");
	}
}
