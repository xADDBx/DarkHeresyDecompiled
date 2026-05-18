using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Controllers.Net;

[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
[MemoryPackable(GenerateType.Object)]
public struct SynchronizedData : IMemoryPackable<SynchronizedData>, IMemoryPackFormatterRegister, IOwlPackable, IOwlPackable<SynchronizedData>
{
	public static class CameraDataType
	{
		public const byte None = 0;

		public const byte NoData = 1;

		public const byte Repeat = 2;

		public const byte RigCamera = 3;

		public const byte NonRigCamera = 4;
	}

	[Preserve]
	private sealed class SynchronizedDataFormatter : MemoryPackFormatter<SynchronizedData>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SynchronizedData value)
		{
			SynchronizedData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref SynchronizedData value)
		{
			SynchronizedData.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SynchronizedData value)
		{
			SynchronizedData.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref SynchronizedData value)
		{
			SynchronizedData.DeserializeJson(ref reader, ref value);
		}
	}

	[JsonProperty(PropertyName = "ct")]
	[OwlPackInclude]
	[MemoryPackInclude]
	public byte cameraType;

	[JsonProperty(PropertyName = "cm")]
	[OwlPackInclude]
	[MemoryPackInclude]
	public CameraData camera;

	[JsonProperty(PropertyName = "ls")]
	[OwlPackInclude]
	[MemoryPackInclude]
	public LeftStickData leftStick;

	[JsonProperty(PropertyName = "sh")]
	[OwlPackInclude]
	[MemoryPackInclude]
	public int stateHash;

	[JsonProperty(PropertyName = "ml")]
	[OwlPackInclude]
	[MemoryPackInclude]
	public byte maxLag;

	public static readonly TypeInfo OwlPackTypeInfo;

	[MemoryPackIgnore]
	public Matrix4x4 cameraMatrix => camera?.matrix ?? Matrix4x4.zero;

	[MemoryPackIgnore]
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

	[MemoryPackConstructor]
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

	static SynchronizedData()
	{
		OwlPackTypeInfo = new TypeInfo
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
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SynchronizedData>())
		{
			MemoryPackFormatterProvider.Register(new SynchronizedDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SynchronizedData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SynchronizedData>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref SynchronizedData value) where TBufferWriter : class, IBufferWriter<byte>
	{
		writer.WriteUnmanagedWithObjectHeader(5, in value.cameraType);
		writer.WritePackable(in value.camera);
		writer.WritePackable(in value.leftStick);
		writer.WriteUnmanaged(in value.stateHash, in value.maxLag);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SynchronizedData value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(SynchronizedData);
			return;
		}
		byte value2;
		CameraData value3;
		LeftStickData value4;
		int value5;
		byte value6;
		if (memberCount == 5)
		{
			reader.ReadUnmanaged<byte>(out value2);
			value3 = reader.ReadPackable<CameraData>();
			value4 = reader.ReadPackable<LeftStickData>();
			reader.ReadUnmanaged<int, byte>(out value5, out value6);
		}
		else
		{
			if (memberCount > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SynchronizedData), 5, memberCount);
				return;
			}
			value2 = 0;
			value3 = null;
			value4 = null;
			value5 = 0;
			value6 = 0;
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<byte>(out value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadPackable(ref value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<int>(out value5);
							if (memberCount != 4)
							{
								reader.ReadUnmanaged<byte>(out value6);
								_ = 5;
							}
						}
					}
				}
			}
		}
		value = new SynchronizedData(value2, value3, value4, value5, value6);
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref SynchronizedData value)
	{
		writer.WriteObjectHeader();
		writer.WriteProperty("cameraType");
		writer.WriteUnmanaged(value.cameraType);
		writer.WriteProperty("camera");
		writer.WritePackable(value.camera);
		writer.WriteProperty("leftStick");
		writer.WritePackable(value.leftStick);
		writer.WriteProperty("stateHash");
		writer.WriteUnmanaged(value.stateHash);
		writer.WriteProperty("maxLag");
		writer.WriteUnmanaged(value.maxLag);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref SynchronizedData value)
	{
		if (!reader.CheckObjectStart())
		{
			value = default(SynchronizedData);
			reader.Advance();
			return;
		}
		reader.Advance();
		byte v = 0;
		CameraData cameraData = null;
		LeftStickData leftStickData = null;
		int v2 = 0;
		byte v3 = 0;
		bool[] array = new bool[5];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			switch (text)
			{
			case "cameraType":
				reader.ReadUnmanaged<byte>(out v);
				array[0] = true;
				break;
			case "camera":
				cameraData = reader.ReadPackable<CameraData>();
				array[1] = true;
				break;
			case "leftStick":
				leftStickData = reader.ReadPackable<LeftStickData>();
				array[2] = true;
				break;
			case "stateHash":
				reader.ReadUnmanaged<int>(out v2);
				array[3] = true;
				break;
			case "maxLag":
				reader.ReadUnmanaged<byte>(out v3);
				array[4] = true;
				break;
			}
		}
		value = new SynchronizedData(v, cameraData, leftStickData, v2, v3);
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
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
