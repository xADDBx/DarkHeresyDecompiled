using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Code.Framework.Utility.UnityExtensions;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Kingmaker.QA;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[OwlPackable(OwlPackableMode.Generate)]
public class Feature : UnitFact<BlueprintFeature>, IFactWithRanks, IHashable, IOwlPackable<Feature>
{
	public class ParamContextData : ContextData<ParamContextData>
	{
		public FeatureParam Param { get; private set; }

		public ParamContextData Setup(FeatureParam param)
		{
			Param = param;
			return this;
		}

		protected override void Reset()
		{
			Param = null;
		}
	}

	public class Data : ContextData<Data>
	{
		public Feature Feature { get; private set; }

		public Data Setup(Feature feature)
		{
			Feature = feature;
			return this;
		}

		protected override void Reset()
		{
			Feature = null;
		}
	}

	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "Feature",
		OldNames = null,
		Fields = new FieldInfo[15]
		{
			new FieldInfo("m_ComponentsData", typeof(Dictionary<string, List<IEntityFactComponentSavableData>>)),
			new FieldInfo("m_Components", typeof(List<EntityFactComponent>)),
			new FieldInfo("m_Sources", typeof(List<EntityFactSource>)),
			new FieldInfo("m_ChildrenFacts", typeof(List<EntityFactRef>)),
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_Blueprint", typeof(BlueprintFact)),
			new FieldInfo("IsActive", typeof(bool)),
			new FieldInfo("ChildOf", typeof(EntityFactRef)),
			new FieldInfo("m_ParentContext", typeof(MechanicsContext)),
			new FieldInfo("m_Context", typeof(MechanicsContext)),
			new FieldInfo("Rank", typeof(int)),
			new FieldInfo("IsTemporary", typeof(bool)),
			new FieldInfo("Param", typeof(FeatureParam)),
			new FieldInfo("IgnorePrerequisites", typeof(bool)),
			new FieldInfo("DisabledBecauseOfPrerequisites", typeof(bool))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public int Rank { get; protected set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool IsTemporary { get; set; }

	[JsonProperty(IsReference = false)]
	[OwlPackInclude]
	public FeatureParam Param { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool IgnorePrerequisites { get; set; } = true;


	[JsonProperty]
	[OwlPackInclude]
	public bool DisabledBecauseOfPrerequisites { get; set; }

	[CanBeNull]
	public EntityFactSource SourceWithMaxLevel
	{
		get
		{
			int? num = null;
			EntityFactSource result = null;
			foreach (EntityFactSource source in base.Sources)
			{
				if (!num.HasValue || source.PathRank > num)
				{
					num = source.PathRank;
					result = source;
				}
			}
			return result;
		}
	}

	public int SourceLevel => (SourceWithMaxLevel?.PathRank).GetValueOrDefault();

	[CanBeNull]
	public BlueprintRace SourceRace => SourceWithMaxLevel?.Blueprint as BlueprintRace;

	public override bool IsEnabled
	{
		get
		{
			if (base.IsEnabled)
			{
				return !DisabledBecauseOfPrerequisites;
			}
			return false;
		}
	}

	public override bool Hidden
	{
		get
		{
			if (!base.Hidden)
			{
				return base.Blueprint.HideInUI;
			}
			return true;
		}
	}

	public override string Name
	{
		get
		{
			string text = (base.Name.IsNullOrEmpty() ? GetNameFromItemSource() : base.Name);
			if (Param == null)
			{
				return text;
			}
			LocalizedTexts instance = LocalizedTexts.Instance;
			FeatureParam param = Param;
			if (param.WeaponCategory.HasValue)
			{
				return text + " (" + instance.Stats.GetText(param.WeaponCategory.Value) + ")";
			}
			if (param.StatType.HasValue)
			{
				return text + " (" + instance.Stats.GetText(param.StatType.Value) + ")";
			}
			if (param.Blueprint is IUIDataProvider iUIDataProvider)
			{
				return text + " (" + iUIDataProvider.Name + ")";
			}
			return text;
		}
	}

	private string GetNameFromItemSource()
	{
		EntityFactSource entityFactSource = base.Sources.FirstOrDefault((EntityFactSource source) => source.Entity is ItemEntity);
		if (!(entityFactSource != null))
		{
			return "";
		}
		return ((ItemEntity)entityFactSource.Entity).Name;
	}

	public Feature(BlueprintFeature blueprint, MechanicsContext parentContext, int rank = 1)
		: base(blueprint, parentContext)
	{
		if (rank < 1)
		{
			throw new ArgumentOutOfRangeException("rank");
		}
		Rank = rank;
		Param = ContextData<ParamContextData>.Current?.Param ?? new FeatureParam();
	}

	public Feature(JsonConstructorMark _)
	{
	}

	protected Feature()
	{
	}

	public override int GetRank()
	{
		return Rank;
	}

	public void AddRank(int count = 1, MechanicEntity caster = null)
	{
		if (count <= 0)
		{
			return;
		}
		try
		{
			m_IsReapplying.Retain();
			bool isActive = base.IsActive;
			if (isActive)
			{
				Deactivate();
			}
			Rank += count;
			if (isActive)
			{
				Activate();
			}
		}
		finally
		{
			m_IsReapplying.Release();
		}
	}

	public void RemoveRank(int count = 1, MechanicEntity caster = null)
	{
		if (Rank < 1)
		{
			PFLog.Default.ErrorWithReport($"Can't remove rank from feature {base.Blueprint} (current rank {Rank})");
		}
		else
		{
			if (count <= 0)
			{
				return;
			}
			int num = Math.Max(Rank - count, 0);
			if (num >= Rank)
			{
				return;
			}
			if (num >= 1)
			{
				try
				{
					m_IsReapplying.Retain();
					bool isActive = base.IsActive;
					if (isActive)
					{
						Deactivate();
					}
					EntityFactSource entityFactSource = base.Sources.FirstItem((EntityFactSource i) => i.FeatureRank == Rank);
					if (entityFactSource != null)
					{
						RemoveSource(entityFactSource);
					}
					Rank = num;
					if (isActive)
					{
						Activate();
					}
					return;
				}
				finally
				{
					m_IsReapplying.Release();
				}
			}
			base.Manager.Remove(this);
		}
	}

	public override void RunActionInContext(ActionList action, ITargetWrapper target = null)
	{
		using (ContextData<Data>.Request().Setup(this))
		{
			base.RunActionInContext(action, target);
		}
	}

	protected override bool SupportsMultipleSources()
	{
		return true;
	}

	public void AddSource(BlueprintPath path, BlueprintScriptableObject pathFeatureSource, int pathRank)
	{
		if (base.Sources.FirstItem((EntityFactSource i) => i.Blueprint == path && i.FeatureRank >= Rank)?.Blueprint != null)
		{
			PFLog.EntityFact.ErrorWithReport("Feature.AddSource: conflictingSource.FeatureRank >= Rank");
		}
		AddSource(new EntityFactSource(path, pathFeatureSource, pathRank, Rank));
	}

	public void SetSamePathSource(Feature feature)
	{
		foreach (EntityFactSource source in feature.Sources)
		{
			if (source.Path != null && source.PathRank.HasValue && source.FeatureRank.HasValue)
			{
				AddSource(new EntityFactSource(source.Path, source.PathFeatureSource, source.PathRank.Value, source.FeatureRank.Value));
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		int val2 = Rank;
		result.Append(ref val2);
		bool val3 = IsTemporary;
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<FeatureParam>.GetHash128(Param);
		result.Append(ref val4);
		bool val5 = IgnorePrerequisites;
		result.Append(ref val5);
		bool val6 = DisabledBecauseOfPrerequisites;
		result.Append(ref val6);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		Feature source = new Feature();
		result = Unsafe.As<Feature, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<Feature>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_ComponentsData", ref m_ComponentsData, state);
		List<EntityFactComponent> value = base.m_Components;
		formatter.Field(1, "m_Components", ref value, state);
		formatter.Field(2, "m_Sources", ref m_Sources, state);
		formatter.Field(3, "m_ChildrenFacts", ref m_ChildrenFacts, state);
		string value2 = base.UniqueId;
		formatter.StringField(4, "UniqueId", ref value2, state);
		formatter.Field(5, "m_Blueprint", ref m_Blueprint, state);
		bool value3 = base.IsActive;
		formatter.UnmanagedField(6, "IsActive", ref value3, state);
		EntityFactRef value4 = base.ChildOf;
		formatter.Field(7, "ChildOf", ref value4, state);
		formatter.Field(8, "m_ParentContext", ref m_ParentContext, state);
		formatter.Field(9, "m_Context", ref m_Context, state);
		int value5 = Rank;
		formatter.UnmanagedField(10, "Rank", ref value5, state);
		bool value6 = IsTemporary;
		formatter.UnmanagedField(11, "IsTemporary", ref value6, state);
		FeatureParam value7 = Param;
		formatter.Field(12, "Param", ref value7, state);
		bool value8 = IgnorePrerequisites;
		formatter.UnmanagedField(13, "IgnorePrerequisites", ref value8, state);
		bool value9 = DisabledBecauseOfPrerequisites;
		formatter.UnmanagedField(14, "DisabledBecauseOfPrerequisites", ref value9, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Feature>();
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
				m_ComponentsData = formatter.ReadPackable<Dictionary<string, List<IEntityFactComponentSavableData>>>(state);
				break;
			case 1:
				base.m_Components = formatter.ReadPackable<List<EntityFactComponent>>(state);
				break;
			case 2:
				m_Sources = formatter.ReadPackable<List<EntityFactSource>>(state);
				break;
			case 3:
				m_ChildrenFacts = formatter.ReadPackable<List<EntityFactRef>>(state);
				break;
			case 4:
				base.UniqueId = formatter.ReadString(state);
				break;
			case 5:
				m_Blueprint = formatter.ReadPackable<BlueprintFact>(state);
				break;
			case 6:
				base.IsActive = formatter.ReadUnmanaged<bool>(state);
				break;
			case 7:
				base.ChildOf = formatter.ReadPackable<EntityFactRef>(state);
				break;
			case 8:
				m_ParentContext = formatter.ReadPackable<MechanicsContext>(state);
				break;
			case 9:
				m_Context = formatter.ReadPackable<MechanicsContext>(state);
				break;
			case 10:
				Rank = formatter.ReadUnmanaged<int>(state);
				break;
			case 11:
				IsTemporary = formatter.ReadUnmanaged<bool>(state);
				break;
			case 12:
				Param = formatter.ReadPackable<FeatureParam>(state);
				break;
			case 13:
				IgnorePrerequisites = formatter.ReadUnmanaged<bool>(state);
				break;
			case 14:
				DisabledBecauseOfPrerequisites = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
