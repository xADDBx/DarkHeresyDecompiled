using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.StateHasher.Hashers;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[HashRoot]
[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public readonly struct BlueprintComponentReference : IEquatable<BlueprintComponentReference>, IComparable<BlueprintComponentReference>, IMemoryPackable<BlueprintComponentReference>, IMemoryPackFormatterRegister, IHashable, IOwlPackable, IOwlPackable<BlueprintComponentReference>
{
	[Preserve]
	private sealed class BlueprintComponentReferenceFormatter : MemoryPackFormatter<BlueprintComponentReference>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref BlueprintComponentReference value)
		{
			BlueprintComponentReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref BlueprintComponentReference value)
		{
			BlueprintComponentReference.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintComponentReference value)
		{
			BlueprintComponentReference.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref BlueprintComponentReference value)
		{
			BlueprintComponentReference.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly BlueprintScriptableObject Blueprint;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	[MemoryPackInclude]
	private readonly string ComponentName;

	public static readonly TypeInfo OwlPackTypeInfo;

	[MemoryPackConstructor]
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

	static BlueprintComponentReference()
	{
		OwlPackTypeInfo = new TypeInfo
		{
			Name = "BlueprintComponentReference",
			Fields = new FieldInfo[2]
			{
				new FieldInfo("Blueprint", typeof(BlueprintScriptableObject)),
				new FieldInfo("ComponentName", typeof(string))
			}
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintComponentReference>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintComponentReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintComponentReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintComponentReference>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref BlueprintComponentReference value) where TBufferWriter : class, IBufferWriter<byte>
	{
		writer.WriteObjectHeader(2);
		writer.WriteValue(in value.Blueprint);
		writer.WriteString(value.ComponentName);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintComponentReference value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(BlueprintComponentReference);
			return;
		}
		BlueprintScriptableObject value2;
		string componentName;
		if (memberCount == 2)
		{
			value2 = reader.ReadValue<BlueprintScriptableObject>();
			componentName = reader.ReadString();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintComponentReference), 2, memberCount);
				return;
			}
			value2 = null;
			componentName = null;
			if (memberCount != 0)
			{
				reader.ReadValue(ref value2);
				if (memberCount != 1)
				{
					componentName = reader.ReadString();
					_ = 2;
				}
			}
		}
		value = new BlueprintComponentReference(value2, componentName);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref BlueprintComponentReference value)
	{
		writer.WriteObjectHeader();
		writer.WriteProperty("Blueprint");
		writer.WriteValue(value.Blueprint);
		writer.WriteProperty("ComponentName");
		writer.WriteString(value.ComponentName);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref BlueprintComponentReference value)
	{
		if (!reader.CheckObjectStart())
		{
			value = default(BlueprintComponentReference);
			reader.Advance();
			return;
		}
		reader.Advance();
		BlueprintScriptableObject blueprint = null;
		string componentName = null;
		bool[] array = new bool[2];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (!(text == "Blueprint"))
			{
				if (text == "ComponentName")
				{
					componentName = reader.ReadString();
					array[1] = true;
				}
			}
			else
			{
				blueprint = reader.ReadValue<BlueprintScriptableObject>();
				array[0] = true;
			}
		}
		value = new BlueprintComponentReference(blueprint, componentName);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
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
