using Code.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace Kingmaker.Gameplay.Features.AreaEffects;

public sealed class AreaEffectDescriptionEntry
{
	public readonly ContextActionDealDamage? Damage;

	public readonly BlueprintBuff? Buff;

	public readonly DOT? DOT;

	public readonly bool Conditional;

	public bool IsHarmful
	{
		get
		{
			if (Damage == null && !DOT.HasValue)
			{
				return Buff?.IsHardCrowdControl ?? false;
			}
			return true;
		}
	}

	private AreaEffectDescriptionEntry(bool conditional)
	{
		Conditional = conditional;
	}

	public AreaEffectDescriptionEntry(ContextActionDealDamage? damage, bool conditional)
		: this(conditional)
	{
		Damage = damage;
	}

	public AreaEffectDescriptionEntry(BlueprintBuff? buff, bool conditional)
		: this(conditional)
	{
		Buff = buff;
	}

	public AreaEffectDescriptionEntry(DOT? dot, bool conditional)
		: this(conditional)
	{
		DOT = dot;
	}
}
