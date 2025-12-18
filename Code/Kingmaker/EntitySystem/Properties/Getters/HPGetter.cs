using System;
using Code.Enums;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("9cb6f49c104fb044db79e499b6fcd002")]
public class HPGetter : IntPropertyGetter
{
	public DurabilityHpValueType Type;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (Type != DurabilityHpValueType.CurrentPercent)
		{
			if (Type != DurabilityHpValueType.Max)
			{
				return "Hit Points of " + FormulaTargetScope.Current;
			}
			return "Maximum Hit Points of " + FormulaTargetScope.Current;
		}
		return "Hit Points percent of " + FormulaTargetScope.Current;
	}

	protected override int GetBaseValue()
	{
		PartHealth healthOptional = base.CurrentEntity.GetHealthOptional();
		if (healthOptional == null)
		{
			return 0;
		}
		return Type switch
		{
			DurabilityHpValueType.Current => healthOptional.HitPointsLeft, 
			DurabilityHpValueType.CurrentPercent => (int)Math.Floor((float)healthOptional.HitPointsLeft * 100f / (float)healthOptional.MaxHitPoints), 
			DurabilityHpValueType.Max => healthOptional.MaxHitPoints, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
