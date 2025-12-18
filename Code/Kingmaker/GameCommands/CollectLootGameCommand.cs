using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class CollectLootGameCommand : GameCommand, IOwlPackable<CollectLootGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private List<EntityRef<ItemEntity>> m_Items;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CollectLootGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Items", typeof(List<EntityRef<ItemEntity>>))
		}
	};

	public override bool IsSynchronized => true;

	private CollectLootGameCommand()
	{
	}

	[JsonConstructor]
	public CollectLootGameCommand(List<EntityRef<ItemEntity>> items)
	{
		m_Items = items;
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.TryCollect(m_Items);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CollectLootGameCommand source = new CollectLootGameCommand();
		result = Unsafe.As<CollectLootGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CollectLootGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Items", ref m_Items, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CollectLootGameCommand>();
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
				m_Items = formatter.ReadPackable<List<EntityRef<ItemEntity>>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
