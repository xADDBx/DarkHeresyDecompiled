using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Collisions;
using Owlcat.Runtime.Visual.XPBD.Solvers;
using Owlcat.Runtime.Visual.XPBD.Stats;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Owlcat.Runtime.Visual.XPBD;

public static class XPBD
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct PreFixedUpdate
	{
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct PostLateUpdate
	{
	}

	public const float kMemoryIncreaseScale = 1.5f;

	public const float kFloatEpsilon = 1E-06f;

	private static XPBDConfig s_Config;

	private static Simulation s_Simulation;

	internal static bool IsLayoutPreviewStageOpened;

	public static XPBDConfig Config => s_Config;

	public static Solver Solver => s_Simulation.Solver;

	public static bool IsEmpty
	{
		get
		{
			if (s_Simulation == null)
			{
				return true;
			}
			return s_Simulation.Solver.BodyAllocator.IsEmpty;
		}
	}

	static XPBD()
	{
		s_Config = XPBDConfig.Instance;
	}

	private static void Dispose()
	{
		if (s_Simulation != null)
		{
			s_Simulation.Dispose();
			s_Simulation = null;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void CreateSimulation()
	{
		if (s_Simulation != null)
		{
			return;
		}
		s_Config = XPBDConfig.Instance;
		if ((bool)s_Config)
		{
			if (s_Config.SimulationSettings.Backend != Backend.Empty)
			{
				s_Simulation = new Simulation(s_Config);
			}
		}
		else
		{
			UnityEngine.Debug.LogWarning("There is no XBPDConfig. Please create one with default filename in 'Resources' foulder");
		}
	}

	private static void GeneratePlayerLoopLog()
	{
		PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
		Stack<PlayerLoopSystem> stack = new Stack<PlayerLoopSystem>();
		stack.Push(currentPlayerLoop);
		StringBuilder stringBuilder = new StringBuilder();
		while (stack.Count > 0)
		{
			PlayerLoopSystem playerLoopSystem = stack.Pop();
			if (playerLoopSystem.type != null)
			{
				stringBuilder.AppendLine(playerLoopSystem.type.ToString());
			}
			else
			{
				stringBuilder.AppendLine(playerLoopSystem.GetType().ToString());
			}
			if (playerLoopSystem.subSystemList != null)
			{
				PlayerLoopSystem[] subSystemList = playerLoopSystem.subSystemList;
				foreach (PlayerLoopSystem item in subSystemList)
				{
					stack.Push(item);
				}
			}
		}
		UnityEngine.Debug.Log(stringBuilder.ToString());
	}

	private static void OnApplicationQuit()
	{
		Dispose();
	}

	internal static void RegisterAuthoring(AuthoringBase authoring)
	{
		s_Simulation?.RegisterAuthoring(authoring);
	}

	internal static void UnregisterAuthoring(AuthoringBase authoring)
	{
		s_Simulation?.UnregisterAuthoring(authoring);
	}

	internal static void SyncAuthoringEnabledState(AuthoringBase authoringBase)
	{
		s_Simulation?.SyncAuthoringEnabledState(authoringBase);
	}

	internal static void ResetAuthoring(AuthoringBase authoring)
	{
		s_Simulation?.ResetAuthoring(authoring);
	}

	internal static void RegisterParticleAttachment(ParticleAttachment particleAttachment)
	{
		s_Simulation?.RegisterParticleAttachment(particleAttachment);
	}

	internal static void UnregisterParticleAttachment(ParticleAttachment particleAttachment)
	{
		s_Simulation?.UnregisterParticleAttachment(particleAttachment);
	}

	internal static void RegisterCollider(XpbdCollider xpbdCollider)
	{
		s_Simulation?.RegisterCollider(xpbdCollider);
	}

	internal static void UnregisterCollider(XpbdCollider xpbdCollider)
	{
		s_Simulation?.UnregisterCollider(xpbdCollider);
	}

	internal static void RegisterMeshDeformer(MeshDeformer meshDeformer)
	{
		s_Simulation?.RegisterMeshDeformer(meshDeformer);
	}

	internal static void UnregisterMeshDeformer(MeshDeformer meshDeformer)
	{
		s_Simulation?.UnregisterMeshDeformer(meshDeformer);
	}

	internal static void GetColliderDesc(XpbdCollider xpbdCollider, out ColliderDescriptor desc)
	{
		desc = default(ColliderDescriptor);
		s_Simulation?.GetColliderDesc(xpbdCollider, out desc);
	}

	public static MainStats GetStats()
	{
		MainStats result = default(MainStats);
		if (s_Simulation != null)
		{
			result.MemoryStats = s_Simulation.Solver.GetMemoryStats();
			result.BroadphaseStats = s_Simulation.Solver.BroadphaseImpl.GetStats();
		}
		return result;
	}

	public static string GetBodyAllocatorStats()
	{
		if (s_Simulation != null)
		{
			return s_Simulation.Solver.BodyAllocator.GetDetailedMemoryStats();
		}
		return string.Empty;
	}

	public static void TickByScript()
	{
		s_Simulation?.TickByScript();
	}
}
