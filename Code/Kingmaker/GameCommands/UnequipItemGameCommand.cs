using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class UnequipItemGameCommand : GameCommand, IOwlPackable<UnequipItemGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private ItemSlotRef m_From;

	[JsonProperty]
	[OwlPackInclude]
	private ItemSlotRef m_To;

	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<MechanicEntity> m_Owner;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnequipItemGameCommand",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("m_From", typeof(ItemSlotRef)),
			new FieldInfo("m_To", typeof(ItemSlotRef)),
			new FieldInfo("m_Owner", typeof(EntityRef<MechanicEntity>))
		}
	};

	public override bool IsSynchronized => true;

	private UnequipItemGameCommand()
	{
	}

	[JsonConstructor]
	public UnequipItemGameCommand(MechanicEntity owner, ItemSlotRef from, ItemSlotRef to)
	{
		m_From = from;
		m_To = to;
		m_Owner = new EntityRef<MechanicEntity>(owner);
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.UnequipItem(m_Owner, m_From, m_To);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnequipItemGameCommand source = new UnequipItemGameCommand();
		result = Unsafe.As<UnequipItemGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnequipItemGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_From", ref m_From, state);
		formatter.Field(1, "m_To", ref m_To, state);
		formatter.Field(2, "m_Owner", ref m_Owner, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnequipItemGameCommand>();
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
				m_From = formatter.ReadPackable<ItemSlotRef>(state);
				break;
			case 1:
				m_To = formatter.ReadPackable<ItemSlotRef>(state);
				break;
			case 2:
				m_Owner = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
