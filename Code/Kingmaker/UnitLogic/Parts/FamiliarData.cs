using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class FamiliarData : IHashable, IOwlPackable, IOwlPackable<FamiliarData>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<AbstractUnitEntity> m_Unit;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "FamiliarData",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_Unit", typeof(EntityRef<AbstractUnitEntity>)),
			new FieldInfo("Source", typeof(EntityFactSource))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public EntityFactSource Source { get; private set; }

	[CanBeNull]
	public AbstractUnitEntity Unit
	{
		get
		{
			return m_Unit;
		}
		private set
		{
			m_Unit = value;
		}
	}

	public FamiliarData(AbstractUnitEntity unit, EntityFactSource source)
	{
		Unit = unit;
		Source = source;
	}

	[JsonConstructor]
	private FamiliarData()
	{
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<AbstractUnitEntity> obj = m_Unit;
		Hash128 val = StructHasher<EntityRef<AbstractUnitEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<EntityFactSource>.GetHash128(Source);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		FamiliarData source = new FamiliarData();
		result = Unsafe.As<FamiliarData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<FamiliarData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Unit", ref m_Unit, state);
		EntityFactSource value = Source;
		formatter.Field(1, "Source", ref value, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<FamiliarData>();
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
				m_Unit = formatter.ReadPackable<EntityRef<AbstractUnitEntity>>(state);
				break;
			case 1:
				Source = formatter.ReadPackable<EntityFactSource>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
