using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Networking.Serialization;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities.Base;

[OwlPackable(OwlPackableMode.Generate)]
public class EntityPartsManager : IDisposable, IHashable, IOwlPackable, IOwlPackable<EntityPartsManager>
{
	public class PartsGameStateAdapter : JsonConverter<List<EntityPart>>
	{
		public override bool CanRead => false;

		public override List<EntityPart> ReadJson(JsonReader reader, Type objectType, List<EntityPart> existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override void WriteJson(JsonWriter writer, List<EntityPart> value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}
			writer.WriteStartArray();
			foreach (EntityPart item in value.Where(IsItemValid))
			{
				serializer.Serialize(writer, item);
			}
			writer.WriteEndArray();
		}

		public static Hash128 GetHash128(List<EntityPart> obj)
		{
			Hash128 result = default(Hash128);
			if (obj == null)
			{
				return result;
			}
			foreach (EntityPart item in obj)
			{
				if (IsItemValid(item))
				{
					Hash128 val = ClassHasher<EntityPart>.GetHash128(item);
					result.Append(ref val);
				}
			}
			return result;
		}

		private static bool IsItemValid(EntityPart obj)
		{
			if (obj != null)
			{
				return !(obj is PartUnitUISettings);
			}
			return false;
		}
	}

	public struct PartsByTypeEnumerator<TPart> : IEnumerator<TPart>, IEnumerator, IDisposable where TPart : class
	{
		private List<EntityPart>.Enumerator m_ListEnumerator;

		public TPart Current => m_ListEnumerator.Current as TPart;

		object IEnumerator.Current
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public PartsByTypeEnumerator(List<EntityPart> parts)
		{
			m_ListEnumerator = parts.GetEnumerator();
		}

		public void Dispose()
		{
			m_ListEnumerator.Dispose();
		}

		public bool MoveNext()
		{
			m_ListEnumerator.MoveNext();
			while (!(m_ListEnumerator.Current is TPart))
			{
				if (!m_ListEnumerator.MoveNext())
				{
					return false;
				}
			}
			return true;
		}

		public void Reset()
		{
		}
	}

	public struct PartsByTypeEnumerable<TPart> : IEnumerable<TPart>, IEnumerable where TPart : class
	{
		private readonly List<EntityPart> m_Parts;

		public PartsByTypeEnumerable(List<EntityPart> parts)
		{
			m_Parts = parts;
		}

		public PartsByTypeEnumerator<TPart> GetEnumerator()
		{
			return new PartsByTypeEnumerator<TPart>(m_Parts);
		}

		IEnumerator<TPart> IEnumerable<TPart>.GetEnumerator()
		{
			return new PartsByTypeEnumerator<TPart>(m_Parts);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException("Iteration over parts by type via IEnumerator is not supported, use IEnumerator<T>");
		}
	}

	private readonly struct ProhibitAddAndRemove : IDisposable
	{
		private readonly EntityPartsManager _partsManager;

		public ProhibitAddAndRemove(EntityPartsManager partsManager)
		{
			_partsManager = partsManager;
			_partsManager.m_IsAddingAndRemovingProhibited++;
		}

		public void Dispose()
		{
			_partsManager.m_IsAddingAndRemovingProhibited--;
		}
	}

	private static int TotalPartsCount;

	private static Dictionary<Type, int> Indices;

	[ItemCanBeNull]
	private readonly EntityPart[] m_PartsCache = new EntityPart[TotalPartsCount];

	[ItemNotNull]
	[GameStateInclude]
	[HasherCustom(Type = typeof(PartsGameStateAdapter))]
	private readonly List<EntityPart> m_Parts = new List<EntityPart>();

	private int m_IsAddingAndRemovingProhibited;

	public static readonly OwlPack.Runtime.TypeInfo OwlPackTypeInfo;

	[JsonProperty]
	[UsedImplicitly]
	[GameStateIgnore("This is a runtime adapter for m_Parts")]
	[OwlPackInclude]
	private EntityPart[] Container
	{
		get
		{
			return m_Parts?.ToArray() ?? Array.Empty<EntityPart>();
		}
		set
		{
			m_Parts.Clear();
			m_Parts.AddRange(value);
			foreach (EntityPart entityPart in value)
			{
				int index = GetIndex(entityPart.GetType());
				m_PartsCache[index] = entityPart;
			}
		}
	}

