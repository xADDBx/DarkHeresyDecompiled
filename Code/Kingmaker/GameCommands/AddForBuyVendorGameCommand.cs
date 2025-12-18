using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class AddForBuyVendorGameCommand : GameCommand, IOwlPackable<AddForBuyVendorGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<ItemEntity> m_Item;

	[JsonProperty]
	[OwlPackInclude]
	private int m_Count;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_MakeDeal;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AddForBuyVendorGameCommand",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("m_Item", typeof(EntityRef<ItemEntity>)),
			new FieldInfo("m_Count", typeof(int)),
			new FieldInfo("m_MakeDeal", typeof(bool))
		}
	};

	public override bool IsSynchronized => true;

	private AddForBuyVendorGameCommand()
	{
	}

	[JsonConstructor]
	public AddForBuyVendorGameCommand(ItemEntity itemEntity, int count, bool makeDeal)
	{
		m_Item = itemEntity;
		m_Count = count;
		m_MakeDeal = makeDeal;
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.AddForBuy(m_Item, m_Count);
		if (m_MakeDeal)
		{
			Game.Instance.TradeLogic.MakeDealWithCurrentVendor();
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AddForBuyVendorGameCommand source = new AddForBuyVendorGameCommand();
		result = Unsafe.As<AddForBuyVendorGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AddForBuyVendorGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Item", ref m_Item, state);
		formatter.UnmanagedField(1, "m_Count", ref m_Count, state);
		formatter.UnmanagedField(2, "m_MakeDeal", ref m_MakeDeal, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AddForBuyVendorGameCommand>();
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
			case 2:
				m_MakeDeal = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
