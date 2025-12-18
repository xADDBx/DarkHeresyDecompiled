using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using UnityEngine;

namespace Kingmaker.AreaLogic.SceneControllables;

public class SceneControllablesController : IControllerStop, IController
{
	private Dictionary<string, ControllableComponent> m_Components = new Dictionary<string, ControllableComponent>();

	private SceneControllablesState m_CurrentState;

	private const string STATE_PREFIX = "scene_controllables_";

	private SceneControllablesState CurrentState
	{
		get
		{
			if (m_CurrentState == null || m_CurrentState.UniqueId != StateName)
			{
				InitState();
			}
			return m_CurrentState;
		}
		set
		{
			m_CurrentState = value;
		}
	}

	private string StateName => "scene_controllables_" + Game.Instance.LoadedAreaState.MainState.SceneName;

	public IEnumerable<ControllableComponent> AllControllables => m_Components.Values;

	public bool TryGetControllable(string idOfObject, out ControllableComponent controllableComponent)
	{
		return m_Components.TryGetValue(idOfObject, out controllableComponent);
	}

	public void RegisterControllable(ControllableComponent controllable)
	{
		if (m_Components.TryGetValue(controllable.UniqueId, out var value) && value != null)
		{
			if (value == controllable)
			{
				return;
			}
			controllable.Reset();
		}
		m_Components[controllable.UniqueId] = controllable;
		if (CurrentState.TryGetValue(controllable.UniqueId, out var state))
		{
			controllable.SetState(state);
			return;
		}
		controllable.SetDefaultState();
		ControllableState state2 = controllable.GetState() ?? new ControllableState
		{
			Active = controllable.gameObject.activeSelf
		};
		SetState(controllable.UniqueId, state2);
	}

	public void UnregisterControllable(ControllableComponent controllable)
	{
		m_Components.Remove(controllable.UniqueId);
	}

	public void SetState(string idOfObject, ControllableState state)
	{
		if (!Game.Instance.Controllers.SceneControllables.TryGetState(idOfObject, out var state2))
		{
			state2 = new ControllableState();
		}
		state2.MergeWith(state);
		if (!m_Components.TryGetValue(idOfObject, out var value))
		{
			PFLog.Entity.Warning("Cant find controllable with id " + idOfObject);
		}
		else
		{
			value.SetState(state);
		}
		CurrentState.SetState(idOfObject, state2);
	}

	public bool TryGetState(string idOfObject, out ControllableState state)
	{
		state = null;
		if (CurrentState == null)
		{
			return false;
		}
		if (CurrentState.TryGetValue(idOfObject, out state))
		{
			return true;
		}
		return false;
	}

	public void Rescan()
	{
		InitState();
		ControllableComponent[] array = Object.FindObjectsOfType<ControllableComponent>(includeInactive: true);
		foreach (ControllableComponent controllable in array)
		{
			RegisterControllable(controllable);
		}
	}

	private void InitState()
	{
		string stateName = StateName;
		SceneControllablesState sceneControllablesState = EntityService.Instance.GetEntity<SceneControllablesState>(stateName);
		if (sceneControllablesState == null)
		{
			SceneControllablesState sceneControllablesState2 = new SceneControllablesState(stateName, isInGame: true);
			Game.Instance.LoadedAreaState.MainState.AddEntityData(sceneControllablesState2);
			sceneControllablesState = Entity.Initialize(sceneControllablesState2);
		}
		CurrentState = sceneControllablesState;
	}

	public void OnStop()
	{
		m_Components.Clear();
	}
}
