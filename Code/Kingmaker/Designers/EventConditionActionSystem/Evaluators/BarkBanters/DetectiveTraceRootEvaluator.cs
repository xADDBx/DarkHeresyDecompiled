using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators.BarkBanters;

[Serializable]
[OwlPackable(OwlPackableMode.Generate)]
[TypeId("fb2016ec6c554b099dd69241f8a32351")]
public class DetectiveTraceRootEvaluator : MapObjectEvaluator, IOwlPackable<DetectiveTraceRootEvaluator>
{
	[AllowedEntityType(typeof(DetectiveTraceRootView))]
	[ValidateNotEmpty]
	public EntityReference DetectiveTraceRoot;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DetectiveTraceRootEvaluator",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override MapObjectEntity GetMapObjectInternal()
	{
		return DetectiveTraceRoot.FindData() as DetectiveTraceEntity;
	}

	public override string GetCaptionShort()
	{
		return DetectiveTraceRoot?.ToString();
	}

	public override string GetCaption()
	{
		return DetectiveTraceRoot.ToString();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		DetectiveTraceRootEvaluator source = new DetectiveTraceRootEvaluator();
		result = Unsafe.As<DetectiveTraceRootEvaluator, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DetectiveTraceRootEvaluator>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DetectiveTraceRootEvaluator>();
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