	public Entity Owner { get; private set; }

	private bool IsAddingAndRemovingProhibited => m_IsAddingAndRemovingProhibited > 0;

	[CanBeNull]
	private IEntityPartsManagerDelegate Delegate => Owner as IEntityPartsManagerDelegate;

	static EntityPartsManager()
	{
		OwlPackTypeInfo = new OwlPack.Runtime.TypeInfo
		{
			Name = "EntityPartsManager",
			OldNames = null,
			Fields = new OwlPack.Runtime.FieldInfo[1]
			{
				new OwlPack.Runtime.FieldInfo("Container", typeof(EntityPart[]))
			}
		};
		Type[] array = (from i in typeof(EntityPart).GetSubclasses()
			where i.GetConstructor(Type.EmptyTypes) != null
			select i into t
			orderby t.FullName
			select t).ToArray();
		TotalPartsCount = array.Length;
		Indices = new Dictionary<Type, int>(TotalPartsCount);
		IEnumerable<Type> enumerable = array.Where((Type i) => i.ContainsGenericParameters);
		if (enumerable.Any())
		{
			string text = string.Join(", ", enumerable);
			throw new Exception("Non abstract EntityParts with generic parameters are forbidden: " + text);
		}
		for (int j = 0; j < array.Length; j++)
		{
			Type type = array[j];
			typeof(Indexer<>).MakeGenericType(type).GetField("Index", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, j);
			Indices.Add(type, j);
		}
	}

	private static int GetIndex(Type type)
	{
		return Indices.Get(type, -1);
	}

	public EntityPartsManager(Entity owner)
	{
		Owner = owner;
	}

	private EntityPartsManager()
	{
	}

	[JsonConstructor]
	private EntityPartsManager(JsonConstructorMark _)
	{
	}

	[CanBeNull]
	public TPart GetOptional<TPart>() where TPart : EntityPart, new()
	{
		return Get<TPart>();
	}

	[CanBeNull]
	public TPart GetOptional<TPart>(Type type) where TPart : EntityPart
	{
		return Get<TPart>(type);
	}

	[NotNull]
	public TPart GetRequired<TPart>() where TPart : EntityPart, new()
	{
		return GetOptional<TPart>() ?? throw new Exception($"{Owner}: missing required part {typeof(TPart).Name}");
	}

	[NotNull]
	public TPart GetOrCreate<TPart>() where TPart : EntityPart, new()
	{
		return GetOptional<TPart>() ?? Add<TPart>();
	}

