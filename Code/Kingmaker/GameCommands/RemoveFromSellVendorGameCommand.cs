using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class RemoveFromSellVendorGameCommand : GameCommand, IOwlPackable<RemoveFromSellVendorGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<ItemEntity> m_Item;

	[JsonProperty]
	[OwlPackInclude]
	private int m_Count;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "RemoveFromSellVendorGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_Item", typeof(EntityRef<ItemEntity>)),
			new FieldInfo("m_Count", typeof(int))
		}
	};

	public override bool IsSynchronized => true;

	private RemoveFromSellVendorGameCommand()
	{
	}

	[JsonConstructor]
	public RemoveFromSellVendorGameCommand(ItemEntity itemEntity, int count)
	{
		m_Item = itemEntity;
		m_Count = count;
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.RemoveFromSell(m_Item, m_Count);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		RemoveFromSellVendorGameCommand source = new RemoveFromSellVendorGameCommand();
		result = Unsafe.As<RemoveFromSellVendorGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<RemoveFromSellVendorGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Item", ref m_Item, state);
		formatter.UnmanagedField(1, "m_Count", ref m_Count, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<RemoveFromSellVendorGameCommand>();
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
				m_Count = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
