using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Roaming;

[HashRoot]
[OwlPackable(OwlPackableMode.Generate)]
public class RoamingPoint : IRoamingPoint, IOwlPackable, IOwlPackable<IRoamingPoint>, IHashable, IOwlPackable<RoamingPoint>
{
	[JsonProperty]
	[OwlPackInclude]
	public float IdleTime;

	[CanBeNull]
	[JsonProperty]
	[OwlPackInclude]
	public BlueprintCutscene IdleCutscene;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "RoamingPoint",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("Position", typeof(Vector3)),
			new FieldInfo("IdleTime", typeof(float)),
			new FieldInfo("IdleCutscene", typeof(BlueprintCutscene))
		}
	};

	[JsonProperty(IsReference = false)]
	[OwlPackInclude]
	public Vector3 Position { get; internal set; }

	public float? Orientation => null;

	public TimeSpan SelectIdleTime(StatefulRandom random)
	{
		return IdleTime.Seconds();
	}

	public BlueprintCutscene SelectCutscene(StatefulRandom random)
	{
		return IdleCutscene;
	}

	public IRoamingPoint SelectNextPoint(StatefulRandom random)
	{
		return null;
	}

	public IRoamingPoint SelectPrevPoint(StatefulRandom random)
	{
		return null;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Vector3 val = Position;
		result.Append(ref val);
		result.Append(ref IdleTime);
		Hash128 val2 = SimpleBlueprintHasher.GetHash128(IdleCutscene);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		RoamingPoint source = new RoamingPoint();
		result = Unsafe.As<RoamingPoint, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<RoamingPoint>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Vector3 value = Position;
		formatter.Field(0, "Position", ref value, state);
		formatter.UnmanagedField(1, "IdleTime", ref IdleTime, state);
		formatter.Field(2, "IdleCutscene", ref IdleCutscene, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<RoamingPoint>();
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
				Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 1:
				IdleTime = formatter.ReadUnmanaged<float>(state);
				break;
			case 2:
				IdleCutscene = formatter.ReadPackable<BlueprintCutscene>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