	public PartsByTypeEnumerable<TPart> GetAll<TPart>() where TPart : class
	{
		return new PartsByTypeEnumerable<TPart>(m_Parts);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CanBeNull]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	[Il2CppSetOption(Option.NullChecks, false)]
	private TPart Get<TPart>() where TPart : EntityPart, new()
	{
		EntityPart[] partsCache = m_PartsCache;
		int index = Indexer<TPart>.Index;
		return Unsafe.As<TPart>(partsCache[index]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CanBeNull]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	[Il2CppSetOption(Option.NullChecks, false)]
	private TPart Get<TPart>(Type type) where TPart : EntityPart
	{
		int index = GetIndex(type);
		if (index == -1)
		{
			return null;
		}
		return Unsafe.As<TPart>(m_PartsCache[index]);
	}

	private TPart Add<TPart>() where TPart : EntityPart, new()
	{
		if (IsAddingAndRemovingProhibited)
		{
			throw new InvalidOperationException("Can't add parts now");
		}
		if (Get<TPart>() != null)
		{
			throw new Exception($"{Owner}: part {typeof(TPart).Name} already exists");
		}
		if (Owner.ForbidFactsAndPartsModifications && Owner.IsInitialized)
		{
			throw new Exception($"Can't add part to constant entity {Owner}");
		}
		TPart val = new TPart();
		m_Parts.Add(val);
		m_PartsCache[Indexer<TPart>.Index] = val;
		try
		{
			Delegate?.OnPartAppears(val);
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
		}
		val.Attach(Owner);
		return val;
	}

	public void Remove<TPart>() where TPart : EntityPart, new()
	{
		TPart val = Get<TPart>();
		if (val != null)
		{
			Remove(val);
		}
	}

	public void Remove([NotNull] EntityPart part)
	{
		if (IsAddingAndRemovingProhibited)
		{
			throw new InvalidOperationException("Can't remove parts now");
		}
		int index = GetIndex(part.GetType());
		if (m_PartsCache[index] != part)
		{
			throw new Exception($"{Owner}: part {part.GetType().Name} isn't owned by EntityPartsManager");
		}
		part.Detach();
		m_Parts.Remove(part);
		m_PartsCache[index] = null;
		try
		{
			Delegate?.OnPartDisappears(part);
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
		}
	}

	public void RemoveAll<TPart>(Predicate<TPart> pred) where TPart : EntityPart
	{
		List<TPart> list = TempList.Get<TPart>();
		foreach (EntityPart part in m_Parts)
		{
			if (part is TPart val && pred(val))
			{
				list.Add(val);
			}
		}
		foreach (TPart item in list)
		{
			Remove(item);
		}
	}

	public void RemoveAll<TPart>() where TPart : EntityPart
	{
		RemoveAll((TPart _) => true);
	}

	public void ViewDidAttach()
	{
		using (new ProhibitAddAndRemove(this))
		{
			foreach (EntityPart part in m_Parts)
			{
				part.ViewDidAttach();
			}
		}
	}

	public void ViewWillDetach()
	{
		using (new ProhibitAddAndRemove(this))
		{
			foreach (EntityPart part in m_Parts)
			{
				part.ViewWillDetach();
			}
		}
	}

	public void PreSave()
	{
		using (new ProhibitAddAndRemove(this))
		{
			foreach (EntityPart part in m_Parts)
			{
				part.PreSave();
			}
		}
	}

	public void PrePostLoad(Entity owner)
	{
		using (new ProhibitAddAndRemove(this))
		{
			Owner = owner;
			foreach (EntityPart part in m_Parts)
			{
				part.PrePostLoad(owner);
				try
				{
					Delegate?.OnPartAppears(part);
				}
				catch (Exception ex)
				{
					PFLog.Entity.Exception(ex);
				}
			}
		}
	}

	public void PostLoad()
	{
		using (new ProhibitAddAndRemove(this))
		{
			foreach (EntityPart part in m_Parts)
			{
				part.PostLoad();
			}
		}
	}

	public void DidPostLoad()
	{
		using (new ProhibitAddAndRemove(this))
		{
			foreach (EntityPart part in m_Parts)
			{
				part.DidPostLoad();
			}
		}
	}

	public void ApplyPostLoadFixes()
	{
		foreach (EntityPart item in m_Parts.ToTempList())
		{
			item.ApplyPostLoadFixes();
		}
	}

	public void Subscribe()
	{
		using (new ProhibitAddAndRemove(this))
		{
			foreach (EntityPart part in m_Parts)
			{
				part.Subscribe();
			}
		}
	}

	public void Unsubscribe()
	{
		using (new ProhibitAddAndRemove(this))
		{
			foreach (EntityPart part in m_Parts)
			{
				part.Unsubscribe();
			}
		}
	}

	public void OnHoldingStateChanged()
	{
		using (new ProhibitAddAndRemove(this))
		{
			foreach (EntityPart part in m_Parts)
			{
				part.HoldingStateChanged();
			}
		}
	}

	public void AreaLoadingComplete()
	{
		using (new ProhibitAddAndRemove(this))
		{
			foreach (EntityPart part in m_Parts)
			{
				part.AreaLoadingComplete();
			}
		}
	}

	public void Dispose()
	{
		List<EntityPart> list = m_Parts.ToTempList();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Remove(list[num]);
		}
		m_Parts.Clear();
		Array.Fill(m_PartsCache, null);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = PartsGameStateAdapter.GetHash128(m_Parts);
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		EntityPartsManager source = new EntityPartsManager();
		result = Unsafe.As<EntityPartsManager, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<EntityPartsManager>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		EntityPart[] value = Container;
		formatter.Field(0, "Container", ref value, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		OwlPack.Runtime.TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EntityPartsManager>();
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
				Container = formatter.ReadPackable<EntityPart[]>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
