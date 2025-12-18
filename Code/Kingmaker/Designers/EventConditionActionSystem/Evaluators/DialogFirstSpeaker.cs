using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("4672b5eaedf80cc4392a6475dfeea78e")]
[OwlPackable(OwlPackableMode.Generate)]
public class DialogFirstSpeaker : AbstractUnitEvaluator, IOwlPackable<DialogFirstSpeaker>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DialogFirstSpeaker",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return Game.Instance.Controllers.DialogController.FirstSpeaker;
	}

	public override string GetCaption()
	{
		return "First Dialog Speaker";
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		DialogFirstSpeaker source = new DialogFirstSpeaker();
		result = Unsafe.As<DialogFirstSpeaker, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DialogFirstSpeaker>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DialogFirstSpeaker>();
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
