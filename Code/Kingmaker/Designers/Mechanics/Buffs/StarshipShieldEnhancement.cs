using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Buffs;

[Obsolete]
[ComponentName("Add stat bonus from ability value")]
[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[TypeId("db89aba32de193149950d16cc9d588f8")]
public class StarshipShieldEnhancement : UnitBuffComponentDelegate
{
	public bool applyToAll;

	[HideIf("applyToAll")]
	public StarshipSectorShieldsType shieldType;

	public int bonusFlat;

	public int bonusPct;

	public bool isReinforcement;

	public Buff OwnerBuff => base.Buff;

	public bool ValidFor(StarshipSectorShieldsType type)
	{
		if (!applyToAll)
		{
			return type == shieldType;
		}
		return true;
	}
}
