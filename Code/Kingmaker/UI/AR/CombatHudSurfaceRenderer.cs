using System.Collections.Generic;
using JetBrains.Annotations;
using Pathfinding;
using UnityEngine;

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

	public IAreaSource AllyCohesionAreaSource { get; set; }

	public IAreaSource HostileCohesionAreaSource { get; set; }

	public List<AreaSourceData> AllyDebugAreaDataList { get; } = new List<AreaSourceData>();


	public List<AreaSourceData> HostileDebugAreaDataList { get; } = new List<AreaSourceData>();


	public CombatHudCommandSetAsset AbilityCommandsOverride { get; set; }

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
		if (AllyCohesionAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(512u, AllyCohesionAreaSource));
		}
		if (HostileCohesionAreaSource != null)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(1024u, HostileCohesionAreaSource));
		}
		foreach (AreaSourceData allyDebugAreaData in AllyDebugAreaDataList)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(64u, allyDebugAreaData.Source, 1, isStratagem: true));
		}
		foreach (AreaSourceData hostileDebugAreaData in HostileDebugAreaDataList)
		{
			surfaceServiceRequest.Areas.Add(new AreaData(2048u, hostileDebugAreaData.Source, 1, isStratagem: true));
		}
		bool num = DeploymentPermittedAreaSource != null;
		bool flag = MovementAreaSource != null;
		bool flag2 = MinRangeAreaSource != null || MaxRangeAreaSource != null || EffectiveRangeAreaSource != null;
		bool flag3 = PrimaryAreaSource != null || SecondaryAreaSource != null;
		CombatHudCommand[] array = (num ? combatHudSurfaceRendererAsset.deploymentCommands : (flag ? combatHudSurfaceRendererAsset.movementCommands : ((flag3 && flag2) ? ((AbilityCommandsOverride != null) ? AbilityCommandsOverride.Commands : combatHudSurfaceRendererAsset.abilityPatternRangeCommands) : (flag2 ? ((AbilityCommandsOverride != null) ? AbilityCommandsOverride.Commands : combatHudSurfaceRendererAsset.abilityRangeCommands) : ((!flag3) ? null : ((AbilityCommandsOverride != null) ? AbilityCommandsOverride.Commands : combatHudSurfaceRendererAsset.abilityPatternCommands))))));
		if (array != null)
		{
			CombatHudCommand[] array2 = array;
			foreach (CombatHudCommand combatHudCommand in array2)
			{
				combatHudCommand.PushCommand(surfaceServiceRequest, m_MaterialBindingDataSource);
			}
		}
		int num2 = 0;
		if (AllyDebugAreaDataList.Count > 0)
		{
			foreach (AreaSourceData allyDebugAreaData2 in AllyDebugAreaDataList)
			{
				CombatHudCommand[] array2 = combatHudSurfaceRendererAsset.allyDebugCommands;
				foreach (CombatHudCommand combatHudCommand2 in array2)
				{
					m_MaterialBindingDataSource.SetIcon(IconOverrideSource.Stratagem, allyDebugAreaData2.IconTexture);
					m_MaterialBindingDataSource.SetMaterialRemap(allyDebugAreaData2.MaterialRemapAsset);
					combatHudCommand2.PushCommand(surfaceServiceRequest, m_MaterialBindingDataSource, num2);
				}
				num2++;
			}
			m_MaterialBindingDataSource.SetMaterialRemap(null);
		}
		if (HostileDebugAreaDataList.Count > 0)
		{
			foreach (AreaSourceData hostileDebugAreaData2 in HostileDebugAreaDataList)
			{
				CombatHudCommand[] array2 = combatHudSurfaceRendererAsset.hostileDebugCommands;
				foreach (CombatHudCommand combatHudCommand3 in array2)
				{
					m_MaterialBindingDataSource.SetIcon(IconOverrideSource.Stratagem, hostileDebugAreaData2.IconTexture);
					m_MaterialBindingDataSource.SetMaterialRemap(hostileDebugAreaData2.MaterialRemapAsset);
					combatHudCommand3.PushCommand(surfaceServiceRequest, m_MaterialBindingDataSource, num2);
				}
				num2++;
			}
			m_MaterialBindingDataSource.SetMaterialRemap(null);
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
}
