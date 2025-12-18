using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Interaction;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[OwlPackable(OwlPackableMode.Generate)]
public class UnlockRestrictionPart : InteractionRestrictionPart<UnlockRestrictionSettings>, IUnlockableFlagReference, IInteractionVariantActor, IInteractionRestriction, IHashable, IOwlPackable<UnlockRestrictionPart>
{
	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnlockRestrictionPart",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("IsDisabled", typeof(bool))
		}
	};

	public BlueprintAdditionalCombatObjective CombatObjective => null;

	bool IInteractionVariantActor.ShowInteractFx => false;

	public int? RequiredItemsCount => null;

	public BlueprintItem RequiredItem => null;

	public StatType Skill => StatType.Unknown;

	public int? InteractionDC => null;

	public InteractionActorType Type => InteractionActorType.Unlock;

	public UIInteractionType UIType => UtilitySkillcheck.GetUITypeFromActor(Type);

	public AbstractInteractionPart InteractionPart => base.ConcreteOwner.GetAll<AbstractInteractionPart>().FirstOrDefault();

	public bool CheckOnlyOnce => false;

	public bool CanUse => true;

	public bool AlreadyUsed => false;

	public override int GetUserPriority(BaseUnitEntity user)
	{
		if (!base.Settings.Flag.IsUnlocked)
		{
			return -1;
		}
		return 0;
	}

	public override bool CheckRestriction(BaseUnitEntity user)
	{
		return base.Settings.Flag.IsUnlocked;
	}

	public UnlockableFlagReferenceType GetUsagesFor(BlueprintUnlockableFlag flag)
	{
		if (flag != base.Settings.Flag)
		{
			return UnlockableFlagReferenceType.None;
		}
		return UnlockableFlagReferenceType.Check;
	}

	public string GetInteractionName()
	{
		return null;
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

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnlockRestrictionPart source = new UnlockRestrictionPart();
		result = Unsafe.As<UnlockRestrictionPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnlockRestrictionPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		formatter.UnmanagedField(1, "IsDisabled", ref IsDisabled, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnlockRestrictionPart>();
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
