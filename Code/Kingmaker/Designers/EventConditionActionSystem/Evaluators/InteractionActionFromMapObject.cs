using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("9ad1a2cfea5241d3917e06f9145b040f")]
[PlayerUpgraderAllowed(true)]
[OwlPackable(OwlPackableMode.Generate)]
public class InteractionActionFromMapObject : InteractionActionEvaluator, IOwlPackable, IOwlPackable<InteractionActionFromMapObject>
{
	[ValidateNotNull]
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "InteractionActionFromMapObject",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public override string GetCaption()
	{
		return $"InteractionAction from {MapObject}";
	}

	protected override InteractionAction GetValueInternal()
	{
		MapObjectEntity value = MapObject.GetValue();
		InteractionActionPart optional = value.Parts.GetOptional<InteractionActionPart>();
		if (optional == null)
		{
			PFLog.EventConditionActionSystem.Warning($"Entity {value} doesn't have InteractionActionPart");
			return null;
		}
		return (InteractionAction)optional.Source;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		InteractionActionFromMapObject source = new InteractionActionFromMapObject();
		result = Unsafe.As<InteractionActionFromMapObject, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<InteractionActionFromMapObject>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<InteractionActionFromMapObject>();
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
