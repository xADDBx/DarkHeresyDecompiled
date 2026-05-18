using System.Collections.Generic;
using JetBrains.Annotations;
using Pathfinding;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.UI.AR;

public sealed class CombatHudSurfaceRenderer : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private MeshRenderer m_FillMeshRenderer;

	[SerializeField]
	private MeshFilter m_FillMeshFilter;

	[SerializeField]
	private MeshRenderer m_OutlineMeshRenderer;

	[SerializeField]
	private MeshFilter m_OutlineMeshFilter;

	[Header("Asset")]
	[SerializeField]
	private CombatHudSurfaceRendererAsset m_SurfaceCombatAsset;

	private SurfaceService m_Service;

	private SurfaceServiceRequestPool m_ServiceRequestPool;

	private readonly MaterialBindingDataSource m_MaterialBindingDataSource = new MaterialBindingDataSource();

	private Material[] m_PendingOverrideMaterials;

	private static readonly int s_CohesionIntersectionShift = GetIntersectionShift(CombatHudAreas.Cohesion, CombatHudAreas.CohesionIntersection);

	[Header("Los Optimization")]
	[field: SerializeField]
	public int losChunkSize { get; private set; } = 10;


	[field: SerializeField]
	public int losRadiusDetection { get; private set; } = 75;


	public IAreaSource MovementAreaSource { get; set; }

	public IAreaSource DeploymentPermittedAreaSource { get; set; }

	public IAreaSource DeploymentForbiddenAreaSource { get; set; }

	public IAreaSource ActiveUnitAreaSource { get; set; }

	public IAreaSource ThreateningAreaSource { get; set; }

	public IAreaSource MinRangeAreaSource { get; set; }

	public IAreaSource MaxRangeAreaSource { get; set; }

	public IAreaSource EffectiveRangeAreaSource { get; set; }

	public IAreaSource PrimaryAreaSource { get; set; }

	public IAreaSource SecondaryAreaSource { get; set; }

	public IAreaSource LosBlockerAreaSource { get; set; }

	public IAreaSource HarmfulAreaSource { get; set; }

	public IAreaSource ChannelingAbilityAreaSource { get; set; }

	public IAreaSource CohesionAreaSource { get; set; }

	public IAreaSource HoveredUnitCohesionAreaSource { get; set; }

	public List<AreaSourceData> CohesionAreaDataList { get; } = new List<AreaSourceData>();


	public List<AreaSourceData> AllyDebugAreaDataList { get; } = new List<AreaSourceData>();


	public List<AreaSourceData> HostileDebugAreaDataList { get; } = new List<AreaSourceData>();


	public CombatHudCommandSetAsset AbilityCommandsOverride { get; set; }

	public bool AdditionalInfoModeEnabled { get; set; }

	public bool PointCharacterInfoModeEnabled { get; set; }

	public HighlightData HighlightSpaceCombatMovementAreaPhaseThree { get; set; }

	[UsedImplicitly]
	private void OnEnable()
	{
		m_ServiceRequestPool = new SurfaceServiceRequestPool();
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		m_Service?.Dispose();
		m_Service = null;
		m_ServiceRequestPool.Dispose();
	}

	[UsedImplicitly]
	private void LateUpdate()
	{
		m_Service?.Update();
	}

	public void SetOverrideMaterials([CanBeNull] Material[] overrideMaterials)
	{
		if (m_Service == null)
		{
			m_PendingOverrideMaterials = overrideMaterials;
		}
		else
		{
			m_Service.SetOverrideMaterials(overrideMaterials);
		}
	}

	public CombatHudSurfaceRendererAsset ResolveAsset()
	{
		return m_SurfaceCombatAsset;
	}

	public void Display()
	{
		GridGraph graph = CombatHudGraphDataSource.FindGraph();
		if (!EnsurePrerequisites(graph))
		{
			return;
		}
		CombatHudSurfaceRendererAsset combatHudSurfaceRendererAsset = ResolveAsset();
		CombatHudCommand[] globalCommands = combatHudSurfaceRendererAsset.globalCommands;
		m_Service.DiscardPendingRequest();
		m_MaterialBindingDataSource.Clear();
		m_MaterialBindingDataSource.SetHighlight(HighlightDataSource.SpaceCombatMovement3, HighlightSpaceCombatMovementAreaPhaseThree);
		SurfaceServiceRequest surfaceServiceRequest = m_ServiceRequestPool.Get();
		surfaceServiceRequest.Graph = graph;
		surfaceServiceRequest.OutlineSettings = combatHudSurfaceRendererAsset.outlineSettings;
		surfaceServiceRequest.FillSettings = combatHudSurfaceRendererAsset.fillSettings;
		surfaceServiceRequest.InsertMaterial(null, default(MaterialOverrides));
		if (MovementAreaSource != null || DeploymentPermittedAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(2u, MovementAreaSource ?? DeploymentPermittedAreaSource));
		}
		if (ActiveUnitAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(4u, ActiveUnitAreaSource));
		}
		if (ThreateningAreaSource != null || DeploymentForbiddenAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(8u, ThreateningAreaSource ?? DeploymentForbiddenAreaSource));
		}
		if (MinRangeAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(16u, MinRangeAreaSource));
		}
		if (MaxRangeAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(32u, MaxRangeAreaSource));
		}
		if (PrimaryAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(128u, PrimaryAreaSource));
		}
		if (SecondaryAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(256u, SecondaryAreaSource));
		}
		if (LosBlockerAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(8192u, LosBlockerAreaSource));
		}
		if (HarmfulAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(4096u, HarmfulAreaSource));
		}
		if (ChannelingAbilityAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(65536u, ChannelingAbilityAreaSource));
		}
		CombatHudCommand[] array = (AdditionalInfoModeEnabled ? combatHudSurfaceRendererAsset.AdditionalInfoMode : null);
		CombatHudCommand[] array2 = ((PointCharacterInfoModeEnabled && HoveredUnitCohesionAreaSource != null) ? combatHudSurfaceRendererAsset.pointCharacterInfoCommands : null);
		if (array2 != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(512u, HoveredUnitCohesionAreaSource, s_CohesionIntersectionShift));
		}
		bool num = DeploymentPermittedAreaSource != null;
		bool flag = MovementAreaSource != null;
		bool flag2 = MinRangeAreaSource != null || MaxRangeAreaSource != null || EffectiveRangeAreaSource != null;
		bool flag3 = PrimaryAreaSource != null || SecondaryAreaSource != null;
		CombatHudCommand[] array3 = (num ? combatHudSurfaceRendererAsset.deploymentCommands : (flag ? combatHudSurfaceRendererAsset.movementCommands : ((flag3 && flag2) ? ((AbilityCommandsOverride != null) ? AbilityCommandsOverride.Commands : combatHudSurfaceRendererAsset.abilityPatternRangeCommands) : (flag2 ? ((AbilityCommandsOverride != null) ? AbilityCommandsOverride.Commands : combatHudSurfaceRendererAsset.abilityRangeCommands) : ((!flag3) ? null : ((AbilityCommandsOverride != null) ? AbilityCommandsOverride.Commands : combatHudSurfaceRendererAsset.abilityPatternCommands))))));
		bool flag4 = false;
		if (array3 != null)
		{
			CombatHudCommand[] array4 = array3;
			foreach (CombatHudCommand combatHudCommand in array4)
			{
				if ((combatHudCommand.GetExecutionMode() & CombatHudCommandExecutionMode.PerCohesion) != 0)
				{
					flag4 = true;
				}
			}
		}
		if (array != null)
		{
			CombatHudCommand[] array4 = array;
			foreach (CombatHudCommand combatHudCommand2 in array4)
			{
				if ((combatHudCommand2.GetExecutionMode() & CombatHudCommandExecutionMode.PerCohesion) != 0)
				{
					flag4 = true;
				}
			}
		}
		if (array2 != null)
		{
			CombatHudCommand[] array4 = array2;
			foreach (CombatHudCommand combatHudCommand3 in array4)
			{
				if ((combatHudCommand3.GetExecutionMode() & CombatHudCommandExecutionMode.PerCohesion) != 0)
				{
					flag4 = true;
				}
			}
		}
		if (globalCommands != null)
		{
			CombatHudCommand[] array4 = globalCommands;
			foreach (CombatHudCommand combatHudCommand4 in array4)
			{
				if ((combatHudCommand4.GetExecutionMode() & CombatHudCommandExecutionMode.PerCohesion) != 0)
				{
					flag4 = true;
				}
			}
		}
		bool flag5 = flag4 && CohesionAreaDataList.Count > 0;
		if (CohesionAreaSource != null && !flag5)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(512u, CohesionAreaSource, s_CohesionIntersectionShift));
		}
		int num2 = 0;
		int num3 = -1;
		if (flag5)
		{
			num3 = num2;
			foreach (AreaSourceData cohesionAreaData in CohesionAreaDataList)
			{
				surfaceServiceRequest.Areas.Add(new AreaData(512u, cohesionAreaData.Source, s_CohesionIntersectionShift, isStratagem: true));
				num2++;
			}
		}
		int num4 = -1;
		if (AllyDebugAreaDataList.Count > 0)
		{
			num4 = num2;
			foreach (AreaSourceData allyDebugAreaData in AllyDebugAreaDataList)
			{
				surfaceServiceRequest.Areas.Add(new AreaData(64u, allyDebugAreaData.Source, 1, isStratagem: true));
				num2++;
			}
		}
		int num5 = -1;
		if (HostileDebugAreaDataList.Count > 0)
		{
			num5 = num2;
			foreach (AreaSourceData hostileDebugAreaData in HostileDebugAreaDataList)
			{
				surfaceServiceRequest.Areas.Add(new AreaData(2048u, hostileDebugAreaData.Source, 1, isStratagem: true));
				num2++;
			}
		}
		List<CombatHudCommand> value;
		using (CollectionPool<List<CombatHudCommand>, CombatHudCommand>.Get(out value))
		{
			if (array3 != null)
			{
				CombatHudCommand[] array4 = array3;
				for (int i = 0; i < array4.Length; i++)
				{
					CombatHudCommand item = array4[i];
					CombatHudCommandExecutionMode executionMode = item.GetExecutionMode();
					if ((executionMode & CombatHudCommandExecutionMode.Main) != 0)
					{
						item.PushCommand(surfaceServiceRequest, m_MaterialBindingDataSource);
					}
					if ((executionMode & CombatHudCommandExecutionMode.PerCohesion) != 0)
					{
						value.Add(item);
					}
				}
			}
			if (array != null)
			{
				CombatHudCommand[] array4 = array;
				for (int i = 0; i < array4.Length; i++)
				{
					CombatHudCommand item2 = array4[i];
					CombatHudCommandExecutionMode executionMode2 = item2.GetExecutionMode();
					if ((executionMode2 & CombatHudCommandExecutionMode.Main) != 0)
					{
						item2.PushCommand(surfaceServiceRequest, m_MaterialBindingDataSource);
					}
					if ((executionMode2 & CombatHudCommandExecutionMode.PerCohesion) != 0)
					{
						value.Add(item2);
					}
				}
			}
			if (array2 != null)
			{
				CombatHudCommand[] array4 = array2;
				for (int i = 0; i < array4.Length; i++)
				{
					CombatHudCommand item3 = array4[i];
					CombatHudCommandExecutionMode executionMode3 = item3.GetExecutionMode();
					if ((executionMode3 & CombatHudCommandExecutionMode.Main) != 0)
					{
						item3.PushCommand(surfaceServiceRequest, m_MaterialBindingDataSource);
					}
					if ((executionMode3 & CombatHudCommandExecutionMode.PerCohesion) != 0)
					{
						value.Add(item3);
					}
				}
			}
			if (globalCommands != null)
			{
				CombatHudCommand[] array4 = globalCommands;
				for (int i = 0; i < array4.Length; i++)
				{
					CombatHudCommand item4 = array4[i];
					CombatHudCommandExecutionMode executionMode4 = item4.GetExecutionMode();
					if ((executionMode4 & CombatHudCommandExecutionMode.Main) != 0)
					{
						item4.PushCommand(surfaceServiceRequest, m_MaterialBindingDataSource);
					}
					if ((executionMode4 & CombatHudCommandExecutionMode.PerCohesion) != 0)
					{
						value.Add(item4);
					}
				}
			}
			if (AllyDebugAreaDataList.Count > 0)
			{
				for (int j = 0; j < AllyDebugAreaDataList.Count; j++)
				{
					AreaSourceData areaSourceData = AllyDebugAreaDataList[j];
					int stratagemId = num4 + j;
					CombatHudCommand[] array4 = combatHudSurfaceRendererAsset.allyDebugCommands;
					foreach (CombatHudCommand combatHudCommand5 in array4)
					{
						m_MaterialBindingDataSource.SetIcon(IconOverrideSource.Stratagem, areaSourceData.IconTexture);
						m_MaterialBindingDataSource.SetMaterialRemap(areaSourceData.MaterialRemapAsset);
						combatHudCommand5.PushCommand(surfaceServiceRequest, m_MaterialBindingDataSource, stratagemId);
					}
				}
				m_MaterialBindingDataSource.SetMaterialRemap(null);
			}
			if (HostileDebugAreaDataList.Count > 0)
			{
				for (int k = 0; k < HostileDebugAreaDataList.Count; k++)
				{
					AreaSourceData areaSourceData2 = HostileDebugAreaDataList[k];
					int stratagemId2 = num5 + k;
					CombatHudCommand[] array4 = combatHudSurfaceRendererAsset.hostileDebugCommands;
					foreach (CombatHudCommand combatHudCommand6 in array4)
					{
						m_MaterialBindingDataSource.SetIcon(IconOverrideSource.Stratagem, areaSourceData2.IconTexture);
						m_MaterialBindingDataSource.SetMaterialRemap(areaSourceData2.MaterialRemapAsset);
						combatHudCommand6.PushCommand(surfaceServiceRequest, m_MaterialBindingDataSource, stratagemId2);
					}
				}
				m_MaterialBindingDataSource.SetMaterialRemap(null);
			}
			if (flag5 && value.Count > 0)
			{
				for (int l = 0; l < CohesionAreaDataList.Count; l++)
				{
					AreaSourceData areaSourceData3 = CohesionAreaDataList[l];
					int stratagemId3 = num3 + l;
					surfaceServiceRequest.CommandBuffer.ClearFill();
					surfaceServiceRequest.CommandBuffer.ClearOutline();
					m_MaterialBindingDataSource.SetIcon(IconOverrideSource.Stratagem, areaSourceData3.IconTexture);
					m_MaterialBindingDataSource.SetMaterialRemap(areaSourceData3.MaterialRemapAsset);
					foreach (CombatHudCommand item5 in value)
					{
						item5.PushCommand(surfaceServiceRequest, m_MaterialBindingDataSource, stratagemId3);
					}
				}
				m_MaterialBindingDataSource.SetMaterialRemap(null);
			}
		}
		m_Service.SetPendingRequest(surfaceServiceRequest);
	}

	private bool EnsurePrerequisites(GridGraph graph)
	{
		if (graph == null)
		{
			return false;
		}
		if (m_FillMeshFilter == null)
		{
			return false;
		}
		if (m_FillMeshRenderer == null)
		{
			return false;
		}
		if (m_OutlineMeshFilter == null)
		{
			return false;
		}
		if (m_OutlineMeshRenderer == null)
		{
			return false;
		}
		if (ResolveAsset() == null)
		{
			return false;
		}
		if (m_Service == null)
		{
			m_Service = new SurfaceService(m_FillMeshFilter, m_FillMeshRenderer, m_OutlineMeshFilter, m_OutlineMeshRenderer, m_ServiceRequestPool);
			if (m_PendingOverrideMaterials != null)
			{
				m_Service.SetOverrideMaterials(m_PendingOverrideMaterials);
				m_PendingOverrideMaterials = null;
			}
		}
		return true;
	}

	private static int GetIntersectionShift(CombatHudAreas areaFlag, CombatHudAreas intersectionFlag)
	{
		int singleBitIndex = GetSingleBitIndex((uint)areaFlag);
		int num = GetSingleBitIndex((uint)intersectionFlag) - singleBitIndex;
		if (num <= 0)
		{
			return 0;
		}
		return num;
	}

	private static int GetSingleBitIndex(uint mask)
	{
		for (int i = 0; i < 32; i++)
		{
			if ((mask & (uint)(1 << i)) != 0)
			{
				return i;
			}
		}
		return -1;
	}
}
