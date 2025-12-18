using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class CharGenSetEquipmentColorGameCommand : GameCommand, IOwlPackable<CharGenSetEquipmentColorGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly int m_PrimaryIndex;

	[JsonProperty]
	[OwlPackInclude]
	private readonly int m_SecondaryIndex;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CharGenSetEquipmentColorGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_PrimaryIndex", typeof(int)),
			new FieldInfo("m_SecondaryIndex", typeof(int))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public CharGenSetEquipmentColorGameCommand(int m_primaryIndex, int m_secondaryIndex)
	{
		m_PrimaryIndex = m_primaryIndex;
		m_SecondaryIndex = m_secondaryIndex;
	}

	private CharGenSetEquipmentColorGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetEquipmentColor(m_PrimaryIndex, m_SecondaryIndex);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSetEquipmentColorGameCommand source = new CharGenSetEquipmentColorGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSetEquipmentColorGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSetEquipmentColorGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		int value = m_PrimaryIndex;
		formatter.UnmanagedField(0, "m_PrimaryIndex", ref value, state);
		int value2 = m_SecondaryIndex;
		formatter.UnmanagedField(1, "m_SecondaryIndex", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSetEquipmentColorGameCommand>();
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
				Unsafe.AsRef(in m_PrimaryIndex) = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_SecondaryIndex) = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
