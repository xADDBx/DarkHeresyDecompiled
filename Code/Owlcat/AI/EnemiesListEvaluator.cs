using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using UnityEngine;

namespace Owlcat.AI;

[ComponentName("AI/EnemiesListEvaluator")]
[TypeId("925df81ab1a0411ca3c935953c883ff9")]
[OwlPackable(OwlPackableMode.Generate)]
public class EnemiesListEvaluator : MechanicEntityListEvaluator, IOwlPackable<EnemiesListEvaluator>
{
	[SerializeReference]
	[ValidateNotNull]
	public MechanicEntityEvaluator Entity;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "EnemiesListEvaluator",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public override string GetCaption()
	{
		return $"Enemies of {Entity}";
	}

	protected override List<Entity> GetValueInternal()
	{
		MechanicEntity entity = Entity.GetValue();
		List<Entity> enemies = new List<Entity>();
		Game.Instance.Controllers.TurnController.AllUnits.Where((MechanicEntity u) => u.IsInCombat && u != entity && entity.IsEnemy(u)).ForEach(delegate(MechanicEntity u)
		{
			enemies.Add(u);
		});
		return enemies;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		EnemiesListEvaluator source = new EnemiesListEvaluator();
		result = Unsafe.As<EnemiesListEvaluator, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<EnemiesListEvaluator>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EnemiesListEvaluator>();
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
