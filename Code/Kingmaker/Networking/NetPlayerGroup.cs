using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.Networking;

[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
public readonly struct NetPlayerGroup : IEquatable<NetPlayerGroup>, IOwlPackable, IOwlPackable<NetPlayerGroup>
{
	public const byte MinPlayerIndex = 1;

	public const byte MaxPlayerIndex = 6;

	private const byte AllOnes = byte.MaxValue;

	public static readonly NetPlayerGroup Empty = default(NetPlayerGroup);

	public static readonly NetPlayerGroup All = new NetPlayerGroup(byte.MaxValue);

	public static readonly NetPlayerGroup Offline = new NetPlayerGroup(NetPlayer.Offline);

	[JsonProperty(PropertyName = "v")]
	[OwlPackInclude]
	private readonly byte m_Value;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "NetPlayerGroup",
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Value", typeof(byte))
		}
	};

	public bool IsEmpty => m_Value == 0;

	private NetPlayerGroup(byte value)
	{
		m_Value = value;
	}

	public NetPlayerGroup(NetPlayer player)
	{
		m_Value = (byte)player.Mask;
	}

	public NetPlayerGroup Add(NetPlayer player)
	{
		return new NetPlayerGroup((byte)(m_Value | player.Mask));
	}

	public NetPlayerGroup Del(NetPlayer player)
	{
		return new NetPlayerGroup((byte)(m_Value & ~player.Mask));
	}

	public NetPlayerGroup Del(NetPlayerGroup playerGroup)
	{
		return new NetPlayerGroup((byte)(m_Value & ~playerGroup.m_Value));
	}

	public NetPlayerGroup Intersection(NetPlayerGroup playerGroup)
	{
		return new NetPlayerGroup((byte)(m_Value & playerGroup.m_Value));
	}

	public bool Contains(NetPlayer player)
	{
		if (!player.IsEmpty)
		{
			return (m_Value & player.Mask) == player.Mask;
		}
		return false;
	}

	public bool Contains(NetPlayerGroup playerGroup)
	{
		return (m_Value & playerGroup.m_Value) == playerGroup.m_Value;
	}

	public override string ToString()
	{
		return Convert.ToString(m_Value, 2);
	}

	public bool Equals(NetPlayerGroup other)
	{
		return m_Value == other.m_Value;
	}

	public override bool Equals(object obj)
	{
		if (obj is NetPlayerGroup other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		byte value = m_Value;
		return value.GetHashCode();
	}

	public int Count()
	{
		int num = 0;
		for (int num2 = Del(NetPlayer.Offline).m_Value; num2 != 0; num2 &= num2 - 1)
		{
			num++;
		}
		return num;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		NetPlayerGroup source = default(NetPlayerGroup);
		result = Unsafe.As<NetPlayerGroup, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<NetPlayerGroup>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		byte value = m_Value;
		formatter.UnmanagedField(0, "m_Value", ref value, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<NetPlayerGroup>();
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
				Unsafe.AsRef(in m_Value) = formatter.ReadUnmanaged<byte>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
