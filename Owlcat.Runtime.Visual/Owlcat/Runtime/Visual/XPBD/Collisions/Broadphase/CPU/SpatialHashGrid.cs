using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU.Jobs;
using Owlcat.Runtime.Visual.XPBD.Culling;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Owlcat.Runtime.Visual.XPBD.Stats;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase.CPU;

public class SpatialHashGrid : IMemoryCounter
{
	public const int kEmpty = -1;

	public const float kSizeDescretizer = 100f;

	public const float kSpacingScale = 2f;

	public const int kMaxHashtableLookupIterations = 100;

	private int m_HashmapSize;

	private int m_ObjectsCount;

	private NativeArray<int> m_HashmapKeys;

	private NativeArray<int> m_HashmapValues;

	private NativeReference<int> m_ObjectSizeSum;

	private NativeReference<int> m_OccupiedCellsCount;

	private NativeReference<int> m_ActiveContactsCount;

	public NativeArray<int> HashmapKeys => m_HashmapKeys;

	public NativeArray<int> HashmapValues => m_HashmapValues;

	public int HashmapSize => m_HashmapSize;

	public NativeReference<int> ObjectSizeSum => m_ObjectSizeSum;

	public int ObjectsCount => m_ObjectsCount;

	public NativeReference<int> ActiveContactsCount => m_ActiveContactsCount;

	public void Dispose()
	{
		if (m_HashmapKeys.IsCreated)
		{
			m_HashmapKeys.Dispose();
		}
		if (m_HashmapValues.IsCreated)
		{
			m_HashmapValues.Dispose();
		}
		if (m_ObjectSizeSum.IsCreated)
		{
			m_ObjectSizeSum.Dispose();
		}
		if (m_OccupiedCellsCount.IsCreated)
		{
			m_OccupiedCellsCount.Dispose();
		}
		if (m_ActiveContactsCount.IsCreated)
		{
			m_ActiveContactsCount.Dispose();
		}
	}

	internal JobHandle BuildCollidersGrid(JobHandle inputDep, NativeArray<int> aabbMap, NativeArray<Aabb> aabbs, int aabbCount)
	{
		m_ObjectsCount = aabbCount;
		ResizeArraysIfNeed();
		m_ActiveContactsCount.Value = 0;
		inputDep = ClearGrid(inputDep);
		inputDep = CalculateCollidersSpacing(inputDep, aabbMap, aabbs, aabbCount);
		inputDep = BuildGridForColliders(inputDep, aabbMap, aabbs);
		inputDep = CalculateLoadFactor(inputDep);
		return inputDep;
	}

	internal JobHandle BuildParticlesGrid(JobHandle inputDep, BodyAllocator bodyAllocator, Culler culler)
	{
		m_ObjectsCount = CalculateActiveSimplexCount(bodyAllocator, culler);
		ResizeArraysIfNeed();
		m_ActiveContactsCount.Value = 0;
		inputDep = ClearGrid(inputDep);
		inputDep = CalculateParticlesSpacing(inputDep, bodyAllocator, culler);
		inputDep = BuildGridForParticles(inputDep, bodyAllocator, culler);
		inputDep = CalculateLoadFactor(inputDep);
		return inputDep;
	}

	private int CalculateActiveSimplexCount(BodyAllocator bodyAllocator, Culler culler)
	{
		int num = 0;
		foreach (KeyValuePair<AuthoringBase, int> item in bodyAllocator.EntityAllocationMap)
		{
			int2 @int = bodyAllocator.BodyDescriptorSoA.SimplexConstraintsRange[item.Value];
			int2 int2 = bodyAllocator.BodyDescriptorSoA.ConstraintSettingsRange[item.Value];
			bool num2 = bodyAllocator.ConstraintSettingsSoA.Array[int2.x + 4].z > 0f;
			uint bitMask = bodyAllocator.BodyDescriptorSoA.EnabledConstraintTypeMask[item.Value];
			bool flag = culler.BodyVisibility[item.Value] != 0;
			int num3;
			if (num2 && @int.x > -1)
			{
				int bitIndex = 4;
				num3 = (XPBDMath.IsBitSetted(in bitMask, in bitIndex) ? 1 : 0);
			}
			else
			{
				num3 = 0;
			}
			if (((uint)num3 & (flag ? 1u : 0u)) != 0)
			{
				num += @int.y;
			}
		}
		return num;
	}

	private JobHandle ClearGrid(JobHandle inputDep)
	{
		ClearGridJob clearGridJob = default(ClearGridJob);
		clearGridJob.HashmapKeys = m_HashmapKeys;
		clearGridJob.HashmapValues = m_HashmapValues;
		ClearGridJob jobData = clearGridJob;
		return IJobParallelForExtensions.ScheduleByRef(ref jobData, m_HashmapSize, 32, inputDep);
	}

