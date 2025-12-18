using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.Spawners;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartGuard : BaseUnitPart, IHashable, IOwlPackable<UnitPartGuard>
{
	[JsonProperty]
	[OwlPackInclude]
	public bool UseLosInsteadOfVisibility;

	public readonly HashSet<BaseUnitEntity> DetectedUnits = new HashSet<BaseUnitEntity>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartGuard",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("Range", typeof(float)),
			new FieldInfo("Source", typeof(EntityRef<UnitSpawnerBase.MyData>)),
			new FieldInfo("UseLosInsteadOfVisibility", typeof(bool))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public float Range { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public EntityRef<UnitSpawnerBase.MyData> Source { get; private set; }

	internal void Init(SpawnerGuardSettings.Part spawner)
	{
		Range = spawner.Source.Range;
		Source = new EntityRef<UnitSpawnerBase.MyData>(spawner.Owner.UniqueId);
		UseLosInsteadOfVisibility = spawner.Source.UseLosInsteadOfVisibility;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		float val2 = Range;
		result.Append(ref val2);
		EntityRef<UnitSpawnerBase.MyData> obj = Source;
		Hash128 val3 = StructHasher<EntityRef<UnitSpawnerBase.MyData>>.GetHash128(ref obj);
		result.Append(ref val3);
		result.Append(ref UseLosInsteadOfVisibility);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartGuard source = new UnitPartGuard();
		result = Unsafe.As<UnitPartGuard, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartGuard>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		float value = Range;
		formatter.UnmanagedField(0, "Range", ref value, state);
		EntityRef<UnitSpawnerBase.MyData> value2 = Source;
		formatter.Field(1, "Source", ref value2, state);
		formatter.UnmanagedField(2, "UseLosInsteadOfVisibility", ref UseLosInsteadOfVisibility, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartGuard>();
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
				Range = formatter.ReadUnmanaged<float>(state);
				break;
			case 1:
				Source = formatter.ReadPackable<EntityRef<UnitSpawnerBase.MyData>>(state);
				break;
			case 2:
				UseLosInsteadOfVisibility = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
