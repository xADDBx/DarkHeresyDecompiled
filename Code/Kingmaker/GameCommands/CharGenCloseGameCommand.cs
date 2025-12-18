using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class CharGenCloseGameCommand : GameCommand, IOwlPackable<CharGenCloseGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly bool m_WithComplete;

	[JsonProperty]
	[OwlPackInclude]
	private readonly bool m_SyncPortrait;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CharGenCloseGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_WithComplete", typeof(bool)),
			new FieldInfo("m_SyncPortrait", typeof(bool))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public CharGenCloseGameCommand(bool m_withComplete, bool m_syncPortrait)
	{
		m_WithComplete = m_withComplete;
		m_SyncPortrait = m_syncPortrait;
	}

	private CharGenCloseGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenCloseHandler h)
		{
			h.HandleClose(m_WithComplete, m_SyncPortrait);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenCloseGameCommand source = new CharGenCloseGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenCloseGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenCloseGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		bool value = m_WithComplete;
		formatter.UnmanagedField(0, "m_WithComplete", ref value, state);
		bool value2 = m_SyncPortrait;
		formatter.UnmanagedField(1, "m_SyncPortrait", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenCloseGameCommand>();
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
				Unsafe.AsRef(in m_WithComplete) = formatter.ReadUnmanaged<bool>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_SyncPortrait) = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
