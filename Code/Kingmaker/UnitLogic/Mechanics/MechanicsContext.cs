using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Framework.Utility;
using Kingmaker.Items;
using Kingmaker.Networking.Serialization;
using Kingmaker.Pathfinding;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using Newtonsoft.Json;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

[OwlPackable(OwlPackableMode.Generate)]
public class MechanicsContext : IUIDataProvider, IDisposable, IEvalContext, IHashable, IOwlPackable, IOwlPackable<MechanicsContext>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public struct SourceInfo : IHashable, IOwlPackable, IOwlPackable<SourceInfo>
	{
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		[OwlPackInclude]
		private EntityRef<MechanicEntity> m_CasterRef;

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		[OwlPackInclude]
		private EntityFactRef<MechanicEntityFact> m_FactRef;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "SourceInfo",
			Fields = new FieldInfo[6]
			{
				new FieldInfo("m_CasterRef", typeof(EntityRef<MechanicEntity>)),
				new FieldInfo("m_FactRef", typeof(EntityFactRef<MechanicEntityFact>)),
				new FieldInfo("ClickedTarget", typeof(TargetWrapper)),
				new FieldInfo("CastPosition", typeof(Vector3?)),
				new FieldInfo("Blueprint", typeof(BlueprintScriptableObject)),
				new FieldInfo("Ability", typeof(AbilityData))
			}
		};

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		[OwlPackInclude]
		public TargetWrapper ClickedTarget { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		[OwlPackInclude]
		public Vector3? CastPosition { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		[OwlPackInclude]
		public BlueprintScriptableObject Blueprint { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		[OwlPackInclude]
		public AbilityData Ability { get; set; }

		public NodeList? Nodes { get; set; }

		[CanBeNull]
		public MechanicEntity Caster
		{
			get
			{
				return m_CasterRef;
			}
			set
			{
				m_CasterRef = value;
			}
		}

		[CanBeNull]
		public MechanicEntityFact Fact
		{
			get
			{
				return m_FactRef;
			}
			set
			{
				m_FactRef = value;
			}
		}

		[CanBeNull]
		public IUIDataProvider SelectUIData(UIDataType type)
		{
			return (Blueprint as IUIDataProvider)?.SelectUIData(type);
		}

		public Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			EntityRef<MechanicEntity> obj = m_CasterRef;
			Hash128 val = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
			result.Append(ref val);
			EntityFactRef<MechanicEntityFact> obj2 = m_FactRef;
			Hash128 val2 = StructHasher<EntityFactRef<MechanicEntityFact>>.GetHash128(ref obj2);
			result.Append(ref val2);
			Hash128 val3 = ClassHasher<TargetWrapper>.GetHash128(ClickedTarget);
			result.Append(ref val3);
			if (CastPosition.HasValue)
			{
				Vector3 val4 = CastPosition.Value;
				result.Append(ref val4);
			}
			Hash128 val5 = SimpleBlueprintHasher.GetHash128(Blueprint);
			result.Append(ref val5);
			Hash128 val6 = ClassHasher<AbilityData>.GetHash128(Ability);
			result.Append(ref val6);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			SourceInfo source = default(SourceInfo);
			result = Unsafe.As<SourceInfo, TPossiblyBase>(ref source);
		}

		public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
		{
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort type = state.TypeLibrary.RegisterType<SourceInfo>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.Field(0, "m_CasterRef", ref m_CasterRef, state);
			formatter.Field(1, "m_FactRef", ref m_FactRef, state);
			TargetWrapper value = ClickedTarget;
			formatter.Field(2, "ClickedTarget", ref value, state);
			Vector3? value2 = CastPosition;
			formatter.NullableField(3, "CastPosition", ref value2, state);
			BlueprintScriptableObject value3 = Blueprint;
			formatter.Field(4, "Blueprint", ref value3, state);
			AbilityData value4 = Ability;
			formatter.Field(5, "Ability", ref value4, state);
			formatter.EndObject();
		}

		public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SourceInfo>();
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
					m_CasterRef = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
					break;
				case 1:
					m_FactRef = formatter.ReadPackable<EntityFactRef<MechanicEntityFact>>(state);
					break;
				case 2:
					ClickedTarget = formatter.ReadPackable<TargetWrapper>(state);
					break;
				case 3:
					CastPosition = formatter.ReadNullablePackable<Vector3>(state);
					break;
				case 4:
					Blueprint = formatter.ReadPackable<BlueprintScriptableObject>(state);
					break;
				case 5:
					Ability = formatter.ReadPackable<AbilityData>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	private static readonly int CountOfProperties = Enum.GetValues(typeof(ContextPropertyName)).Cast<int>().Max() + 1;

	private static readonly Stack<MechanicsContext> Pool = new Stack<MechanicsContext>();

	[JsonProperty]
	[OwlPackInclude]
	protected EntityRef<MechanicEntity> m_OwnerRef;

	[JsonProperty]
	[OwlPackInclude]
	protected EntityRef<MechanicEntity> m_CasterRef;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	protected EntityFactRef<MechanicEntityFact> m_FactRef;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	protected AbilityData m_Ability;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	protected SourceInfo m_Source;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[OwlPackInclude]
	protected TargetWrapper m_ClickedTarget;

	[GameStateInclude]
	private readonly int[] m_Properties = new int[CountOfProperties];

	private ScopedStack<TargetWrapper> m_ScopeTarget;

	private ScopedStack<RulebookEvent> m_ScopeRule;

	private ScopedStack<MechanicEntity> m_ScopeCurrentEntity;

	private ScopedStack<AreaEffectEntity> m_ScopeAreaEffect;

	private ScopedStack<AbilityData> m_ScopeSourceAbility;

	private static readonly Dictionary<string, ContextPropertyName> _NameToProperty = BuildNameToPropertyMap();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MechanicsContext",
		OldNames = null,
		Fields = new FieldInfo[10]
		{
			new FieldInfo("m_OwnerRef", typeof(EntityRef<MechanicEntity>)),
			new FieldInfo("m_CasterRef", typeof(EntityRef<MechanicEntity>)),
			new FieldInfo("m_FactRef", typeof(EntityFactRef<MechanicEntityFact>)),
			new FieldInfo("m_Ability", typeof(AbilityData)),
			new FieldInfo("m_Source", typeof(SourceInfo)),
			new FieldInfo("m_ClickedTarget", typeof(TargetWrapper)),
			new FieldInfo("Blueprint", typeof(BlueprintScriptableObject)),
			new FieldInfo("Direction", typeof(Vector3)),
			new FieldInfo("DisableFx", typeof(bool)),
			new FieldInfo("PropertiesConverter", typeof(Dictionary<ContextPropertyName, int>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintScriptableObject Blueprint { get; protected set; }

	[JsonProperty(IsReference = false, DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public Vector3 Direction { get; protected set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	public bool DisableFx { get; protected set; }

	[CanBeNull]
	[GameStateIgnore]
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	protected Dictionary<ContextPropertyName, int> PropertiesConverter
	{
		get
		{
			Dictionary<ContextPropertyName, int> dictionary = null;
			int num = Math.Min(m_Properties.Length, CountOfProperties);
			for (int i = 0; i < num; i++)
			{
				if (m_Properties[i] > 0)
				{
					if (dictionary == null)
					{
						dictionary = new Dictionary<ContextPropertyName, int>();
					}
					dictionary.Add((ContextPropertyName)i, m_Properties[i]);
				}
			}
			return dictionary;
		}
		set
		{
			if (value == null)
			{
				return;
			}
			foreach (var (contextPropertyName2, num2) in value)
			{
				m_Properties[(int)contextPropertyName2] = num2;
			}
		}
	}

	internal int[] PropertiesArray => m_Properties;

	public int this[ContextPropertyName propertyName]
	{
		get
		{
			return m_Properties[(int)propertyName];
		}
		set
		{
			m_Properties[(int)propertyName] = value;
		}
	}

	[CanBeNull]
	public MechanicEntity MaybeCaster
	{
		get
		{
			MechanicEntity mechanicEntity = m_CasterRef.Entity;
			if (mechanicEntity == null)
			{
				if (!(MaybeOwner?.UniqueId == m_CasterRef.Id))
				{
					return null;
				}
				mechanicEntity = MaybeOwner;
			}
			return mechanicEntity;
		}
	}

	[CanBeNull]
	public MechanicEntity MaybeOwner => m_OwnerRef.Entity;

	public virtual TargetWrapper ClickedTarget
	{
		get
		{
			TargetWrapper targetWrapper = m_ClickedTarget;
			if ((object)targetWrapper == null)
			{
				MechanicEntity entity = m_OwnerRef.Entity;
				if (entity == null)
				{
					return new TargetWrapper(Vector3.zero);
				}
				targetWrapper = entity;
			}
			return targetWrapper;
		}
	}

	[CanBeNull]
	public MechanicEntityFact Fact => m_FactRef;

	[CanBeNull]
	public AbilityData Ability
	{
		get
		{
			AbilityData abilityData = m_Ability;
			if ((object)abilityData == null)
			{
				Ability obj = Fact as Ability;
				if (obj == null)
				{
					return null;
				}
				abilityData = obj.MaybeData;
			}
			return abilityData;
		}
	}

	[CanBeNull]
	public BlueprintAbility AbilityBlueprint => Ability?.Blueprint.OriginalBlueprint;

	public BlueprintScriptableObject SourceBlueprint => m_Source.Blueprint ?? Blueprint;

	[CanBeNull]
	public BlueprintAbility SourceAbilityBlueprint => m_Source.Ability?.Blueprint.OriginalBlueprint ?? AbilityBlueprint;

	[CanBeNull]
	public AbilityData SourceAbility => m_ScopeSourceAbility.Peek() ?? m_Source.Ability ?? Ability;

	[CanBeNull]
	public MechanicEntity SourceCaster => m_Source.Caster ?? MaybeCaster;

	public TargetWrapper SourceClickedTarget => m_Source.ClickedTarget ?? ClickedTarget;

	public NodeList SourcePatternNodes => m_Source.Nodes ?? AsAbilityContext?.Pattern.Nodes ?? NodeList.Empty;

	[CanBeNull]
	public MechanicEntityFact SourceFact => m_Source.Fact ?? Fact;

	public virtual Vector3? SourceCastPosition => m_Source.CastPosition;

	public string Name => SelectUIData(UIDataType.Name)?.Name ?? "";

	public string Description => SelectUIData(UIDataType.Description)?.Description ?? "";

	[CanBeNull]
	public Sprite Icon => SelectUIData(UIDataType.Icon)?.Icon;

	public string NameForAcronym => SelectUIData(UIDataType.NameForAcronym)?.NameForAcronym ?? "";

	[CanBeNull]
	public AbilityExecutionContext AsAbilityContext => this as AbilityExecutionContext;

	[CanBeNull]
	MechanicEntity IEvalContext.Caster => MaybeCaster;

	[CanBeNull]
	MechanicEntity IEvalContext.Owner => MaybeOwner;

	TargetWrapper IEvalContext.Target => m_ScopeTarget.Peek() ?? ClickedTarget;

	[CanBeNull]
	TargetWrapper IEvalContext.ClickedTarget => m_ClickedTarget;

	[CanBeNull]
	RulebookEvent IEvalContext.Rule => m_ScopeRule.Peek();

	BlueprintScriptableObject IEvalContext.SourceBlueprint => SourceBlueprint;

	TargetWrapper IEvalContext.SourceClickedTarget => SourceClickedTarget;

	Vector3? IEvalContext.SourceCastPosition => SourceCastPosition;

	int[] IEvalContext.PropertiesArray => m_Properties;

	[CanBeNull]
	public virtual MechanicEntity CurrentEntity => null;

	[CanBeNull]
	public virtual AreaEffectEntity AreaEffect => m_ScopeAreaEffect.Peek() ?? (MaybeOwner as AreaEffectEntity);

	[CanBeNull]
	AbilityExecutionContext IEvalContext.AbilityExecution => AsAbilityContext;

	public virtual OrientedPatternData Pattern => OrientedPatternData.Empty;

	[CanBeNull]
	public ItemEntityWeapon AbilityWeapon => Ability?.Weapon;

	[CanBeNull]
	public MechanicEntity RuleInitiator => m_ScopeRule.Peek()?.Initiator;

	[CanBeNull]
	public MechanicEntity RuleTarget => m_ScopeRule.Peek()?.Target;

	[CanBeNull]
	public MechanicEntity CurrentTargetEntity => ((IEvalContext)this).Target?.Entity;

	[CanBeNull]
	public TargetWrapper ContextMainTarget => m_ClickedTarget;

	public Vector3 ContextMainTargetPosition => m_ClickedTarget?.Point ?? default(Vector3);

	[CanBeNull]
	public MechanicEntity ContextOwner => MaybeOwner;

	[CanBeNull]
	public MechanicEntity ContextCaster => MaybeCaster;

	public virtual LosDescription LosToClickedTarget => default(LosDescription);

	public int this[string key]
	{
		get
		{
			if (!_NameToProperty.TryGetValue(key, out var value))
			{
				return 0;
			}
			return this[value];
		}
		set
		{
			if (_NameToProperty.TryGetValue(key, out var value2))
			{
				this[value2] = value;
			}
		}
	}

	protected void Setup(BlueprintScriptableObject blueprint, [CanBeNull] MechanicEntity caster, [CanBeNull] MechanicEntity owner = null, [CanBeNull] IEvalContext parent = null, [CanBeNull] TargetWrapper clickedTarget = null, [CanBeNull] MechanicEntityFact fact = null, [CanBeNull] AbilityData ability = null)
	{
		Blueprint = blueprint ?? throw new ArgumentNullException("blueprint");
		m_CasterRef = caster ?? owner ?? throw new NullReferenceException("MechanicsContext: caster and owner are null");
		m_OwnerRef = owner ?? caster;
		m_FactRef = fact ?? ability?.Fact;
		m_Ability = ability;
		if (clickedTarget != null)
		{
			m_ClickedTarget = clickedTarget;
		}
		else if (parent is AbilityExecutionContext)
		{
			m_ClickedTarget = parent.ClickedTarget;
		}
		else
		{
			m_ClickedTarget = MaybeOwner;
		}
		if (parent != null)
		{
			Direction = parent.Direction;
			int[] propertiesArray = parent.PropertiesArray;
			if (propertiesArray != null)
			{
				Array.Copy(propertiesArray, m_Properties, CountOfProperties);
			}
		}
		m_Source = new SourceInfo
		{
			Caster = parent?.SourceCaster,
			ClickedTarget = parent?.SourceClickedTarget,
			CastPosition = parent?.SourceCastPosition,
			Ability = parent?.SourceAbility,
			Blueprint = parent?.SourceBlueprint,
			Nodes = parent?.SourcePatternNodes,
			Fact = parent?.SourceFact
		};
	}

	public static MechanicsContext Claim(BlueprintScriptableObject blueprint, MechanicEntity caster, [CanBeNull] MechanicEntity owner = null, [CanBeNull] IEvalContext parent = null, [CanBeNull] TargetWrapper clickedTarget = null, [CanBeNull] MechanicEntityFact fact = null, [CanBeNull] AbilityData ability = null)
	{
		if (!Pool.TryPop(out var result))
		{
			result = new MechanicsContext();
		}
		result.Setup(blueprint, caster, owner, parent, clickedTarget, fact, ability);
		return result;
	}

	protected void Reset()
	{
		m_OwnerRef = default(EntityRef<MechanicEntity>);
		m_CasterRef = default(EntityRef<MechanicEntity>);
		m_FactRef = default(EntityFactRef<MechanicEntityFact>);
		m_Ability = null;
		m_ClickedTarget = null;
		m_Source = default(SourceInfo);
		Blueprint = null;
		DisableFx = false;
		Direction = default(Vector3);
		Array.Fill(m_Properties, 0);
	}

	public void ReplaceClickedTarget(TargetWrapper newTarget)
	{
		m_ClickedTarget = newTarget;
	}

	public virtual void Dispose()
	{
		Reset();
		Pool.Push(this);
	}

	[CanBeNull]
	public IUIDataProvider SelectUIData(UIDataType type)
	{
		return (Blueprint as IUIDataProvider)?.SelectUIData(type) ?? m_Source.SelectUIData(type);
	}

	public void Recalculate()
	{
		if (MaybeCaster == null)
		{
			return;
		}
		foreach (IPropertyCalculatorComponent allCalculator in this.GetAllCalculators())
		{
			if (allCalculator.SaveToContext != 0)
			{
				MechanicEntity mechanicEntity = ((allCalculator.SaveToContext == SaveToContextType.ForCaster) ? MaybeCaster : ClickedTarget.Entity);
				if (mechanicEntity == null)
				{
					PFLog.Default.ErrorWithReport("Can't calculate property: unit is null ({0}, {1}, {2})", allCalculator.SaveToContext, allCalculator.Name, Blueprint);
				}
				else
				{
					this[allCalculator.Name] = allCalculator.GetValue(this, mechanicEntity);
				}
			}
		}
	}

	public virtual T TriggerRule<T>(T rule) where T : RulebookEvent
	{
		RuleReason left = rule.Reason;
		RuleReason right = default(RuleReason);
		if (left == right)
		{
			rule.Reason = this;
		}
		return Rulebook.Trigger(rule);
	}

	private static Dictionary<string, ContextPropertyName> BuildNameToPropertyMap()
	{
		Dictionary<string, ContextPropertyName> dictionary = new Dictionary<string, ContextPropertyName>();
		foreach (ContextPropertyName value in Enum.GetValues(typeof(ContextPropertyName)))
		{
			dictionary[value.ToString()] = value;
		}
		return dictionary;
	}

	[CanBeNull]
	public MechanicEntity GetEntityByType(PropertyTargetType type)
	{
		return type switch
		{
			PropertyTargetType.CurrentEntity => CurrentEntity, 
			PropertyTargetType.CurrentTarget => CurrentTargetEntity, 
			PropertyTargetType.ContextCaster => ContextCaster, 
			PropertyTargetType.ContextOwner => ContextOwner, 
			PropertyTargetType.ContextMainTarget => ContextMainTarget?.Entity, 
			PropertyTargetType.RuleInitiator => RuleInitiator, 
			PropertyTargetType.RuleTarget => RuleTarget, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public Vector3 GetEntityPositionByType(PropertyTargetType type)
	{
		MechanicEntity entityByType = GetEntityByType(type);
		if (entityByType != null)
		{
			return entityByType.Position;
		}
		if (type == PropertyTargetType.ContextMainTarget)
		{
			return ContextMainTargetPosition;
		}
		return default(Vector3);
	}

	public IntRect GetEntityRectByType(PropertyTargetType type)
	{
		return GetEntityByType(type)?.SizeRect ?? default(IntRect);
	}

	[CanBeNull]
	public T Get<T>() where T : ContextData<T>, new()
	{
		return ContextData<T>.Current;
	}

	public ScopedStackFrame PushTarget(TargetWrapper target)
	{
		return m_ScopeTarget.Push(target);
	}

	public ScopedStackFrame PushRule(RulebookEvent rule)
	{
		return m_ScopeRule.Push(rule);
	}

	public ScopedStackFrame PushCurrentEntity(MechanicEntity entity)
	{
		return m_ScopeCurrentEntity.Push(entity);
	}

	public ScopedStackFrame PushAreaEffect(AreaEffectEntity ae)
	{
		return m_ScopeAreaEffect.Push(ae);
	}

	public ScopedStackFrame PushSourceAbility(AbilityData ability)
	{
		return m_ScopeSourceAbility.Push(ability);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<MechanicEntity> obj = m_OwnerRef;
		Hash128 val = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		EntityRef<MechanicEntity> obj2 = m_CasterRef;
		Hash128 val2 = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj2);
		result.Append(ref val2);
		EntityFactRef<MechanicEntityFact> obj3 = m_FactRef;
		Hash128 val3 = StructHasher<EntityFactRef<MechanicEntityFact>>.GetHash128(ref obj3);
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<AbilityData>.GetHash128(m_Ability);
		result.Append(ref val4);
		SourceInfo obj4 = m_Source;
		Hash128 val5 = StructHasher<SourceInfo>.GetHash128(ref obj4);
		result.Append(ref val5);
		Hash128 val6 = ClassHasher<TargetWrapper>.GetHash128(m_ClickedTarget);
		result.Append(ref val6);
		Hash128 val7 = SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val7);
		Vector3 val8 = Direction;
		result.Append(ref val8);
		bool val9 = DisableFx;
		result.Append(ref val9);
		result.Append(m_Properties);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MechanicsContext source = new MechanicsContext();
		result = Unsafe.As<MechanicsContext, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MechanicsContext>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_OwnerRef", ref m_OwnerRef, state);
		formatter.Field(1, "m_CasterRef", ref m_CasterRef, state);
		formatter.Field(2, "m_FactRef", ref m_FactRef, state);
		formatter.Field(3, "m_Ability", ref m_Ability, state);
		formatter.Field(4, "m_Source", ref m_Source, state);
		formatter.Field(5, "m_ClickedTarget", ref m_ClickedTarget, state);
		BlueprintScriptableObject value = Blueprint;
		formatter.Field(6, "Blueprint", ref value, state);
		Vector3 value2 = Direction;
		formatter.Field(7, "Direction", ref value2, state);
		bool value3 = DisableFx;
		formatter.UnmanagedField(8, "DisableFx", ref value3, state);
		Dictionary<ContextPropertyName, int> value4 = PropertiesConverter;
		formatter.Field(9, "PropertiesConverter", ref value4, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MechanicsContext>();
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
				m_OwnerRef = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			case 1:
				m_CasterRef = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
				break;
			case 2:
				m_FactRef = formatter.ReadPackable<EntityFactRef<MechanicEntityFact>>(state);
				break;
			case 3:
				m_Ability = formatter.ReadPackable<AbilityData>(state);
				break;
			case 4:
				m_Source = formatter.ReadPackable<SourceInfo>(state);
				break;
			case 5:
				m_ClickedTarget = formatter.ReadPackable<TargetWrapper>(state);
				break;
			case 6:
				Blueprint = formatter.ReadPackable<BlueprintScriptableObject>(state);
				break;
			case 7:
				Direction = formatter.ReadPackable<Vector3>(state);
				break;
			case 8:
				DisableFx = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				PropertiesConverter = formatter.ReadPackable<Dictionary<ContextPropertyName, int>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
