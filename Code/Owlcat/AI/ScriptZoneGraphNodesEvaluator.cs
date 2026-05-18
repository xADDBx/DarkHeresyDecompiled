using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects.SriptZones;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using Pathfinding;
using UnityEngine;

namespace Owlcat.AI;

[ComponentName("AI/ScriptZoneGraphNodesEvaluator")]
[TypeId("dc616216a2ca4c188c13a2b49d8cded6")]
[OwlPackable(OwlPackableMode.Generate)]
public class ScriptZoneGraphNodesEvaluator : GraphNodeListEvaluator, IOwlPackable, IOwlPackable<ScriptZoneGraphNodesEvaluator>
{
	[AllowedEntityType(typeof(ScriptZone))]
	[SerializeField]
	[ValidateNotEmpty]
	private EntityReference ScriptZone;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ScriptZoneGraphNodesEvaluator",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public override string GetCaption()
	{
		return $"Graph nodes covered by {ScriptZone}";
	}

	protected override List<GraphNode> GetValueInternal()
	{
		ScriptZoneEntity scriptZoneEntity = ScriptZone.FindData() as ScriptZoneEntity;
		HashSet<GraphNode> hashSet = new HashSet<GraphNode>();
		if (scriptZoneEntity != null)
		{
			foreach (IScriptZoneShape shape in scriptZoneEntity.Config.Shapes)
			{
				hashSet.AddRange(shape.CoveredNodes);
			}
		}
		return hashSet.ToList();
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ScriptZoneGraphNodesEvaluator source = new ScriptZoneGraphNodesEvaluator();
		result = Unsafe.As<ScriptZoneGraphNodesEvaluator, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ScriptZoneGraphNodesEvaluator>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ScriptZoneGraphNodesEvaluator>();
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
