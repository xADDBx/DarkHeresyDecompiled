using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Gameplay.Features.Vendor;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class StartTradingGameCommand : GameCommandWithSynchronized, IOwlPackable<StartTradingGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<MechanicEntity> m_Vendor;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "StartTradingGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_Vendor", typeof(EntityRef<MechanicEntity>))
		}
	};

	private StartTradingGameCommand(EntityRef<MechanicEntity> m_vendor)
	{
		m_Vendor = m_vendor;
	}

	private StartTradingGameCommand(OwlPackConstructorParameter _)
	{
	}

	public StartTradingGameCommand(MechanicEntity vendor, bool isSynchronized)
		: this(vendor)
	{
		m_IsSynchronized = isSynchronized;
	}

	protected override void ExecuteInternal()
	{
		VendorHelper.TradeLogic.BeginTrading(m_Vendor.Entity);
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		StartTradingGameCommand source = new StartTradingGameCommand(default(OwlPackConstructorParameter));
		result = Unsafe.As<StartTradingGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<StartTradingGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Vendor", ref m_Vendor, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<StartTradingGameCommand>();
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
				m_Vendor = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
