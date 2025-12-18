using System.Collections.Generic;
using System.Linq;
using Code.GameCore.Mics;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Networking.Settings;
using Newtonsoft.Json;

namespace Kingmaker.EntitySystem;

public class PersistentState : IPersistentState, InterfaceService
{
	public readonly List<AreaPersistentState> SavedAreaStates = new List<AreaPersistentState>();

	public AreaPersistentState LoadedAreaState { get; set; }

	public SceneEntitiesState CrossSceneState { get; set; }

	public SceneEntitiesState PlayerState { get; set; }

	public InGameSettings InGameSettings { get; set; }

	public CoopData CoopData { get; set; }

	public GameStatistic Statistic { get; set; }

	[JsonConstructor]
	public PersistentState(JsonConstructorMark _)
	{
	}

	public PersistentState()
	{
		Reset();
	}

	public AreaPersistentState GetStateForArea(BlueprintArea area)
	{
		AreaPersistentState areaPersistentState = SavedAreaStates.SingleOrDefault((AreaPersistentState s) => s.Blueprint == area);
		if (areaPersistentState == null)
		{
			SavedAreaStates.Add(areaPersistentState = new AreaPersistentState(area));
		}
		return areaPersistentState;
	}

	public IEntity GetEntityDataFromLoadedAreaState(string uniqueID)
	{
		return LoadedAreaState.AllEntityData.SingleOrDefault((Entity e) => e.UniqueId == uniqueID);
	}

	public void Reset()
	{
		foreach (AreaPersistentState savedAreaState in SavedAreaStates)
		{
			savedAreaState.Dispose();
		}
		SavedAreaStates.Clear();
		CrossSceneState?.Dispose();
		PlayerState?.Dispose();
		CrossSceneState = new SceneEntitiesState("cross-scene");
		PlayerState = new SceneEntitiesState("player");
		InGameSettings = new InGameSettings();
		CoopData = new CoopData();
		Statistic = new GameStatistic();
		LoadedAreaState = null;
	}
}
