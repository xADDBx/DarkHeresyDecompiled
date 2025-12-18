using Kingmaker.EntitySystem.Entities;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class SkillUseWithoutItemRestrictionPart<T> : SkillUseRestrictionPart<T>, IHashable, IOwlPackable<SkillUseWithoutItemRestrictionPart<T>> where T : SkillUseRestrictionSettings, new()
{
	protected override bool ShouldRestrictAfterFail(BaseUnitEntity user)
	{
		return true;
	}

	public override void OnFailedInteract(BaseUnitEntity user)
	{
		base.OnFailedInteract(user);
		InteractionHelper.MarkUnitAsInteracted(user, base.InteractionPart);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
