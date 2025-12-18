using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class DropItemGameCommand : GameCommand, IOwlPackable<DropItemGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<ItemEntity> m_Item;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_Split;

	[JsonProperty]
	[OwlPackInclude]
	private int m_SplitCount;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DropItemGameCommand",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("m_Item", typeof(EntityRef<ItemEntity>)),
			new FieldInfo("m_Split", typeof(bool)),
			new FieldInfo("m_SplitCount", typeof(int))
		}
	};

	public override bool IsSynchronized => true;

	private DropItemGameCommand()
	{
	}

	[JsonConstructor]
	public DropItemGameCommand(ItemEntity item, bool split, int splitCount)
	{
		m_Item = new EntityRef<ItemEntity>(item);
		m_Split = split;
		m_SplitCount = splitCount;
	}

	protected override void ExecuteInternal()
	{
		if (m_Item.Entity != null)
		{
			GameCommandHelper.DropItem(m_Item.Entity, m_Split, m_SplitCount);
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		DropItemGameCommand source = new DropItemGameCommand();
		result = Unsafe.As<DropItemGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DropItemGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Item", ref m_Item, state);
		formatter.UnmanagedField(1, "m_Split", ref m_Split, state);
		formatter.UnmanagedField(2, "m_SplitCount", ref m_SplitCount, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DropItemGameCommand>();
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
				m_Item = formatter.ReadPackable<EntityRef<ItemEntity>>(state);
				break;
			case 1:
				m_Split = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_SplitCount = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
