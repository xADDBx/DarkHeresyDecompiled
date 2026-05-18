using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Interaction;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class SkillUseRestrictionPart<T> : InteractionRestrictionPart<T>, IInteractionVariantActor, IInteractionRestriction, IHashable, IOwlPackable<SkillUseRestrictionPart<T>> where T : SkillUseRestrictionSettings, new()
{
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public int DCOverride;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool Unlocked;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool Failed;

	public abstract InteractionActorType Type { get; }

	public UIInteractionType UIType => UtilitySkillcheck.GetUITypeFromActor(Type);

	public virtual StatType Skill => base.Settings.GetSkill();

	public BlueprintAdditionalCombatObjective CombatObjective => null;

	public virtual bool ShowInteractFx => false;

	public int? RequiredItemsCount
	{
		get
		{
			if (base.Settings?.GetItem() == null)
			{
				return null;
			}
			return 1;
		}
	}

	public BlueprintItem RequiredItem => base.Settings?.GetItem();

	public bool InteractOnlyByNotInteractedUnit => ConfigRoot.Instance.Interaction.GlobalSkillCheckRestrictionSettings.CheckInteractOnlyByNotInteractedUnit(base.Settings.GetSkill());

	public new MapObjectEntity Owner => (MapObjectEntity)base.Owner;

	public int? InteractionDC => base.Settings?.GetDC();

	public AbstractInteractionPart InteractionPart => Owner.GetAll<AbstractInteractionPart>().FirstOrDefault();

	public bool CheckOnlyOnce => false;

	public bool CanUse => true;

	public bool AlreadyUsed => false;

	protected virtual bool ShouldRestrictAfterFail(BaseUnitEntity user)
	{
		if (base.Settings.InteractOnlyWithToolIfFailed)
		{
			return base.Settings.GetItem() == null;
		}
		return false;
	}

	protected override string GetDefaultBark(BaseUnitEntity user, bool restricted)
	{
		if (restricted)
		{
			BlueprintItem item = base.Settings.GetItem();
			if (item != null && !user.Inventory.Contains(item))
			{
				return string.Concat(ConfigRoot.Instance.LocalizedTexts.NeedSupplyPrefix, " ", item.Name);
			}
		}
		return restricted ? ConfigRoot.Instance.LocalizedTexts.LockedContainer : ConfigRoot.Instance.LocalizedTexts.UnlockedContainer;
	}

	public override int GetUserPriority(BaseUnitEntity user)
	{
		if (!CheckRestriction(user))
		{
			return -1;
		}
		if ((int)user.Actor.GetStat(base.Settings.GetSkill(), null, default(StatContext), "GetUserPriority") <= 0 && !Game.Instance.Player.CapitalPartyMode)
		{
			return -1;
		}
		return user.Actor.GetStat(base.Settings.GetSkill(), null, default(StatContext), "GetUserPriority");
	}

	public override bool CheckRestriction(BaseUnitEntity user)
	{
		if (IsDisabled || Unlocked)
		{
			return true;
		}
		if (Failed && ShouldRestrictAfterFail(user))
		{
			return false;
		}
		BlueprintItem item = base.Settings.GetItem();
		if (item != null && !user.Inventory.Contains(item))
		{
			return false;
		}
		return true;
	}

	public override void OnDidInteract(BaseUnitEntity user)
	{
		ConsumeItemIfNeeded();
	}

	public override void OnFailedInteract(BaseUnitEntity user)
	{
		ConsumeItemIfNeeded();
		Failed = true;
	}

	private void ConsumeItemIfNeeded()
	{
		BlueprintItem item = base.Settings.GetItem();
		if (item != null && !IsDisabled)
		{
			GameHelper.GetPlayerCharacter().Inventory.Remove(item);
		}
	}

	private bool PerformSkillCheck(BaseUnitEntity user)
	{
		int difficulty = ((DCOverride == 0) ? base.Settings.GetDC() : DCOverride);
		RulePerformSkillCheck rulePerformSkillCheck = GameHelper.TriggerSkillCheck(new RulePerformSkillCheck(user, base.Settings.GetSkill(), difficulty)
		{
			Voice = RulePerformSkillCheck.VoicingType.All
		});
		if (rulePerformSkillCheck.ResultIsSuccess)
		{
			base.EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IPickLockHandler>)delegate(IPickLockHandler h)
			{
				h.HandlePickLockSuccess(Owner);
			}, isCheckRuntime: true);
		}
		else
		{
			base.EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IPickLockHandler>)delegate(IPickLockHandler h)
			{
				h.HandlePickLockFail(Owner, critical: false);
			}, isCheckRuntime: true);
		}
		return rulePerformSkillCheck.ResultIsSuccess;
	}

	protected override void OnSettingsDidSet(bool isNewSettings)
	{
		if (isNewSettings)
		{
			Unlocked = base.Settings.StartUnlocked;
			DCOverride = base.Settings.GetDC();
		}
	}

	public virtual string GetInteractionName()
	{
		return LocalizedTexts.Instance.Stats.GetText(base.Settings.GetSkill());
	}

	bool IInteractionRestriction.CheckRestriction(BaseUnitEntity user)
	{
		return CheckRestriction(user);
	}

	bool IInteractionVariantActor.TryInteract(BaseUnitEntity user)
	{
		if (!base.Settings.IsPartyCheck)
		{
			return PerformSkillCheck(user);
		}
		foreach (BaseUnitEntity item in ((IEnumerable<BaseUnitEntity>)Game.Instance.Player.Party).OrderByDescending((Func<BaseUnitEntity, int>)((BaseUnitEntity x) => x.Actor.GetStat(base.Settings.GetSkill(), null, default(StatContext), "TryInteract"))))
		{
			if ((int)item.Actor.GetStat(base.Settings.GetSkill(), null, default(StatContext), "TryInteract") <= 0)
			{
				break;
			}
			if (PerformSkillCheck(item))
			{
				return true;
			}
		}
		return false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref DCOverride);
		result.Append(ref Unlocked);
		result.Append(ref Failed);
		return result;
	}
}
