using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class AreaTransitionGameCommand : GameCommand, IOwlPackable<AreaTransitionGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private BlueprintMultiEntranceEntryReference m_MultiEntranceEntryRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AreaTransitionGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_MultiEntranceEntryRef", typeof(BlueprintMultiEntranceEntryReference))
		}
	};

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private AreaTransitionGameCommand()
	{
	}

	public AreaTransitionGameCommand([NotNull] BlueprintMultiEntranceEntry multiEntrance)
	{
		m_MultiEntranceEntryRef = multiEntrance.ToReference<BlueprintMultiEntranceEntryReference>();
	}

	protected override void ExecuteInternal()
	{
		BlueprintMultiEntranceEntry blueprintMultiEntranceEntry = m_MultiEntranceEntryRef?.Get();
		if (blueprintMultiEntranceEntry != null)
		{
			blueprintMultiEntranceEntry.Enter();
			EventBus.RaiseEvent(delegate(IAreaTransitionHandler h)
			{
				h.HandleAreaTransition();
			});
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AreaTransitionGameCommand source = new AreaTransitionGameCommand();
		result = Unsafe.As<AreaTransitionGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AreaTransitionGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_MultiEntranceEntryRef", ref m_MultiEntranceEntryRef, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AreaTransitionGameCommand>();
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
				m_MultiEntranceEntryRef = formatter.ReadPackable<BlueprintMultiEntranceEntryReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
