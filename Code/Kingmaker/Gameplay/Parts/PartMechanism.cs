using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.UnitLogic.Mechanics;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Parts;

[Obsolete("Use ForbidDirectHpDamageComponent, CountHpAsArmorComponent, CountAsDestructibleComponent instead.")]
[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartMechanism : MechanicEntityPart, IHashable, IOwlPackable<PartMechanism>
{
	[OwlPackInclude]
	private int _retainCount;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartMechanism",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("_retainCount", typeof(int))
		}
	};

	public void Retain()
	{
		_retainCount++;
	}

	public void Release()
	{
		if (_retainCount-- < 1)
		{
			RemoveSelf();
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
		PartMechanism source = new PartMechanism();
		result = Unsafe.As<PartMechanism, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartMechanism>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "_retainCount", ref _retainCount, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartMechanism>();
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
				_retainCount = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
