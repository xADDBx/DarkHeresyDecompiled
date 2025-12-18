using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.View.Mechanics;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("a4aa1729f3bc4ff8bfffb1cc9d02c662")]
[PlayerUpgraderAllowed(true)]
[OwlPackable(OwlPackableMode.Generate)]
public class MechanicEntityFromScene : MechanicEntityEvaluator, IOwlPackable<MechanicEntityFromScene>
{
	[AllowedEntityType(typeof(MechanicEntityView))]
	[ValidateNotEmpty]
	public EntityReference EntityRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MechanicEntityFromScene",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override Entity GetValueInternal()
	{
		return EntityRef.FindData() as MechanicEntity;
	}

	public override string GetCaptionShort()
	{
		return EntityRef?.ToString();
	}

	public override string GetCaption()
	{
		return "Object from scene " + EntityRef;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MechanicEntityFromScene source = new MechanicEntityFromScene();
		result = Unsafe.As<MechanicEntityFromScene, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MechanicEntityFromScene>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MechanicEntityFromScene>();
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
