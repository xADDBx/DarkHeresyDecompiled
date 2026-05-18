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
public class CameraData : IMemoryPackable<CameraData>, IMemoryPackFormatterRegister, IOwlPackable, IOwlPackable<CameraData>
{
	[Preserve]
	private sealed class CameraDataFormatter : MemoryPackFormatter<CameraData>
	{
		[Preserve]
		public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CameraData value)
		{
			CameraData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void SerializeJson(ref MemoryPackJsonWriter writer, ref CameraData value)
		{
			CameraData.SerializeJson(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CameraData value)
		{
			CameraData.Deserialize(ref reader, ref value);
		}

		[Preserve]
		public override void DeserializeJson(ref MemoryPackJsonReader reader, ref CameraData value)
		{
			CameraData.DeserializeJson(ref reader, ref value);
		}
	}

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

	[MemoryPackIgnore]
	public Matrix4x4 matrix;

	public static readonly TypeInfo OwlPackTypeInfo;

	public bool IsEquals(CameraData other)
	{
		if (position.Equals(other.position) && rotation.Equals(other.rotation) && projParams.Equals(other.projParams) && isScrollingByRoutine.Equals(other.isScrollingByRoutine))
		{
			return parentPosition.Equals(other.parentPosition);
		}
		return false;
	}

	static CameraData()
	{
		OwlPackTypeInfo = new TypeInfo
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
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CameraData>())
		{
			MemoryPackFormatterProvider.Register(new CameraDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CameraData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CameraData>());
		}
	}

	[Preserve]
	public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref CameraData? value) where TBufferWriter : class, IBufferWriter<byte>
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(5, in value.position, in value.rotation, in value.projParams, in value.parentPosition, in value.isScrollingByRoutine);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CameraData? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		Vector3 value2;
		Quaternion value3;
		Vector4 value4;
		Vector3 value5;
		bool value6;
		if (memberCount == 5)
		{
			if (value != null)
			{
				value2 = value.position;
				value3 = value.rotation;
				value4 = value.projParams;
				value5 = value.parentPosition;
				value6 = value.isScrollingByRoutine;
				reader.ReadUnmanaged<Vector3>(out value2);
				reader.ReadUnmanaged<Quaternion>(out value3);
				reader.ReadUnmanaged<Vector4>(out value4);
				reader.ReadUnmanaged<Vector3>(out value5);
				reader.ReadUnmanaged<bool>(out value6);
				goto IL_012e;
			}
			reader.ReadUnmanaged<Vector3, Quaternion, Vector4, Vector3, bool>(out value2, out value3, out value4, out value5, out value6);
		}
		else
		{
			if (memberCount > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CameraData), 5, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(Vector3);
				value3 = default(Quaternion);
				value4 = default(Vector4);
				value5 = default(Vector3);
				value6 = false;
			}
			else
			{
				value2 = value.position;
				value3 = value.rotation;
				value4 = value.projParams;
				value5 = value.parentPosition;
				value6 = value.isScrollingByRoutine;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<Vector3>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<Quaternion>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<Vector4>(out value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<Vector3>(out value5);
							if (memberCount != 4)
							{
								reader.ReadUnmanaged<bool>(out value6);
								_ = 5;
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_012e;
			}
		}
		value = new CameraData
		{
			position = value2,
			rotation = value3,
			projParams = value4,
			parentPosition = value5,
			isScrollingByRoutine = value6
		};
		return;
		IL_012e:
		value.position = value2;
		value.rotation = value3;
		value.projParams = value4;
		value.parentPosition = value5;
		value.isScrollingByRoutine = value6;
	}

	[Preserve]
	public static void SerializeJson(ref MemoryPackJsonWriter writer, ref CameraData? value)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteObjectHeader();
		writer.WriteProperty("position");
		writer.WriteUnmanaged(value.position);
		writer.WriteProperty("rotation");
		writer.WriteUnmanaged(value.rotation);
		writer.WriteProperty("projParams");
		writer.WriteUnmanaged(value.projParams);
		writer.WriteProperty("parentPosition");
		writer.WriteUnmanaged(value.parentPosition);
		writer.WriteProperty("isScrollingByRoutine");
		writer.WriteUnmanaged(value.isScrollingByRoutine);
		writer.WriteObjectFooter();
	}

	[Preserve]
	public static void DeserializeJson(ref MemoryPackJsonReader reader, ref CameraData? value)
	{
		if (!reader.CheckObjectStart())
		{
			value = null;
			reader.Advance();
			return;
		}
		reader.Advance();
		Vector3 v;
		Quaternion v2;
		Vector4 v3;
		Vector3 v4;
		bool v5;
		if (value == null)
		{
			v = default(Vector3);
			v2 = default(Quaternion);
			v3 = default(Vector4);
			v4 = default(Vector3);
			v5 = false;
		}
		else
		{
			v = value.position;
			v2 = value.rotation;
			v3 = value.projParams;
			v4 = value.parentPosition;
			v5 = value.isScrollingByRoutine;
		}
		bool[] array = new bool[5];
		string text = null;
		while ((text = reader.ReadPropertyName()) != null)
		{
			if (value == null)
			{
				switch (text)
				{
				case "position":
					reader.ReadUnmanaged<Vector3>(out v);
					array[0] = true;
					break;
				case "rotation":
					reader.ReadUnmanaged<Quaternion>(out v2);
					array[1] = true;
					break;
				case "projParams":
					reader.ReadUnmanaged<Vector4>(out v3);
					array[2] = true;
					break;
				case "parentPosition":
					reader.ReadUnmanaged<Vector3>(out v4);
					array[3] = true;
					break;
				case "isScrollingByRoutine":
					reader.ReadUnmanaged<bool>(out v5);
					array[4] = true;
					break;
				}
			}
			else
			{
				switch (text)
				{
				case "position":
					reader.ReadUnmanaged<Vector3>(out v);
					break;
				case "rotation":
					reader.ReadUnmanaged<Quaternion>(out v2);
					break;
				case "projParams":
					reader.ReadUnmanaged<Vector4>(out v3);
					break;
				case "parentPosition":
					reader.ReadUnmanaged<Vector3>(out v4);
					break;
				case "isScrollingByRoutine":
					reader.ReadUnmanaged<bool>(out v5);
					break;
				}
			}
		}
		if (value != null)
		{
			value.position = v;
			value.rotation = v2;
			value.projParams = v3;
			value.parentPosition = v4;
			value.isScrollingByRoutine = v5;
		}
		else
		{
			value = new CameraData
			{
				position = v,
				rotation = v2,
				projParams = v3,
				parentPosition = v4,
				isScrollingByRoutine = v5
			};
		}
		if (!reader.CheckObjectEnd())
		{
			throw new Exception("Expected object end");
		}
		reader.Advance();
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
