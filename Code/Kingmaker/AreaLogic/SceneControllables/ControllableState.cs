using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.SceneControllables;

[Serializable]
[OwlPackable(OwlPackableMode.Generate)]
public class ControllableState : IHashable, IOwlPackable, IOwlPackable<ControllableState>
{
	[JsonProperty]
	[OwlPackInclude]
	public bool? Active;

	[JsonProperty]
	[OwlPackInclude]
	public int? State;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ControllableState",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("Active", typeof(bool?)),
			new FieldInfo("State", typeof(int?))
		}
	};

	public void MergeWith(ControllableState otherState)
	{
		Active = otherState.Active ?? Active;
		State = otherState.State ?? State;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		if (Active.HasValue)
		{
			bool val = Active.Value;
			result.Append(ref val);
		}
		if (State.HasValue)
		{
			int val2 = State.Value;
			result.Append(ref val2);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ControllableState source = new ControllableState();
		result = Unsafe.As<ControllableState, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ControllableState>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedNullableField(0, "Active", ref Active, state);
		formatter.UnmanagedNullableField(1, "State", ref State, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ControllableState>();
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
				Active = formatter.ReadNullableUnmanaged<bool>(state);
				break;
			case 1:
				State = formatter.ReadNullableUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
