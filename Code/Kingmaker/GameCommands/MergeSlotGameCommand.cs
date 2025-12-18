using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class MergeSlotGameCommand : GameCommand, IOwlPackable<MergeSlotGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private ItemSlotRef m_From;

	[JsonProperty]
	[OwlPackInclude]
	private ItemSlotRef m_To;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MergeSlotGameCommand",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_From", typeof(ItemSlotRef)),
			new FieldInfo("m_To", typeof(ItemSlotRef))
		}
	};

	public override bool IsSynchronized => true;

	private MergeSlotGameCommand()
	{
	}

	[JsonConstructor]
	public MergeSlotGameCommand(ItemSlotRef from, ItemSlotRef to)
	{
		m_From = from;
		m_To = to;
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.MergeSlot(m_From, m_To);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MergeSlotGameCommand source = new MergeSlotGameCommand();
		result = Unsafe.As<MergeSlotGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MergeSlotGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_From", ref m_From, state);
		formatter.Field(1, "m_To", ref m_To, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MergeSlotGameCommand>();
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
			}
		}
		formatter.LeaveObject();
	}
}
