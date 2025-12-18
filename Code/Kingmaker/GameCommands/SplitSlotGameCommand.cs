using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class SplitSlotGameCommand : GameCommand, IOwlPackable<SplitSlotGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private ItemSlotRef m_From;

	[JsonProperty]
	[OwlPackInclude]
	private ItemSlotRef m_To;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_IsLoot;

	[JsonProperty]
	[OwlPackInclude]
	private int m_Count;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SplitSlotGameCommand",
		OldNames = null,
		Fields = new FieldInfo[4]
		{
			new FieldInfo("m_From", typeof(ItemSlotRef)),
			new FieldInfo("m_To", typeof(ItemSlotRef)),
			new FieldInfo("m_IsLoot", typeof(bool)),
			new FieldInfo("m_Count", typeof(int))
		}
	};

	public override bool IsSynchronized => true;

	private SplitSlotGameCommand()
	{
	}

	[JsonConstructor]
	public SplitSlotGameCommand(ItemSlotRef from, ItemSlotRef to, bool isLoot, int count)
	{
		m_From = from;
		m_To = to;
		m_IsLoot = isLoot;
		m_Count = count;
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.TrySplitSlot(m_From, m_To, m_IsLoot, m_Count);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SplitSlotGameCommand source = new SplitSlotGameCommand();
		result = Unsafe.As<SplitSlotGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SplitSlotGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_From", ref m_From, state);
		formatter.Field(1, "m_To", ref m_To, state);
		formatter.UnmanagedField(2, "m_IsLoot", ref m_IsLoot, state);
		formatter.UnmanagedField(3, "m_Count", ref m_Count, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SplitSlotGameCommand>();
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
				m_IsLoot = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				m_Count = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
