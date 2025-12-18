using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.AreaLogic;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Code.AreaLogic;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.EntitySystem;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
[HashRoot]
[OwlPackable(OwlPackableMode.Generate)]
public class AreaPersistentState : IHashable, IOwlPackable, IOwlPackable<AreaPersistentState>
{
	private string m_AreaGuid;

	[JsonProperty]
	[OwlPackInclude]
	private Area m_Area;

	[JsonProperty]
	[OwlPackInclude]
	public UnitReference ServiceCaster;

	private readonly List<SceneEntitiesState> m_AddStates = new List<SceneEntitiesState>();

	public readonly SavedFogMasks SavedFogOfWarMasks = new SavedFogMasks();

	public readonly RuntimeAreaSettings Settings = new RuntimeAreaSettings();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AreaPersistentState",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("m_Area", typeof(Area)),
			new FieldInfo("m_MainState", typeof(SceneEntitiesState)),
			new FieldInfo("ServiceCaster", typeof(UnitReference))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	private SceneEntitiesState m_MainState { get; set; }

	public bool ShouldLoad { get; set; }

	public PartVeil AreaVeilPart => m_Area.GetOptional<PartVeil>();

	public IEnumerable<Entity> AllEntityData => GetAllSceneStates().SelectMany((SceneEntitiesState s) => s.AllEntityData);

	public BlueprintArea Blueprint
	{
		get
		{
			if (m_Area != null)
			{
				return m_Area.Blueprint;
			}
			return ResourcesLibrary.TryGetBlueprint<BlueprintArea>(m_AreaGuid);
		}
	}

	[NotNull]
	public Area Area => m_Area;

	public SceneEntitiesState MainState => m_MainState;

	public string AreaGuid => m_AreaGuid;

	public AreaPersistentState([NotNull] BlueprintArea blueprint)
	{
		m_Area = Entity.Initialize(new Area(blueprint));
		m_AreaGuid = blueprint.AssetGuid;
		m_MainState = new SceneEntitiesState(Blueprint.MainStateName);
	}

	public AreaPersistentState(string areaId)
	{
		m_AreaGuid = areaId;
		m_MainState = new SceneEntitiesState("");
	}

	public AreaPersistentState()
	{
	}

	public void RestoreAreaBlueprint()
	{
		if (m_Area == null)
		{
			m_Area = Entity.Initialize(new Area(Blueprint));
		}
		m_MainState.SceneName = Blueprint.DynamicScene.SceneName;
	}

	[UsedImplicitly]
	private AreaPersistentState(JsonConstructorMark _)
	{
	}

	public void Dispose()
	{
		using (ContextData<SceneEntitiesState.DisposeInProgress>.Request())
		{
			foreach (SceneEntitiesState allSceneState in GetAllSceneStates())
			{
				foreach (Entity allEntityDatum in allSceneState.AllEntityData)
				{
					if (allEntityDatum is BaseUnitEntity { IsInCombat: not false } baseUnitEntity)
					{
						baseUnitEntity.CombatState.LeaveCombat();
					}
					(allEntityDatum as CutscenePlayerData)?.Stop();
				}
			}
			m_Area?.Dispose();
			m_MainState.Dispose();
			foreach (SceneEntitiesState addState in m_AddStates)
			{
				if (addState.IsSceneLoaded)
				{
					addState.Dispose();
				}
			}
		}
	}

	[NotNull]
	public SceneEntitiesState GetStateForScene(string sceneName)
	{
		if (sceneName == Area.Blueprint.DynamicScene.SceneName)
		{
			return m_MainState;
		}
		SceneEntitiesState sceneEntitiesState = m_AddStates.SingleOrDefault((SceneEntitiesState s) => s.SceneName == sceneName);
		if (sceneEntitiesState == null)
		{
			sceneEntitiesState = new SceneEntitiesState(sceneName);
			m_AddStates.Add(sceneEntitiesState);
		}
		return sceneEntitiesState;
	}

