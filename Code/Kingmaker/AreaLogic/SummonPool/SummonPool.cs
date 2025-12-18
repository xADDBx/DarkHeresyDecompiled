using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.SummonPool;

public class SummonPool : ISummonPool
{
	private class SuppressEvents : ContextFlag<SuppressEvents>
	{
	}

	[OwlPackable(OwlPackableMode.Generate)]
	public class PooledPart : EntityPart<AbstractUnitEntity>, IHashable, IOwlPackable<PooledPart>
	{
		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "PooledPart",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("Pools", typeof(List<BlueprintSummonPool>))
			}
		};

		[JsonProperty]
		[OwlPackInclude]
		public List<BlueprintSummonPool> Pools { get; private set; } = new List<BlueprintSummonPool>();


		protected override void OnPostLoad()
		{
			foreach (BlueprintSummonPool pool in Pools)
			{
				Game.Instance.SummonPools.Register(pool, base.Owner);
			}
		}

		protected override void OnDetach()
		{
			using (ContextData<SuppressEvents>.Request())
			{
				foreach (BlueprintSummonPool item in Pools.ToTempList())
				{
					Game.Instance.SummonPools.Unregister(item, base.Owner, keepReference: true);
				}
			}
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			List<BlueprintSummonPool> pools = Pools;
			if (pools != null)
			{
				for (int i = 0; i < pools.Count; i++)
				{
					Hash128 val2 = SimpleBlueprintHasher.GetHash128(pools[i]);
					result.Append(ref val2);
				}
			}
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			PooledPart source = new PooledPart();
			result = Unsafe.As<PooledPart, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<PooledPart>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			List<BlueprintSummonPool> value = Pools;
			formatter.Field(0, "Pools", ref value, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PooledPart>();
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
					Pools = formatter.ReadPackable<List<BlueprintSummonPool>>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[JsonProperty]
	private readonly BlueprintSummonPool m_Blueprint;

	[JsonProperty]
	private readonly List<EntityRef<AbstractUnitEntity>> m_Units = new List<EntityRef<AbstractUnitEntity>>();

	public BlueprintSummonPool Blueprint => m_Blueprint;

	public int Count => m_Units.Count;

	public IEnumerable<AbstractUnitEntity> Units => m_Units.Select((EntityRef<AbstractUnitEntity> r) => r.Entity);

	public SummonPool(BlueprintSummonPool blueprint)
	{
		m_Blueprint = blueprint;
	}

	private SummonPool(JsonConstructorMark _)
	{
	}

	public int CountWithFactions(IEnumerable<BlueprintFaction> factions)
	{
		return m_Units.Select((EntityRef<AbstractUnitEntity> u) => u.Entity.GetFactionOptional()?.Blueprint).NotNull().Count(factions.Contains<BlueprintFaction>);
	}

	public void Register(AbstractUnitEntity unit)
	{
		if (m_Units.HasItem(unit))
		{
			return;
		}
		m_Units.Add(unit);
		List<BlueprintSummonPool> pools = unit.GetOrCreate<PooledPart>().Pools;
		if (!pools.Contains(Blueprint))
		{
			pools.Add(Blueprint);
		}
		if (!ContextData<SuppressEvents>.Current)
		{
			EventBus.RaiseEvent((IMechanicEntity)unit, (Action<ISummonPoolHandler>)delegate(ISummonPoolHandler l)
			{
				l.HandleUnitAdded(this);
			}, isCheckRuntime: true);
		}
	}

	public void Unregister(AbstractUnitEntity unit, bool keepReference = false)
	{
		if (!m_Units.Remove(unit))
		{
			return;
		}
		if (!keepReference)
		{
			PooledPart optional = unit.GetOptional<PooledPart>();
			optional?.Pools.Remove(Blueprint);
			if (optional != null && optional.Pools.Empty())
			{
				unit.Remove<PooledPart>();
			}
		}
		if ((bool)ContextData<SuppressEvents>.Current)
		{
			return;
		}
		EventBus.RaiseEvent((IMechanicEntity)unit, (Action<ISummonPoolHandler>)delegate(ISummonPoolHandler l)
		{
			l.HandleUnitRemoved(this);
		}, isCheckRuntime: true);
		if (m_Units.Empty())
		{
			EventBus.RaiseEvent((IMechanicEntity)unit, (Action<ISummonPoolHandler>)delegate(ISummonPoolHandler l)
			{
				l.HandleLastUnitRemoved(this);
			}, isCheckRuntime: true);
		}
	}
}
