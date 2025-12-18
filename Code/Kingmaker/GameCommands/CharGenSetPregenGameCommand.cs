using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class CharGenSetPregenGameCommand : GameCommand, IOwlPackable<CharGenSetPregenGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_UnitRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CharGenSetPregenGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_UnitRef", typeof(EntityRef<BaseUnitEntity>))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private CharGenSetPregenGameCommand(EntityRef<BaseUnitEntity> m_unitRef)
	{
		m_UnitRef = m_unitRef;
	}

	private CharGenSetPregenGameCommand(OwlPackConstructorParameter _)
	{
	}

	public CharGenSetPregenGameCommand([CanBeNull] BaseUnitEntity unit)
		: this((EntityRef<BaseUnitEntity>)unit)
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenPregenHandler h)
		{
			h.HandleSetPregen(m_UnitRef.Entity);
		});
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CharGenSetPregenGameCommand source = new CharGenSetPregenGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<CharGenSetPregenGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CharGenSetPregenGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityRef<BaseUnitEntity> value = m_UnitRef;
		formatter.Field(0, "m_UnitRef", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CharGenSetPregenGameCommand>();
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
				Unsafe.AsRef(in m_UnitRef) = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
