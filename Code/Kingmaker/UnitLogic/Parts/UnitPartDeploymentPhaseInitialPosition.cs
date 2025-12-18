using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartDeploymentPhaseInitialPosition : BaseUnitPart, IHashable, IOwlPackable<UnitPartDeploymentPhaseInitialPosition>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartDeploymentPhaseInitialPosition",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("InitialPosition", typeof(Vector3))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public Vector3 InitialPosition { get; set; }

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Vector3 val2 = InitialPosition;
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartDeploymentPhaseInitialPosition source = new UnitPartDeploymentPhaseInitialPosition();
		result = Unsafe.As<UnitPartDeploymentPhaseInitialPosition, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartDeploymentPhaseInitialPosition>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Vector3 value = InitialPosition;
		formatter.Field(0, "InitialPosition", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartDeploymentPhaseInitialPosition>();
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
				InitialPosition = formatter.ReadPackable<Vector3>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
