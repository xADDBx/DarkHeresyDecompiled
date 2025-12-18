using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Networking.Serialization;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.EntitySystem;

[HashRoot]
[OwlPackable(OwlPackableMode.Generate)]
public class SceneEntitiesState : IHashable, IOwlPackable, IOwlPackable<SceneEntitiesState>
{
	public class DisposeInProgress : ContextFlag<DisposeInProgress>
	{
	}

	public const int CycleLengthA = 5;

	public const int CycleLengthB = 8;

	public const int CompleteCycleLength = 40;

	[JsonProperty]
	[OwlPackInclude]
	public string SceneName;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SceneEntitiesState",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("SceneName", typeof(string)),
			new FieldInfo("HasEntityData", typeof(bool)),
			new FieldInfo("m_EntityData", typeof(List<Entity>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public bool HasEntityData { get; set; }

	[OwlPackInclude]
	private List<Entity> m_EntityData { get; set; } = new List<Entity>();


	[JsonProperty(PropertyName = "m_EntityData")]
	private List<Entity> EntityData
	{
		get
		{
			if (ContextData<GameStateSerializationContext>.Current != null)
			{
				if (!ContextData<GameStateSerializationContext>.Current.SplitState)
				{
					return m_EntityData;
				}
				int count = m_EntityData.Count;
				int num = count / 8 + 1;
				int num2 = Game.Instance.RealTimeController.CurrentNetworkTick / 5 % 8 * num;
				int num3 = Mathf.Min(num2 + num, count);
				List<Entity> list = TempList.Get<Entity>();
				list.IncreaseCapacity(num3 - num2);
				for (int i = num2; i < num3; i++)
				{
					Entity item = m_EntityData[i];
					list.Add(item);
				}
				return list;
			}
			return m_EntityData;
		}
	}

	public bool SkipSerialize { get; set; }

	public List<Entity> AllEntityData => m_EntityData;

	public bool IsSceneLoaded => SceneManager.GetSceneByName(SceneName).isLoaded;

	public bool IsSceneLoadedThreadSafe { get; set; }

	public bool IsPrePostLoadExecuted { get; private set; }

	public bool IsPostLoadExecuted { get; private set; }

	public static event Action<SceneEntitiesState, Entity> OnAdded;

	public static event Action<SceneEntitiesState, Entity> OnRemoved;

	public static void ClearSubscriptions()
	{
		SceneEntitiesState.OnAdded = null;
		SceneEntitiesState.OnRemoved = null;
	}

	public SceneEntitiesState(string sceneName)
	{
		SceneName = sceneName;
		IsPostLoadExecuted = true;
		IsPrePostLoadExecuted = true;
	}

	[UsedImplicitly]
	private SceneEntitiesState(JsonConstructorMark _)
	{
	}

	private SceneEntitiesState()
	{
	}

	public void Dispose()
	{
		using (ContextData<DisposeInProgress>.Request())
		{
			foreach (Entity item in AllEntityData.ToList())
			{
				if (item.HoldingState == this)
				{
					RemoveEntityData(item);
					item.Dispose();
				}
			}
			IsSceneLoadedThreadSafe = false;
		}
	}

	public void AddEntityData([NotNull] Entity data)
	{
		if (m_EntityData.HasItem((Entity e) => e.UniqueId == data.UniqueId))
		{
			PFLog.Default.Error($"Can't add {data} to state {SceneName}: duplicate id {data.UniqueId}");
			return;
		}
		m_EntityData.Add(data);
		data.SetHoldingState(this);
		SceneEntitiesState.OnAdded?.Invoke(this, data);
	}

	public void RemoveEntityData([NotNull] Entity data)
	{
		data.SetHoldingState(null);
		if (m_EntityData.Remove(data))
		{
			SceneEntitiesState.OnRemoved?.Invoke(this, data);
		}
	}

	public void PrePostLoad()
	{
		if (IsPrePostLoadExecuted)
		{
			return;
		}
		IsPrePostLoadExecuted = true;
		foreach (Entity entityDatum in m_EntityData)
		{
			entityDatum.PrePostLoad();
		}
	}

	public void PostLoad()
	{
		if (IsPostLoadExecuted)
		{
			PFLog.System.Error("AreaPersistentState.PostLoad: already executed");
			return;
		}
		IsPostLoadExecuted = true;
		PrePostLoad();
		foreach (Entity entityDatum in m_EntityData)
		{
			try
			{
				entityDatum.PostLoad();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
				entityDatum.SetHoldingState(null);
			}
		}
		foreach (Entity entityDatum2 in m_EntityData)
		{
			try
			{
				ApplyPostLoadFixes(entityDatum2);
			}
			catch (Exception ex2)
			{
				PFLog.Default.Exception(ex2);
				entityDatum2.SetHoldingState(null);
			}
		}
		foreach (Entity entityDatum3 in m_EntityData)
		{
			if (entityDatum3 is MechanicEntity { Blueprint: null })
			{
				Game.Instance.Player.BrokenEntities.Add(entityDatum3.UniqueId);
				entityDatum3.SetHoldingState(null);
				UberDebug.LogError("Broken unit in state {0}, id: {1}", SceneName, entityDatum3.UniqueId);
			}
			else
			{
				SceneEntitiesState.OnAdded?.Invoke(this, entityDatum3);
				entityDatum3.SetHoldingState(this);
			}
		}
	}

	private void ApplyPostLoadFixes(Entity data)
	{
		data.ApplyPostLoadFixes();
	}

	public void PreSave()
	{
		foreach (Entity entityDatum in m_EntityData)
		{
			try
			{
				entityDatum.PreSave();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
	}

	public void Subscribe()
	{
		foreach (Entity entityDatum in m_EntityData)
		{
			try
			{
				entityDatum.Subscribe();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
	}

	public void Unsubscribe()
	{
		foreach (Entity entityDatum in m_EntityData)
		{
			try
			{
				entityDatum.Unsubscribe();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
	}

	public override string ToString()
	{
		return GetType().Name + "#" + SceneName;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(SceneName);
		bool val = HasEntityData;
		result.Append(ref val);
		List<Entity> entityData = EntityData;
		if (entityData != null)
		{
			for (int i = 0; i < entityData.Count; i++)
			{
				Hash128 val2 = ClassHasher<Entity>.GetHash128(entityData[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SceneEntitiesState source = new SceneEntitiesState();
		result = Unsafe.As<SceneEntitiesState, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SceneEntitiesState>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "SceneName", ref SceneName, state);
		bool value = HasEntityData;
		formatter.UnmanagedField(1, "HasEntityData", ref value, state);
		List<Entity> value2 = m_EntityData;
		formatter.Field(2, "m_EntityData", ref value2, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SceneEntitiesState>();
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
				SceneName = formatter.ReadString(state);
				break;
			case 1:
				HasEntityData = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_EntityData = formatter.ReadPackable<List<Entity>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
