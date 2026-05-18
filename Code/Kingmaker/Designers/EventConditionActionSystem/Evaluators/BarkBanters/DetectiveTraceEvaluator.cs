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
[TypeId("1b72f25b6a3c4bc984c704e09f5046ed")]
public class DetectiveTraceEvaluator : MapObjectEvaluator, IOwlPackable<DetectiveTraceEvaluator>
{
	[AllowedEntityType(typeof(DetectiveTraceView))]
	[ValidateNotEmpty]
	public EntityReference DetectiveTrace;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DetectiveTraceEvaluator",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override MapObjectEntity GetMapObjectInternal()
	{
		DetectiveTraceView detectiveTraceView = DetectiveTrace.FindView() as DetectiveTraceView;
		if (detectiveTraceView != null)
		{
			return detectiveTraceView.Data;
		}
		return null;
	}

	public override string GetCaptionShort()
	{
		return DetectiveTrace?.ToString();
	}

	public override string GetCaption()
	{
		return DetectiveTrace.ToString();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		DetectiveTraceEvaluator source = new DetectiveTraceEvaluator();
		result = Unsafe.As<DetectiveTraceEvaluator, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DetectiveTraceEvaluator>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DetectiveTraceEvaluator>();
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
