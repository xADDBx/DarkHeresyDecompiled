using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Interaction;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Mechanics.Interactions.Restrictions;

[OwlPackable(OwlPackableMode.Generate)]
public class SkillUseWithoutToolRestrictionPart : InteractionRestrictionPart<SkillUseWithoutToolRestrictionSettings>, IInteractionVariantActor, IInteractionRestriction, IHashable, IOwlPackable<SkillUseWithoutToolRestrictionPart>
{
	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SkillUseWithoutToolRestrictionPart",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("IsDisabled", typeof(bool))
		}
	};

	public override bool ShouldCheckSourceComponent => false;

	public int? InteractionDC => InteractionPart?.Settings?.GetDC();

	public BlueprintAdditionalCombatObjective CombatObjective => null;

	bool IInteractionVariantActor.ShowInteractFx => false;

	public int? RequiredItemsCount => null;

	public BlueprintItem RequiredItem => base.Settings?.Type switch
	{
		InteractionActorType.TechUse => base.Owner.ToEntity().GetOptional<RitualSetRestrictionPart>()?.Settings?.GetItem(), 
		InteractionActorType.LoreXenos => base.Owner.ToEntity().GetOptional<MultikeyRestrictionPart>()?.Settings?.GetItem(), 
		_ => null, 
	};

	public StatType Skill => InteractionPart.GetSkill();

	public bool CheckOnlyOnce => InteractionPart.Settings.OnlyCheckOnce;

	public bool CanUse => true;

	public bool AlreadyUsed => InteractionPart.AlreadyUsed;

	public InteractionActorType Type => base.Settings.Type;

	public UIInteractionType UIType => UtilitySkillcheck.GetUITypeFromActor(Type);

	AbstractInteractionPart IInteractionVariantActor.InteractionPart => InteractionPart;

	private InteractionSkillCheckPart InteractionPart => base.ConcreteOwner.GetRequired<InteractionSkillCheckPart>();

	protected override void OnAttach()
	{
		base.OnAttach();
		base.Settings.Type = InteractionPart.GetSkill().ToInteractionActorType();
	}

	public string GetInteractionName()
	{
		return LocalizedTexts.Instance.Stats.GetText(InteractionPart.GetSkill());
	}

	bool IInteractionRestriction.CheckRestriction(BaseUnitEntity user)
	{
		return CheckRestriction(user);
	}

	public override void OnDidInteract(BaseUnitEntity user)
	{
		base.OnDidInteract(user);
		if (!InteractionPart.IsFailed)
		{
			return;
		}
		SkillUseWithoutToolRestrictionSettings settings = base.Settings;
		if (settings != null && settings.Type == InteractionActorType.TechUse)
		{
			EventBus.RaiseEvent(delegate(IVariativeInteractionUIHandler h)
			{
				h.HandleInteractionRequest(InteractionPart.Owner);
			});
		}
	}

	public override int GetUserPriority(BaseUnitEntity user)
	{
		StatType skill = InteractionPart.GetSkill();
		return user.Stats.GetStatOptional(skill)?.ModifiedValue ?? 0;
	}

	public override bool CheckRestriction(BaseUnitEntity user)
	{
		if (InteractionPart.IsFailed)
		{
			return !InteractionPart.Settings.InteractOnlyWithToolAfterFail;
		}
		return true;
	}

	bool IInteractionVariantActor.TryInteract(BaseUnitEntity user)
	{
		return true;
	}

	protected override string GetDefaultBark(BaseUnitEntity baseUnitEntity, bool restricted)
	{
		if (!restricted)
		{
			return null;
		}
		BlueprintItem item = base.Settings.Type switch
		{
			InteractionActorType.TechUse => base.ConcreteOwner.GetOptional<RitualSetRestrictionPart>()?.Settings?.GetItem(), 
			InteractionActorType.LoreXenos => base.ConcreteOwner.GetOptional<MultikeyRestrictionPart>()?.Settings?.GetItem(), 
			_ => null, 
		};
		if (item != null)
		{
			return ConfigRoot.Instance.LocalizedTexts.InteractOnlyWithTool.ToString(delegate
			{
				GameLogContext.Text = item.Name;
			});
		}
		return null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SkillUseWithoutToolRestrictionPart source = new SkillUseWithoutToolRestrictionPart();
		result = Unsafe.As<SkillUseWithoutToolRestrictionPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SkillUseWithoutToolRestrictionPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		formatter.UnmanagedField(1, "IsDisabled", ref IsDisabled, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SkillUseWithoutToolRestrictionPart>();
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
				IsDisabled = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
