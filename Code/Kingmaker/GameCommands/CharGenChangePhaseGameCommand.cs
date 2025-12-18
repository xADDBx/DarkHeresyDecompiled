using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class CharGenChangePhaseGameCommand : GameCommand, IOwlPackable<CharGenChangePhaseGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly CharGenPhaseType m_PhaseType;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CharGenChangePhaseGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_PhaseType", typeof(CharGenPhaseType))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public CharGenChangePhaseGameCommand(CharGenPhaseType m_phaseType)
	{
		m_PhaseType = m_phaseType;
	}

	private CharGenChangePhaseGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenChangePhaseHandler h)
		{
			h.HandlePhaseChange(m_PhaseType);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenChangePhaseGameCommand source = new CharGenChangePhaseGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenChangePhaseGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenChangePhaseGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		CharGenPhaseType value = m_PhaseType;
		formatter.EnumField(0, "m_PhaseType", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenChangePhaseGameCommand>();
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
				Unsafe.AsRef(in m_PhaseType) = formatter.ReadEnum<CharGenPhaseType>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
