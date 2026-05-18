using System.Collections.Generic;
using Owlcat.Runtime.Visual.XPBD.Solvers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD;

public sealed class Updater
{
	private readonly Solver m_Solver;

	private int m_AccumulatedSteps;

	private bool m_CurrentFrameSimulated;

	private double m_LastUpdateTime;

	private float m_AccumulatedTime;

	private double m_LastSimulationTime;

	public int MaxStepsPerFrame => m_Solver.Config.SimulationSettings.MaxStepsPerFrame;

	public int Substeps => m_Solver.Config.SimulationSettings.Substeps;

	public Updater(Solver solver)
	{
		m_Solver = solver;
	}

	public void FixedUpdate()
	{
		m_AccumulatedSteps++;
	}

	public void PostLateUpdate()
	{
		double timeAsDouble = Time.timeAsDouble;
		m_AccumulatedTime = math.min((float)(timeAsDouble - m_LastUpdateTime), Time.maximumDeltaTime);
		m_LastUpdateTime = timeAsDouble;
	}

	internal void TickBySript()
	{
		if (Time.timeScale <= 0f)
		{
			m_AccumulatedSteps = 1;
		}
		else
		{
			UnityEngine.Debug.LogError("XPBD: TickByScript can be called only in paused state (timeScale = 0)");
		}
	}

	internal void OnBeginRendering(ScriptableRenderContext context, List<Camera> cameras)
	{
		m_Solver.SolverImpl.EnsureRenderBuffersInitialized(context);
		m_Solver.Culler.CollectCameras(cameras);
		int num;
		float stepDelta;
		if (m_Solver.Config.SimulationSettings.TickMode == SimulationTickMode.Relaxed && Time.timeScale > 0f)
		{
			if (m_AccumulatedTime > 0f)
			{
				num = Mathf.CeilToInt(m_AccumulatedTime / Time.fixedDeltaTime);
				stepDelta = m_AccumulatedTime / (float)num;
			}
			else
			{
				num = 0;
				stepDelta = 0f;
			}
		}
		else
		{
			stepDelta = Time.fixedDeltaTime;
			num = m_AccumulatedSteps;
		}
		if (num > 0)
		{
			using (new ProfilingScope(ProfilingSampler.Get(ProfilingId.UpdaterPrepareFrame)))
			{
				m_Solver.PrepareFrame();
			}
			if (CanTick())
			{
				Simulate(context, cameras, stepDelta, num);
			}
		}
		m_AccumulatedSteps = 0;
		m_AccumulatedTime = 0f;
	}

	private void Simulate(ScriptableRenderContext context, List<Camera> cameras, float stepDelta, int simulationSteps)
	{
		float num = stepDelta * (float)simulationSteps;
		float substepTime = stepDelta / (float)Substeps;
		if (MaxStepsPerFrame <= 0)
		{
			return;
		}
		int num2 = math.min(MaxStepsPerFrame, simulationSteps);
		UpdateContext updateContext = default(UpdateContext);
		updateContext.Context = context;
		updateContext.Cameras = cameras;
		updateContext.StepDelta = stepDelta;
		updateContext.SimulationSteps = simulationSteps;
		updateContext.SimulatedTime = num;
		updateContext.SubstepTime = substepTime;
		updateContext.TimeLeft = num;
		updateContext.DeltaTimeBetweenSimulations = (float)(Time.realtimeSinceStartupAsDouble - m_LastSimulationTime);
		UpdateContext updateContext2 = updateContext;
		using (new ProfilingScope(ProfilingSampler.Get(ProfilingId.UpdaterBeginStep)))
		{
			m_Solver.BeginStep(in updateContext2);
		}
		using (new ProfilingScope(ProfilingSampler.Get(ProfilingId.UpdaterStep)))
		{
			for (int i = 0; i < num2; i++)
			{
				updateContext2.StepIndex = i;
				m_Solver.Step(in updateContext2);
				updateContext2.TimeLeft -= stepDelta;
			}
		}
		using (new ProfilingScope(ProfilingSampler.Get(ProfilingId.UpdaterEndStep)))
		{
			m_Solver.EndStep(in updateContext2);
		}
		m_LastSimulationTime = Time.realtimeSinceStartupAsDouble;
	}

	public bool CanTick()
	{
		return m_Solver.BodyAllocator.Allocations.Count > 0;
	}
}
