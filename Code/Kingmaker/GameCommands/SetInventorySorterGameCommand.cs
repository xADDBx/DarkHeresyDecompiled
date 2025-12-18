using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class SetInventorySorterGameCommand : GameCommand, IOwlPackable<SetInventorySorterGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private ItemsSorterType m_SorterType;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SetInventorySorterGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_SorterType", typeof(ItemsSorterType))
		}
	};

	private SetInventorySorterGameCommand()
	{
	}

	[JsonConstructor]
	public SetInventorySorterGameCommand(ItemsSorterType sorterType)
	{
		m_SorterType = sorterType;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Player.UISettings.InventorySorter = m_SorterType;
		EventBus.RaiseEvent(delegate(ISetInventorySorterHandler h)
		{
			h.HandleSetInventorySorter(m_SorterType);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SetInventorySorterGameCommand source = new SetInventorySorterGameCommand();
		result = Unsafe.As<SetInventorySorterGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SetInventorySorterGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EnumField(0, "m_SorterType", ref m_SorterType, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SetInventorySorterGameCommand>();
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
				m_SorterType = formatter.ReadEnum<ItemsSorterType>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
