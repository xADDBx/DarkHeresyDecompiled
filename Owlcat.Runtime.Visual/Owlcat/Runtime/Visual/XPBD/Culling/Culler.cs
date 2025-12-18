using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.XPBD.Culling.Jobs;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Owlcat.Runtime.Visual.XPBD.Solvers;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Culling;

public class Culler : IDisposable
{
	private NativeArray<int> m_BodyVisibility;

	private NativeList<int> m_VisibleBodyIndices;

	private NativeList<float4x4> m_CameraMatrices;

	private HashSet<Camera> m_Cameras = new HashSet<Camera>();

	private Solver m_Solver;

	public NativeArray<int> BodyVisibility => m_BodyVisibility;

	public NativeList<int> VisibleBodyIndices => m_VisibleBodyIndices;

	public Culler(Solver solver)
	{
		m_Solver = solver;
		m_BodyVisibility = new NativeArray<int>(16, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_VisibleBodyIndices = new NativeList<int>(Allocator.Persistent);
		m_CameraMatrices = new NativeList<float4x4>(Allocator.Persistent);
	}

	public void Dispose()
	{
		if (m_BodyVisibility.IsCreated)
		{
			m_BodyVisibility.Dispose();
		}
		if (m_VisibleBodyIndices.IsCreated)
		{
			m_VisibleBodyIndices.Dispose();
		}
		if (m_CameraMatrices.IsCreated)
		{
			m_CameraMatrices.Dispose();
		}
	}

	public void Cull()
	{
		ResizeIfNeed(m_Solver.BodyAllocator);
		UpdateCameraMatrices();
		JobHandle inputDep = ClearVisibilityData(m_Solver.BodyAllocator, default(JobHandle));
		if (m_Solver.Config.SimulationSettings.CameraCullingEnabled)
		{
			inputDep = DoCulling(m_Solver.BodyAllocator, inputDep);
		}
		inputDep.Complete();
	}

	private JobHandle DoCulling(BodyAllocator bodyAllocator, JobHandle inputDep)
	{
		CullBodiesJob cullBodiesJob = default(CullBodiesJob);
		cullBodiesJob.BodyVisibility = m_BodyVisibility;
		cullBodiesJob.VisibleBodyIndices = m_VisibleBodyIndices.AsParallelWriter();
		cullBodiesJob.BodyIndicesMap = bodyAllocator.IndicesMap;
		cullBodiesJob.BodyAabbs = bodyAllocator.BodyDescriptorSoA.Aabb;
		cullBodiesJob.BodyEnabled = bodyAllocator.BodyDescriptorSoA.Enabled;
		cullBodiesJob.CameraMatrices = m_CameraMatrices;
		CullBodiesJob jobData = cullBodiesJob;
		inputDep = IJobParallelForExtensions.ScheduleByRef(ref jobData, bodyAllocator.EntityAllocationMap.Count, 16, inputDep);
		return inputDep;
	}

	private JobHandle ClearVisibilityData(BodyAllocator bodyAllocator, JobHandle inputDep)
	{
		if (!m_Solver.Config.SimulationSettings.CameraCullingEnabled)
		{
			m_VisibleBodyIndices.AddRange(bodyAllocator.IndicesMap);
		}
		ClearCullingDataJob clearCullingDataJob = default(ClearCullingDataJob);
		clearCullingDataJob.CullingEnabled = m_Solver.Config.SimulationSettings.CameraCullingEnabled;
		clearCullingDataJob.BodyVisibility = m_BodyVisibility;
		ClearCullingDataJob jobData = clearCullingDataJob;
		inputDep = IJobParallelForExtensions.ScheduleByRef(ref jobData, bodyAllocator.BodyDescriptorSoA.Capacity, 16, inputDep);
		return inputDep;
	}

	private void UpdateCameraMatrices()
	{
		m_CameraMatrices.Clear();
		foreach (Camera camera in m_Cameras)
		{
			if (camera != null)
			{
				ref NativeList<float4x4> cameraMatrices = ref m_CameraMatrices;
				float4x4 value = camera.projectionMatrix * camera.worldToCameraMatrix;
				cameraMatrices.Add(in value);
			}
		}
		m_Cameras.Clear();
	}

	private void ResizeIfNeed(BodyAllocator bodyAllocator)
	{
		if (m_BodyVisibility.Length != bodyAllocator.BodyDescriptorSoA.Capacity)
		{
			if (m_BodyVisibility.IsCreated)
			{
				m_BodyVisibility.Dispose();
			}
			m_BodyVisibility = new NativeArray<int>(bodyAllocator.BodyDescriptorSoA.Capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
		if (m_VisibleBodyIndices.Capacity != bodyAllocator.EntityAllocationMap.Count)
		{
			if (m_VisibleBodyIndices.IsCreated)
			{
				m_VisibleBodyIndices.Dispose();
			}
			m_VisibleBodyIndices = new NativeList<int>(bodyAllocator.EntityAllocationMap.Count, Allocator.Persistent);
		}
		m_VisibleBodyIndices.Clear();
	}

	internal bool GetVisibility(int bodyIndex)
	{
		return m_BodyVisibility[bodyIndex] > 0;
	}

	internal void CollectCameras(List<Camera> cameras)
	{
		for (int i = 0; i < cameras.Count; i++)
		{
			Camera camera = cameras[i];
			if (camera != null && CheckCameraType(camera))
			{
				m_Cameras.Add(cameras[i]);
			}
		}
	}

	private bool CheckCameraType(Camera camera)
	{
		if (m_Solver.Config.DebugSettings.UseOnlyGameCameraForCulling)
		{
			return camera.cameraType == CameraType.Game;
		}
		if (camera.cameraType != CameraType.Game)
		{
			return camera.cameraType == CameraType.SceneView;
		}
		return true;
	}
}
