using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[OwlPackable(OwlPackableMode.Generate)]
public class RitualSetRestrictionPart : NeedItemRestrictionPart<RitualSetRestrictionSettings>, IHashable, IOwlPackable<RitualSetRestrictionPart>
{
	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "RitualSetRestrictionPart",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("IsDisabled", typeof(bool))
		}
	};

	public override bool ShouldCheckSourceComponent => false;

	public override bool CanUse
	{
		get
		{
			if (base.InteractionPart is InteractionSkillCheckPart interactionSkillCheckPart)
			{
				if (!interactionSkillCheckPart.Settings.OnlyCheckOnce)
				{
					return interactionSkillCheckPart.IsFailed;
				}
				return false;
			}
			return true;
		}
	}

	protected override string GetDefaultBark(BaseUnitEntity user, bool restricted)
	{
		if (restricted && base.Settings.GetItem() != null && !user.Inventory.Contains(base.Settings.GetItem()))
		{
			return string.Concat(ConfigRoot.Instance.LocalizedTexts.NeedSupplyPrefix, " ", base.Settings.GetItem().Name);
		}
		return restricted ? ConfigRoot.Instance.LocalizedTexts.AccessDenied : ConfigRoot.Instance.LocalizedTexts.AccessReceived;
	}

	public override bool CheckRestriction(BaseUnitEntity user)
	{
		if (base.CheckRestriction(user))
		{
			if (base.InteractionPart is InteractionSkillCheckPart interactionSkillCheckPart)
			{
				return interactionSkillCheckPart.IsFailed;
			}
			return true;
		}
		return false;
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
		RitualSetRestrictionPart source = new RitualSetRestrictionPart();
		result = Unsafe.As<RitualSetRestrictionPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<RitualSetRestrictionPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		formatter.UnmanagedField(1, "IsDisabled", ref IsDisabled, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<RitualSetRestrictionPart>();
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
