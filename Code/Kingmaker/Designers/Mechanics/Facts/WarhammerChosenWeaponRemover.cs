using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("26af72a281cf5a04c99ab81014666307")]
public class WarhammerChosenWeaponRemover : UnitBuffComponentDelegate
{
	protected override void OnActivate()
	{
		WarhammerUnitPartChooseWeapon orCreate = base.Owner.GetOrCreate<WarhammerUnitPartChooseWeapon>();
		ItemEntityWeapon maybeWeapon = base.Owner.Body.PrimaryHand.MaybeWeapon;
		if (maybeWeapon != null)
		{
			orCreate.ChooseWeapon(maybeWeapon);
		}
	}

	protected override void OnDeactivate()
	{
		base.Owner.Remove<WarhammerUnitPartChooseWeapon>();
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
	}
}
