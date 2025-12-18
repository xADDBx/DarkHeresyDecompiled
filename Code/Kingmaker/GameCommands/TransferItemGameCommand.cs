using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class TransferItemGameCommand : GameCommand, IOwlPackable<TransferItemGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private ItemsCollectionRef m_To;

	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<ItemEntity> m_Item;

	[JsonProperty]
	[OwlPackInclude]
	private int m_Count;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "TransferItemGameCommand",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("m_To", typeof(ItemsCollectionRef)),
			new FieldInfo("m_Item", typeof(EntityRef<ItemEntity>)),
			new FieldInfo("m_Count", typeof(int))
		}
	};

	public override bool IsSynchronized => true;

	private TransferItemGameCommand()
	{
	}

	[JsonConstructor]
	public TransferItemGameCommand(ItemsCollectionRef to, ItemEntity item, int count)
	{
		m_To = to;
		m_Item = item;
		m_Count = count;
	}

	protected override void ExecuteInternal()
	{
		if (m_Item.Entity != null)
		{
			GameCommandHelper.TransferCount(m_To.ItemsCollection, m_Item, m_Count);
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		TransferItemGameCommand source = new TransferItemGameCommand();
		result = Unsafe.As<TransferItemGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<TransferItemGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_To", ref m_To, state);
		formatter.Field(1, "m_Item", ref m_Item, state);
		formatter.UnmanagedField(2, "m_Count", ref m_Count, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<TransferItemGameCommand>();
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
				m_To = formatter.ReadPackable<ItemsCollectionRef>(state);
				break;
			case 1:
				m_Item = formatter.ReadPackable<EntityRef<ItemEntity>>(state);
				break;
			case 2:
				m_Count = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
