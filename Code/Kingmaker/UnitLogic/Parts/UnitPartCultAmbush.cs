using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartCultAmbush : BaseUnitPart, IHashable, IOwlPackable<UnitPartCultAmbush>
{
	public enum VisibilityStatuses
	{
		NotVisible,
		FirstShow,
		Visible
	}

	[OwlPackable(OwlPackableMode.Generate)]
	public class VisibilityData : IHashable, IOwlPackable, IOwlPackable<VisibilityData>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "VisibilityData",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("VisibilityStatus", typeof(VisibilityStatuses))
			}
		};

		[JsonProperty]
		[OwlPackInclude]
		public VisibilityStatuses VisibilityStatus { get; protected set; }

		public void UpdateVisibilityStatus(VisibilityStatuses visibilityStatus)
		{
			VisibilityStatus = visibilityStatus;
		}

		protected VisibilityData(VisibilityStatuses visibilityStatus)
		{
			VisibilityStatus = visibilityStatus;
		}

		public VisibilityData()
		{
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			VisibilityStatuses val = VisibilityStatus;
			result.Append(ref val);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			VisibilityData source = new VisibilityData();
			result = Unsafe.As<VisibilityData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<VisibilityData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			VisibilityStatuses value = VisibilityStatus;
			formatter.EnumField(0, "VisibilityStatus", ref value, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<VisibilityData>();
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
					VisibilityStatus = formatter.ReadEnum<VisibilityStatuses>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[OwlPackable(OwlPackableMode.Generate)]
	public class VisibilityAbilityData : VisibilityData, IHashable, IOwlPackable<VisibilityAbilityData>
	{
		public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "VisibilityAbilityData",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("VisibilityStatus", typeof(VisibilityStatuses)),
				new FieldInfo("AbilityReference", typeof(BlueprintAbilityReference))
			}
		};

		[JsonProperty]
		[OwlPackInclude]
		public BlueprintAbilityReference AbilityReference { get; private set; }

		public VisibilityAbilityData(VisibilityStatuses visibilityStatus, BlueprintAbility blueprintAbility)
			: base(visibilityStatus)
		{
			AbilityReference = blueprintAbility.ToReference<BlueprintAbilityReference>();
		}

		public VisibilityAbilityData()
		{
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			Hash128 val2 = BlueprintReferenceHasher.GetHash128(AbilityReference);
			result.Append(ref val2);
			return result;
		}

		public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			VisibilityAbilityData source = new VisibilityAbilityData();
			result = Unsafe.As<VisibilityAbilityData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<VisibilityAbilityData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			VisibilityStatuses value = base.VisibilityStatus;
			formatter.EnumField(0, "VisibilityStatus", ref value, state);
			BlueprintAbilityReference value2 = AbilityReference;
			formatter.Field(1, "AbilityReference", ref value2, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<VisibilityAbilityData>();
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
					base.VisibilityStatus = formatter.ReadEnum<VisibilityStatuses>(state);
					break;
				case 1:
					AbilityReference = formatter.ReadPackable<BlueprintAbilityReference>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[OwlPackable(OwlPackableMode.Generate)]
	public class VisibilityFeatureData : VisibilityData, IHashable, IOwlPackable<VisibilityFeatureData>
	{
		public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "VisibilityFeatureData",
			OldNames = null,
			Fields = new FieldInfo[2]
			{
				new FieldInfo("VisibilityStatus", typeof(VisibilityStatuses)),
				new FieldInfo("FeatureReference", typeof(BlueprintFeatureReference))
			}
		};

		[JsonProperty]
		[OwlPackInclude]
		public BlueprintFeatureReference FeatureReference { get; private set; }

		public VisibilityFeatureData(VisibilityStatuses visibilityStatus, BlueprintFeature blueprintFeature)
			: base(visibilityStatus)
		{
			FeatureReference = blueprintFeature.ToReference<BlueprintFeatureReference>();
		}

		public VisibilityFeatureData()
		{
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			Hash128 val2 = BlueprintReferenceHasher.GetHash128(FeatureReference);
			result.Append(ref val2);
			return result;
		}

		public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			VisibilityFeatureData source = new VisibilityFeatureData();
			result = Unsafe.As<VisibilityFeatureData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<VisibilityFeatureData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			VisibilityStatuses value = base.VisibilityStatus;
			formatter.EnumField(0, "VisibilityStatus", ref value, state);
			BlueprintFeatureReference value2 = FeatureReference;
			formatter.Field(1, "FeatureReference", ref value2, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<VisibilityFeatureData>();
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
					base.VisibilityStatus = formatter.ReadEnum<VisibilityStatuses>(state);
					break;
				case 1:
					FeatureReference = formatter.ReadPackable<BlueprintFeatureReference>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	private bool m_IsAllVisibility;

	[JsonProperty]
	[OwlPackInclude]
	private List<VisibilityData> m_VisibilityData = new List<VisibilityData>();

	[JsonIgnore]
	private readonly Dictionary<string, int> m_VisibilityDataHash = new Dictionary<string, int>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartCultAmbush",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_IsAllVisibility", typeof(bool)),
			new FieldInfo("m_VisibilityData", typeof(List<VisibilityData>))
		}
	};

	[JsonIgnore]
	public bool IsAllVisibility => m_IsAllVisibility;

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		m_VisibilityDataHash.Clear();
		for (int num = m_VisibilityData.Count - 1; num >= 0; num--)
		{
			VisibilityData visibilityData = m_VisibilityData[num];
			string assetGuid;
			if (!(visibilityData is VisibilityAbilityData visibilityAbilityData))
			{
				if (!(visibilityData is VisibilityFeatureData visibilityFeatureData) || visibilityFeatureData.FeatureReference.IsEmpty())
				{
					continue;
				}
				assetGuid = visibilityFeatureData.FeatureReference.Get().AssetGuid;
			}
			else
			{
				if (visibilityAbilityData.AbilityReference.IsEmpty())
				{
					continue;
				}
				assetGuid = visibilityAbilityData.AbilityReference.Get().AssetGuid;
			}
			m_VisibilityDataHash.Add(assetGuid, num);
		}
	}

	public VisibilityStatuses Visibility(Ability ability, bool isFirstShow = false)
	{
		if (m_IsAllVisibility || ability == null || ability.Owner != base.Owner || ability.Data.Weapon != null)
		{
			return VisibilityStatuses.Visible;
		}
		return Visibility(ability.Blueprint.AssetGuid, isFirstShow);
	}

	public VisibilityStatuses Visibility(Feature feature, bool isFirstShow = false)
	{
		if (m_IsAllVisibility || feature == null || feature.Owner != base.Owner)
		{
			return VisibilityStatuses.Visible;
		}
		return Visibility(feature.Blueprint.AssetGuid, isFirstShow);
	}

	public VisibilityStatuses Visibility(BaseUnitEntity owner, BlueprintAbility ability, bool isFirstShow = false)
	{
		if (m_IsAllVisibility || ability == null || owner != base.Owner)
		{
			return VisibilityStatuses.Visible;
		}
		return Visibility(ability.AssetGuid, isFirstShow);
	}

	public VisibilityStatuses Visibility(BaseUnitEntity owner, BlueprintFeature feature, bool isFirstShow = false)
	{
		if (m_IsAllVisibility || feature == null || owner != base.Owner)
		{
			return VisibilityStatuses.Visible;
		}
		return Visibility(feature.AssetGuid, isFirstShow);
	}

	private VisibilityStatuses Visibility(string guid, bool isFirstShow)
	{
		if (m_VisibilityDataHash.TryGetValue(guid, out var value) && value >= 0 && value < m_VisibilityData.Count)
		{
			VisibilityData visibilityData = m_VisibilityData[value];
			VisibilityStatuses visibilityStatus = visibilityData.VisibilityStatus;
			if (visibilityStatus == VisibilityStatuses.FirstShow && isFirstShow)
			{
				visibilityData.UpdateVisibilityStatus(VisibilityStatuses.Visible);
			}
			return visibilityStatus;
		}
		return VisibilityStatuses.NotVisible;
	}

	public void ActivateCultAmbushAbilityFact(BlueprintFeature feature)
	{
		if (!m_IsAllVisibility && feature != null && !m_VisibilityDataHash.ContainsKey(feature.AssetGuid))
		{
			m_VisibilityData.Add(new VisibilityFeatureData(VisibilityStatuses.FirstShow, feature));
			m_VisibilityDataHash.Add(feature.AssetGuid, m_VisibilityData.Count - 1);
		}
	}

	public void Use(BlueprintAbilityWrapper ability, bool isWeapon)
	{
		if (!(m_IsAllVisibility || isWeapon) && ability != null && !m_VisibilityDataHash.ContainsKey(ability.AssetGuid) && base.Owner.IsInCombat)
		{
			m_VisibilityData.Add(new VisibilityAbilityData(VisibilityStatuses.FirstShow, ability.OriginalBlueprint));
			m_VisibilityDataHash.Add(ability.AssetGuid, m_VisibilityData.Count - 1);
			base.EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUICultAmbushVisibilityChangeHandler>)delegate(IUICultAmbushVisibilityChangeHandler h)
			{
				h.HandleCultAmbushVisibilityChange();
			}, isCheckRuntime: true);
		}
	}

	public void Use(BlueprintFeature feature)
	{
		if (!m_IsAllVisibility && feature != null && !m_VisibilityDataHash.ContainsKey(feature.AssetGuid) && base.Owner.IsInCombat)
		{
			m_VisibilityData.Add(new VisibilityFeatureData(VisibilityStatuses.FirstShow, feature));
			m_VisibilityDataHash.Add(feature.AssetGuid, m_VisibilityData.Count - 1);
			base.EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUICultAmbushVisibilityChangeHandler>)delegate(IUICultAmbushVisibilityChangeHandler h)
			{
				h.HandleCultAmbushVisibilityChange();
			}, isCheckRuntime: true);
		}
	}

	public void MarkAllAsVisibility(bool isCombatPreparation)
	{
		if (isCombatPreparation)
		{
			MarkAllAsVisibilityImpl();
		}
		else if (base.Owner.IsInCombat)
		{
			MarkAllAsVisibilityImpl();
		}
	}

	private void MarkAllAsVisibilityImpl()
	{
		m_IsAllVisibility = true;
		m_VisibilityData.Clear();
		m_VisibilityDataHash.Clear();
		base.EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUICultAmbushVisibilityChangeHandler>)delegate(IUICultAmbushVisibilityChangeHandler h)
		{
			h.HandleCultAmbushVisibilityChange();
		}, isCheckRuntime: true);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_IsAllVisibility);
		List<VisibilityData> visibilityData = m_VisibilityData;
		if (visibilityData != null)
		{
			for (int i = 0; i < visibilityData.Count; i++)
			{
				Hash128 val2 = ClassHasher<VisibilityData>.GetHash128(visibilityData[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartCultAmbush source = new UnitPartCultAmbush();
		result = Unsafe.As<UnitPartCultAmbush, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartCultAmbush>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "m_IsAllVisibility", ref m_IsAllVisibility, state);
		formatter.Field(1, "m_VisibilityData", ref m_VisibilityData, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartCultAmbush>();
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
				m_IsAllVisibility = formatter.ReadUnmanaged<bool>(state);
				break;
			case 1:
				m_VisibilityData = formatter.ReadPackable<List<VisibilityData>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
