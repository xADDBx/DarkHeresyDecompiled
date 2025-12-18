using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class CharGenSetTattooGameCommand : GameCommand, IOwlPackable<CharGenSetTattooGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly EquipmentEntityLink m_EquipmentEntityLink;

	[JsonProperty]
	[OwlPackInclude]
	private readonly int m_Index;

	[JsonProperty]
	[OwlPackInclude]
	private readonly int m_TattooTabIndex;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CharGenSetTattooGameCommand",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("m_EquipmentEntityLink", typeof(EquipmentEntityLink)),
			new FieldInfo("m_Index", typeof(int)),
			new FieldInfo("m_TattooTabIndex", typeof(int))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public CharGenSetTattooGameCommand([NotNull] EquipmentEntityLink m_equipmentEntityLink, int m_index, int m_tattooTabIndex)
	{
		if (m_equipmentEntityLink == null)
		{
			throw new ArgumentNullException("m_equipmentEntityLink");
		}
		m_EquipmentEntityLink = m_equipmentEntityLink;
		m_Index = m_index;
		m_TattooTabIndex = m_tattooTabIndex;
	}

	private CharGenSetTattooGameCommand(OwlPackConstructorParameter _)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetTattoo(m_EquipmentEntityLink, m_Index, m_TattooTabIndex);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSetTattooGameCommand source = new CharGenSetTattooGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSetTattooGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSetTattooGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EquipmentEntityLink value = m_EquipmentEntityLink;
		formatter.Field(0, "m_EquipmentEntityLink", ref value, state);
		int value2 = m_Index;
		formatter.UnmanagedField(1, "m_Index", ref value2, state);
		int value3 = m_TattooTabIndex;
		formatter.UnmanagedField(2, "m_TattooTabIndex", ref value3, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSetTattooGameCommand>();
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
				Unsafe.AsRef(in m_EquipmentEntityLink) = formatter.ReadPackable<EquipmentEntityLink>(state);
				break;
			case 1:
				Unsafe.AsRef(in m_Index) = formatter.ReadUnmanaged<int>(state);
				break;
			case 2:
				Unsafe.AsRef(in m_TattooTabIndex) = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
