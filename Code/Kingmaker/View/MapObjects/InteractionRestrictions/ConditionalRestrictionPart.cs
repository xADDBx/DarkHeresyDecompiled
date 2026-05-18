using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[OwlPackable(OwlPackableMode.Generate)]
public class ConditionalRestrictionPart : InteractionRestrictionPart<ConditionalRestrictionSettings>, IHashable, IOwlPackable<ConditionalRestrictionPart>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ConditionalRestrictionPart",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("IsDisabled", typeof(bool))
		}
	};

	public override LocalizedString RestrictedBark => base.Settings.RestrictedBark;

	public override LocalizedString AllowedBark => base.Settings.AllowedBark;

	protected override string GetDefaultBark(BaseUnitEntity baseUnitEntity, bool restricted)
	{
		if (base.Settings.HideBark)
		{
			return null;
		}
		return base.GetDefaultBark(baseUnitEntity, restricted);
	}

	public override int GetUserPriority(BaseUnitEntity user)
	{
		return 0;
	}

	public override bool CheckRestriction(BaseUnitEntity user)
	{
		ConditionsReference condition = base.Settings.Condition;
		if (condition != null && condition.Get()?.Conditions.HasConditions == true)
		{
			using (ContextData<MechanicEntityData>.Request().Setup((MapObjectEntity)base.Owner))
			{
				using (ContextData<InteractingUnitData>.Request().Setup(user))
				{
					if (!condition.Get().Conditions.Check())
					{
						return false;
					}
				}
			}
		}
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
		ConditionalRestrictionPart source = new ConditionalRestrictionPart();
		result = Unsafe.As<ConditionalRestrictionPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ConditionalRestrictionPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		formatter.UnmanagedField(1, "IsDisabled", ref IsDisabled, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ConditionalRestrictionPart>();
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
