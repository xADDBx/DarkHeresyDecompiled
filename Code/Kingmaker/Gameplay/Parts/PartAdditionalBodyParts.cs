using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Gameplay.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartAdditionalBodyParts : BaseUnitPart, IHashable, IOwlPackable<PartAdditionalBodyParts>
{
	private readonly struct Source
	{
		public readonly EntityFactRef SourceFact;

		public readonly BlueprintComponent SourceComponent;

		public Source(EntityFactRef sourceFact, BlueprintComponent sourceComponent)
		{
			SourceFact = sourceFact;
			SourceComponent = sourceComponent;
		}

		public bool Is(EntityFact fact, BlueprintComponent component)
		{
			if (SourceFact == fact)
			{
				return SourceComponent == component;
			}
			return false;
		}
	}

	private readonly Dictionary<BlueprintBodyPart, List<Source>> _entries = new Dictionary<BlueprintBodyPart, List<Source>>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartAdditionalBodyParts",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public IEnumerable<BlueprintBodyPart> List => _entries.Keys;

	public void Add(BlueprintBodyPart blueprint, EntityFact sourceFact, BlueprintComponent sourceComponent)
	{
		List<Source> list = _entries.Get(blueprint);
		if (list == null)
		{
			list = (_entries[blueprint] = new List<Source>());
		}
		else if (list.HasItem((Source i) => i.Is(sourceFact, sourceComponent)))
		{
			throw new InvalidOperationException($"Body part {blueprint} already added with source {sourceFact.Blueprint.name}.{sourceComponent.name}");
		}
		list.Add(new Source(sourceFact, sourceComponent));
	}

	public void Remove(EntityFact sourceFact, BlueprintComponent sourceComponent)
	{
		List<BlueprintBodyPart> value;
		using (CollectionPool<List<BlueprintBodyPart>, BlueprintBodyPart>.Get(out value))
		{
			foreach (var (item, list2) in _entries)
			{
				list2.RemoveAll((Source i) => i.Is(sourceFact, sourceComponent));
				if (list2.Empty())
				{
					value.Add(item);
				}
			}
			foreach (BlueprintBodyPart item2 in value)
			{
				_entries.Remove(item2);
			}
			if (_entries.Empty())
			{
				RemoveSelf();
			}
		}
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
		PartAdditionalBodyParts source = new PartAdditionalBodyParts();
		result = Unsafe.As<PartAdditionalBodyParts, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartAdditionalBodyParts>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartAdditionalBodyParts>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
