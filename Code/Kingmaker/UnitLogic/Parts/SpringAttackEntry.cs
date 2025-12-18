using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class SpringAttackEntry : IHashable, IOwlPackable, IOwlPackable<SpringAttackEntry>
{
	[JsonProperty]
	[OwlPackInclude]
	public Vector3 OldPosition;

	[JsonProperty]
	[OwlPackInclude]
	public Vector3 NewPosition;

	[JsonProperty]
	[OwlPackInclude]
	public int Index;

	[JsonProperty]
	[OwlPackInclude]
	public EntityRef<AreaEffectEntity> AreaMark;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SpringAttackEntry",
		OldNames = null,
		Fields = new FieldInfo[4]
		{
			new FieldInfo("OldPosition", typeof(Vector3)),
			new FieldInfo("NewPosition", typeof(Vector3)),
			new FieldInfo("Index", typeof(int)),
			new FieldInfo("AreaMark", typeof(EntityRef<AreaEffectEntity>))
		}
	};

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref OldPosition);
		result.Append(ref NewPosition);
		result.Append(ref Index);
		EntityRef<AreaEffectEntity> obj = AreaMark;
		Hash128 val = StructHasher<EntityRef<AreaEffectEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SpringAttackEntry source = new SpringAttackEntry();
		result = Unsafe.As<SpringAttackEntry, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SpringAttackEntry>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "OldPosition", ref OldPosition, state);
		formatter.Field(1, "NewPosition", ref NewPosition, state);
		formatter.UnmanagedField(2, "Index", ref Index, state);
		formatter.Field(3, "AreaMark", ref AreaMark, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SpringAttackEntry>();
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
				OldPosition = formatter.ReadPackable<Vector3>(state);
				break;
			case 1:
				NewPosition = formatter.ReadPackable<Vector3>(state);
				break;
			case 2:
				Index = formatter.ReadUnmanaged<int>(state);
				break;
			case 3:
				AreaMark = formatter.ReadPackable<EntityRef<AreaEffectEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
