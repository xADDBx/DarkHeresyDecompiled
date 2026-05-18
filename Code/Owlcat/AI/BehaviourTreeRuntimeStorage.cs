using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Owlcat.BehaviourTrees;

namespace Owlcat.AI;

public class BehaviourTreeRuntimeStorage
{
	private readonly BehaviourTreeRuntimeContext m_RuntimeContext = new BehaviourTreeRuntimeContext();

	private readonly Dictionary<MechanicEntity, List<MechanicEntityBehaviourTreeRuntime>> m_AIAgentToRuntime = new Dictionary<MechanicEntity, List<MechanicEntityBehaviourTreeRuntime>>();

	public BehaviourTreeRuntimeStorage()
	{
		UpdateBehaviourTreeConfig();
		BehaviourTreeRuntimeDatabaseWrapper.Initialize(new BlueprintBehaviourTreeRuntimeDatabaseImpl());
		PFLog.AI.Log("Behaviour Tree Tick Controller");
		EnsureBreakpoints();
	}

	private static void UpdateBehaviourTreeConfig()
	{
		BehaviourTreeConfig.TimeProvider = new SimulationBehaviourTreeTimeProvider();
		BehaviourTreeConfig.RandomProvider = new SimulationBehaviourTreeRandomProvider();
		BehaviourTreeConfig.Logger = new BehaviourTreeLogger();
		BehaviourTreeConfig.BreakpointHandlingType = BreakpointHandlingType.Immediate;
		BehaviourTreeConfig.Features.LogicalBranchesEnabled = true;
		BehaviourTreeConfig.Features.ConditionAbortEnabled = false;
	}

	public bool TryGetRuntime(MechanicEntity agent, out MechanicEntityBehaviourTreeRuntime runtime)
	{
		object obj;
		if (!m_AIAgentToRuntime.TryGetValue(agent, out var value))
		{
			obj = null;
		}
		else
		{
			List<MechanicEntityBehaviourTreeRuntime> list = value;
			obj = list[list.Count - 1];
		}
		runtime = (MechanicEntityBehaviourTreeRuntime)obj;
		return runtime != null;
	}

	public void OnReset()
	{
		m_AIAgentToRuntime.Clear();
	}

	private void EnsureBreakpoints()
	{
	}

	public void Register(MechanicEntity agent, IParameterizedBehaviourTreeProvider provider)
	{
		EnsureBreakpoints();
		try
		{
			MechanicEntityBehaviourTreeRuntime mechanicEntityBehaviourTreeRuntime = new MechanicEntityBehaviourTreeRuntime(m_RuntimeContext, agent, provider);
			if (!m_AIAgentToRuntime.TryGetValue(mechanicEntityBehaviourTreeRuntime.Agent, out var value))
			{
				value = new List<MechanicEntityBehaviourTreeRuntime>();
				m_AIAgentToRuntime.Add(mechanicEntityBehaviourTreeRuntime.Agent, value);
			}
			value.Add(mechanicEntityBehaviourTreeRuntime);
			mechanicEntityBehaviourTreeRuntime.RuntimeBridge.SetAgentVariable(mechanicEntityBehaviourTreeRuntime.Agent);
			mechanicEntityBehaviourTreeRuntime.RuntimeBridge.SetRuntimeInternalDataVariable(mechanicEntityBehaviourTreeRuntime.RuntimeInternalData);
			mechanicEntityBehaviourTreeRuntime.RestoreRuntimeState();
		}
		catch (Exception ex)
		{
			PFLog.AI.Exception(ex, "Failed to create BehaviourTree runtime: " + provider.BehaviourTree.Title);
		}
	}

	public void RegisterViewForDebug(MechanicEntity agent, IBehaviourTreeProvider provider)
	{
	}

	public void UnRegister(MechanicEntity agent, IBehaviourTreeProvider provider)
	{
		if (!m_AIAgentToRuntime.TryGetValue(agent, out var value))
		{
			return;
		}
		MechanicEntityBehaviourTreeRuntime runtime = value.FirstOrDefault((MechanicEntityBehaviourTreeRuntime r) => r.Provider == provider);
		if (runtime != null)
		{
			m_RuntimeContext.Delete(runtime.RuntimeBridge);
			value.Remove(runtime);
			if (TryGetRuntime(agent, out runtime))
			{
				RegisterViewForDebug(agent, runtime.Provider);
			}
			else
			{
				m_AIAgentToRuntime.Remove(agent);
			}
		}
	}
}
