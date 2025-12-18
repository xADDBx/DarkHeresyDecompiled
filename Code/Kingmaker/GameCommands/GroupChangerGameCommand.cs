using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class GroupChangerGameCommand : GameCommand, IOwlPackable<GroupChangerGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly UnitReference m_UnitReference;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "GroupChangerGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_UnitReference", typeof(UnitReference))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private GroupChangerGameCommand()
	{
	}

	public GroupChangerGameCommand(UnitReference m_unitReference)
	{
		m_UnitReference = m_unitReference;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(IChangeGroupHandler h)
		{
			h.HandleChangeGroup(m_UnitReference.Id);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		GroupChangerGameCommand source = new GroupChangerGameCommand();
		result = Unsafe.As<GroupChangerGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<GroupChangerGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		UnitReference value = m_UnitReference;
		formatter.Field(0, "m_UnitReference", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<GroupChangerGameCommand>();
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
				Unsafe.AsRef(in m_UnitReference) = formatter.ReadPackable<UnitReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
