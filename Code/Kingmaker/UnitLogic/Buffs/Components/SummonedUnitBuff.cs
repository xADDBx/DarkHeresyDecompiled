using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete]
[ComponentName("Buffs/Special/Summoned unit")]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("38da9c69d457e7f4f88a857ad14d8cb0")]
public class SummonedUnitBuff : UnitBuffComponentDelegate
{
	protected override void OnRemoved()
	{
		base.OnRemoved();
		if (!base.Owner.Destroyed)
		{
			GameObject gameObject = base.Owner.Blueprint.VisualSettings.BloodPuddleFx.Load();
			DismemberUnitFX dismemberUnitFX = ((gameObject != null) ? gameObject.GetComponent<DismemberUnitFX>() : null);
			if (!dismemberUnitFX)
			{
				5f.Seconds();
			}
			else
			{
				dismemberUnitFX.Delay.Seconds();
			}
			if ((bool)gameObject && (bool)dismemberUnitFX)
			{
				FxHelper.SpawnFxOnEntity(gameObject, base.Owner.View);
				return;
			}
			PFLog.Default.Error(base.Owner.Blueprint, "Missing DismemberUnitFX in OnDeathEffect: {0}", base.Owner);
		}
	}
}
