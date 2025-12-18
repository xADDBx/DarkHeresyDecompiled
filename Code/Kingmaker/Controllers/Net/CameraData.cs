using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Controllers.Net;

[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
public class CameraData : IHashable, IOwlPackable, IOwlPackable<CameraData>
{
	[JsonProperty(PropertyName = "p")]
	[OwlPackInclude]
	public Vector3 position;

	[JsonProperty(PropertyName = "q")]
	[OwlPackInclude]
	public Quaternion rotation;

	[JsonProperty(PropertyName = "pr")]
	[OwlPackInclude]
	public Vector4 projParams;

	[JsonProperty(PropertyName = "pp")]
	[OwlPackInclude]
	public Vector3 parentPosition;

	[JsonProperty(PropertyName = "sr")]
	[OwlPackInclude]
	public bool isScrollingByRoutine;

	public Matrix4x4 matrix;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CameraData",
		OldNames = null,
		Fields = new FieldInfo[5]
		{
			new FieldInfo("position", typeof(Vector3)),
			new FieldInfo("rotation", typeof(Quaternion)),
			new FieldInfo("projParams", typeof(Vector4)),
			new FieldInfo("parentPosition", typeof(Vector3)),
			new FieldInfo("isScrollingByRoutine", typeof(bool))
		}
	};

	public bool IsEquals(CameraData other)
	{
		if (position.Equals(other.position) && rotation.Equals(other.rotation) && projParams.Equals(other.projParams) && isScrollingByRoutine.Equals(other.isScrollingByRoutine))
		{
			return parentPosition.Equals(other.parentPosition);
		}
		return false;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref position);
		result.Append(ref rotation);
		result.Append(ref projParams);
		result.Append(ref parentPosition);
		result.Append(ref isScrollingByRoutine);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CameraData source = new CameraData();
		result = Unsafe.As<CameraData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CameraData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "position", ref position, state);
		formatter.Field(1, "rotation", ref rotation, state);
		formatter.Field(2, "projParams", ref projParams, state);
		formatter.Field(3, "parentPosition", ref parentPosition, state);
		formatter.UnmanagedField(4, "isScrollingByRoutine", ref isScrollingByRoutine, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CameraData>();
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
				position = formatter.ReadPackable<Vector3>(state);
				break;
			case 1:
				rotation = formatter.ReadPackable<Quaternion>(state);
				break;
			case 2:
				projParams = formatter.ReadPackable<Vector4>(state);
				break;
			case 3:
				parentPosition = formatter.ReadPackable<Vector3>(state);
				break;
			case 4:
				isScrollingByRoutine = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
