using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

internal static class TerrainLayerPVSFactory
{
	[BurstCompile]
	private readonly struct BuildMasksJob : IJobParallelForBatch
	{
		[NativeDisableUnsafePtrRestriction]
		[ReadOnly]
		private unsafe readonly float* m_AlphamapBufferPtr;

		private readonly int3 m_AlphamapBufferSize;

		[NativeDisableUnsafePtrRestriction]
		[WriteOnly]
		private unsafe readonly ulong* m_MaskBufferPtr;

		private readonly int2 m_MaskBufferSize;

		public unsafe BuildMasksJob(float* alphamapBufferPtr, int3 alphamapBufferSize, ulong* maskBufferPtr, int2 maskBufferSize)
		{
			m_AlphamapBufferPtr = alphamapBufferPtr;
			m_AlphamapBufferSize = alphamapBufferSize;
			m_MaskBufferPtr = maskBufferPtr;
			m_MaskBufferSize = maskBufferSize;
		}

		public unsafe void Execute(int startIndex, int count)
		{
			Span<float> alphamapBuffer = new Span<float>(m_AlphamapBufferPtr, m_AlphamapBufferSize.x * m_AlphamapBufferSize.y * m_AlphamapBufferSize.z);
			Span<ulong> span = new Span<ulong>(m_MaskBufferPtr, m_MaskBufferSize.x * m_MaskBufferSize.y);
			int2 @int = m_AlphamapBufferSize.xy / m_MaskBufferSize.yx;
			for (int i = 0; i < count; i++)
			{
				int num = startIndex + i;
				int2 int2 = new int2(num % m_MaskBufferSize.x, num / m_MaskBufferSize.x);
				span[num] = GetRegionLayerMask(alphamapBuffer, m_AlphamapBufferSize, int2.yx * @int, int2.yx * @int + @int);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong GetRegionLayerMask(Span<float> alphamapBuffer, int3 alphamapBufferSize, int2 min, int2 max)
		{
			ulong num = 0uL;
			int num2 = alphamapBufferSize.z * alphamapBufferSize.y;
			int z = alphamapBufferSize.z;
			for (int i = min.x; i < max.x; i++)
			{
				for (int j = min.y; j < max.y; j++)
				{
					num |= GetLayerMask(alphamapBuffer.Slice(i * num2 + j * z, alphamapBufferSize.z));
				}
			}
			return num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong GetLayerMask(Span<float> alphamaps)
		{
			float4 @float = new float4(0);
			int4 @int = new int4(-1);
			for (int i = 0; i < alphamaps.Length; i++)
			{
				float num = alphamaps[i];
				if (num > @float[0])
				{
					@float[3] = @float[2];
					@int[3] = @int[2];
					@float[2] = @float[1];
					@int[2] = @int[1];
					@float[1] = @float[0];
					@int[1] = @int[0];
					@float[0] = num;
					@int[0] = i;
				}
				else if (num > @float[1])
				{
					@float[3] = @float[2];
					@int[3] = @int[2];
					@float[2] = @float[1];
					@int[2] = @int[1];
					@float[1] = num;
					@int[1] = i;
				}
				else if (num > @float[2])
				{
					@float[3] = @float[2];
					@int[3] = @int[2];
					@float[2] = num;
					@int[2] = i;
				}
				else if (num > @float[3])
				{
					@float[3] = num;
					@int[3] = i;
				}
			}
			ulong num2 = 0uL;
			if (@int.x >= 0)
			{
				num2 |= (ulong)(1L << @int.x);
			}
			if (@int.y >= 0)
			{
				num2 |= (ulong)(1L << @int.y);
			}
			if (@int.z >= 0)
			{
				num2 |= (ulong)(1L << @int.z);
			}
			if (@int.w >= 0)
			{
				num2 |= (ulong)(1L << @int.w);
			}
			return num2;
		}
	}

	[BurstCompile]
	private readonly struct BuildPvsNodesJob : IJobParallelForBatch
	{
		private readonly struct Plotter : PixelDrawUtility.ILinePlotter
		{
			private unsafe readonly ulong* m_AlphamapMasks;

			private readonly int2 m_AlphamapMasksSize;

			private unsafe readonly ulong* m_Node;

			public unsafe Plotter(ulong* alphamapMasks, int2 alphamapMasksSize, ulong* node)
			{
				m_AlphamapMasks = alphamapMasks;
				m_AlphamapMasksSize = alphamapMasksSize;
				m_Node = node;
			}

			public unsafe void Plot(int xMin, int xMax, int y)
			{
				if (y >= 0 && y < m_AlphamapMasksSize.y)
				{
					xMin = math.max(xMin, 0);
					xMax = math.min(xMax, m_AlphamapMasksSize.x - 1);
					int num = y * m_AlphamapMasksSize.x;
					for (int i = xMin; i <= xMax; i++)
					{
						*m_Node |= m_AlphamapMasks[i + num];
					}
				}
			}
		}

		[NativeDisableUnsafePtrRestriction]
		[ReadOnly]
		private unsafe readonly ulong* m_MaskBufferPtr;

		private readonly int2 m_MaskBufferSize;

		[NativeDisableUnsafePtrRestriction]
		[WriteOnly]
		private unsafe readonly ulong* m_PvsNodeBufferPtr;

		private readonly int2 m_PvsNodeBufferSize;

		private readonly int2 m_PvsNodeOffset;

		private readonly int2 m_PvsNodeInterval;

		private readonly int m_PvsNodeRadius;

		public unsafe BuildPvsNodesJob(ulong* maskBufferPtr, int2 maskBufferSize, ulong* pvsNodeBufferPtr, int2 pvsNodeBufferSize, int2 pvsNodeOffset, int2 pvsNodeInterval, int pvsNodeRadius)
		{
			m_MaskBufferPtr = maskBufferPtr;
			m_MaskBufferSize = maskBufferSize;
			m_PvsNodeBufferPtr = pvsNodeBufferPtr;
			m_PvsNodeBufferSize = pvsNodeBufferSize;
			m_PvsNodeOffset = pvsNodeOffset;
			m_PvsNodeInterval = pvsNodeInterval;
			m_PvsNodeRadius = pvsNodeRadius;
		}

		public unsafe void Execute(int startIndex, int count)
		{
			for (int i = 0; i < count; i++)
			{
				int num = startIndex + i;
				int2 @int = new int2(num % m_PvsNodeBufferSize.x, num / m_PvsNodeBufferSize.x);
				int2 int2 = m_PvsNodeOffset + @int * m_PvsNodeInterval;
				PixelDrawUtility.FillCircle(plotter: new Plotter(m_MaskBufferPtr, m_MaskBufferSize, m_PvsNodeBufferPtr + num), centerX: int2.x, centerY: int2.y, radius: m_PvsNodeRadius);
			}
		}
	}

	public unsafe static TerrainLayerPVS Create(UnityEngine.Terrain terrain, float cellSize, float probeExtentRadius0, float probeExtentRadius1)
	{
		if (terrain == null)
		{
			throw new ArgumentNullException("terrain");
		}
		TerrainData terrainData = terrain.terrainData;
		if (terrainData == null)
		{
			throw new ArgumentException("Terrain has no TerrainData", "terrain");
		}
		float2 position = new float2(terrain.GetPosition().x, terrain.GetPosition().z);
		float2 @float = new float2(terrainData.size.x, terrainData.size.z);
		int3 alphamapBufferSize = new int3(terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers);
		float[,,] alphamaps = terrainData.GetAlphamaps(0, 0, alphamapBufferSize.x, alphamapBufferSize.y);
		int2 maskBufferSize = alphamapBufferSize.yx >> 2;
		using NativeArray<ulong> nativeArray = new NativeArray<ulong>(maskBufferSize.x * maskBufferSize.y, Allocator.TempJob);
		float num = @float.x / (float)maskBufferSize.x;
		int num2 = (int)math.ceil(cellSize / num);
		if (num2 % 2 == 0)
		{
			num2++;
		}
		int num3 = (int)math.ceil((float)maskBufferSize.x / (float)num2);
		int num4 = (int)math.ceil((float)num2 / 2f);
		int num5 = num2 / 2;
		int pvsNodeRadius = num5 + (int)math.ceil(probeExtentRadius0 / num);
		int pvsNodeRadius2 = num5 + (int)math.ceil(probeExtentRadius1 / num);
		ulong[] array = new ulong[num3 * num3];
		ulong[] array2 = new ulong[num3 * num3];
		fixed (float* alphamapBufferPtr = alphamaps)
		{
			fixed (ulong* pvsNodeBufferPtr = array)
			{
				fixed (ulong* pvsNodeBufferPtr2 = array2)
				{
					BuildMasksJob jobData = new BuildMasksJob(alphamapBufferPtr, alphamapBufferSize, (ulong*)nativeArray.GetUnsafePtr(), maskBufferSize);
					BuildPvsNodesJob jobData2 = new BuildPvsNodesJob((ulong*)nativeArray.GetUnsafePtr(), maskBufferSize, pvsNodeBufferPtr, new int2(num3, num3), num4, num2, pvsNodeRadius);
					BuildPvsNodesJob jobData3 = new BuildPvsNodesJob((ulong*)nativeArray.GetUnsafePtr(), maskBufferSize, pvsNodeBufferPtr2, new int2(num3, num3), num4, num2, pvsNodeRadius2);
					JobHandle dependsOn = jobData.ScheduleBatch(maskBufferSize.x * maskBufferSize.y, 64);
					dependsOn = jobData2.ScheduleBatch(num3 * num3, 64, dependsOn);
					jobData3.ScheduleBatch(num3 * num3, 64, dependsOn).Complete();
				}
			}
		}
		TerrainLayerPVS result = default(TerrainLayerPVS);
		result.Size = (float)(num3 * num2) * num;
		result.Position = position;
		result.Radius0 = probeExtentRadius0;
		result.Radius1 = probeExtentRadius1;
		result.NodesCount = new int2(num3, num3);
		result.Nodes0 = array;
		result.Nodes1 = array2;
		return result;
	}
}
