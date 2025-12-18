using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class CameraFollowTimeScaleGameCommand : GameCommand, IOwlPackable<CameraFollowTimeScaleGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private byte m_Scale;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_Force;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CameraFollowTimeScaleGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_Scale", typeof(byte)),
			new FieldInfo("m_Force", typeof(bool))
		}
	};

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	private CameraFollowTimeScaleGameCommand()
	{
	}

	private CameraFollowTimeScaleGameCommand(byte m_scale, bool m_force)
	{
		m_Scale = m_scale;
		m_Force = m_force;
	}

	public CameraFollowTimeScaleGameCommand(float scale, bool force)
		: this(FloatToByte(scale), force)
	{
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Controllers.TimeController.SetCameraFollowTimeScale(ByteToFloat(m_Scale), m_Force);
	}

	private static byte FloatToByte(float scale)
	{
		return (byte)(255f * Mathf.Clamp01(scale));
	}

	private static float ByteToFloat(byte scale)
	{
		return (float)(int)scale / 255f;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CameraFollowTimeScaleGameCommand source = new CameraFollowTimeScaleGameCommand();
		result = Unsafe.As<CameraFollowTimeScaleGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CameraFollowTimeScaleGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_Scale", ref m_Scale, state);
		formatter.UnmanagedField(1, "m_Force", ref m_Force, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CameraFollowTimeScaleGameCommand>();
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
				m_Scale = formatter.ReadUnmanaged<byte>(state);
				break;
			case 1:
				m_Force = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
