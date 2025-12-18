using Kingmaker.EntitySystem.Entities;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoBigPortraitVM : CharInfoComponentVM
{
	public Sprite UnitPortraitFull => Unit.CurrentValue?.UISettings.Portrait.FullLengthPortrait;

	public string UnitName => Unit.CurrentValue?.CharacterName;

	public CharInfoHitPointsVM HitPoints { get; }

	public CharInfoExperienceVM Experience { get; }

	public CharInfoBigPortraitVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		HitPoints = new CharInfoHitPointsVM(unit).AddTo(this);
		Experience = new CharInfoExperienceVM(unit).AddTo(this);
	}
}