	[NotNull]
	public SceneEntitiesState GetStateForScene(SceneReference sr)
	{
		return GetStateForScene(sr.SceneName);
	}

	public void PostLoad()
	{
		if (m_Area == null)
		{
			RestoreAreaBlueprint();
		}
		m_MainState.PostLoad();
		foreach (SceneEntitiesState addState in m_AddStates)
		{
			if (addState.IsSceneLoaded)
			{
				addState.PostLoad();
			}
		}
		m_Area.PostLoad();
	}

	public void PreSave()
	{
		m_MainState.PreSave();
		foreach (SceneEntitiesState addState in m_AddStates)
		{
			if (addState.IsSceneLoaded)
			{
				addState.PreSave();
			}
		}
		m_Area.PreSave();
	}

	public void Subscribe()
	{
		m_MainState.Subscribe();
		foreach (SceneEntitiesState addState in m_AddStates)
		{
			if (addState.IsSceneLoaded)
			{
				addState.Subscribe();
			}
		}
		m_Area.Subscribe();
	}

	public void Unsubscribe()
	{
		m_MainState.Unsubscribe();
		foreach (SceneEntitiesState addState in m_AddStates)
		{
			if (addState.IsSceneLoaded)
			{
				addState.Unsubscribe();
			}
		}
		m_Area.Unsubscribe();
	}

	public void Activate()
	{
		if (!m_Area.MainFact.Active)
		{
			m_Area.MainFact.Activate();
			return;
		}
		m_Area.MainFact.CallComponents(delegate(ITriggerOnLoad c)
		{
			c.TriggerOnLoad();
		});
	}

	public void Deactivate()
	{
		if (m_Area.MainFact.Active)
		{
			m_Area.MainFact.Deactivate();
		}
	}

	public IEnumerable<SceneEntitiesState> GetAllSceneStates()
	{
		yield return m_MainState;
		foreach (SceneEntitiesState addState in m_AddStates)
		{
			yield return addState;
		}
	}

	public List<SceneEntitiesState> GetAdditionalSceneStates()
	{
		return m_AddStates;
	}

	public void CollectAllEntities<T>(List<T> result) where T : Entity
	{
		CollectAllEntities(m_MainState, result);
		foreach (SceneEntitiesState addState in m_AddStates)
		{
			CollectAllEntities(addState, result);
		}
	}

	private void CollectAllEntities<T>(SceneEntitiesState sceneState, List<T> result) where T : Entity
	{
		foreach (Entity allEntityDatum in sceneState.AllEntityData)
		{
			if (allEntityDatum is T item)
			{
				result.Add(item);
			}
		}
	}

	public void SetDeserializedSceneState([NotNull] SceneEntitiesState sceneState)
	{
		m_AddStates.RemoveAll((SceneEntitiesState s) => s.SceneName == sceneState.SceneName);
		m_AddStates.Add(sceneState);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ClassHasher<Area>.GetHash128(m_Area);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<SceneEntitiesState>.GetHash128(m_MainState);
		result.Append(ref val2);
		UnitReference obj = ServiceCaster;
		Hash128 val3 = UnitReferenceHasher.GetHash128(ref obj);
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AreaPersistentState source = new AreaPersistentState();
		result = Unsafe.As<AreaPersistentState, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AreaPersistentState>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_Area", ref m_Area, state);
		SceneEntitiesState value = m_MainState;
		formatter.Field(1, "m_MainState", ref value, state);
		formatter.Field(2, "ServiceCaster", ref ServiceCaster, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AreaPersistentState>();
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
				m_Area = formatter.ReadPackable<Area>(state);
				break;
			case 1:
				m_MainState = formatter.ReadPackable<SceneEntitiesState>(state);
				break;
			case 2:
				ServiceCaster = formatter.ReadPackable<UnitReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
