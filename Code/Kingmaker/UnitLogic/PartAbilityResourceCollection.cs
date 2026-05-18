using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[OwlPackable(OwlPackableMode.Generate)]
public class PartAbilityResourceCollection : EntityPart, IHashable, IOwlPackable<PartAbilityResourceCollection>
{
	public interface IOwner : IEntityPartOwner<PartAbilityResourceCollection>, IEntityPartOwner
	{
		PartAbilityResourceCollection AbilityResources { get; }
	}

	[GameStateInclude]
	private Dictionary<BlueprintScriptableObject, AbilityResource> m_Resources = new Dictionary<BlueprintScriptableObject, AbilityResource>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartAbilityResourceCollection",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("PersistantResources", typeof(AbilityResource[]))
		}
	};

	[JsonProperty]
	[UsedImplicitly]
	[GameStateIgnore("This adapter uses LINQ, so original m_Objectives used for serialization")]
	[OwlPackInclude]
	public AbilityResource[] PersistantResources
	{
		get
		{
			return m_Resources.Values.ToArray();
		}
		set
		{
			m_Resources = value.ToDictionary((AbilityResource i) => i.Blueprint, (AbilityResource i) => i);
		}
	}

	public ICollection<AbilityResource> GetResources => m_Resources.Values;

	private IEnumerable<BlueprintScriptableObject> Enumerable => m_Resources.Select((KeyValuePair<BlueprintScriptableObject, AbilityResource> i) => i.Value.Blueprint);

	public IEnumerator<BlueprintScriptableObject> GetEnumerator()
	{
		return Enumerable.GetEnumerator();
	}

	[CanBeNull]
	private AbilityResource GetResource(BlueprintScriptableObject blueprint)
	{
		m_Resources.TryGetValue(blueprint, out var value);
		return value;
	}

	public int GetResourceAmount(BlueprintScriptableObject blueprint)
	{
		return GetResource(blueprint)?.Amount ?? 0;
	}

	public int GetResourceMax(BlueprintScriptableObject blueprint)
	{
		return GetResource(blueprint)?.GetMaxAmount(base.ConcreteOwner) ?? 0;
	}

	public void Add(BlueprintScriptableObject blueprint, bool restoreAmount)
	{
		if (ContainsResource(blueprint))
		{
			PFLog.Default.Error("Can't add ability's resource ({0}) twice. Unit: {1}", blueprint, base.Owner);
			return;
		}
		AbilityResource abilityResource = new AbilityResource(blueprint);
		if (restoreAmount)
		{
			abilityResource.Amount = abilityResource.GetMaxAmount(base.ConcreteOwner);
		}
		m_Resources[blueprint] = abilityResource;
	}

	public void Remove(BlueprintScriptableObject blueprint)
	{
		if (!ContainsResource(blueprint))
		{
			LogChannel.Default.Error($"UnitAbilityResourceCollection.Remove: has no resource {blueprint} ({base.Owner})");
		}
		else
		{
			m_Resources.Remove(blueprint);
		}
	}

	private void Restore(BlueprintScriptableObject blueprint, int amount, bool restoreFull)
	{
		if (amount < 0)
		{
			PFLog.Default.Error("UnitAbilityResourceCollection.Restore: {0} invalid restore amount {1} for resource {2}", base.Owner, amount, blueprint);
		}
		else
		{
			if (amount == 0 && !restoreFull)
			{
				return;
			}
			AbilityResource resource = GetResource(blueprint);
			if (resource == null)
			{
				PFLog.Default.Error("UnitAbilityResourceCollection.Restore: {0} has no resource {1}", base.Owner, blueprint);
				return;
			}
			int maxAmount = resource.GetMaxAmount(base.ConcreteOwner);
			resource.Amount = (restoreFull ? maxAmount : Math.Min(resource.Amount + amount, maxAmount));
			base.EventBus.RaiseEvent(base.Owner, delegate(IAbilityResourceHandler h)
			{
				h.HandleAbilityResourceChange(resource);
			});
		}
	}

	public void Restore(BlueprintScriptableObject blueprint, int amount)
	{
		Restore(blueprint, amount, restoreFull: false);
	}

	public void Restore(BlueprintScriptableObject blueprint)
	{
		Restore(blueprint, 0, restoreFull: true);
	}

	public void FullRestoreAll()
	{
		m_Resources.ForEach(delegate(KeyValuePair<BlueprintScriptableObject, AbilityResource> x)
		{
			Restore(x.Key, 0, restoreFull: true);
		});
	}

	public void Spend(BlueprintScriptableObject blueprint, int amount)
	{
		AbilityResource resource = GetResource(blueprint);
		if (resource == null)
		{
			PFLog.Default.Error("Unit {0} has no resource {1}", base.Owner, blueprint);
			return;
		}
		if (resource.Amount < amount)
		{
			PFLog.Default.Error("Unit {0} has no enough resource {1} (has {2}, required {3})", base.Owner, blueprint, resource.Amount, amount);
			amount = resource.Amount;
		}
		resource.Amount -= amount;
		base.EventBus.RaiseEvent(base.Owner, delegate(IAbilityResourceHandler h)
		{
			h.HandleAbilityResourceChange(resource);
		});
	}

	public bool ContainsResource(BlueprintScriptableObject blueprint)
	{
		return GetResource(blueprint) != null;
	}

	public bool HasEnoughResource(BlueprintScriptableObject blueprint, int amount)
	{
		AbilityResource resource = GetResource(blueprint);
		if (resource == null)
		{
			return false;
		}
		return resource.Amount >= amount;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Dictionary<BlueprintScriptableObject, AbilityResource> resources = m_Resources;
		if (resources != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<BlueprintScriptableObject, AbilityResource> item in resources)
			{
				Hash128 hash = default(Hash128);
				Hash128 val3 = SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val3);
				Hash128 val4 = ClassHasher<AbilityResource>.GetHash128(item.Value);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartAbilityResourceCollection source = new PartAbilityResourceCollection();
		result = Unsafe.As<PartAbilityResourceCollection, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartAbilityResourceCollection>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		AbilityResource[] value = PersistantResources;
		formatter.Field(0, "PersistantResources", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartAbilityResourceCollection>();
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
				PersistantResources = formatter.ReadPackable<AbilityResource[]>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
