using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartUniqueAreaEffects : BaseUnitPart, IAreaHandler, ISubscriber, IHashable, IOwlPackable<UnitPartUniqueAreaEffects>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class AreaEffectListEntry : IHashable, IOwlPackable, IOwlPackable<AreaEffectListEntry>
	{
		[JsonProperty]
		[CanBeNull]
		[OwlPackInclude]
		public BlueprintUnitFact Feature;

		[JsonProperty]
		[CanBeNull]
		[OwlPackInclude]
		public string AreaId;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "AreaEffectListEntry",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("Feature", typeof(BlueprintUnitFact)),
				new FieldInfo("AreaId", typeof(string))
			}
		};

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = SimpleBlueprintHasher.GetHash128(Feature);
			result.Append(ref val);
			result.Append(AreaId);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			AreaEffectListEntry source = new AreaEffectListEntry();
			result = Unsafe.As<AreaEffectListEntry, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<AreaEffectListEntry>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.Field(0, "Feature", ref Feature, state);
			formatter.StringField(1, "AreaId", ref AreaId, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AreaEffectListEntry>();
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
					Feature = formatter.ReadPackable<BlueprintUnitFact>(state);
					break;
				case 1:
					AreaId = formatter.ReadString(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	public List<AreaEffectListEntry> Areas = new List<AreaEffectListEntry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartUniqueAreaEffects",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Areas", typeof(List<AreaEffectListEntry>))
		}
	};

	public void NewAreaEffect(AreaEffectEntity newAreaEffect, BlueprintUnitFact feature)
	{
		List<AreaEffectListEntry> list = new List<AreaEffectListEntry>();
		foreach (AreaEffectListEntry area in Areas)
		{
			AreaEffectEntity entity = EntityService.Instance.GetEntity<AreaEffectEntity>(area.AreaId);
			if (entity == null)
			{
				list.Add(area);
			}
			else if (entity.Blueprint == newAreaEffect.Blueprint || area.Feature == feature)
			{
				list.Add(area);
				entity.ForceEnd();
			}
		}
		foreach (AreaEffectListEntry item2 in list)
		{
			Areas.Remove(item2);
		}
		AreaEffectListEntry item = new AreaEffectListEntry
		{
			Feature = feature,
			AreaId = newAreaEffect.UniqueId
		};
		Areas.Add(item);
	}

	public void RemoveAreaEffect(AreaEffectEntity newAreaEffect, BlueprintUnitFact feature)
	{
		List<AreaEffectListEntry> list = new List<AreaEffectListEntry>();
		foreach (AreaEffectListEntry area in Areas)
		{
			AreaEffectEntity entity = EntityService.Instance.GetEntity<AreaEffectEntity>(area.AreaId);
			if (entity == null)
			{
				list.Add(area);
			}
			else if (entity.Blueprint == newAreaEffect.Blueprint || area.Feature == feature)
			{
				list.Add(area);
				entity.ForceEnd();
			}
		}
		foreach (AreaEffectListEntry item in list)
		{
			Areas.Remove(item);
		}
	}

	public void OnAreaBeginUnloading()
	{
		foreach (AreaEffectListEntry area in Areas)
		{
			EntityService.Instance.GetEntity<AreaEffectEntity>(area.AreaId).ForceEnd();
		}
		Areas.Clear();
	}

	public void OnAreaDidLoad()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<AreaEffectListEntry> areas = Areas;
		if (areas != null)
		{
			for (int i = 0; i < areas.Count; i++)
			{
				Hash128 val2 = ClassHasher<AreaEffectListEntry>.GetHash128(areas[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartUniqueAreaEffects source = new UnitPartUniqueAreaEffects();
		result = Unsafe.As<UnitPartUniqueAreaEffects, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartUniqueAreaEffects>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "Areas", ref Areas, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartUniqueAreaEffects>();
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
				Areas = formatter.ReadPackable<List<AreaEffectListEntry>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
