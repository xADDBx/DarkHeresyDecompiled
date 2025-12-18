using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[OwlPackable(OwlPackableMode.Generate)]
public class AbilityResource : IHashable, IOwlPackable, IOwlPackable<AbilityResource>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AbilityResource",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("Blueprint", typeof(BlueprintScriptableObject)),
			new FieldInfo("Amount", typeof(int))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintScriptableObject Blueprint { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public int Amount { get; set; }

	public int GetMaxAmount(Entity owner)
	{
		if (Blueprint is BlueprintAbilityResource blueprintAbilityResource)
		{
			return blueprintAbilityResource.GetMaxAmount(owner);
		}
		AddAbilityResources component = Blueprint.GetComponent<AddAbilityResources>();
		if (component != null && component.UseThisAsResource)
		{
			return component.Amount;
		}
		PFLog.Default.Error("Can't extract resource amount from {0}", Blueprint);
		return 0;
	}

	[JsonConstructor]
	public AbilityResource(BlueprintScriptableObject blueprint)
	{
		Blueprint = blueprint;
	}

	public AbilityResource()
	{
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val);
		int val2 = Amount;
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AbilityResource source = new AbilityResource();
		result = Unsafe.As<AbilityResource, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<AbilityResource>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintScriptableObject value = Blueprint;
		formatter.Field(0, "Blueprint", ref value, state);
		int value2 = Amount;
		formatter.UnmanagedField(1, "Amount", ref value2, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AbilityResource>();
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
				Blueprint = formatter.ReadPackable<BlueprintScriptableObject>(state);
				break;
			case 1:
				Amount = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
