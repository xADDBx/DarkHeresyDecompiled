using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class CharGenTryAdvanceStatGameCommand : GameCommand, IOwlPackable<CharGenTryAdvanceStatGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly StatType m_StatType;

	[JsonProperty]
	[OwlPackInclude]
	private readonly bool m_Advance;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CharGenTryAdvanceStatGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_StatType", typeof(StatType)),
			new FieldInfo("m_Advance", typeof(bool))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public CharGenTryAdvanceStatGameCommand(StatType m_statType, bool m_advance)
	{
		m_StatType = m_statType;
		m_Advance = m_advance;
	}

	private CharGenTryAdvanceStatGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenAttributesPhaseHandler h)
		{
			h.HandleTryAdvanceStat(m_StatType, m_Advance);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenTryAdvanceStatGameCommand source = new CharGenTryAdvanceStatGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenTryAdvanceStatGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenTryAdvanceStatGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		StatType value = m_StatType;
		formatter.EnumField(0, "m_StatType", ref value, state);
		bool value2 = m_Advance;
		formatter.UnmanagedField(1, "m_Advance", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenTryAdvanceStatGameCommand>();
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
				Unsafe.AsRef(in m_StatType) = formatter.ReadEnum<StatType>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_Advance) = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
