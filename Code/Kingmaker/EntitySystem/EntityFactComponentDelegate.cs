using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Code.GameCore.ElementsSystem;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem;

[TypeId("7fee73fef2544a278ec147707330d9a7")]
public abstract class EntityFactComponentDelegate<TEntity> : BlueprintComponent, IRuntimeEntityFactComponentProvider, ISubscriber, IOverrideOnActivateMethod where TEntity : Entity
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class ComponentRuntime : EntityFactComponent<TEntity>, ISubscriptionProxy, IHashable, IOwlPackable<ComponentRuntime>
	{
		public static readonly OwlPack.Runtime.TypeInfo OwlPackTypeInfo = new OwlPack.Runtime.TypeInfo
		{
			Name = "ComponentRuntime",
			OldNames = null,
			Fields = new OwlPack.Runtime.FieldInfo[1]
			{
				new OwlPack.Runtime.FieldInfo("SourceBlueprintComponentName", typeof(string))
			}
		};

		private EntityFactComponentDelegate<TEntity> Delegate => (EntityFactComponentDelegate<TEntity>)base.SourceBlueprintComponent;

		public FeatureParam Param => (base.Fact as Feature)?.Param;

		public bool IsReapplying => base.Fact.IsReapplying;

		public override void Setup(BlueprintComponent component)
		{
			if (!(component is EntityFactComponentDelegate<TEntity>))
			{
				LogChannel.System.Error("EntityFactComponentDelegate<{0}>.Runtime.Setup: invalid component type {1}", typeof(TEntity).Name, component.GetType().Name);
			}
			else
			{
				base.Setup(component);
			}
		}

		protected override void OnFactAttached()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				Delegate.OnFactAttached();
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnFactDetached()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				Delegate.OnFactDetached();
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnInitialize()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				Delegate.OnInitialize();
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnActivate()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				Delegate.OnActivate();
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnActivateOrPostLoad()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				Delegate.OnActivateOrPostLoad();
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnDeactivate()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				Delegate.OnDeactivate();
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnPostLoad()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				Delegate.OnPostLoad();
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnApplyPostLoadFixes()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				Delegate.OnApplyPostLoadFixes();
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnViewDidAttach()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				Delegate.OnViewDidAttach();
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnViewWillDetach()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				Delegate.OnViewWillDetach();
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnAreaLoadingComplete()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				Delegate.OnAreaLoadingComplete();
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		protected override void OnDispose()
		{
			if (Delegate == null)
			{
				return;
			}
			try
			{
				Delegate.OnDispose();
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
		}

		public ISubscriber GetSubscriber()
		{
			return Delegate;
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			ComponentRuntime source = new ComponentRuntime();
			result = Unsafe.As<ComponentRuntime, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<ComponentRuntime>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			string value = base.SourceBlueprintComponentName;
			formatter.StringField(0, "SourceBlueprintComponentName", ref value, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			OwlPack.Runtime.TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ComponentRuntime>();
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
					base.SourceBlueprintComponentName = formatter.ReadString(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public virtual bool IsOverrideOnActivateMethod
	{
		get
		{
			bool flag = false;
			Type type = GetType();
			while (type != null && type != typeof(EntityFactComponentDelegate) && !flag)
			{
				flag = type.GetMethod("OnActivate", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public) != null;
				type = type.BaseType;
			}
			return flag;
		}
	}

	protected int ExecutesCount { get; set; }

	public ComponentRuntime Runtime => (SimpleContextData<EntityFactComponent, EntityFactComponent.Scope>.Current as ComponentRuntime) ?? throw new InvalidOperationException();

	[NotNull]
	protected TEntity Owner => Runtime.Owner;

	protected EntityFact Fact => Runtime.Fact;

	protected MechanicsContext Context => Runtime.Fact.MaybeContext;

	protected bool IsReapplying => Runtime.IsReapplying;

	[NotNull]
	protected TSavableData RequestSavableData<TSavableData>() where TSavableData : IEntityFactComponentSavableData, new()
	{
		return Fact.RequestSavableData<TSavableData>(Runtime);
	}

	[NotNull]
	protected TTransientData RequestTransientData<TTransientData>() where TTransientData : IEntityFactComponentTransientData, new()
	{
		return Fact.RequestTransientData<TTransientData>(Runtime);
	}

	public IEntity GetSubscribingEntity()
	{
		return null;
	}

	public virtual EntityFactComponent CreateRuntimeFactComponent()
	{
		return new ComponentRuntime();
	}

	protected virtual void OnFactAttached()
	{
	}

	protected virtual void OnFactDetached()
	{
	}

	protected virtual void OnInitialize()
	{
	}

	protected virtual void OnActivate()
	{
	}

	protected virtual void OnActivateOrPostLoad()
	{
	}

	protected virtual void OnDeactivate()
	{
	}

	protected virtual void OnPreSave()
	{
	}

	protected virtual void OnPostLoad()
	{
	}

	protected virtual void OnApplyPostLoadFixes()
	{
	}

	protected virtual void OnViewDidAttach()
	{
	}

	protected virtual void OnViewWillDetach()
	{
	}

	protected virtual void OnAreaLoadingComplete()
	{
	}

	protected virtual void OnDispose()
	{
	}

	protected void RemoveAllFactsOriginatedFromThisComponent([CanBeNull] Entity factsOwner)
	{
		if (factsOwner == null)
		{
			return;
		}
		List<EntityFact> list = null;
		foreach (EntityFact item in factsOwner.Facts.List)
		{
			if (item.IsFrom(Fact, this))
			{
				list = list ?? TempList.Get<EntityFact>();
				list.Add(item);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (EntityFact item2 in list)
		{
			factsOwner.Facts.Remove(item2);
		}
	}
}
[TypeId("7fee73fef2544a278ec147707330d9a7")]
public abstract class EntityFactComponentDelegate : EntityFactComponentDelegate<Entity>
{
}
