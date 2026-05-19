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
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.EntitySystem.Interfaces.View;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
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

	public new ITrapEntityView View => (ITrapEntityView)base.View;

	public new ITrapEntityConfig Config => (ITrapEntityConfig)base.Config;

	[CanBeNull]
	public TrapObjectData LinkedTrap => Config.LinkedTrap.Entity;

	[CanBeNull]
	public TrapObjectData Device => Config.Device.Entity;

	[CanBeNull]
	public MapObjectEntity TrappedObject => Config.TrappedObject.Entity;

	[CanBeNull]
	public ScriptZoneEntity ScriptZone => Config.ScriptZone.Entity;

	public abstract int DisableTriggerMargin { get; }

	public abstract bool IsHiddenWhenInactive { get; }

	protected abstract StatType DisarmSkill { get; }

	public abstract bool CanTrigger();

	public abstract void RunTrapActions();

	public abstract bool CanUnitDisable(BaseUnitEntity unit);

	public abstract void RunDisableActions(BaseUnitEntity user);

	protected TrapObjectData(ITrapEntityConfig config)
		: base(config)
	{
	}

	[UsedImplicitly]
	protected TrapObjectData(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override void OnIsInGameChanged()
	{
		base.OnIsInGameChanged();
		if (Config != null)
		{
			ScriptZoneEntity scriptZone = ScriptZone;
			if (scriptZone != null)
			{
				scriptZone.IsInGame = base.IsInGame;
			}
		}
	}

	protected override void OnPreSave()
	{
		base.OnPreSave();
		ScriptZoneId = ScriptZone?.UniqueId;
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
			if ((units.Count <= 1 || !item.IsPet) && (baseUnitEntity == null || (int)item.Actor.GetStat(DisarmSkill, null, default(StatContext), "SelectUnit") > (int)baseUnitEntity.Actor.GetStat(DisarmSkill, null, default(StatContext), "SelectUnit")))
			{
				baseUnitEntity = item;
			}
		}
		return baseUnitEntity;
	}

	public void Interact(BaseUnitEntity user)
	{
		if (Config.IsNotScriptZoneTrigger && !base.IsAwarenessCheckPassed)
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
				base.EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IInteractionRestrictionHandler>)delegate(IInteractionRestrictionHandler h)
				{
					h.HandleCantDisarmTrap(this);
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
				View?.OnDisarmed();
				Deactivate();
				base.EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IDisarmTrapHandler>)delegate(IDisarmTrapHandler h)
				{
					h.HandleDisarmTrapSuccess(this);
				}, isCheckRuntime: true);
			}
			else if (rulePerformSkillCheck.RollResult >= rulePerformSkillCheck.EffectiveSkill + DisableTriggerMargin)
			{
				base.EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IDisarmTrapHandler>)delegate(IDisarmTrapHandler h)
				{
					h.HandleDisarmTrapCriticalFail(this);
				}, isCheckRuntime: true);
				TriggerTrap(user);
			}
			else
			{
				View?.OnDisarmFailed();
				base.EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IDisarmTrapHandler>)delegate(IDisarmTrapHandler h)
				{
					h.HandleDisarmTrapFail(this);
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
		using (ContextData<BlueprintTrap.ElementsData>.Request().Setup(triggeringUnit, this))
		{
			if (!CanTrigger())
			{
				return;
			}
			base.EventBus.RaiseEvent((IBaseUnitEntity)triggeringUnit, (Action<ITrapActivationHandler>)delegate(ITrapActivationHandler h)
			{
				h.HandleTrapActivation(this);
			}, isCheckRuntime: true);
			try
			{
				RunTrapActions();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex, "Trap " + base.Blueprint?.AssetGuid + ": failed to run actions");
			}
			finally
			{
				Deactivate();
				View?.OnTriggered();
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
		View?.OnDeactivated();
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
