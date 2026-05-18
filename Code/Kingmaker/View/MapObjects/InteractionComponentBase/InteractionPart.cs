using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.DialogSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.Interaction;
using Kingmaker.Interaction;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Sound.Base;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using Kingmaker.View.MapObjects.Traps;
using Kingmaker.Visual.Animation.Kingmaker;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionComponentBase;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class InteractionPart : AbstractInteractionPart, IHashable, IOwlPackable<InteractionPart>
{
	[JsonProperty]
	[OwlPackInclude]
	protected bool m_Enabled = true;

	private List<BaseUnitEntity> m_UnitsCanInteract;

	public InteractionSettings Settings => GetSettings();

	public override List<BaseUnitEntity> UnitsCanInteract => m_UnitsCanInteract;

	public override InteractionType Type => Settings.Type;

	public override UIInteractionType UIInteractionType
	{
		get
		{
			if (Settings.UIType != 0)
			{
				return Settings.UIType;
			}
			return GetDefaultUIType();
		}
	}

	public override int ApproachRadius
	{
		get
		{
			if (Settings.ProximityRadius <= 0)
			{
				return 2;
			}
			return Settings.ProximityRadius;
		}
	}

	public override float OvertipRevealDistance
	{
		get
		{
			if (!ShowOvertip)
			{
				return 6.35f;
			}
			return Mathf.Max(ApproachRadius.Cells().Meters, 6.35f);
		}
	}

	public override bool ShowOvertip => Settings.ShowOvertip;

	public override float OvertipVerticalCorrection => Settings.OvertipVerticalCorrection;

	public override bool ShowHighlight => Settings.ShowHighlight;

	public override bool NotInCombat => Settings.NotInCombat;

	public override string InteractionStopSound => Settings.InteractionStopSound;

	public override InteractionSettings.InteractWithToolFXData InteractWithMeltaChargeFXData => Settings.InteractWithMeltaChargeFXData;

	public override TrapObjectData Trap => Settings.TrapRef;

	protected override bool UnlimitedInteractionsPerRound => Settings.UnlimitedInteractionsPerRound;

	private bool InteractThroughVariants
	{
		get
		{
			if (this is IHasInteractionVariantActors)
			{
				return ((IHasInteractionVariantActors)this).InteractThroughVariants;
			}
			return false;
		}
	}

	public override bool Enabled
	{
		get
		{
			if (!Settings.AlwaysDisabled)
			{
				return m_Enabled;
			}
			return false;
		}
		set
		{
			if (m_Enabled != value)
			{
				m_Enabled = value;
				m_Enabled = Enabled;
				base.View?.UpdateHighlight();
				OnEnabledChanged();
				base.EventBus.RaiseEvent((IMapObjectEntity)base.Owner, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
				{
					h.HandleObjectInteractChanged();
				}, isCheckRuntime: true);
			}
		}
	}

	public override int ActionPointsCost
	{
		get
		{
			if (!Settings.OverrideActionPointsCost)
			{
				return 2;
			}
			return Settings.ActionPointsCost;
		}
	}

	public override UnitAnimationInteractionType UseAnimationState
	{
		get
		{
			if (HasVisibleTrap())
			{
				return UnitAnimationInteractionType.DisarmTrap;
			}
			if (Settings.Type == InteractionType.Direct)
			{
				return UnitAnimationInteractionType.None;
			}
			return Settings.UseAnimationState;
		}
	}

	protected abstract InteractionSettings GetSettings();

	protected virtual UIInteractionType GetDefaultUIType()
	{
		return UIInteractionType.Move;
	}

	protected override void OnViewDidAttach()
	{
		m_UnitsCanInteract = GetMaxUnitsToInteract();
	}

	protected virtual void OnEnabledChanged()
	{
	}

	protected sealed override InteractionProcess InteractInternal(BaseUnitEntity user)
	{
		SetVisited();
		TrapObjectData entity = Settings.TrapRef.Entity;
		if (entity != null && entity.TrapActive)
		{
			entity.Interact(user);
			base.View?.UpdateHighlight();
			return InteractionProcess.Finished;
		}
		using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
		{
			List<InteractionRestrictionPart> restrictions = GetRestrictions();
			bool flag = ContextData<InteractionVariantData>.Current?.VariantActor != null;
			bool flag2;
			if ((flag2 = CheckRestrictions(restrictions, user, out var passedRestriction, out var failedRestriction)) && flag)
			{
				flag2 = ContextData<InteractionVariantData>.Current.VariantActor.TryInteract(user);
				base.EventBus.RaiseEvent(delegate(IInteractWithVariantActorHandler h)
				{
					h.HandleInteractWithVariantActor(this, ContextData<InteractionVariantData>.Current.VariantActor);
				});
			}
			if (flag2)
			{
				if (passedRestriction != null)
				{
					passedRestriction.ShowSuccessBark(user);
					UISounds.Instance.PlayInteractionSound(Settings.UIType, base.View.GO, isSuccess: true);
				}
				OnInteract(user);
				foreach (InteractionRestrictionPart item in restrictions)
				{
					item.OnDidInteract(user);
				}
				if (flag && ContextData<InteractionVariantData>.Current.VariantActor != this && !(ContextData<InteractionVariantData>.Current.VariantActor is InteractionRestrictionPart))
				{
					ContextData<InteractionVariantData>.Current.VariantActor.OnDidInteract(user);
				}
				if (UseAnimationState == UnitAnimationInteractionType.None && !string.IsNullOrEmpty(Settings.InteractionSound))
				{
					SoundEventsManager.PostEvent(Settings.InteractionSound, base.View?.gameObject);
				}
				base.EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IInteractionHandler>)delegate(IInteractionHandler h)
				{
					h.OnInteract(this);
				}, isCheckRuntime: true);
				OnDidInteract(user);
			}
			else
			{
				if (Settings.Dialog != null && base.View != null)
				{
					DialogData data = DialogController.SetupDialogWithMapObject(Settings.Dialog, base.Owner, null, user);
					Game.Instance.Controllers.DialogController.StartDialog(data);
				}
				failedRestriction?.ShowRestrictionBark(user);
				UISounds.Instance.PlayInteractionSound(Settings.UIType, base.View.GO, isSuccess: false);
				foreach (InteractionRestrictionPart item2 in restrictions)
				{
					item2.OnFailedInteract(user);
				}
				if (flag && ContextData<InteractionVariantData>.Current.VariantActor != this && !(ContextData<InteractionVariantData>.Current.VariantActor is InteractionRestrictionPart))
				{
					ContextData<InteractionVariantData>.Current.VariantActor.OnFailedInteract(user);
				}
				if (Settings?.InteractionDisabledSound != null && Settings.InteractionDisabledSound != "")
				{
					SoundEventsManager.PostEvent(Settings.InteractionDisabledSound, base.View?.gameObject);
				}
				base.EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IInteractionHandler>)delegate(IInteractionHandler h)
				{
					h.OnInteractionRestricted(this);
				}, isCheckRuntime: true);
			}
			base.EventBus.RaiseEvent((IMapObjectEntity)base.Owner, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
			{
				h.HandleObjectInteractChanged();
			}, isCheckRuntime: true);
			return InteractionProcess.Finished;
		}
	}

	protected abstract void OnInteract(BaseUnitEntity user);

	protected virtual void OnDidInteract(BaseUnitEntity user)
	{
		SetVisited();
		if (Settings.DisableAfterUse)
		{
			Enabled = false;
		}
	}

	private bool CheckRestrictions(List<InteractionRestrictionPart> restrictions, BaseUnitEntity user, out IInteractionRestriction passedRestriction, out IInteractionRestriction failedRestriction)
	{
		passedRestriction = null;
		failedRestriction = null;
		if (InteractThroughVariants)
		{
			passedRestriction = (failedRestriction = ContextData<InteractionVariantData>.Current?.VariantActor);
			if (passedRestriction != null)
			{
				return passedRestriction.CheckRestriction(user);
			}
			return true;
		}
		if (base.AlreadyUnlocked)
		{
			return true;
		}
		failedRestriction = restrictions.FirstOrDefault();
		if (failedRestriction == null)
		{
			return true;
		}
		passedRestriction = restrictions.FirstOrDefault((InteractionRestrictionPart r) => r.CheckRestriction(user));
		return passedRestriction != null;
	}

	public override bool HasVisibleTrap()
	{
		TrapObjectData entity = Settings.TrapRef.Entity;
		if (entity != null && entity.IsAwarenessCheckPassed)
		{
			return entity.TrapActive;
		}
		return false;
	}

	private List<BaseUnitEntity> GetMaxUnitsToInteract()
	{
		List<BaseUnitEntity> list = null;
		IEnumerable<ISkillUseRestrictionWithoutItem> enumerable = (from i in GetRestrictions()
			where i is ISkillUseRestrictionWithoutItem
			select i).OfType<ISkillUseRestrictionWithoutItem>();
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			foreach (ISkillUseRestrictionWithoutItem item2 in enumerable)
			{
				if (InteractionHelper.GetInteractionSkillCheckChance(item, item2.Skill, item2.DCOverrideValue) > 0)
				{
					if (list == null)
					{
						list = new List<BaseUnitEntity>();
					}
					list.Add(item);
				}
			}
		}
		return list;
	}

	public override void PlayStartSound(BaseUnitEntity user)
	{
		if (UseAnimationState != 0)
		{
			SoundEventsManager.PostEvent(Settings.InteractionSound, base.View?.gameObject);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_Enabled);
		return result;
	}
}
[OwlPackable(OwlPackableMode.Generate)]
public abstract class InteractionPart<TSettings> : InteractionPart, IHashable, IOwlPackable<InteractionPart<TSettings>> where TSettings : InteractionSettings, new()
{
	public new TSettings Settings { get; private set; } = new TSettings();


	public virtual bool InteractThroughVariants { get; protected set; }

	protected override InteractionSettings GetSettings()
	{
		return Settings;
	}

	public override void SetConfig(IEntityPartConfig source)
	{
		IEntityPartConfig source2 = base.Source;
		base.SetConfig(source);
		Settings = source.GetSettings() as TSettings;
		OnSettingsDidSet(source2 != source);
		ConfigureRestrictions();
	}

	public void SetSettings(TSettings settings)
	{
		if (settings != Settings)
		{
			Settings = settings;
			OnSettingsDidSet(isNewSettings: true);
			ConfigureRestrictions();
		}
	}

	protected virtual void OnSettingsDidSet(bool isNewSettings)
	{
	}

	protected virtual void ConfigureRestrictions()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
