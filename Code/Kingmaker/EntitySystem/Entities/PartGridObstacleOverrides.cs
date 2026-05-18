using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.View.Covers;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.EntitySystem.Entities;

[OwlPackable(OwlPackableMode.Generate)]
public class PartGridObstacleOverrides : EntityPart<DestructibleEntity>, IHashable, IOwlPackable<PartGridObstacleOverrides>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public struct Entry : IOwlPackable, IOwlPackable<Entry>
	{
		[OwlPackInclude]
		public EntityFactRef Source;

		[OwlPackInclude]
		public int ObstacleIndex;

		[OwlPackInclude]
		public LosCalculations.CoverType DowngradedTo;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "Entry",
			Fields = new FieldInfo[3]
			{
				new FieldInfo("Source", typeof(EntityFactRef)),
				new FieldInfo("ObstacleIndex", typeof(int)),
				new FieldInfo("DowngradedTo", typeof(LosCalculations.CoverType))
			}
		};

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			Entry source = default(Entry);
			result = Unsafe.As<Entry, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<Entry>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.Field(0, "Source", ref Source, state);
			formatter.UnmanagedField(1, "ObstacleIndex", ref ObstacleIndex, state);
			formatter.EnumField(2, "DowngradedTo", ref DowngradedTo, state);
			formatter.EndObject();
		}

		public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Entry>();
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
					Source = formatter.ReadPackable<EntityFactRef>(state);
					break;
				case 1:
					ObstacleIndex = formatter.ReadUnmanaged<int>(state);
					break;
				case 2:
					DowngradedTo = formatter.ReadEnum<LosCalculations.CoverType>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[OwlPackInclude]
	private List<Entry> _entries = new List<Entry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartGridObstacleOverrides",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("_entries", typeof(List<Entry>))
		}
	};

	public void Add(EntityFact source, int obstacleIndex, LosCalculations.CoverType downgradedTo)
	{
		EntityFactRef entityFactRef = new EntityFactRef(source);
		for (int i = 0; i < _entries.Count; i++)
		{
			Entry value = _entries[i];
			if (value.Source.Equals(entityFactRef) && value.ObstacleIndex == obstacleIndex)
			{
				if (value.DowngradedTo != downgradedTo)
				{
					value.DowngradedTo = downgradedTo;
					_entries[i] = value;
					RecomputeAndApply(obstacleIndex);
				}
				return;
			}
		}
		_entries.Add(new Entry
		{
			Source = entityFactRef,
			ObstacleIndex = obstacleIndex,
			DowngradedTo = downgradedTo
		});
		RecomputeAndApply(obstacleIndex);
	}

	public void Remove(EntityFact source)
	{
		EntityFactRef other = new EntityFactRef(source);
		HashSet<int> value;
		using (CollectionPool<HashSet<int>, int>.Get(out value))
		{
			for (int num = _entries.Count - 1; num >= 0; num--)
			{
				if (_entries[num].Source.Equals(other))
				{
					value.Add(_entries[num].ObstacleIndex);
					_entries.RemoveAt(num);
				}
			}
			foreach (int item in value)
			{
				RecomputeAndApply(item);
			}
		}
	}

	protected override void OnViewDidAttach()
	{
		HashSet<int> value;
		using (CollectionPool<HashSet<int>, int>.Get(out value))
		{
			foreach (Entry entry in _entries)
			{
				value.Add(entry.ObstacleIndex);
			}
			foreach (int item in value)
			{
				RecomputeAndApply(item);
			}
		}
	}

	private void RecomputeAndApply(int obstacleIndex)
	{
		GridObstacle[] array = base.Owner?.AllGridObstacles;
		if (array == null || obstacleIndex < 0 || obstacleIndex >= array.Length)
		{
			return;
		}
		GridObstacle gridObstacle = array[obstacleIndex];
		if (gridObstacle == null)
		{
			return;
		}
		bool flag = false;
		LosCalculations.CoverType coverType = LosCalculations.CoverType.Obstacle;
		foreach (Entry entry in _entries)
		{
			if (entry.ObstacleIndex == obstacleIndex && (!flag || entry.DowngradedTo < coverType))
			{
				coverType = entry.DowngradedTo;
				flag = true;
			}
		}
		if (!flag)
		{
			gridObstacle.RestoreOriginal();
		}
		else
		{
			gridObstacle.Override(coverType, coverType);
		}
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
		PartGridObstacleOverrides source = new PartGridObstacleOverrides();
		result = Unsafe.As<PartGridObstacleOverrides, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartGridObstacleOverrides>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "_entries", ref _entries, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartGridObstacleOverrides>();
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
				_entries = formatter.ReadPackable<List<Entry>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
