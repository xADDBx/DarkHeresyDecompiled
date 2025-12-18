using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("UI/UIEventTrigger")]
[TypeId("92d47e384bca4f328c246dd05139d6d8")]
[OwlPackable(OwlPackableMode.Generate)]
public class UIEventTrigger : EntityFactComponentDelegate, IUIEventHandler, ISubscriber, IOwlPackable, IOwlPackable<UIEventTrigger>
{
	public UIEventType EventType;

	public ConditionsChecker Conditions;

	public ActionList Actions;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UIEventTrigger",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void HandleUIEvent(UIEventType type)
	{
		if (type == EventType)
		{
			Game.Instance.GameCommandQueue.UIEventTrigger(this);
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UIEventTrigger source = new UIEventTrigger();
		result = Unsafe.As<UIEventTrigger, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<UIEventTrigger>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UIEventTrigger>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
