using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class CloseScreenCommand : GameCommandWithSynchronized, IOwlPackable<CloseScreenCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly byte m_Screen;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CloseScreenCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Screen", typeof(byte))
		}
	};

	private CloseScreenCommand(byte m_screen)
	{
		m_Screen = m_screen;
	}

	private CloseScreenCommand(OwlPackConstructorParameter _)
	{
	}

	public CloseScreenCommand(IScreenUIHandler.ScreenType screen, bool isSynchronized)
		: this((byte)screen)
	{
		m_IsSynchronized = isSynchronized;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(IScreenUIHandler h)
		{
			h.CloseScreen((IScreenUIHandler.ScreenType)m_Screen);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CloseScreenCommand source = new CloseScreenCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CloseScreenCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CloseScreenCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		byte value = m_Screen;
		formatter.UnmanagedField(0, "m_Screen", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CloseScreenCommand>();
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
				Unsafe.AsRef(in m_Screen) = formatter.ReadUnmanaged<byte>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
