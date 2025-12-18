using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[HashRoot]
[OwlPackable(OwlPackableMode.Generate)]
public readonly struct BlueprintComponentReference : IEquatable<BlueprintComponentReference>, IComparable<BlueprintComponentReference>, IHashable, IOwlPackable, IOwlPackable<BlueprintComponentReference>
{
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	private readonly BlueprintScriptableObject Blueprint;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	private readonly string ComponentName;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "BlueprintComponentReference",
		Fields = new FieldInfo[2]
		{
			new FieldInfo("Blueprint", typeof(BlueprintScriptableObject)),
			new FieldInfo("ComponentName", typeof(string))
		}
	};

	public BlueprintComponentReference(BlueprintScriptableObject blueprint, string componentName)
	{
		Blueprint = blueprint;
		ComponentName = componentName;
	}

	public BlueprintComponentReference([CanBeNull] BlueprintComponent component)
	{
		Blueprint = component?.OwnerBlueprint;
		ComponentName = component?.name;
	}

	[CanBeNull]
	public BlueprintComponent Get()
	{
		if (Blueprint == null || string.IsNullOrEmpty(ComponentName))
		{
			return null;
		}
		BlueprintComponent[] componentsArray = Blueprint.ComponentsArray;
		foreach (BlueprintComponent blueprintComponent in componentsArray)
		{
			if (blueprintComponent.name == ComponentName)
			{
				return blueprintComponent;
			}
		}
		return null;
	}

	public bool Equals(BlueprintComponentReference other)
	{
		if (Blueprint == other.Blueprint)
		{
			return string.Equals(ComponentName, other.ComponentName, StringComparison.Ordinal);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is BlueprintComponentReference other)
		{
			return Equals(other);
		}
		return false;
	}

	public static bool operator ==(BlueprintComponentReference lhs, BlueprintComponentReference rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(BlueprintComponentReference lhs, BlueprintComponentReference rhs)
	{
		return !lhs.Equals(rhs);
	}

	public int CompareTo(BlueprintComponentReference other)
	{
		throw new NotImplementedException();
	}

	public static implicit operator BlueprintComponent(BlueprintComponentReference reference)
	{
		return reference.Get();
	}

	public static implicit operator BlueprintComponentReference(BlueprintComponent component)
	{
		return new BlueprintComponentReference(component);
	}

	public override string ToString()
	{
		if (!(this != null))
		{
			return "<null>";
		}
		return $"{Blueprint}[{ComponentName}]";
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Blueprint, ComponentName);
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val);
		result.Append(ComponentName);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		BlueprintComponentReference source = default(BlueprintComponentReference);
		result = Unsafe.As<BlueprintComponentReference, TPossiblyBase>(ref source);
	}

	public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<BlueprintComponentReference>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintScriptableObject value = Blueprint;
		formatter.Field(0, "Blueprint", ref value, state);
		string value2 = ComponentName;
		formatter.StringField(1, "ComponentName", ref value2, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<BlueprintComponentReference>();
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
				Unsafe.AsRef(in Blueprint) = formatter.ReadPackable<BlueprintScriptableObject>(state);
				break;
			case 1:
				Unsafe.AsRef(in ComponentName) = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
