using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Controllers.Net;

[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
public struct SynchronizedData : IHashable, IOwlPackable, IOwlPackable<SynchronizedData>
{
	public static class CameraDataType
	{
		public const byte None = 0;

		public const byte NoData = 1;

		public const byte Repeat = 2;

		public const byte RigCamera = 3;

		public const byte NonRigCamera = 4;
	}

	[JsonProperty(PropertyName = "ct")]
	[OwlPackInclude]
	public byte cameraType;

	[JsonProperty(PropertyName = "cm")]
	[OwlPackInclude]
	public CameraData camera;

	[JsonProperty(PropertyName = "ls")]
	[OwlPackInclude]
	public LeftStickData leftStick;

	[JsonProperty(PropertyName = "sh")]
	[OwlPackInclude]
	public int stateHash;

	[JsonProperty(PropertyName = "ml")]
	[OwlPackInclude]
	public byte maxLag;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SynchronizedData",
		Fields = new FieldInfo[5]
		{
			new FieldInfo("cameraType", typeof(byte)),
			new FieldInfo("camera", typeof(CameraData)),
			new FieldInfo("leftStick", typeof(LeftStickData)),
			new FieldInfo("stateHash", typeof(int)),
			new FieldInfo("maxLag", typeof(byte))
		}
	};

	public Matrix4x4 cameraMatrix => camera?.matrix ?? Matrix4x4.zero;

	public bool IsEmpty
	{
		get
		{
			if (cameraType == 0 && camera == null && leftStick == null && stateHash == 0)
			{
				return maxLag == 0;
			}
			return false;
		}
	}

	private SynchronizedData(byte cameraType, CameraData camera, LeftStickData leftStick, int stateHash, byte maxLag)
	{
		this.cameraType = cameraType;
		this.camera = camera;
		this.leftStick = leftStick;
		this.stateHash = stateHash;
		this.maxLag = maxLag;
	}

	public SynchronizedData(SynchronizedData other, byte cameraType, CameraData camera = null)
		: this(cameraType, camera, other.leftStick, other.stateHash, other.maxLag)
	{
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref cameraType);
		Hash128 val = ClassHasher<CameraData>.GetHash128(camera);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<LeftStickData>.GetHash128(leftStick);
		result.Append(ref val2);
		result.Append(ref stateHash);
		result.Append(ref maxLag);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SynchronizedData source = default(SynchronizedData);
		result = Unsafe.As<SynchronizedData, TPossiblyBase>(ref source);
	}

	public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<SynchronizedData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "cameraType", ref cameraType, state);
		formatter.Field(1, "camera", ref camera, state);
		formatter.Field(2, "leftStick", ref leftStick, state);
		formatter.UnmanagedField(3, "stateHash", ref stateHash, state);
		formatter.UnmanagedField(4, "maxLag", ref maxLag, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SynchronizedData>();
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
				cameraType = formatter.ReadUnmanaged<byte>(state);
				break;
			case 1:
				camera = formatter.ReadPackable<CameraData>(state);
				break;
			case 2:
				leftStick = formatter.ReadPackable<LeftStickData>(state);
				break;
			case 3:
				stateHash = formatter.ReadUnmanaged<int>(state);
				break;
			case 4:
				maxLag = formatter.ReadUnmanaged<byte>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
