using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace Kingmaker.UI.AR;

[Serializable]
public struct CombatHudCommand
{
	[Serializable]
	public struct WriteSurfaceArgs
	{
		public Material material;

		[CombatHudMaterialRemapTag]
		public int materialRemapTag;

		[FormerlySerializedAs("highlight")]
		public HighlightDataSource highlightBinding;

		public SurfaceCellFilter shape;
	}

	[Serializable]
	public struct BuildSurfaceArgs
	{
		public float meshOffset;
	}

	[Serializable]
	public struct BuildOutlineArgs
	{
		public Material material;

		public Material[] additionalMaterials;

		[CombatHudMaterialRemapTag]
		public int materialRemapTag;

		public OutlineType lineType;

		public bool overwrite;

		public bool separatePass;

		public float meshOffset;

		public OutlineCellFilter shape;

		public OutlineCellFilter mask;
	}

	[Serializable]
	public struct SurfaceCellFilter
	{
		public CombatHudAreas belongToAll;

		public CombatHudAreas belongToAny;

		public CombatHudAreas notBelongToAny;

		public static implicit operator SurfaceCellFilterData(in SurfaceCellFilter value)
		{
			SurfaceCellFilterData result = default(SurfaceCellFilterData);
			result.belongToAllAreaMask = (uint)value.belongToAll;
			result.belongToAnyAreasMask = (uint)value.belongToAny;
			result.notBelongToAnyAreasMask = (uint)value.notBelongToAny;
			return result;
		}

		public CombatHudAreas GetUsedAreas()
		{
			return belongToAll | belongToAny | notBelongToAny;
		}
	}

	[Serializable]
	public struct OutlineCellFilter
	{
		public CombatHudAreas belongToAll;

		public CombatHudAreas belongToAny;

		public CombatHudAreas notBelongToAny;

		public SurfaceBufferMask surfaceBuffer;

		public static implicit operator OutlineCellFilterData(OutlineCellFilter value)
		{
			OutlineCellFilterData result = default(OutlineCellFilterData);
			result.belongToAllAreaMask = (uint)value.belongToAll;
			result.belongToAnyAreasMask = (uint)value.belongToAny;
			result.notBelongToAnyAreasMask = (uint)value.notBelongToAny;
			result.surfaceBuffer = value.surfaceBuffer;
			return result;
		}

		public CombatHudAreas GetUsedAreas()
		{
			return belongToAll | belongToAny | notBelongToAny;
		}
	}

	public CombatHudCommandCode code;

	public CombatHudCommandExecutionMode executionMode;

	public WriteSurfaceArgs writeSurfaceArgs;

	public BuildSurfaceArgs buildSurfaceArgs;

	public BuildOutlineArgs buildOutlineArgs;

	public CombatHudAreas GetUsedAreas()
	{
		switch (code)
		{
		case CombatHudCommandCode.WriteFill:
		case CombatHudCommandCode.WriteDebugFill:
			return writeSurfaceArgs.shape.GetUsedAreas();
		case CombatHudCommandCode.BuildOutline:
			return buildOutlineArgs.shape.GetUsedAreas() | buildOutlineArgs.mask.GetUsedAreas();
		default:
			return (CombatHudAreas)0u;
		}
	}

	public void OnValidate()
	{
		if (code != CombatHudCommandCode.WriteFill && code != CombatHudCommandCode.WriteDebugFill)
		{
			writeSurfaceArgs = default(WriteSurfaceArgs);
		}
		if (code != CombatHudCommandCode.BuildFill)
		{
			buildSurfaceArgs = default(BuildSurfaceArgs);
		}
		if (code != CombatHudCommandCode.BuildOutline)
		{
			buildOutlineArgs = default(BuildOutlineArgs);
		}
	}

	public CombatHudCommandExecutionMode GetExecutionMode()
	{
		if (executionMode != 0)
		{
			return executionMode;
		}
		return CombatHudCommandExecutionMode.Main;
	}

	public void PushCommand(SurfaceServiceRequest request, MaterialBindingDataSource bindingDataSource, int stratagemId = -1)
	{
		switch (code)
		{
		case CombatHudCommandCode.WriteFill:
		{
			Material material2 = bindingDataSource.RemapMaterial(writeSurfaceArgs.material, writeSurfaceArgs.materialRemapTag);
			MaterialOverrides overrides2 = bindingDataSource.GetOverrides(IconOverrideSource.None, writeSurfaceArgs.highlightBinding);
			int materialId2 = request.InsertMaterial(material2, overrides2);
			int shapeId = ((stratagemId >= 0) ? stratagemId : (-1));
			request.CommandBuffer.WriteFill(materialId2, shapeId, writeSurfaceArgs.shape);
			break;
		}
		case CombatHudCommandCode.WriteDebugFill:
		{
			Material material = bindingDataSource.RemapMaterial(writeSurfaceArgs.material, writeSurfaceArgs.materialRemapTag);
			MaterialOverrides overrides = bindingDataSource.GetOverrides(IconOverrideSource.Stratagem, writeSurfaceArgs.highlightBinding);
			int materialId = request.InsertMaterial(material, overrides);
			request.CommandBuffer.WriteFill(materialId, (stratagemId >= 0) ? stratagemId : (-1), writeSurfaceArgs.shape);
			break;
		}
		case CombatHudCommandCode.ClearFillBuffer:
			request.CommandBuffer.ClearFill();
			break;
		case CombatHudCommandCode.ClearOutlineBuffer:
			request.CommandBuffer.ClearOutline();
			break;
		case CombatHudCommandCode.BuildFill:
			request.CommandBuffer.BuildFill(new float3(0f, buildSurfaceArgs.meshOffset, 0f));
			break;
		case CombatHudCommandCode.BuildOutline:
		{
			List<Material> value;
			using (CollectionPool<List<Material>, Material>.Get(out value))
			{
				bindingDataSource.RemapMaterials(buildOutlineArgs.material, buildOutlineArgs.additionalMaterials, buildOutlineArgs.materialRemapTag, value);
				if (value.Count <= 0)
				{
					break;
				}
				if (buildOutlineArgs.separatePass)
				{
					request.CommandBuffer.ClearOutline();
				}
				OutlineCellFilterData shape = buildOutlineArgs.shape;
				CombatHudAreas combatHudAreas = buildOutlineArgs.shape.belongToAll | buildOutlineArgs.shape.belongToAny;
				if (buildOutlineArgs.separatePass && (combatHudAreas & CombatHudAreas.Cohesion) != 0)
				{
					shape.notBelongToAnyAreasMask |= 16384u;
				}
				request.CommandBuffer.ComposeOutlineMesh(buildOutlineArgs.lineType, buildOutlineArgs.overwrite, new float3(0f, buildOutlineArgs.meshOffset, 0f), shape, buildOutlineArgs.mask);
				foreach (Material item in value)
				{
					request.CommandBuffer.AppendOutlineMesh(request.InsertMaterial(item, default(MaterialOverrides)));
				}
				break;
			}
		}
		}
	}
}
