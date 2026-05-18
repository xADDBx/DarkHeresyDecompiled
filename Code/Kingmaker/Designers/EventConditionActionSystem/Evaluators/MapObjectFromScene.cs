using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("2f7f1ee24f13dca42977f3b2b5a847f8")]
[PlayerUpgraderAllowed(true)]
[OwlPackable(OwlPackableMode.Generate)]
public class MapObjectFromScene : MapObjectEvaluator, IOwlPackable<MapObjectFromScene>
{
	[AllowedEntityType(typeof(MapObjectView))]
	[ValidateNotEmpty]
	public EntityReference MapObject;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MapObjectFromScene",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	protected override MapObjectEntity GetMapObjectInternal()
	{
		return MapObject.FindData() as MapObjectEntity;
	}

	public override string GetCaptionShort()
	{
		return MapObject?.ToString();
	}

	public override string GetCaption()
	{
		return "Object from scene " + MapObject;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MapObjectFromScene source = new MapObjectFromScene();
		result = Unsafe.As<MapObjectFromScene, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MapObjectFromScene>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MapObjectFromScene>();
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
