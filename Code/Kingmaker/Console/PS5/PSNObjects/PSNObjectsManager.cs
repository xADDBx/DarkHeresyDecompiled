using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Console.PS5.PSNObjects;

[JsonObject]
[OwlPackable(OwlPackableMode.Generate)]
public class PSNObjectsManager : IHashable, IOwlPackable, IOwlPackable<PSNObjectsManager>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PSNObjectsManager",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void Activate()
	{
	}

	public void StartActivity(string activityId)
	{
		SetActivityStatus(activityId, ActivityStatus.IN_PROGRESS);
	}

	public void CompleteActivity(string activityId)
	{
		SetActivityStatus(activityId, ActivityStatus.COMPLETED);
	}

	public void FailActivity(string activityId)
	{
		SetActivityStatus(activityId, ActivityStatus.FAILED);
	}

	public void AbandonActivity(string activityId)
	{
		SetActivityStatus(activityId, ActivityStatus.ABANDONED);
	}

	private void SetActivityStatus(string activityId, ActivityStatus status)
	{
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PSNObjectsManager source = new PSNObjectsManager();
		result = Unsafe.As<PSNObjectsManager, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PSNObjectsManager>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PSNObjectsManager>();
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
