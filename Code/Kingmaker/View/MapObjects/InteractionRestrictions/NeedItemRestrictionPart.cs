using System;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Interaction;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class NeedItemRestrictionPart<T> : InteractionRestrictionPart<T>, IInteractionVariantActor, IInteractionRestriction, IHashable, IOwlPackable<NeedItemRestrictionPart<T>> where T : NeedItemRestrictionSettings, new()
{
	public BlueprintAdditionalCombatObjective CombatObjective => null;

	public virtual bool ShowInteractFx => false;

	public int? RequiredItemsCount => 1;

	public BlueprintItem RequiredItem => base.Settings.GetItem();

	public int? InteractionDC => (InteractionPart as InteractionSkillCheckPart)?.Settings.GetDC();

	public virtual InteractionActorType Type
	{
		get
		{
			if (base.Settings.GetItem() == ConfigRoot.Instance.Consumables.MeltaChargeItem)
			{
				return InteractionActorType.MeltaCharge;
			}
			if (base.Settings.GetItem() == ConfigRoot.Instance.Consumables.MultikeyItem)
			{
				return InteractionActorType.Key;
			}
			if (base.Settings.GetItem() == ConfigRoot.Instance.Consumables.RitualSetItem)
			{
				return InteractionActorType.Ritual;
			}
			throw new NotImplementedException();
		}
	}

	public UIInteractionType UIType => UtilitySkillcheck.GetUITypeFromActor(Type);

	public AbstractInteractionPart InteractionPart => base.ConcreteOwner.GetAll<AbstractInteractionPart>().FirstOrDefault();

	public StatType Skill => (InteractionPart as InteractionSkillCheckPart)?.Settings.Skill ?? StatType.Unknown;

	public virtual bool CheckOnlyOnce => (InteractionPart as InteractionSkillCheckPart)?.Settings.OnlyCheckOnce ?? false;

	public virtual bool CanUse => true;

	public bool AlreadyUsed => (InteractionPart as InteractionSkillCheckPart)?.AlreadyUsed ?? false;

	protected override string GetDefaultBark(BaseUnitEntity baseUnitEntity, bool restricted)
	{
		if (!restricted)
		{
			return null;
		}
		return string.Concat(ConfigRoot.Instance.LocalizedTexts.NeedSupplyPrefix, " ", base.Settings.GetItem().Name);
	}

	public override int GetUserPriority(BaseUnitEntity user)
	{
		if (!user.Inventory.Contains(base.Settings.GetItem()))
		{
			return -1;
		}
		return 0;
	}

	public override bool CheckRestriction(BaseUnitEntity user)
	{
		if (!IsDisabled)
		{
			return user.Inventory.Contains(base.Settings.GetItem());
		}
		return true;
	}

	public override void OnDidInteract(BaseUnitEntity user)
	{
		if (!IsDisabled)
		{
			GameHelper.GetPlayerCharacter().Inventory.Remove(base.Settings.GetItem());
		}
	}

	public override void OnFailedInteract(BaseUnitEntity user)
	{
		if (!IsDisabled)
		{
			GameHelper.GetPlayerCharacter().Inventory.Remove(base.Settings.GetItem());
		}
	}

	public virtual string GetInteractionName()
	{
		InteractionSkillCheckSettings interactionSkillCheckSettings = (InteractionPart as InteractionSkillCheckPart)?.Settings;
		if (interactionSkillCheckSettings != null)
		{
			return LocalizedTexts.Instance.Stats.GetText(interactionSkillCheckSettings.Skill);
		}
		return base.Settings?.GetItem()?.Name;
	}

	bool IInteractionRestriction.CheckRestriction(BaseUnitEntity user)
	{
		return CheckRestriction(user);
	}

	bool IInteractionVariantActor.TryInteract(BaseUnitEntity user)
	{
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
