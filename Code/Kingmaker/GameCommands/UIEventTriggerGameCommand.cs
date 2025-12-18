using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.GameCommands;

[OwlPackable(OwlPackableMode.Generate)]
public class UIEventTriggerGameCommand : GameCommand, IOwlPackable<UIEventTriggerGameCommand>
{
	[JsonProperty]
	[OwlPackInclude]
	private BlueprintComponentReference m_UIEventTrigger;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UIEventTriggerGameCommand",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_UIEventTrigger", typeof(BlueprintComponentReference))
		}
	};

	public override bool IsSynchronized => true;

	private UIEventTriggerGameCommand()
	{
	}

	[JsonConstructor]
	public UIEventTriggerGameCommand([NotNull] UIEventTrigger uiEventTrigger)
	{
		m_UIEventTrigger = uiEventTrigger;
	}

	protected override void ExecuteInternal()
	{
		if (m_UIEventTrigger.Get() is UIEventTrigger uIEventTrigger && uIEventTrigger.Conditions.Check())
		{
			uIEventTrigger.Actions.Run();
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UIEventTriggerGameCommand source = new UIEventTriggerGameCommand();
		result = Unsafe.As<UIEventTriggerGameCommand, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UIEventTriggerGameCommand>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_UIEventTrigger", ref m_UIEventTrigger, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UIEventTriggerGameCommand>();
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
				m_UIEventTrigger = formatter.ReadPackable<BlueprintComponentReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
