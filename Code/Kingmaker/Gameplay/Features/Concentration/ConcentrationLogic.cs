using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Concentration;

[Serializable]
[AllowedOn(typeof(BlueprintBuff))]
[ComponentName("Concentration/ConcentrationLogic")]
[TypeId("49fd2e09bfcc414aa56ae9d694b3703b")]
public class ConcentrationLogic : UnitBuffComponentDelegate
{
	protected override void OnActivate()
	{
		if (!base.IsReapplying)
		{
			base.Owner.GetOrCreate<PartConcentration>().Add(base.Buff, this);
		}
	}

	protected override void OnDeactivate()
	{
		if (!base.IsReapplying)
		{
			base.Owner.GetOptional<PartConcentration>()?.Remove(base.Buff, this);
		}
	}
}
