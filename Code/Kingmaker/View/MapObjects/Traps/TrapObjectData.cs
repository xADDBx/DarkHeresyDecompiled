using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.Traps;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class TrapObjectData : MapObjectEntity, IHashable, IOwlPackable<TrapObjectData>
{
	private LocalizedString canNotBeDisarmedDirectly;

	[JsonProperty]
	[OwlPackInclude]
	public bool TrapActive { get; set; } = true;


	[JsonProperty]
	[OwlPackInclude]
	public string ScriptZoneId { get; set; }

	public abstract int DisableDC { get; set; }

	public new TrapObjectView View => (TrapObjectView)base.View;

	public TrapObjectViewSettings Settings => View.Settings;

	[CanBeNull]
	public TrapObjectData LinkedTrap => View.LinkedTrap.Or(null)?.Data;

	[CanBeNull]
	public TrapObjectData Device => View.Device.Or(null)?.Data;

	public abstract int DisableTriggerMargin { get; }

	public abstract bool IsHiddenWhenInactive { get; }

	protected abstract StatType DisarmSkill { get; }

	public abstract bool CanTrigger();

	public abstract void RunTrapActions();

	public abstract bool CanUnitDisable(BaseUnitEntity unit);

	public abstract void RunDisableActions(BaseUnitEntity user);

	protected TrapObjectData(TrapObjectView trapView)
		: base(trapView)
	{
	}

	protected TrapObjectData(string uniqueId, bool isInGame, BlueprintMechanicEntityFact blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	[UsedImplicitly]
	protected TrapObjectData(JsonConstructorMark _)
		: base(_)
	{
	}

	protected TrapObjectData()
	{
	}

	protected override void OnIsInGameChanged()
	{
		base.OnIsInGameChanged();
		if (!(View == null))
		{
			if ((bool)Settings.ScriptZoneTrigger)
			{
				Settings.ScriptZoneTrigger.Data.IsInGame = base.IsInGame;
			}
			if ((bool)View.Collider)
			{
				View.Collider.gameObject.SetActive(base.IsInGame);
			}
		}
	}

	protected override void OnPreSave()
	{
		base.OnPreSave();
		ScriptZoneId = View.Settings.ScriptZoneTrigger.Or(null)?.UniqueId;
	}

	public void OnAreaBeginUnloading()
	{
	}

	[CanBeNull]
	public BaseUnitEntity SelectUnit(ReadonlyList<BaseUnitEntity> units, bool muteEvents = false)
	{
		if (!base.IsAwarenessCheckPassed)
		{
			return null;
		}
		BaseUnitEntity baseUnitEntity = null;
		foreach (BaseUnitEntity item in units)
		{
			if ((units.Count <= 1 || !item.IsPet) && (baseUnitEntity == null || (int)item.Stats.GetStat(DisarmSkill) > (int)baseUnitEntity.Stats.GetStat(DisarmSkill)))
			{
				baseUnitEntity = item;
			}
		}
		return baseUnitEntity;
	}

	public void Interact(BaseUnitEntity user)
	{
		if (View.IsNotScriptZoneTrigger && !base.IsAwarenessCheckPassed)
		{
			TriggerTrap(user);
		}
		else
		{
			if (!base.IsAwarenessCheckPassed)
			{
				return;
			}
			if (!CanUnitDisable(user))
			{
				EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IInteractionRestrictionHandler>)delegate(IInteractionRestrictionHandler h)
				{
					h.HandleCantDisarmTrap(View);
				}, isCheckRuntime: true);
				return;
			}
			if (Device != null)
			{
				canNotBeDisarmedDirectly = ConfigRoot.Instance.LocalizedTexts.TrapCanNotBeDisarmedDirectly;
				BarkPlayer.Bark(user, canNotBeDisarmedDirectly, VoiceOverType.Bark, user.VoGuid, -1f, user);
				return;
			}
			RulePerformSkillCheck rulePerformSkillCheck = GameHelper.TriggerSkillCheck(new RulePerformSkillCheck(user, DisarmSkill, DisableDC)
			{
				Voice = RulePerformSkillCheck.VoicingType.All
			}, null, allowPartyCheckInCamp: false);
			if (rulePerformSkillCheck.ResultIsSuccess)
			{
				RunDisableActions(user);
				if (Settings?.DisabledSound != "")
				{
					View.PostSoundEvent(Settings.DisabledSound);
				}
				Deactivate();
				EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IDisarmTrapHandler>)delegate(IDisarmTrapHandler h)
				{
					h.HandleDisarmTrapSuccess(View);
				}, isCheckRuntime: true);
			}
			else if (rulePerformSkillCheck.RollResult >= rulePerformSkillCheck.EffectiveSkill + DisableTriggerMargin)
			{
				EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IDisarmTrapHandler>)delegate(IDisarmTrapHandler h)
				{
					h.HandleDisarmTrapCriticalFail(View);
				}, isCheckRuntime: true);
				TriggerTrap(user);
			}
			else
			{
				View.PostSoundEvent(Settings.DisableFailSound);
				EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IDisarmTrapHandler>)delegate(IDisarmTrapHandler h)
				{
					h.HandleDisarmTrapFail(View);
				}, isCheckRuntime: true);
			}
		}
	}

	public void TryTriggerTrap(BaseUnitEntity unit)
	{
		if (TrapActive)
		{
			TriggerTrap(unit);
		}
	}

	private void TriggerTrap(BaseUnitEntity triggeringUnit)
	{
		using (ContextData<BlueprintTrap.ElementsData>.Request().Setup(triggeringUnit, View))
		{
			if (CanTrigger())
			{
				EventBus.RaiseEvent((IBaseUnitEntity)triggeringUnit, (Action<ITrapActivationHandler>)delegate(ITrapActivationHandler h)
				{
					h.HandleTrapActivation(View);
				}, isCheckRuntime: true);
				RunTrapActions();
				Deactivate();
				View.PostSoundEvent(Settings.TriggerSound);
			}
		}
	}

	private void Deactivate()
	{
		TrapActive = false;
		base.AwarenessCheck.SetPassed(value: true);
		if (IsHiddenWhenInactive)
		{
			base.IsInGame = false;
		}
		View.OnDeactivated();
		if (LinkedTrap != null && LinkedTrap.TrapActive)
		{
			LinkedTrap.Deactivate();
		}
		if (Device != null && Device.TrapActive)
		{
			Device.Deactivate();
		}
	}

	public void OnBeforeMechanicsReload()
	{
	}

	[NotNull]
	public Ability RequestAbility(BlueprintAbility ability)
	{
		Ability ability2 = Facts.Get<Ability>(ability);
		if (ability2 == null)
		{
			ability2 = ((Ability)AddFact(ability)) ?? throw new Exception($"Can't add ability {ability} to {this}");
			ability2.AddSource(this);
		}
		return ability2;
	}

	[NotNull]
	public Ability RequestWeaponAbility(BlueprintItemWeapon weaponBlueprint, WeaponAbilityType abilityType)
	{
		PartInventory orCreate = GetOrCreate<PartInventory>();
		ItemEntityWeapon itemEntityWeapon = (ItemEntityWeapon)orCreate.FirstOrDefault((ItemEntity i) => i.Blueprint == weaponBlueprint);
		if (itemEntityWeapon == null)
		{
			itemEntityWeapon = (orCreate.Add(weaponBlueprint) as ItemEntityWeapon) ?? throw new Exception($"Can't add weapon {weaponBlueprint} to trap {this}");
			itemEntityWeapon.OnDidEquipped(this);
		}
		return itemEntityWeapon.Abilities.FirstItem(delegate(Ability i)
		{
			WeaponAbility settingsFromItem = i.Data.SettingsFromItem;
			return settingsFromItem != null && settingsFromItem.Type == abilityType;
		}) ?? throw new Exception($"Can't find suitable ability ({abilityType}) in weapon ({itemEntityWeapon})");
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = TrapActive;
		result.Append(ref val2);
		result.Append(ScriptZoneId);
		return result;
	}

	public abstract override void Serialize<TFormatter>(TFormatter formatter, SerializerState state);

	public abstract override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state);
}
