using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartEncumbrance : BaseUnitPart, IHashable, IOwlPackable<UnitPartEncumbrance>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartEncumbrance",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Value", typeof(Encumbrance))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public Encumbrance Value { get; private set; }

	public static int GetArmorCheckPenalty(BaseUnitEntity owner, Encumbrance encumbrance)
	{
		switch (encumbrance)
		{
		case Encumbrance.Medium:
			return -3;
		case Encumbrance.Heavy:
		case Encumbrance.Overload:
			return -6;
		default:
			return 0;
		}
	}

	public static int? GetMaxDexterityBonus(BaseUnitEntity owner, Encumbrance encumbrance)
	{
		switch (encumbrance)
		{
		case Encumbrance.Medium:
			return 3;
		case Encumbrance.Heavy:
		case Encumbrance.Overload:
			return 1;
		default:
			return null;
		}
	}

	public void Init(Encumbrance encumbrance)
	{
		if (Value != encumbrance)
		{
			Clean();
			Value = encumbrance;
		}
	}

	protected override void OnDetach()
	{
		Clean();
	}

	private void Clean()
	{
	}

	public void Set(Encumbrance value)
	{
		Value = value;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Encumbrance val2 = Value;
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartEncumbrance source = new UnitPartEncumbrance();
		result = Unsafe.As<UnitPartEncumbrance, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartEncumbrance>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Encumbrance value = Value;
		formatter.EnumField(0, "Value", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartEncumbrance>();
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
				Value = formatter.ReadEnum<Encumbrance>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