	private JobHandle CalculateCollidersSpacing(JobHandle inputDep, NativeArray<int> aabbMap, NativeArray<Aabb> aabbs, int aabbCount)
	{
		m_ObjectSizeSum.Value = 0;
		CalculateGridSpacingJob calculateGridSpacingJob = default(CalculateGridSpacingJob);
		calculateGridSpacingJob.AabbMap = aabbMap;
		calculateGridSpacingJob.Aabbs = aabbs;
		calculateGridSpacingJob.ObjectSizeSum = m_ObjectSizeSum;
		CalculateGridSpacingJob jobData = calculateGridSpacingJob;
		return IJobParallelForExtensions.ScheduleByRef(ref jobData, aabbCount, 32, inputDep);
	}

	private JobHandle CalculateParticlesSpacing(JobHandle inputDep, BodyAllocator bodyAllocator, Culler culler)
	{
		m_ObjectSizeSum.Value = 0;
		JobHandle jobHandle = inputDep;
		foreach (KeyValuePair<AuthoringBase, int> item in bodyAllocator.EntityAllocationMap)
		{
			int2 simplexConstraintsRange = bodyAllocator.BodyDescriptorSoA.SimplexConstraintsRange[item.Value];
			int2 constraintsRange = bodyAllocator.BodyDescriptorSoA.ConstraintsRange[item.Value];
			int2 @int = bodyAllocator.BodyDescriptorSoA.ConstraintSettingsRange[item.Value];
			bool num = bodyAllocator.ConstraintSettingsSoA.Array[@int.x + 4].z > 0f;
			uint bitMask = bodyAllocator.BodyDescriptorSoA.EnabledConstraintTypeMask[item.Value];
			bool flag = culler.BodyVisibility[item.Value] != 0;
			int num2;
			if (num && simplexConstraintsRange.x > -1)
			{
				int bitIndex = 4;
				num2 = (XPBDMath.IsBitSetted(in bitMask, in bitIndex) ? 1 : 0);
			}
			else
			{
				num2 = 0;
			}
			if (((uint)num2 & (flag ? 1u : 0u)) != 0)
			{
				CalculateParticleGridSpacingJob calculateParticleGridSpacingJob = default(CalculateParticleGridSpacingJob);
				calculateParticleGridSpacingJob.ConstraintsRange = constraintsRange;
				calculateParticleGridSpacingJob.SimplexConstraintsRange = simplexConstraintsRange;
				calculateParticleGridSpacingJob.SimplexParameters0 = bodyAllocator.ConstraintSoA.Parameters0;
				calculateParticleGridSpacingJob.SimplexParameters1 = bodyAllocator.ConstraintSoA.Parameters1;
				calculateParticleGridSpacingJob.ObjectSizeSum = m_ObjectSizeSum;
				CalculateParticleGridSpacingJob jobData = calculateParticleGridSpacingJob;
				JobHandle job = IJobParallelForExtensions.ScheduleByRef(ref jobData, simplexConstraintsRange.y, 16, inputDep);
				jobHandle = JobHandle.CombineDependencies(jobHandle, job);
			}
		}
		inputDep = jobHandle;
		return inputDep;
	}

	private JobHandle BuildGridForColliders(JobHandle inputDep, NativeArray<int> aabbMap, NativeArray<Aabb> aabbs)
	{
		BuildGridJob buildGridJob = default(BuildGridJob);
		buildGridJob.ObjectSizeSum = m_ObjectSizeSum;
		buildGridJob.ObjectsCount = m_ObjectsCount;
		buildGridJob.HashmapSize = m_HashmapSize;
		buildGridJob.AabbMap = aabbMap;
		buildGridJob.Aabbs = aabbs;
		buildGridJob.HashmapKeys = m_HashmapKeys;
		buildGridJob.HashmapValues = m_HashmapValues;
		BuildGridJob jobData = buildGridJob;
		return IJobParallelForExtensions.ScheduleByRef(ref jobData, m_ObjectsCount, 16, inputDep);
	}

	private JobHandle BuildGridForParticles(JobHandle inputDep, BodyAllocator bodyAllocator, Culler culler)
	{
		JobHandle jobHandle = inputDep;
		foreach (KeyValuePair<AuthoringBase, int> item in bodyAllocator.EntityAllocationMap)
		{
			int2 simplexConstraintsRange = bodyAllocator.BodyDescriptorSoA.SimplexConstraintsRange[item.Value];
			int2 constraintsRange = bodyAllocator.BodyDescriptorSoA.ConstraintsRange[item.Value];
			int2 @int = bodyAllocator.BodyDescriptorSoA.ConstraintSettingsRange[item.Value];
			bool num = bodyAllocator.ConstraintSettingsSoA.Array[@int.x + 4].z > 0f;
			bool flag = culler.BodyVisibility[item.Value] != 0;
			if (num && simplexConstraintsRange.x > -1 && flag)
			{
				BuildParticlesGridJob buildParticlesGridJob = default(BuildParticlesGridJob);
				buildParticlesGridJob.ConstraintsRange = constraintsRange;
				buildParticlesGridJob.SimplexConstraintsRange = simplexConstraintsRange;
				buildParticlesGridJob.ObjectsCount = m_ObjectsCount;
				buildParticlesGridJob.ObjectSizeSum = m_ObjectSizeSum;
				buildParticlesGridJob.HashmapKeys = m_HashmapKeys;
				buildParticlesGridJob.HashmapValues = m_HashmapValues;
				buildParticlesGridJob.HashmapSize = m_HashmapSize;
				buildParticlesGridJob.SimplexParameters0 = bodyAllocator.ConstraintSoA.Parameters0;
				buildParticlesGridJob.SimplexParameters1 = bodyAllocator.ConstraintSoA.Parameters1;
				BuildParticlesGridJob jobData = buildParticlesGridJob;
				jobHandle = JobHandle.CombineDependencies(IJobParallelForExtensions.ScheduleByRef(ref jobData, simplexConstraintsRange.y, 16, inputDep), jobHandle);
			}
		}
		inputDep = jobHandle;
		return inputDep;
	}

