using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Canvas;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.Interaction;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Settings;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using Kingmaker.View.Mechanics.Interactions.Restrictions;
using Newtonsoft.Json;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[OwlPackable(OwlPackableMode.Generate)]
public class InteractionSkillCheckPart : InteractionPart<InteractionSkillCheckSettings>, IAreaEnterPointReference, IHasInteractionVariantActors, ITurnBasedModeHandler, ISubscriber, IInteractionVariantActor, IInteractionRestriction, IHashable, IOwlPackable<InteractionSkillCheckPart>
{
	[JsonProperty]
	[OwlPackInclude]
	private HashSet<UnitReference> m_PunishedUsers = new HashSet<UnitReference>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "InteractionSkillCheckPart",
		OldNames = null,
		Fields = new FieldInfo[12]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("AlreadyUnlocked", typeof(bool)),
			new FieldInfo("AlreadyVisited", typeof(bool)),
			new FieldInfo("m_LastCombatRoundInteractionAttempt", typeof(int)),
			new FieldInfo("m_Enabled", typeof(bool)),
			new FieldInfo("DCOverride", typeof(int)),
			new FieldInfo("AlreadyUsed", typeof(bool)),
			new FieldInfo("ExperienceObtained", typeof(bool)),
			new FieldInfo("CheckPassed", typeof(bool)),
			new FieldInfo("SkillOverride", typeof(StatType)),
			new FieldInfo("InteractedUnits", typeof(HashSet<UnitReference>)),
			new FieldInfo("m_PunishedUsers", typeof(HashSet<UnitReference>))
		}
	};

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public int DCOverride { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool AlreadyUsed { get; private set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool ExperienceObtained { get; private set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool CheckPassed { get; private set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public StatType SkillOverride { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public HashSet<UnitReference> InteractedUnits { get; private set; } = new HashSet<UnitReference>();


	public override bool InteractThroughVariants { get; protected set; }

	public float OvertipCorrection => OvertipVerticalCorrection;

	public bool IsFailed
	{
		get
		{
			if (AlreadyUsed)
			{
				return !CheckPassed;
			}
			return false;
		}
	}

	public bool AutoPass
	{
		get
		{
			if (base.Settings.Difficulty != SkillCheckDifficulty.AutoPass)
			{
				return base.Settings.Skill == StatType.Unknown;
			}
			return true;
		}
	}

	private bool InteractOnlyByNotInteractedUnit => ConfigRoot.Instance.Interaction.GlobalInteractionSkillCheckSettings.CheckInteractOnlyByNotInteractedUnit(GetSkill());

	public int? InteractionDC
	{
		get
		{
			if (base.Settings.Difficulty == SkillCheckDifficulty.AutoPass || base.Settings.HideDC)
			{
				return null;
			}
			return (DCOverride == 0) ? base.Settings.GetDC() : DCOverride;
		}
	}

	public new InteractionActorType Type => InteractionActorType.Default;

	public UIInteractionType UIType => base.Settings.UIType;

	public AbstractInteractionPart InteractionPart => this;

	public BlueprintAdditionalCombatObjective CombatObjective => base.Settings.AdditionalCombatObjective;

	public bool ShowInteractFx => false;

	public int? RequiredItemsCount => RequiresMeltaCharge ? 1 : 0;

	public BlueprintItem RequiredItem
	{
		get
		{
			if (!RequiresMeltaCharge)
			{
				return null;
			}
			return ConfigRoot.Instance.Consumables.MeltaChargeItem;
		}
	}

	private bool RequiresMeltaCharge
	{
		get
		{
			if (base.Settings.Skill == StatType.SkillDemolition)
			{
				return base.Settings.NeedSupply;
			}
			return false;
		}
	}

	public StatType Skill => GetSkill();

	public bool CheckOnlyOnce => base.Settings.OnlyCheckOnce;

	public bool CanUse => Enabled;

	bool IInteractionVariantActor.AlreadyUsed => AlreadyUsed;

	protected override void OnSettingsDidSet(bool isNewSettings)
	{
		base.OnSettingsDidSet(isNewSettings);
		if (isNewSettings)
		{
			DCOverride = base.Settings.GetDC();
		}
	}

	public override bool CanInteract()
	{
		if (AlreadyUsed)
		{
			return base.CanInteract();
		}
		ConditionsReference condition = base.Settings.Condition;
		if (condition != null && condition.Get()?.Conditions.HasConditions == true)
		{
			using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
			{
				if (!condition.Get().Conditions.Check())
				{
					return false;
				}
			}
		}
		return base.CanInteract();
	}

	internal StatType GetSkill()
	{
		if (!SkillOverride.IsSkill())
		{
			return base.Settings.Skill;
		}
		return SkillOverride;
	}

	public string GetInteractionName()
	{
		return base.Settings.DisplayName?.Text ?? LocalizedTexts.Instance.Stats.GetText(GetSkill());
	}

	public bool CheckRestriction(BaseUnitEntity user)
	{
		return CanInteract();
	}

	public void ShowSuccessBark(BaseUnitEntity user)
	{
		ShowBark(user, base.Settings.ShortDescriptionPassed, user);
	}

	public void ShowRestrictionBark(BaseUnitEntity user)
	{
	}

	void IInteractionVariantActor.OnDidInteract(BaseUnitEntity user)
	{
		OnDidInteract(user);
		SetVisited();
	}

	public void OnFailedInteract(BaseUnitEntity user)
	{
	}

	bool IInteractionVariantActor.TryInteract(BaseUnitEntity user)
	{
		return true;
	}

	protected override void OnDidInteract(BaseUnitEntity user)
	{
		base.OnDidInteract(user);
		InteractedUnits.Add(user.FromBaseUnitEntity());
		if (CheckPassed)
		{
			SetUnlocked();
		}
		if (base.Settings.OnlyCheckOnce && AlreadyUsed)
		{
			foreach (InteractionRestrictionPart needItemRestriction in GetNeedItemRestrictions())
			{
				if (needItemRestriction != null)
				{
					needItemRestriction.IsDisabled = true;
				}
			}
		}
		if (IsFailed && InteractThroughVariants && GetSkill() == StatType.SkillTechUse)
		{
			base.EventBus.RaiseEvent(delegate(IVariativeInteractionUIHandler h)
			{
				h.HandleInteractionRequest(base.Owner);
			});
		}
		IEnumerable<InteractionRestrictionPart> GetNeedItemRestrictions()
		{
			yield return base.View.Data.Parts.GetOptional<MeltaChargeRestrictionPart>();
			yield return base.View.Data.Parts.GetOptional<RitualSetRestrictionPart>();
			yield return base.View.Data.Parts.GetOptional<MultikeyRestrictionPart>();
		}
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		bool isCriticalFail = false;
		bool flag = false;
		StatType skill = GetSkill();
		if (!base.Settings.OnlyCheckOnce || !AlreadyUsed || (base.Settings.CheckConditionsOnEveryInteraction && !base.Settings.OnlyCheckOnce))
		{
			int num = ((DCOverride == 0) ? base.Settings.GetDC() : DCOverride);
			int num2 = SettingsRoot.Difficulty.SkillCheckModifier;
			if (AutoPass || num + num2 >= 100)
			{
				CheckPassed = true;
			}
			else
			{
				foreach (BaseUnitEntity rollUnit in GetRollUnits(user, skill))
				{
					CheckPassed = skill != 0 && GameHelper.CheckSkillResult(rollUnit, skill, num, out isCriticalFail, RulePerformSkillCheck.VoicingType.All, GetEnsureSuccess());
					if (CheckPassed)
					{
						if (!ExperienceObtained && skill != 0)
						{
							Experience.GainForSkillCheck(base.Settings.Difficulty, num, rollUnit);
							ExperienceObtained = true;
						}
						break;
					}
				}
			}
			AlreadyUsed = true;
			flag = true;
		}
		if (flag || base.Settings.TriggerActionsEveryClick)
		{
			base.View.FactHolder.Or(null)?.GetFact()?.CallComponents(delegate(ISkillCheckInteractionTrigger t)
			{
				t.OnInteract(user, this, CheckPassed);
			});
			ActionsHolder actionsHolder = ((!CheckPassed) ? base.Settings.CheckFailedActions?.Get() : base.Settings.CheckPassedActions?.Get());
			if (actionsHolder != null)
			{
				ActionList actions = actionsHolder.Actions;
				if (actions != null && actions.HasActions)
				{
					using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
					{
						using (ContextData<InteractingUnitData>.Request().Setup(user))
						{
							actionsHolder.Actions.Run();
						}
					}
				}
			}
		}
		BlueprintAreaEnterPoint blueprintAreaEnterPoint = (CheckPassed ? base.Settings.TeleportOnSuccess : base.Settings.TeleportOnFail);
		if ((bool)blueprintAreaEnterPoint)
		{
			Game.Instance.Teleport(blueprintAreaEnterPoint, includeFollowers: true);
		}
		LocalizedString localizedString = (CheckPassed ? base.Settings.CheckPassedBark : base.Settings.CheckFailBark);
		if (localizedString != null && !localizedString.Empty)
		{
			ShowBark(base.Settings.ShowOnUser ? ((MechanicEntity)user) : ((MechanicEntity)base.Owner), localizedString, user);
		}
		else if (skill == StatType.SkillTechUse)
		{
			ShowBark(user, CheckPassed ? ConfigRoot.Instance.LocalizedTexts.AccessReceived : ConfigRoot.Instance.LocalizedTexts.AccessDenied, user);
		}
		if (!CheckPassed)
		{
			if (base.Settings.FadeOnFail)
			{
				if (base.Settings.ApplyPenaltyAfterFade)
				{
					LoadingProcess.Instance.StartLoadingProcess("InteractionSkillCheck.FadeOutCoroutine", FadeOutCoroutine(), delegate
					{
						ApplyPenalty(user, skill, isCriticalFail);
					}, LoadingProcessTag.TeleportParty);
				}
				else
				{
					FadeCanvas.Instance.Fadeout(fade: true);
					FadeCanvas.Instance.Fadeout(fade: false);
					ApplyPenalty(user, skill, isCriticalFail);
				}
			}
			else
			{
				ApplyPenalty(user, skill, isCriticalFail);
			}
		}
		else if (base.Settings.FadeOnSuccess)
		{
			FadeCanvas.Instance.Fadeout(fade: true);
			FadeCanvas.Instance.Fadeout(fade: false);
		}
		UISounds.Instance.PlayInteractionSound(UIType, base.View.GO, CheckPassed);
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		UpdateMarkerName();
	}

	private void UpdateMarkerName()
	{
		if (base.Settings.ShowAdditionalCombatObjective)
		{
			LocalMapMarkerPart orCreate = base.Owner.GetOrCreate<LocalMapMarkerPart>();
			orCreate.IsRuntimeCreated = true;
			orCreate.Settings.Type = LocalMapMarkType.AdditionalCombatObjective;
			orCreate.Settings.Description = base.Settings.DisplayName;
			orCreate.SetHidden(!TurnController.IsInTurnBasedCombat());
		}
	}

	private static void ShowBark(Entity entity, LocalizedString text, BaseUnitEntity user)
	{
		BarkPlayer.Bark(entity, text, VoiceOverType.Bark, user.VoGuid, -1f, user);
	}

	private List<BaseUnitEntity> GetRollUnits(BaseUnitEntity user, StatType skill)
	{
		if (!base.Settings.IsPartyCheck)
		{
			return new List<BaseUnitEntity> { user };
		}
		List<BaseUnitEntity> list = Game.Instance.Player.Party.Where((BaseUnitEntity x) => (int)x.Actor.GetStat(skill, null, default(StatContext), "GetRollUnits") >= 0).OrderByDescending((Func<BaseUnitEntity, int>)((BaseUnitEntity x) => x.Actor.GetStat(skill, null, default(StatContext), "GetRollUnits"))).ToList();
		if (list.Count != 0)
		{
			return list;
		}
		return new List<BaseUnitEntity> { user };
	}

	private bool? GetEnsureSuccess()
	{
		return base.Settings.FakeResult switch
		{
			InteractionSkillCheckSettings.FakeType.None => null, 
			InteractionSkillCheckSettings.FakeType.FakeSuccess => true, 
			InteractionSkillCheckSettings.FakeType.FakeFailure => false, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private IEnumerator<object> FadeOutCoroutine()
	{
		FadeCanvas.Instance.Fadeout(fade: true);
		FadeCanvas.Instance.Fadeout(fade: false);
		yield break;
	}

	private void ApplyPenalty(BaseUnitEntity user, StatType skill, bool isCriticalFail = false)
	{
		if (CheckPassed || base.Settings.PenaltyForFailedSkillCheck != InteractionSkillCheckSettings.PenaltyType.Debuff || m_PunishedUsers.Contains(user.FromBaseUnitEntity()))
		{
			return;
		}
		BpRef<BlueprintBuff> mentalCriticalEffectBuff = ConfigRoot.Instance.SkillCheckRoot.MentalCriticalEffectBuff;
		BodyPartTags bodyPartTag;
		if (skill == StatType.SkillMettle && mentalCriticalEffectBuff != null)
		{
			user.Buffs.Add(mentalCriticalEffectBuff, MechanicsContext.Claim((BlueprintBuff?)mentalCriticalEffectBuff, base.Owner));
		}
		else if (ConfigRoot.Instance.SkillCheckRoot.SkillToBodyPartCritMap.TryGetValue(skill, out bodyPartTag))
		{
			BlueprintBodyPart blueprintBodyPart = user.BodyParts.FirstOrDefault((BlueprintBodyPart i) => i.Tags.HasAnyFlag(bodyPartTag));
			if (blueprintBodyPart != null)
			{
				Rulebook.Trigger(new RulePerformCriticalEffects(base.Owner, user, blueprintBodyPart, 1)
				{
					DisableResistanceCheck = true
				});
			}
		}
		ConfigRoot.Instance.SkillCheckRoot.SetDebuffForFailedSkillCheck(user, skill, isCriticalFail);
		m_PunishedUsers.Add(user.FromBaseUnitEntity());
	}

	public override BaseUnitEntity SelectUnit(ReadonlyList<BaseUnitEntity> units, bool muteEvents = false, IInteractionVariantActor variantActor = null)
	{
		return SelectUnitInternal(units);
	}

	private BaseUnitEntity SelectUnitInternal(ReadonlyList<BaseUnitEntity> units)
	{
		BaseUnitEntity baseUnitEntity = null;
		int num = int.MinValue;
		foreach (BaseUnitEntity item in units)
		{
			if ((units.Count <= 1 || !item.IsPet) && (!InteractOnlyByNotInteractedUnit || !InteractedUnits.Contains(item.FromBaseUnitEntity())))
			{
				StatType skill = GetSkill();
				int num2 = ((skill == StatType.Unknown) ? 1 : ((int)item.Actor.GetStat(skill, null, default(StatContext), "SelectUnitInternal")));
				if (CanBeSelected(item) && (baseUnitEntity == null || num2 > num))
				{
					baseUnitEntity = item;
					num = num2;
				}
			}
		}
		return baseUnitEntity;
	}

	public bool GetUsagesFor(BlueprintAreaEnterPoint point)
	{
		if (point != base.Settings.TeleportOnSuccess)
		{
			return point == base.Settings.TeleportOnFail;
		}
		return true;
	}

	IEnumerable<IInteractionVariantActor> IHasInteractionVariantActors.GetInteractionVariantActors()
	{
		if (base.Settings.Type == InteractionType.Direct)
		{
			return null;
		}
		if (!InteractThroughVariants || (base.Settings.OnlyCheckOnce && AlreadyUsed && CheckPassed))
		{
			return null;
		}
		List<IInteractionVariantActor> list = base.View.Data.Parts.GetAll<IInteractionVariantActor>().ToList();
		list.Remove(this);
		return list;
	}

	protected override void ConfigureRestrictions()
	{
		switch (GetSkill())
		{
		case StatType.SkillDemolition:
			if (base.Settings.NeedSupply)
			{
				base.View.Data.Parts.GetOrCreate<MeltaChargeRestrictionPart>();
			}
			break;
		case StatType.SkillTechUse:
			base.View.Data.Parts.GetOrCreate<SkillUseWithoutToolRestrictionPart>();
			base.View.Data.Parts.GetOrCreate<RitualSetRestrictionPart>();
			base.Settings.InteractOnlyWithToolAfterFail = true;
			InteractThroughVariants = true;
			break;
		case StatType.SkillLoreXenos:
			if (base.Settings.NeedSupply)
			{
				base.View.Data.Parts.GetOrCreate<SkillUseWithoutToolRestrictionPart>();
				base.View.Data.Parts.GetOrCreate<MultikeyRestrictionPart>();
				base.Settings.InteractOnlyWithToolAfterFail = true;
				InteractThroughVariants = true;
			}
			break;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		int val2 = DCOverride;
		result.Append(ref val2);
		bool val3 = AlreadyUsed;
		result.Append(ref val3);
		bool val4 = ExperienceObtained;
		result.Append(ref val4);
		bool val5 = CheckPassed;
		result.Append(ref val5);
		StatType val6 = SkillOverride;
		result.Append(ref val6);
		HashSet<UnitReference> interactedUnits = InteractedUnits;
		if (interactedUnits != null)
		{
			int num = 0;
			foreach (UnitReference item in interactedUnits)
			{
				UnitReference obj = item;
				num ^= UnitReferenceHasher.GetHash128(ref obj).GetHashCode();
			}
			result.Append(num);
		}
		HashSet<UnitReference> punishedUsers = m_PunishedUsers;
		if (punishedUsers != null)
		{
			int num2 = 0;
			foreach (UnitReference item2 in punishedUsers)
			{
				UnitReference obj2 = item2;
				num2 ^= UnitReferenceHasher.GetHash128(ref obj2).GetHashCode();
			}
			result.Append(num2);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		InteractionSkillCheckPart source = new InteractionSkillCheckPart();
		result = Unsafe.As<InteractionSkillCheckPart, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<InteractionSkillCheckPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		bool value2 = base.AlreadyUnlocked;
		formatter.UnmanagedField(1, "AlreadyUnlocked", ref value2, state);
		bool value3 = AlreadyVisited;
		formatter.UnmanagedField(2, "AlreadyVisited", ref value3, state);
		formatter.UnmanagedField(3, "m_LastCombatRoundInteractionAttempt", ref m_LastCombatRoundInteractionAttempt, state);
		formatter.UnmanagedField(4, "m_Enabled", ref m_Enabled, state);
		int value4 = DCOverride;
		formatter.UnmanagedField(5, "DCOverride", ref value4, state);
		bool value5 = AlreadyUsed;
		formatter.UnmanagedField(6, "AlreadyUsed", ref value5, state);
		bool value6 = ExperienceObtained;
		formatter.UnmanagedField(7, "ExperienceObtained", ref value6, state);
		bool value7 = CheckPassed;
		formatter.UnmanagedField(8, "CheckPassed", ref value7, state);
		StatType value8 = SkillOverride;
		formatter.EnumField(9, "SkillOverride", ref value8, state);
		HashSet<UnitReference> value9 = InteractedUnits;
		formatter.Field(10, "InteractedUnits", ref value9, state);
		formatter.Field(11, "m_PunishedUsers", ref m_PunishedUsers, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<InteractionSkillCheckPart>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				base.SourceType = formatter.ReadString(state);
				break;
			case 1:
				base.AlreadyUnlocked = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				AlreadyVisited = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				m_LastCombatRoundInteractionAttempt = formatter.ReadUnmanaged<int>(state);
				break;
			case 4:
				m_Enabled = formatter.ReadUnmanaged<bool>(state);
				break;
			case 5:
				DCOverride = formatter.ReadUnmanaged<int>(state);
				break;
			case 6:
				AlreadyUsed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 7:
				ExperienceObtained = formatter.ReadUnmanaged<bool>(state);
				break;
			case 8:
				CheckPassed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				SkillOverride = formatter.ReadEnum<StatType>(state);
				break;
			case 10:
				InteractedUnits = formatter.ReadPackable<HashSet<UnitReference>>(state);
				break;
			case 11:
				m_PunishedUsers = formatter.ReadPackable<HashSet<UnitReference>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
