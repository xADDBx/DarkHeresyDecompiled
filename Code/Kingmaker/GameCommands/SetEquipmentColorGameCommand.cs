using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.Visual.CharacterSystem;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class SetEquipmentColorGameCommand : GameCommand, IOwlPackable<SetEquipmentColorGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private UnitReference m_UnitRef;

	[JsonProperty]
	[OwlPackInclude]
	private int m_PrimaryIndex;

	[JsonProperty]
	[OwlPackInclude]
	private int m_SecondaryIndex;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SetEquipmentColorGameCommand",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("m_UnitRef", typeof(UnitReference)),
			new FieldInfo("m_PrimaryIndex", typeof(int)),
			new FieldInfo("m_SecondaryIndex", typeof(int))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private SetEquipmentColorGameCommand()
	{
	}

	private SetEquipmentColorGameCommand(int m_primaryindex, int m_secondaryindex, UnitReference m_unitref)
	{
		m_PrimaryIndex = m_primaryindex;
		m_SecondaryIndex = m_secondaryindex;
		m_UnitRef = m_unitref;
	}

	public SetEquipmentColorGameCommand(RampColorPreset.IndexSet indexSet, BaseUnitEntity unit)
	{
		m_PrimaryIndex = indexSet.PrimaryIndex;
		m_SecondaryIndex = indexSet.SecondaryIndex;
		m_UnitRef = UnitReference.FromIAbstractUnitEntity(unit);
	}

	protected override void ExecuteInternal()
	{
		RampColorPreset.IndexSet indexSet = new RampColorPreset.IndexSet();
		indexSet.PrimaryIndex = m_PrimaryIndex;
		indexSet.SecondaryIndex = m_SecondaryIndex;
		m_UnitRef.Entity.ToBaseUnitEntity().SetUnitEquipmentColorRampIndex(indexSet);
		m_UnitRef.Entity.ToBaseUnitEntity().View.Or(null)?.UpdateEquipmentColor();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SetEquipmentColorGameCommand source = new SetEquipmentColorGameCommand();
		result = Unsafe.As<SetEquipmentColorGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SetEquipmentColorGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_UnitRef", ref m_UnitRef, state);
		formatter.UnmanagedField(1, "m_PrimaryIndex", ref m_PrimaryIndex, state);
		formatter.UnmanagedField(2, "m_SecondaryIndex", ref m_SecondaryIndex, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SetEquipmentColorGameCommand>();
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
				m_UnitRef = formatter.ReadPackable<UnitReference>(state);
				break;
			case 1:
				m_PrimaryIndex = formatter.ReadUnmanaged<int>(state);
				break;
			case 2:
				m_SecondaryIndex = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
