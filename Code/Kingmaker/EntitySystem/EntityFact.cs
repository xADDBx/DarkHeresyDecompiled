using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.Framework.Utility.UnityExtensions;
using Code.GameCore.Mics;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.EntitySystem;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class EntityFact : IDisposable, IUIDataProvider, IEntityFact, IHashable, IOwlPackable, IOwlPackable<EntityFact>
{
	private static class ComponentsDataHasher
	{
		public static Hash128 GetHash128(Dictionary<string, List<IEntityFactComponentSavableData>> obj)
		{
			Hash128 result = default(Hash128);
			if (obj == null)
			{
				return result;
			}
			foreach (KeyValuePair<string, List<IEntityFactComponentSavableData>> item in obj)
			{
				Hash128 val = StringHasher.GetHash128(item.Key);
				result.Append(ref val);
				for (int i = 0; i < item.Value.Count; i++)
				{
					Hash128 val2 = item.Value[i].GetHash128();
					result.Append(ref val2);
				}
			}
			return result;
		}
	}

	public struct ComponentEnumerator<TComponent> : IEnumerator<TComponent>, IEnumerator, IDisposable where TComponent : BlueprintComponent
	{
		private List<BlueprintComponent> m_Components;

		private Func<TComponent, bool> m_Predicate;

		private int m_Index;

		public TComponent Current => (TComponent)m_Components[m_Index];

		object IEnumerator.Current => m_Components[m_Index];

		public ComponentEnumerator(List<BlueprintComponent> components, Func<TComponent, bool> pred)
		{
			m_Index = -1;
			m_Components = components;
			m_Predicate = pred;
		}

		public bool MoveNext()
		{
			if (m_Components == null)
			{
				return false;
			}
			if (m_Predicate == null)
			{
				m_Index++;
				return m_Index < m_Components.Count;
			}
			m_Index++;
			while (m_Index < m_Components.Count)
			{
				if (m_Predicate((TComponent)m_Components[m_Index]))
				{
					return true;
				}
				m_Index++;
			}
			return false;
		}

		public void Reset()
		{
			m_Index = -1;
		}

		public void Dispose()
		{
			Reset();
			m_Components = null;
		}
	}

	public struct ComponentsEnumerable<TComponent> where TComponent : BlueprintComponent
	{
		private List<BlueprintComponent> m_SourceList;

		private Func<TComponent, bool> m_Predicate;

		public ComponentsEnumerable(List<BlueprintComponent> sourceList, Func<TComponent, bool> pred)
		{
			m_SourceList = sourceList;
			m_Predicate = pred;
		}

		public ComponentEnumerator<TComponent> GetEnumerator()
		{
			return new ComponentEnumerator<TComponent>(m_SourceList, m_Predicate);
		}

		public bool Any(Func<TComponent, bool> pred = null)
		{
			if (pred == null)
			{
				GetEnumerator().MoveNext();
			}
			using (ComponentEnumerator<TComponent> componentEnumerator = GetEnumerator())
			{
				while (componentEnumerator.MoveNext())
				{
					TComponent current = componentEnumerator.Current;
					if (pred(current))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	[HasherCustom(Type = typeof(ComponentsDataHasher))]
	[OwlPackInclude]
	protected Dictionary<string, List<IEntityFactComponentSavableData>> m_ComponentsData;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	[OwlPackInclude]
	protected List<EntityFactSource> m_Sources;

	[CanBeNull]
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	protected List<EntityFactRef> m_ChildrenFacts;

	[JsonProperty]
	[OwlPackInclude]
	protected BlueprintFact m_Blueprint;

	private Dictionary<string, List<IEntityFactComponentTransientData>> m_ComponentsTransientData;

	private bool m_RemoveWhenActivatedOrPostLoaded;

	[CanBeNull]
	private (EntityFactComponent Runtime, BlueprintComponent Component)[] m_AllComponentsCache;

	private EntityRef m_CachedOwner;

	protected readonly CountableFlag m_IsReapplying = new CountableFlag();

	private Dictionary<Type, List<BlueprintComponent>> m_ComponentsByType = new Dictionary<Type, List<BlueprintComponent>>();

	public virtual Type RequiredEntityType => EntityInterfacesHelper.EntityInterface;

	[JsonProperty]
	[OwlPackInclude]
	protected List<EntityFactComponent> m_Components { get; set; } = new List<EntityFactComponent>();


	[JsonProperty]
	[OwlPackInclude]
	public string UniqueId { get; protected set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool IsActive { get; protected set; }

	[JsonProperty]
	[OwlPackInclude]
	public EntityFactRef ChildOf { get; protected set; }

	public EntityFactsManager Manager { get; private set; }

	public bool IsPostLoadExecuted { get; private set; }

	public bool IsSubscribedOnEventBus { get; private set; }

	public bool Activating { get; private set; }

	public bool Deactivating { get; private set; }

	public bool IsDisposed { get; private set; }

	public bool SuppressActivationOnAttach { get; set; }

	public BlueprintFact Blueprint => m_Blueprint;

	public bool IsReapplying => m_IsReapplying;

	public ReadonlyList<EntityFactSource> Sources => m_Sources;

	public EntityFactSource FirstSource => m_Sources?.Get(0);

	[CanBeNull]
	public EntityFact SourceFact => FirstSource?.Fact;

	[CanBeNull]
	public IItemEntity SourceItem => FirstSource?.Entity as IItemEntity;

	[CanBeNull]
	public BlueprintAbility SourceAbilityBlueprint => null;

	public List<EntityFactComponent> Components => m_Components;

	public bool IsAttached => Manager != null;

	public virtual bool IsEnabled => true;

	public Entity Owner => (Entity)(Manager?.Owner ?? m_CachedOwner.Entity);

	public IEntity IOwner => Owner;

	public bool Active => IsActive;

	[CanBeNull]
	public virtual MechanicsContext MaybeContext => null;

	private bool AllowActivate
	{
		get
		{
			if (IsEnabled)
			{
				Entity owner = Owner;
				if (owner != null && !owner.IsDisposingNow)
				{
					EntityFactsManager.IFactProcessor factProcessor = Manager?.FirstSuitableDelegate(this);
					return factProcessor == null || factProcessor.IsActive;
				}
			}
			return false;
		}
	}

	private bool AllowSubscribe
	{
		get
		{
			Entity owner = Owner;
			if (owner != null && !owner.IsDisposingNow)
			{
				EntityFactsManager.IFactProcessor factProcessor = Manager?.FirstSuitableDelegate(this);
				return factProcessor == null || factProcessor.IsSubscribedOnEventBus;
			}
			return false;
		}
	}

	private ReadonlyList<(EntityFactComponent Runtime, BlueprintComponent Component)> AllComponentsCache => m_AllComponentsCache ?? (m_AllComponentsCache = (from i in m_Components.Select((EntityFactComponent i) => (i: i, SourceBlueprintComponent: i.SourceBlueprintComponent)).Concat<(EntityFactComponent, BlueprintComponent)>(from i in Blueprint.ComponentsArray
			where !m_Components.HasItem((EntityFactComponent ii) => ii.SourceBlueprintComponent == i)
			select ((EntityFactComponent, BlueprintComponent i))(null, i: i))
		where !i.Item2.Disabled
		select i).ToArray());

	protected virtual IEnumerable<BlueprintScriptableObject> AdditionalComponentsProviders => Enumerable.Empty<BlueprintScriptableObject>();

	public IEnumerable<EntityFact> Children => m_ChildrenFacts?.Select((EntityFactRef i) => i.Fact).NotNull() ?? Enumerable.Empty<EntityFact>();

	public virtual string Name => SelectUIData(UIDataType.Name)?.Name ?? "";

	public virtual string Description => SelectUIData(UIDataType.Description)?.Description ?? "";

	public virtual Sprite Icon => SelectUIData(UIDataType.Icon)?.Icon;

	public virtual string NameForAcronym => SelectUIData(UIDataType.NameForAcronym)?.NameForAcronym ?? "";

	[JsonConstructor]
	public EntityFact()
	{
	}

	public EntityFact(BlueprintFact fact)
		: this()
	{
		IsPostLoadExecuted = true;
		Setup(fact, postLoad: false);
	}

	protected void Setup(BlueprintFact blueprint)
	{
		Setup(blueprint, postLoad: false);
	}

	private void Setup(BlueprintFact blueprint, bool postLoad)
	{
		IsDisposed = false;
		m_Blueprint = blueprint;
		List<BlueprintComponent> list = TempList.Get<BlueprintComponent>();
		CollectComponents(list);
		foreach (EntityFactComponent component2 in m_Components)
		{
			if (component2.SourceBlueprintComponentName.IsNullOrEmpty())
			{
				continue;
			}
			string componentName = component2.SourceBlueprintComponentName;
			BlueprintComponent blueprintComponent = list.FirstOrDefault((BlueprintComponent i) => i.name == componentName);
			if (blueprintComponent == null)
			{
				PFLog.EntityFact.Log($"EntityFact.PostLoad: can't find source component {componentName} ({this})");
				continue;
			}
			component2.Setup(blueprintComponent);
			if (postLoad)
			{
				component2.PostLoad(this);
			}
			if (component2.SourceBlueprintComponent.Disabled)
			{
				PFLog.EntityFact.Log($"Removing disabled component: {component2.SourceBlueprintComponent} {Blueprint.NameSafe()})");
				component2.Deactivate();
				component2.Dispose();
			}
		}
		m_Components.RemoveAll((EntityFactComponent i) => !i.IsInitialized);
		m_ComponentsByType.Clear();
		foreach (BlueprintComponent component in list)
		{
			if (!component.Disabled)
			{
				Type type = component.GetType();
				while (type != typeof(BlueprintComponent))
				{
					if (!m_ComponentsByType.TryGetValue(type, out var value))
					{
						value = new List<BlueprintComponent>();
						m_ComponentsByType.Add(type, value);
					}
					value.Add(component);
					type = type.BaseType;
				}
			}
			if (component is IRuntimeEntityFactComponentProvider runtimeEntityFactComponentProvider && !m_Components.HasItem((EntityFactComponent i) => i.SourceBlueprintComponentName == component.name) && !component.Disabled)
			{
				EntityFactComponent entityFactComponent = runtimeEntityFactComponentProvider.CreateRuntimeFactComponent();
				entityFactComponent.Setup(component);
				AddComponent(entityFactComponent);
			}
		}
	}

	private void CollectComponents(List<BlueprintComponent> result)
	{
		Queue<BlueprintScriptableObject> queue = new Queue<BlueprintScriptableObject>();
		HashSet<BlueprintScriptableObject> hashSet = new HashSet<BlueprintScriptableObject>();
		queue.Enqueue(Blueprint);
		hashSet.Add(Blueprint);
		try
		{
			foreach (BlueprintScriptableObject additionalComponentsProvider in AdditionalComponentsProviders)
			{
				queue.Enqueue(additionalComponentsProvider);
				hashSet.Add(additionalComponentsProvider);
			}
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		while (queue.Count > 0)
		{
			BlueprintComponent[] componentsArray = queue.Dequeue().ComponentsArray;
			foreach (BlueprintComponent blueprintComponent in componentsArray)
			{
				if (blueprintComponent is ComponentsList { List: var list })
				{
					if (list != null && !hashSet.Contains(list))
					{
						queue.Enqueue(list);
						hashSet.Add(list);
					}
				}
				else
				{
					result.Add(blueprintComponent);
				}
			}
		}
	}

	protected bool AddComponent<TComponent>(TComponent component) where TComponent : EntityFactComponent
	{
		if (m_Components.HasItem(component))
		{
			PFLog.EntityFact.Error($"EntityFact.AddComponent: can't add component twice {component}");
			return false;
		}
		if (component.IsActive)
		{
			PFLog.EntityFact.Error($"EntityFact.AddComponent: can't add active component {component}");
			return false;
		}
		m_Components.Add(component);
		component.Initialize(this);
		if (IsActive)
		{
			try
			{
				component.Activate();
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
		}
		return true;
	}

	public override string ToString()
	{
		return ((Blueprint == null) ? GetType().Name : (GetType().Name + "[" + Blueprint.name + "]")) + "#" + Owner;
	}

	public IEntity GetSubscribingEntity()
	{
		IEntity entity = (Manager?.Owner as ItemEntity)?.Wielder;
		IEntity entity2 = entity;
		if (entity2 == null)
		{
			EntityFactsManager manager = Manager;
			if (manager == null)
			{
				return null;
			}
			entity2 = manager.Owner;
		}
		return entity2;
	}

	public virtual int GetRank()
	{
		return 1;
	}

	public ComponentsEnumerable<TComponent> SelectComponents<TComponent>() where TComponent : BlueprintComponent
	{
		if (!m_ComponentsByType.TryGetValue(typeof(TComponent), out var value))
		{
			return new ComponentsEnumerable<TComponent>(null, null);
		}
		return new ComponentsEnumerable<TComponent>(value, null);
	}

	public ComponentsEnumerable<TComponent> SelectComponents<TComponent>(Func<TComponent, bool> pred) where TComponent : BlueprintComponent
	{
		if (!m_ComponentsByType.TryGetValue(typeof(TComponent), out var value))
		{
			return new ComponentsEnumerable<TComponent>(null, null);
		}
		return new ComponentsEnumerable<TComponent>(value, pred);
	}

	public IEnumerable<BlueprintComponentAndRuntime<TComponent>> SelectComponentsWithRuntime<TComponent>() where TComponent : BlueprintComponent
	{
		foreach (EntityFactComponent component2 in m_Components)
		{
			BlueprintComponent sourceBlueprintComponent = component2.SourceBlueprintComponent;
			if (sourceBlueprintComponent is TComponent component && !sourceBlueprintComponent.Disabled)
			{
				yield return new BlueprintComponentAndRuntime<TComponent>(component, component2);
			}
		}
	}

	public IEnumerable<BlueprintComponentAndRuntime<TComponent>> SelectComponentsWithRuntime<TComponent>(Func<TComponent, EntityFactComponent, bool> pred) where TComponent : BlueprintComponent
	{
		foreach (EntityFactComponent component in m_Components)
		{
			BlueprintComponent sourceBlueprintComponent = component.SourceBlueprintComponent;
			if (sourceBlueprintComponent is TComponent val && !sourceBlueprintComponent.Disabled && pred(val, component))
			{
				yield return new BlueprintComponentAndRuntime<TComponent>(val, component);
			}
		}
	}

	[CanBeNull]
	public TComponent GetComponent<TComponent>([CanBeNull] Func<TComponent, bool> pred = null) where TComponent : BlueprintComponent
	{
		foreach (EntityFactComponent component in m_Components)
		{
			BlueprintComponent sourceBlueprintComponent = component.SourceBlueprintComponent;
			if (sourceBlueprintComponent is TComponent val && !sourceBlueprintComponent.Disabled && (pred == null || pred(val)))
			{
				return val;
			}
		}
		BlueprintComponent[] componentsArray = Blueprint.ComponentsArray;
		foreach (BlueprintComponent blueprintComponent in componentsArray)
		{
			if (!(blueprintComponent is IRuntimeEntityFactComponentProvider) && blueprintComponent is TComponent val2 && !blueprintComponent.Disabled && (pred == null || pred(val2)))
			{
				return val2;
			}
		}
		return null;
	}

	public BlueprintComponentAndRuntime<TComponent> GetComponentWithRuntime<TComponent>() where TComponent : BlueprintComponent
	{
		foreach (EntityFactComponent component2 in m_Components)
		{
			BlueprintComponent sourceBlueprintComponent = component2.SourceBlueprintComponent;
			if (sourceBlueprintComponent is TComponent component && !sourceBlueprintComponent.Disabled)
			{
				return new BlueprintComponentAndRuntime<TComponent>(component, component2);
			}
		}
		return default(BlueprintComponentAndRuntime<TComponent>);
	}

	public IEnumerable<TComponent> GetComponents<TComponent>([CanBeNull] Func<TComponent, bool> pred) where TComponent : BlueprintComponent
	{
		foreach (EntityFactComponent component in m_Components)
		{
			BlueprintComponent sourceBlueprintComponent = component.SourceBlueprintComponent;
			if (sourceBlueprintComponent is TComponent val && !sourceBlueprintComponent.Disabled && (pred == null || pred(val)))
			{
				yield return val;
			}
		}
		BlueprintComponent[] componentsArray = Blueprint.ComponentsArray;
		foreach (BlueprintComponent blueprintComponent in componentsArray)
		{
			if (!(blueprintComponent is IRuntimeEntityFactComponentProvider) && blueprintComponent is TComponent val2 && !blueprintComponent.Disabled && (pred == null || pred(val2)))
			{
				yield return val2;
			}
		}
	}

	public void CallComponents<TComponent>(Action<TComponent> action) where TComponent : class
	{
		if (action == null)
		{
			PFLog.Default.ErrorWithReport("Calling components of type " + typeof(TComponent).Name + " for null action.");
			return;
		}
		foreach (var item3 in AllComponentsCache)
		{
			EntityFactComponent item = item3.Runtime;
			BlueprintComponent item2 = item3.Component;
			TComponent val = (item as TComponent) ?? (item2 as TComponent);
			if (!item2.Disabled && val != null)
			{
				using (item?.SetScope())
				{
					action(val);
				}
			}
		}
	}

	public void CallComponentsWithRuntime<TComponent>(Action<TComponent, EntityFactComponent> action) where TComponent : class
	{
		foreach (EntityFactComponent component in m_Components)
		{
			BlueprintComponent sourceBlueprintComponent = component.SourceBlueprintComponent;
			if (!sourceBlueprintComponent.Disabled && sourceBlueprintComponent is TComponent arg)
			{
				using (component.SetScope())
				{
					action(arg, component);
				}
			}
		}
	}

	public void Reapply()
	{
		if (Owner == null || Owner.IsDisposingNow || !IsPostLoadExecuted)
		{
			return;
		}
		try
		{
			m_IsReapplying.Retain();
			MaybeContext?.Recalculate();
			if (IsActive)
			{
				Deactivate();
				Activate();
			}
		}
		finally
		{
			m_IsReapplying.Release();
		}
	}

	public virtual void RunActionInContext(ActionList actions, MechanicEntity target)
	{
		RunActionInContext(actions, target.ToITargetWrapper());
	}

	public virtual void RunActionInContext(ActionList actions, ITargetWrapper target = null)
	{
		if (MaybeContext == null)
		{
			PFLog.Default.ErrorWithReport("There is no Context in " + GetType().Name + ": '" + Name + "' [" + UniqueId + "]. Blueprint: '" + Blueprint?.name + "' [" + Blueprint?.AssetGuid + "]");
		}
		using (MaybeContext?.SetScope(target))
		{
			actions.Run();
		}
	}

	protected virtual bool SupportsMultipleSources()
	{
		return false;
	}

	protected virtual void OnSourceAdded(EntityFactSource source)
	{
	}

	protected void AddSource(EntityFactSource source)
	{
		List<EntityFactSource> sources = m_Sources;
		if (sources == null || !sources.HasItem(source))
		{
			if (!m_Sources.Empty() && !SupportsMultipleSources())
			{
				PFLog.Default.ErrorWithReport($"EntityFact.AddSource ({Blueprint}): !m_Sources.Empty() && !SupportsMultipleSources()");
			}
			(m_Sources ?? (m_Sources = new List<EntityFactSource>())).Add(source);
			OnSourceAdded(source);
		}
	}

	public void AddSource([NotNull] EntityFact fact, BlueprintComponent component = null)
	{
		AddSource(new EntityFactSource(fact, component));
	}

	public void AddSource(Etude etude)
	{
		AddSource(new EntityFactSource(etude));
	}

	public void AddSource(Entity entity)
	{
		AddSource(new EntityFactSource(entity));
	}

	public void AddSource(BlueprintScriptableObject blueprint)
	{
		AddSource(new EntityFactSource(blueprint));
	}

	public void TryAddSource(Element element)
	{
		ICutscenePlayerData current = CutscenePlayerDataScope.Current;
		if (current != null)
		{
			AddSource(new EntityFactSource((CutscenePlayerData)current));
			return;
		}
		MechanicsContext current2 = SimpleContextData<MechanicsContext, MechanicsContext.Scope>.Current;
		if (current2 != null)
		{
			AddSource(new EntityFactSource(current2.Blueprint));
		}
		else if (element.Owner is BlueprintScriptableObject blueprint)
		{
			AddSource(new EntityFactSource(blueprint));
		}
	}

	public void AddSource(BlueprintScriptableObject blueprint, int pathRank)
	{
		AddSource(new EntityFactSource(blueprint, pathRank));
	}

	protected void RemoveSource(EntityFactSource source)
	{
		m_Sources?.Remove(source);
		if (m_Sources.Empty())
		{
			m_Sources = null;
		}
	}

	public bool IsFrom([NotNull] EntityFact fact, [CanBeNull] BlueprintComponent component = null)
	{
		return m_Sources?.HasItem((EntityFactSource i) => i.IsFrom(fact, component)) ?? false;
	}

	public bool IsFrom([NotNull] Entity entity)
	{
		return m_Sources?.HasItem((EntityFactSource i) => i.IsFrom(entity)) ?? false;
	}

	public bool IsFrom(UnitCondition unitCondition)
	{
		return m_Sources?.HasItem((EntityFactSource i) => i.IsFrom(unitCondition)) ?? false;
	}

	public bool IsFrom(Etude etude)
	{
		return m_Sources?.HasItem((EntityFactSource i) => i.IsFrom(etude)) ?? false;
	}

	[NotNull]
	public TData RequestSavableData<TData>(BlueprintComponent component) where TData : IEntityFactComponentSavableData, new()
	{
		List<IEntityFactComponentSavableData> list = m_ComponentsData?.Get(component.name);
		TData val = (TData)list.FirstItem((IEntityFactComponentSavableData i) => i is TData);
		if (val == null)
		{
			if (m_ComponentsData == null)
			{
				m_ComponentsData = new Dictionary<string, List<IEntityFactComponentSavableData>>();
			}
			val = new TData();
			if (list == null)
			{
				list = new List<IEntityFactComponentSavableData>();
				m_ComponentsData.Add(component.name, list);
			}
			list.Add(val);
		}
		return val;
	}

	[NotNull]
	public TData RequestSavableData<TData>(EntityFactComponent component) where TData : IEntityFactComponentSavableData, new()
	{
		return RequestSavableData<TData>(component.SourceBlueprintComponent);
	}

	[NotNull]
	public TData RequestTransientData<TData>(BlueprintComponent component) where TData : IEntityFactComponentTransientData, new()
	{
		List<IEntityFactComponentTransientData> list = m_ComponentsTransientData?.Get(component.name);
		TData val = (TData)list.FirstItem((IEntityFactComponentTransientData i) => i is TData);
		if (val == null)
		{
			if (m_ComponentsTransientData == null)
			{
				m_ComponentsTransientData = new Dictionary<string, List<IEntityFactComponentTransientData>>();
			}
			val = new TData();
			if (list == null)
			{
				list = new List<IEntityFactComponentTransientData>();
				m_ComponentsTransientData.Add(component.name, list);
			}
			list.Add(val);
		}
		return val;
	}

	[NotNull]
	public TData RequestTransientData<TData>(EntityFactComponent component) where TData : IEntityFactComponentTransientData, new()
	{
		return RequestTransientData<TData>(component.SourceBlueprintComponent);
	}

	public void AddChildFact(EntityFact fact)
	{
		if (m_ChildrenFacts == null)
		{
			m_ChildrenFacts = new List<EntityFactRef>();
		}
		m_ChildrenFacts.AddUnique(fact);
		fact.ChildOf = this;
	}

	private void Remove()
	{
		Owner?.Facts.Remove(this);
	}

	private void RemoveChildrenFacts()
	{
		if (m_ChildrenFacts == null)
		{
			return;
		}
		foreach (EntityFactRef childrenFact in m_ChildrenFacts)
		{
			childrenFact.Fact?.Remove();
		}
		m_ChildrenFacts = null;
	}

	public void Attach([NotNull] EntityFactsManager manager)
	{
		if (Manager != null)
		{
			PFLog.EntityFact.Error($"EntityFact.Attach: already attached to manager ({this})");
			return;
		}
		Manager = manager;
		if (UniqueId.IsNullOrEmpty())
		{
			UniqueId = Uuid.Instance.CreateString();
		}
		try
		{
			OnAttach();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		foreach (EntityFactComponent component in m_Components)
		{
			if (!component.RequiredEntityType.IsAssignableFrom(Owner.GetType()))
			{
				PFLog.EntityFact.Error("EntityFact.Attach: invalid component required type {0} (target is {1})", component.RequiredEntityType.Name, Owner.GetType().Name);
			}
			component.FactAttached();
		}
		EntityFactService.Instance.Register(this);
		if (!SuppressActivationOnAttach)
		{
			Activate();
		}
	}

	public void Detach()
	{
		EntityFactService.Instance.Unregister(this);
		if (Manager == null)
		{
			PFLog.EntityFact.Error($"EntityFact.Detach: isn't attached to entity ({this})");
			return;
		}
		if (IsActive)
		{
			Deactivate();
		}
		foreach (EntityFactComponent component in m_Components)
		{
			if (!component.RequiredEntityType.IsAssignableFrom(Owner.GetType()))
			{
				PFLog.EntityFact.Error("EntityFact.Attach: invalid component required type {0} (target is {1})", component.RequiredEntityType.Name, Owner.GetType().Name);
			}
			component.FactDetached();
		}
		try
		{
			OnDetach();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		m_CachedOwner = Manager.ConcreteOwner.Ref;
		Manager = null;
		RemoveChildrenFacts();
	}

	public void Activate()
	{
		if (!AllowActivate)
		{
			return;
		}
		if (!IsAttached)
		{
			PFLog.EntityFact.Error($"EntityFact.Activate: invalid state ({this})");
			return;
		}
		if (IsActive)
		{
			PFLog.EntityFact.Error($"EntityFact.Activate: already active ({this})");
			return;
		}
		IsActive = true;
		try
		{
			Activating = true;
			try
			{
				OnActivate();
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
			try
			{
				MaybeContext?.Recalculate();
			}
			catch (Exception ex2)
			{
				PFLog.EntityFact.Exception(ex2);
			}
			foreach (EntityFactComponent component in m_Components)
			{
				try
				{
					component.Activate();
				}
				catch (Exception ex3)
				{
					PFLog.EntityFact.Exception(ex3);
				}
			}
			try
			{
				OnComponentsDidActivated();
			}
			catch (Exception ex4)
			{
				PFLog.EntityFact.Exception(ex4);
			}
			if (Manager.IsSubscribedOnEventBus)
			{
				Subscribe();
			}
		}
		catch (Exception ex5)
		{
			PFLog.EntityFact.Exception(ex5);
		}
		finally
		{
			Activating = false;
		}
		if (m_RemoveWhenActivatedOrPostLoaded)
		{
			m_RemoveWhenActivatedOrPostLoaded = false;
			Manager.Remove(this);
		}
	}

	public void Deactivate()
	{
		if (!IsActive)
		{
			PFLog.EntityFact.Error($"EntityFact.Deactivate: is not active ({this})");
		}
		else
		{
			if (Deactivating)
			{
				return;
			}
			if (Activating)
			{
				PFLog.EntityFact.ErrorWithReport("EntityFact.Deactivate: invoked from Activate");
			}
			Deactivating = true;
			Unsubscribe();
			foreach (EntityFactComponent component in m_Components)
			{
				try
				{
					component.Deactivate();
				}
				catch (Exception ex)
				{
					PFLog.EntityFact.Exception(ex);
				}
			}
			try
			{
				OnDeactivate();
			}
			catch (Exception ex2)
			{
				PFLog.EntityFact.Exception(ex2);
			}
			IsActive = false;
			Deactivating = false;
		}
	}

	public void UpdateIsActive()
	{
		if (Active && !IsEnabled)
		{
			Deactivate();
		}
		if (!Active && IsEnabled)
		{
			Activate();
		}
	}

	public void Subscribe()
	{
		if (!AllowSubscribe)
		{
			return;
		}
		if (!IsActive)
		{
			PFLog.EntityFact.Error($"EntityFact.TurnOn: invalid state ({this})");
		}
		else
		{
			if (IsSubscribedOnEventBus)
			{
				return;
			}
			EventBus.Subscribe(this);
			foreach (EntityFactComponent component in m_Components)
			{
				try
				{
					component.Subscribe();
				}
				catch (Exception ex)
				{
					PFLog.EntityFact.Exception(ex);
				}
			}
			IsSubscribedOnEventBus = true;
		}
	}

	public void Unsubscribe()
	{
		if (!IsSubscribedOnEventBus)
		{
			return;
		}
		EventBus.Unsubscribe(this);
		foreach (EntityFactComponent component in m_Components)
		{
			try
			{
				component.Unsubscribe();
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
		}
		IsSubscribedOnEventBus = false;
	}

	public void ViewDidAttach()
	{
		try
		{
			OnViewDidAttach();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		foreach (EntityFactComponent component in m_Components)
		{
			component.ViewDidAttach();
		}
	}

	public void ViewWillDetach()
	{
		try
		{
			OnViewWillDetach();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		foreach (EntityFactComponent component in m_Components)
		{
			component.ViewWillDetach();
		}
	}

	public void PrePostLoad(EntityFactsManager manager)
	{
		Manager = manager;
		try
		{
			OnPrePostLoad();
			EntityFactService.Instance.Register(this);
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
	}

	public void PostLoad()
	{
		try
		{
			OnPostLoad();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		Setup(Blueprint, postLoad: true);
		try
		{
			OnComponentsDidPostLoad();
		}
		catch (Exception ex2)
		{
			PFLog.EntityFact.Exception(ex2);
		}
		if (m_RemoveWhenActivatedOrPostLoaded)
		{
			m_RemoveWhenActivatedOrPostLoaded = false;
			Manager.Remove(this);
		}
		IsPostLoadExecuted = true;
	}

	public void ApplyPostLoadFixes()
	{
		foreach (EntityFactComponent component in Components)
		{
			component.ApplyPostLoadFixes();
			if (IsDisposed)
			{
				return;
			}
		}
		Components.RemoveAll((EntityFactComponent i) => !i.IsInitialized);
		try
		{
			OnApplyPostLoadFixes();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
	}

	public void DidPostLoad()
	{
		try
		{
			OnDidPostLoad();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
	}

	public void HoldingStateChanged()
	{
		try
		{
			OnHoldingStateChanged();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
	}

	public void AreaLoadingComplete()
	{
		AreaLoadingCompleteInternal();
		try
		{
			OnAreaLoadingComplete();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		foreach (EntityFactComponent component in m_Components)
		{
			component.AreaLoadingComplete();
		}
	}

	private void AreaLoadingCompleteInternal()
	{
		try
		{
			m_ChildrenFacts?.RemoveAll((EntityFactRef i) => i.Fact == null);
			EntityFactRef childOf = ChildOf;
			if (!childOf.IsEmpty && childOf.Fact == null)
			{
				Remove();
			}
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
	}

	public void Dispose()
	{
		bool flag = Owner?.IsDisposingNow ?? false;
		if (IsAttached && !flag)
		{
			Detach();
		}
		else
		{
			EntityFactService.Instance.Unregister(this);
			Unsubscribe();
		}
		try
		{
			OnDispose();
		}
		catch (Exception ex)
		{
			PFLog.EntityFact.Exception(ex);
		}
		foreach (EntityFactComponent component in m_Components)
		{
			try
			{
				component.Dispose();
			}
			catch (Exception ex2)
			{
				PFLog.EntityFact.Exception(ex2);
			}
		}
		IsDisposed = true;
		RemoveChildrenFacts();
	}

	public void RemoveWhenActivatedOrPostLoaded()
	{
		if (!Activating)
		{
			Entity owner = Owner;
			if (owner != null && owner.IsPostLoadExecuted)
			{
				PFLog.EntityFact.ErrorWithReport("EntityFact.RemoveWhenActivationEnded: invoked, but fact doesn't activating now");
				return;
			}
		}
		m_RemoveWhenActivatedOrPostLoaded = true;
	}

	protected virtual void OnActivate()
	{
	}

	protected virtual void OnComponentsDidActivated()
	{
	}

	protected virtual void OnDeactivate()
	{
	}

	protected virtual void OnViewDidAttach()
	{
	}

	protected virtual void OnViewWillDetach()
	{
	}

	protected virtual void OnPrePostLoad()
	{
	}

	protected virtual void OnPostLoad()
	{
	}

	protected virtual void OnDidPostLoad()
	{
	}

	protected virtual void OnApplyPostLoadFixes()
	{
	}

	protected virtual void OnHoldingStateChanged()
	{
	}

	protected virtual void OnComponentsDidPostLoad()
	{
	}

	protected virtual void OnAttach()
	{
	}

	protected virtual void OnDetach()
	{
	}

	protected virtual void OnAreaLoadingComplete()
	{
	}

	protected virtual void OnDispose()
	{
	}

	[CanBeNull]
	public virtual IUIDataProvider SelectUIData(UIDataType type)
	{
		return (Blueprint as IUIDataProvider)?.SelectUIData(type);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ComponentsDataHasher.GetHash128(m_ComponentsData);
		result.Append(ref val);
		List<EntityFactComponent> components = m_Components;
		if (components != null)
		{
			for (int i = 0; i < components.Count; i++)
			{
				Hash128 val2 = ClassHasher<EntityFactComponent>.GetHash128(components[i]);
				result.Append(ref val2);
			}
		}
		List<EntityFactSource> sources = m_Sources;
		if (sources != null)
		{
			for (int j = 0; j < sources.Count; j++)
			{
				Hash128 val3 = ClassHasher<EntityFactSource>.GetHash128(sources[j]);
				result.Append(ref val3);
			}
		}
		List<EntityFactRef> childrenFacts = m_ChildrenFacts;
		if (childrenFacts != null)
		{
			for (int k = 0; k < childrenFacts.Count; k++)
			{
				EntityFactRef obj = childrenFacts[k];
				Hash128 val4 = StructHasher<EntityFactRef>.GetHash128(ref obj);
				result.Append(ref val4);
			}
		}
		result.Append(UniqueId);
		Hash128 val5 = SimpleBlueprintHasher.GetHash128(m_Blueprint);
		result.Append(ref val5);
		bool val6 = IsActive;
		result.Append(ref val6);
		EntityFactRef obj2 = ChildOf;
		Hash128 val7 = StructHasher<EntityFactRef>.GetHash128(ref obj2);
		result.Append(ref val7);
		return result;
	}

	public abstract void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter;

	public abstract void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter;
}
[OwlPackable(OwlPackableMode.Generate)]
public abstract class EntityFact<TEntity> : EntityFact, IHashable, IOwlPackable<EntityFact<TEntity>> where TEntity : Entity
{
	public override Type RequiredEntityType => typeof(TEntity);

	public new TEntity Owner => (TEntity)base.Owner;

	static EntityFact()
	{
		GenericStaticTypesHolder.Add(typeof(EntityFact<TEntity>));
	}

	public EntityFact()
	{
	}

	public EntityFact(BlueprintFact fact)
		: base(fact)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
