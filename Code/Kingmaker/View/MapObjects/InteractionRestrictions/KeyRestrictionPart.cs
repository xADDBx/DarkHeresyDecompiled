using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
public class KeyRestrictionPart : InteractionRestrictionPart<KeyRestrictionSettings>, IInteractionVariantActor, IInteractionRestriction, IHashable, IOwlPackable<KeyRestrictionPart>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "KeyRestrictionPart",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("IsDisabled", typeof(bool))
		}
	};

	public BlueprintAdditionalCombatObjective CombatObjective => null;

	bool IInteractionVariantActor.ShowInteractFx => false;

	public int? RequiredItemsCount => 1;

	public BlueprintItem RequiredItem => base.Settings?.Key;

	public int? InteractionDC => null;

	public InteractionActorType Type => InteractionActorType.Key;

	public UIInteractionType UIType => UtilitySkillcheck.GetUITypeFromActor(Type);

	public AbstractInteractionPart InteractionPart => base.ConcreteOwner.GetAll<AbstractInteractionPart>().FirstOrDefault();

	public StatType Skill => StatType.Unknown;

	public bool CheckOnlyOnce => false;

	public bool CanUse => true;

	public bool AlreadyUsed => false;

	protected override string GetDefaultBark(BaseUnitEntity baseUnitEntity, bool restricted)
	{
		return restricted ? ConfigRoot.Instance.LocalizedTexts.LockedwithKey : ConfigRoot.Instance.LocalizedTexts.UnlockedWithKey;
	}

	public override int GetUserPriority(BaseUnitEntity user)
	{
		if (!user.Inventory.Contains(base.Settings.Key))
		{
			return -1;
		}
		return 0;
	}

	public override bool CheckRestriction(BaseUnitEntity user)
	{
		if (!IsDisabled)
		{
			return user.Inventory.Contains(base.Settings.Key);
		}
		return true;
	}

	public override void OnDidInteract(BaseUnitEntity user)
	{
		if (!base.Settings.DontRemoveKey && !IsDisabled)
		{
			GameHelper.GetPlayerCharacter().Inventory.Remove(base.Settings.Key);
		}
	}

	public string GetInteractionName()
	{
		return base.Settings?.Key?.Name;
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

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		KeyRestrictionPart source = new KeyRestrictionPart();
		result = Unsafe.As<KeyRestrictionPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<KeyRestrictionPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		formatter.UnmanagedField(1, "IsDisabled", ref IsDisabled, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<KeyRestrictionPart>();
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