	private void ResizeArraysIfNeed()
	{
		int num = XPBDMath.NextPrimeNumber(4 * m_ObjectsCount);
		if (num > m_HashmapSize)
		{
			m_HashmapSize = num;
		}
		else if (GetLoadFactor() > 0.3f)
		{
			m_HashmapSize = XPBDMath.NextPrimeNumber(m_HashmapSize);
		}
		if (!m_HashmapKeys.IsCreated || m_HashmapKeys.Length < m_HashmapSize)
		{
			if (m_HashmapKeys.IsCreated)
			{
				m_HashmapKeys.Dispose();
				m_HashmapValues.Dispose();
			}
			m_HashmapKeys = new NativeArray<int>(m_HashmapSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			m_HashmapValues = new NativeArray<int>(m_HashmapSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
		if (!m_ObjectSizeSum.IsCreated)
		{
			m_ObjectSizeSum = new NativeReference<int>(Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			m_ObjectSizeSum.Value = 0;
		}
		if (!m_OccupiedCellsCount.IsCreated)
		{
			m_OccupiedCellsCount = new NativeReference<int>(Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			m_OccupiedCellsCount.Value = 0;
		}
		if (!m_ActiveContactsCount.IsCreated)
		{
			m_ActiveContactsCount = new NativeReference<int>(Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			m_ActiveContactsCount.Value = 0;
		}
	}

	private JobHandle CalculateLoadFactor(JobHandle inputDep)
	{
		m_OccupiedCellsCount.Value = 0;
		CalculateHashmapLoadFactorJob calculateHashmapLoadFactorJob = default(CalculateHashmapLoadFactorJob);
		calculateHashmapLoadFactorJob.HashmapKeys = m_HashmapKeys;
		calculateHashmapLoadFactorJob.HashmapValues = m_HashmapValues;
		calculateHashmapLoadFactorJob.OccupiedCellsCount = m_OccupiedCellsCount;
		CalculateHashmapLoadFactorJob jobData = calculateHashmapLoadFactorJob;
		return IJobParallelForExtensions.ScheduleByRef(ref jobData, m_HashmapSize, 32, inputDep);
	}

	internal float GetSpacing()
	{
		if (m_ObjectSizeSum.IsCreated)
		{
			return (float)m_ObjectSizeSum.Value / 100f / (float)m_ObjectsCount * 2f;
		}
		return 0f;
	}

	internal float GetObjectsSizeSum()
	{
		if (m_ObjectSizeSum.IsCreated)
		{
			return (float)m_ObjectSizeSum.Value / 100f;
		}
		return 0f;
	}

	internal float GetLoadFactor()
	{
		if (m_OccupiedCellsCount.IsCreated)
		{
			return (float)m_OccupiedCellsCount.Value / (float)m_HashmapSize;
		}
		return 0f;
	}

	internal int GetHashmapSize()
	{
		return m_HashmapSize;
	}

	internal int GetOccupiedCellsCount()
	{
		if (m_OccupiedCellsCount.IsCreated)
		{
			return m_OccupiedCellsCount.Value;
		}
		return 0;
	}

	internal int GetActiveContactsCount()
	{
		if (m_ActiveContactsCount.IsCreated)
		{
			return m_ActiveContactsCount.Value;
		}
		return 0;
	}

	public int GetSizeInBytes()
	{
		return m_HashmapKeys.Length * Marshal.SizeOf<int>() + m_HashmapValues.Length * Marshal.SizeOf<int>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int3 IntCoords(in float3 coords, in float spacing)
	{
		return (int3)math.floor(coords / spacing);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int HashCoords(ref int x, ref int y, ref int z)
	{
		int num = math.abs((x * 92837111) ^ (y * 689287499) ^ (z * 283923481));
		if (num < 0)
		{
			num = int.MaxValue;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float CalculateSpacing(ref NativeReference<int> objectSizeSum, in int objectsCount)
	{
		return (float)objectSizeSum.Value / 100f / (float)objectsCount * 2f;
	}

	public MemoryStat GetMemoryStat()
	{
		MemoryStat result = default(MemoryStat);
		result.Cpu += m_HashmapKeys.Length * Marshal.SizeOf<int>();
		result.Gpu += m_HashmapValues.Length * Marshal.SizeOf<int>();
		return result;
	}
}
