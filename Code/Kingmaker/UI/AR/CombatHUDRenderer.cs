using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Cohesion;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View.Covers;
using Pathfinding;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.UI.AR;

public class CombatHUDRenderer : MonoBehaviour, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, IUnitMovableAreaHandler, ISubscriber<IMechanicEntity>, IAreaEffectHandler, ISubscriber<IAreaEffectEntity>, IAreaHandler, INetRoleSetHandler, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IUnitFactionHandler, ITurnStartHandler
{
	public struct AbilityAreaHudInfo
	{
		public OrientedPatternData pattern;

		public OrientedPatternData haloPattern;

		public IntRect casterRect;

		public int minRange;

		public int maxRange;

		public bool ignoreRangesByDefault;

		public bool ignorePatternPrimaryAreaByDefault;

		public CombatHudCommandSetAsset combatHudCommandsOverride;
	}

	private enum AreaDisplayMode
	{
		Default,
		Movement,
		Ability
	}

	private sealed class DebugAreaCollection
	{
		private struct Item
		{
			public CombatHubCollectionAreaSource Source;

			public Texture2D Icon;

			public CombatHudMaterialRemapAsset MaterialRemapAsset;
		}

		private readonly ObjectPool<CombatHubCollectionAreaSource> m_SourcesPool;

		private readonly Dictionary<AreaEffectEntity, Item> m_Map = new Dictionary<AreaEffectEntity, Item>();

		public DebugAreaCollection(ObjectPool<CombatHubCollectionAreaSource> sourcesPool)
		{
			m_SourcesPool = sourcesPool;
		}

		public void GetAreaDataList(List<AreaSourceData> results)
		{
			results.Clear();
			foreach (Item value in m_Map.Values)
			{
				results.Add(new AreaSourceData
				{
					Source = value.Source,
					IconTexture = value.Icon,
					MaterialRemapAsset = value.MaterialRemapAsset
				});
			}
		}

		public void SetupArea(AreaEffectEntity areaEffectEntity, in NodeList patternNodes)
		{
			CombatHubCollectionAreaSource combatHubCollectionAreaSource;
			if (m_Map.TryGetValue(areaEffectEntity, out var value))
			{
				combatHubCollectionAreaSource = value.Source;
				combatHubCollectionAreaSource.Clear();
			}
			else
			{
				combatHubCollectionAreaSource = m_SourcesPool.Get();
			}
			foreach (GridNodeBase patternNode in patternNodes)
			{
				combatHubCollectionAreaSource.Add(patternNode);
			}
			m_Map[areaEffectEntity] = new Item
			{
				Source = combatHubCollectionAreaSource,
				Icon = areaEffectEntity.Blueprint?.PersistentAreaTexture2D,
				MaterialRemapAsset = areaEffectEntity.Blueprint?.PersistentAreaMaterialRemap
			};
		}

		public bool CleanupArea(AreaEffectEntity areaEffectEntity)
		{
			if (m_Map.Remove(areaEffectEntity, out var value))
			{
				m_SourcesPool.Release(value.Source);
				return true;
			}
			return false;
		}

		public void Clear()
		{
			foreach (Item value in m_Map.Values)
			{
				m_SourcesPool.Release(value.Source);
			}
			m_Map.Clear();
		}
	}

	private const int AbilityMaxAllowedRange = 200;

	[Header("Surface Renderer")]
	[SerializeField]
	private CombatHudSurfaceRenderer m_SurfaceRenderer;

	[SerializeField]
	private float m_DelayForResetToMoveGrid = 0.3f;

	private bool m_TurnBasedModeActive;

	private bool m_MovementAreaDisplayEnabled;

	private bool m_AbilityAreaDisplayEnabled;

	private BaseUnitEntity m_ActiveUnit;

	private bool m_ForceDrawThreatArea;

	private readonly CombatHubCollectionAreaSource m_MovementArea = new CombatHubCollectionAreaSource();

	private readonly CombatHubCollectionAreaSource m_ActiveUnitArea = new CombatHubCollectionAreaSource();

	private readonly CombatHubCollectionAreaSource m_ThreateningArea = new CombatHubCollectionAreaSource();

	private readonly RingAreaSource m_MinRangeArea = new RingAreaSource();

	private readonly RingAreaSource m_MaxRangeArea = new RingAreaSource();

	private readonly CombatHubCollectionAreaSource m_PrimaryAoeArea = new CombatHubCollectionAreaSource();

	private readonly CombatHubCollectionAreaSource m_SecondaryAoeArea = new CombatHubCollectionAreaSource();

	private readonly CombatHubCollectionAreaSource m_AllyCohesionArea = new CombatHubCollectionAreaSource();

	private readonly CombatHubCollectionAreaSource m_HostileCohesionArea = new CombatHubCollectionAreaSource();

	private readonly RingAreaSource m_WeaponEffectiveRangeArea = new RingAreaSource();

	private CombatHubCollectionAreaSource m_LosBlockerArea = new CombatHubCollectionAreaSource();

	private DebugAreaCollection m_AllyDebugAreas;

	private DebugAreaCollection m_HostileDebugAreas;

	private bool m_MovementAreaValid;

	private bool m_ActiveUnitAreaValid;

	private bool m_ThreateningAreaValid;

	private bool m_MinRangeAreaValid;

	private bool m_MaxRangeAreaValid;

	private bool m_PrimaryAoeAreaValid;

	private bool m_SecondaryAoeAreaValid;

	private bool m_WeaponEffectiveRangeAreaValid;

	private bool m_LosBlockerAreaSourceValid;

	private bool m_AllyCohesionAreaValid;

	private bool m_HostileCohesionAreaValid;

	private bool m_PendingRefresh;

	private CombatHudCommandSetAsset m_AbilityCommandsOverride;

	private GridNodeBase m_CurrentUnitNode;

	private Coroutine m_PrepareLosBlockerAreaCoroutine;

	private readonly List<GridNodeBase> m_LosBlockerAreaTemp = new List<GridNodeBase>();

	private readonly ObjectPool<CombatHubCollectionAreaSource> m_CollectionAreaSourcePool = new ObjectPool<CombatHubCollectionAreaSource>(() => new CombatHubCollectionAreaSource(), null, delegate(CombatHubCollectionAreaSource source)
	{
		source.Clear();
	});

	private IDisposable m_ShowDefaultAreas;

	private IDisposable m_ShowShowMovementAreas;

	private List<PartCohesion> cohesionInCombat = new List<PartCohesion>();

	public static CombatHUDRenderer Instance { get; private set; }

	public bool ForceDrawThreatArea
	{
		get
		{
			return m_ForceDrawThreatArea;
		}
		set
		{
			if (m_ForceDrawThreatArea != value)
			{
				m_ForceDrawThreatArea = value;
				if (EvaluateAreaDisplayMode() == AreaDisplayMode.Movement)
				{
					ShowMovementAreas();
				}
			}
		}
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		Instance = this;
		m_AllyDebugAreas = new DebugAreaCollection(m_CollectionAreaSourcePool);
		m_HostileDebugAreas = new DebugAreaCollection(m_CollectionAreaSourcePool);
		EventBus.Subscribe(this);
		AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Combine(AstarPath.OnGraphsUpdated, new OnScanDelegate(OnGraphsUpdated));
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		Instance = this;
		EventBus.Unsubscribe(this);
		AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Remove(AstarPath.OnGraphsUpdated, new OnScanDelegate(OnGraphsUpdated));
	}

	[UsedImplicitly]
	private void LateUpdate()
	{
		if (m_PendingRefresh)
		{
			m_PendingRefresh = false;
			switch (EvaluateAreaDisplayMode())
			{
			case AreaDisplayMode.Default:
				ShowDefaultAreas();
				break;
			case AreaDisplayMode.Movement:
				ShowMovementAreas();
				break;
			}
		}
	}

	private void OnGraphsUpdated(AstarPath script)
	{
		m_PendingRefresh = true;
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		TurnBasedModeHandle(isTurnBased);
	}

	void IAreaHandler.OnAreaBeginUnloading()
	{
	}

	void IAreaHandler.OnAreaDidLoad()
	{
		m_AllyDebugAreas.Clear();
		m_HostileDebugAreas.Clear();
		foreach (AreaEffectEntity areaEffect in Game.Instance.EntityPools.AreaEffects)
		{
			NodeList patternNodes = areaEffect.GetPatternCoveredNodes();
			if (!patternNodes.IsEmpty)
			{
				MechanicEntity maybeCaster = areaEffect.Context.MaybeCaster;
				if (maybeCaster == null || maybeCaster.IsPlayerFaction)
				{
					m_AllyDebugAreas.SetupArea(areaEffect, in patternNodes);
				}
				else
				{
					m_HostileDebugAreas.SetupArea(areaEffect, in patternNodes);
				}
			}
		}
		m_PendingRefresh = true;
	}

	void IAreaEffectHandler.HandleAreaEffectSpawned()
	{
		if (!(EventInvokerExtensions.Entity is AreaEffectEntity areaEffectEntity))
		{
			return;
		}
		NodeList patternNodes = areaEffectEntity.GetPatternCoveredNodes();
		if (!patternNodes.IsEmpty)
		{
			MechanicEntity maybeCaster = areaEffectEntity.Context.MaybeCaster;
			if (maybeCaster == null || maybeCaster.IsPlayerFaction)
			{
				m_AllyDebugAreas.SetupArea(areaEffectEntity, in patternNodes);
			}
			else
			{
				m_HostileDebugAreas.SetupArea(areaEffectEntity, in patternNodes);
			}
			m_PendingRefresh = true;
		}
	}

	void IAreaEffectHandler.HandleAreaEffectDestroyed()
	{
		if (EventInvokerExtensions.Entity is AreaEffectEntity areaEffectEntity)
		{
			MechanicEntity maybeCaster = areaEffectEntity.Context.MaybeCaster;
			if (maybeCaster == null || maybeCaster.IsPlayerFaction)
			{
				m_PendingRefresh |= m_AllyDebugAreas.CleanupArea(areaEffectEntity);
			}
			else
			{
				m_PendingRefresh |= m_HostileDebugAreas.CleanupArea(areaEffectEntity);
			}
		}
	}

	public void HandleSetUnitMovableArea(List<GraphNode> nodes)
	{
		m_MovementAreaDisplayEnabled = true;
		m_ActiveUnit = EventInvokerExtensions.BaseUnitEntity;
		if (EvaluateAreaDisplayMode() == AreaDisplayMode.Movement)
		{
			ShowMovementAreas(nodes);
		}
	}

	public void HandleRemoveUnitMovableArea()
	{
		ClearShowTasks();
		if (m_MovementAreaDisplayEnabled)
		{
			m_MovementAreaDisplayEnabled = false;
			if (EvaluateAreaDisplayMode() == AreaDisplayMode.Default)
			{
				ShowDefaultAreas();
			}
		}
	}

	public void SetAbilityAreaHUD(AbilityAreaHudInfo abilityAreaHudInfo)
	{
		m_AbilityAreaDisplayEnabled = true;
		if (EvaluateAreaDisplayMode() == AreaDisplayMode.Ability)
		{
			ShowAbilityAreas(abilityAreaHudInfo);
		}
	}

	public void UpdateLosBlocker()
	{
		if (EvaluateAreaDisplayMode() == AreaDisplayMode.Movement)
		{
			ClearLosBlocker();
			PopulateLosBlockerArea();
			m_SurfaceRenderer.Display();
		}
	}

	public void RemoveAbilityAreaHUD()
	{
		if (!m_AbilityAreaDisplayEnabled)
		{
			return;
		}
		ClearShowTasks();
		m_AbilityAreaDisplayEnabled = false;
		switch (EvaluateAreaDisplayMode())
		{
		case AreaDisplayMode.Default:
			m_ShowDefaultAreas = DelayedInvoker.InvokeInTime(delegate
			{
				ShowDefaultAreas();
			}, m_DelayForResetToMoveGrid);
			break;
		case AreaDisplayMode.Movement:
			m_ShowShowMovementAreas = DelayedInvoker.InvokeInTime(delegate
			{
				ShowMovementAreas();
			}, m_DelayForResetToMoveGrid);
			break;
		}
	}

	private void ClearShowTasks()
	{
		m_ShowDefaultAreas?.Dispose();
		m_ShowDefaultAreas = null;
		m_ShowShowMovementAreas?.Dispose();
		m_ShowShowMovementAreas = null;
	}

	private void ShowDefaultAreas()
	{
		ClearAreas();
		ClearLosBlocker();
		UpdateSurfaceRenderer();
	}

	private void ShowMovementAreas(List<GraphNode> movementNodes = null)
	{
		ClearAreas();
		ClearLosBlocker();
		try
		{
			PopulateActiveUnitArea();
			PopulateThreateningArea();
			PopulateMovementArea(movementNodes);
			PopulateLosBlockerArea();
			PopulateCohesionAreas();
			UpdateSurfaceRenderer();
		}
		catch
		{
			ClearAreas();
			UpdateSurfaceRenderer();
			throw;
		}
	}

	private void PopulateCohesionAreas()
	{
		if (cohesionInCombat.Count != 0 && m_ActiveUnit != null && m_ActiveUnit.IsMyNetRole())
		{
			PopulateAllyCohesionArea();
			PopulateHostileCohesionArea();
		}
	}

	private void PopulateAllyCohesionArea()
	{
		foreach (PartCohesion item in cohesionInCombat)
		{
			if (item.Owner.Faction.IsPlayer)
			{
				m_AllyCohesionAreaValid = true;
				m_AllyCohesionArea.AddRange(item.PatternNodes.Value);
			}
		}
	}

	private void PopulateHostileCohesionArea()
	{
		foreach (PartCohesion item in cohesionInCombat)
		{
			if (item.Owner.Faction.IsPlayerEnemy)
			{
				m_HostileCohesionAreaValid = true;
				m_HostileCohesionArea.AddRange(item.PatternNodes.Value);
			}
		}
	}

	private void ShowAbilityAreas(AbilityAreaHudInfo abilityAreaHudInfo)
	{
		ClearAreas();
		try
		{
			bool flag;
			bool flag2;
			bool buildPrimaryArea;
			if (abilityAreaHudInfo.combatHudCommandsOverride != null)
			{
				CombatHudAreas usedAreas = abilityAreaHudInfo.combatHudCommandsOverride.GetUsedAreas();
				flag = (usedAreas & (CombatHudAreas.AbilityMinRange | CombatHudAreas.AbilityMaxRange)) != 0;
				flag2 = (usedAreas & (CombatHudAreas.AbilityPrimary | CombatHudAreas.AbilitySecondary)) != 0;
				buildPrimaryArea = (usedAreas & CombatHudAreas.AbilityPrimary) != 0;
			}
			else
			{
				flag = !abilityAreaHudInfo.ignoreRangesByDefault;
				flag2 = true;
				buildPrimaryArea = !abilityAreaHudInfo.ignorePatternPrimaryAreaByDefault;
			}
			m_AbilityCommandsOverride = abilityAreaHudInfo.combatHudCommandsOverride;
			PopulateActiveUnitArea();
			if (flag)
			{
				PopulateAbilityRangeAreas(abilityAreaHudInfo.casterRect, abilityAreaHudInfo.minRange, abilityAreaHudInfo.maxRange);
			}
			if (flag2)
			{
				PopulateAbilityPatternAreas(abilityAreaHudInfo.pattern, abilityAreaHudInfo.haloPattern, buildPrimaryArea);
			}
			UpdateSurfaceRenderer();
		}
		catch
		{
			ClearAreas();
			UpdateSurfaceRenderer();
			throw;
		}
	}

	private void ClearAreas()
	{
		ClearShowTasks();
		m_MovementArea.Clear();
		m_ActiveUnitArea.Clear();
		m_ThreateningArea.Clear();
		m_MinRangeArea.Clear();
		m_MaxRangeArea.Clear();
		m_PrimaryAoeArea.Clear();
		m_SecondaryAoeArea.Clear();
		m_WeaponEffectiveRangeArea.Clear();
		m_AllyCohesionArea.Clear();
		m_HostileCohesionArea.Clear();
		m_MovementAreaValid = false;
		m_ActiveUnitAreaValid = false;
		m_ThreateningAreaValid = false;
		m_MinRangeAreaValid = false;
		m_MaxRangeAreaValid = false;
		m_PrimaryAoeAreaValid = false;
		m_SecondaryAoeAreaValid = false;
		m_WeaponEffectiveRangeAreaValid = false;
		m_AllyCohesionAreaValid = false;
		m_HostileCohesionAreaValid = false;
		m_AbilityCommandsOverride = null;
	}

	private void ClearLosBlocker()
	{
		m_LosBlockerArea.Clear();
		m_LosBlockerAreaSourceValid = false;
	}

	private void PopulateActiveUnitArea()
	{
		BaseUnitEntity activeUnit = m_ActiveUnit;
		if (activeUnit != null && !activeUnit.IsDisposed && m_ActiveUnit.IsMyNetRole())
		{
			m_ActiveUnitAreaValid = true;
			m_ActiveUnitArea.AddRange(m_ActiveUnit.GetOccupiedNodes());
		}
	}

	private void PopulateThreateningArea()
	{
		if (IsDeploymentPhaseActive())
		{
			m_ThreateningAreaValid = true;
			List<GraphNode> deploymentForbiddenArea = Game.Instance.Controllers.UnitMovableAreaController.DeploymentForbiddenArea;
			if (deploymentForbiddenArea != null)
			{
				m_ThreateningArea.AddRange(deploymentForbiddenArea);
			}
			else
			{
				m_ThreateningArea.Clear();
			}
		}
		else
		{
			if (m_ActiveUnit == null || !m_ActiveUnit.IsMyNetRole())
			{
				return;
			}
			m_ThreateningAreaValid = true;
			if (Game.Instance.Controllers.UnitMovableAreaController.ThreateningArea != null)
			{
				m_ThreateningArea.AddRange(Game.Instance.Controllers.UnitMovableAreaController.ThreateningArea);
			}
			else
			{
				m_ThreateningArea.Clear();
			}
			foreach (BaseUnitEntity item in m_ForceDrawThreatArea ? m_ActiveUnit.CombatGroup.Memory.Enemies.Select((UnitGroupMemory.UnitInfo i) => i.Unit) : m_ActiveUnit.GetEngagedByUnits())
			{
				if (item.CanAct && !item.IsInvisible)
				{
					HashSet<GraphNode> threateningArea = item.GetThreateningArea();
					if (threateningArea != null)
					{
						m_ThreateningArea.AddRange(threateningArea);
					}
				}
			}
		}
	}

	private void PopulateMovementArea(List<GraphNode> movementNodes)
	{
		if (m_ActiveUnit == null || !m_ActiveUnit.IsMyNetRole() || !m_ActiveUnit.CanMove)
		{
			return;
		}
		m_MovementAreaValid = true;
		if (movementNodes == null && Game.Instance.Controllers.UnitMovableAreaController.DeploymentPhase && Game.Instance.Controllers.UnitMovableAreaController.CurrentUnit == m_ActiveUnit)
		{
			movementNodes = Game.Instance.Controllers.UnitMovableAreaController.CurrentUnitMovableArea;
		}
		if (movementNodes == null)
		{
			int num = (int)m_ActiveUnit.CombatState.MovementPoints;
			if (num > 0)
			{
				movementNodes = PathfindingService.Instance.FindAllReachableTiles_Blocking(m_ActiveUnit.View.MovementAgent, m_ActiveUnit.Position, num).Keys.ToList();
			}
		}
		if (movementNodes != null)
		{
			ExtendMovementAreaByUnitSize(movementNodes, m_ActiveUnit.SizeRect);
			m_MovementArea.AddRange(movementNodes);
		}
	}

	private void PopulateLosBlockerArea()
	{
		using (ProfileScope.New("PopulateLosBlockerArea"))
		{
			if (m_ActiveUnit == null || !m_ActiveUnit.IsMyNetRole())
			{
				return;
			}
			AstarPath active = AstarPath.active;
			if ((object)active == null)
			{
				return;
			}
			AstarData data = active.data;
			if (data == null)
			{
				return;
			}
			GridGraph gridGraph = data.gridGraph;
			if (gridGraph == null)
			{
				return;
			}
			GridNodeBase nearestNodeXZUnwalkable = (Game.Instance.Controllers.VirtualPositionController.VirtualPosition ?? m_ActiveUnit.Position).GetNearestNodeXZUnwalkable();
			if (m_CurrentUnitNode != nearestNodeXZUnwalkable && nearestNodeXZUnwalkable != null)
			{
				if (m_PrepareLosBlockerAreaCoroutine != null)
				{
					StopCoroutine(m_PrepareLosBlockerAreaCoroutine);
				}
				m_PrepareLosBlockerAreaCoroutine = StartCoroutine(PrepareLosBlockerAreaCoroutine(nearestNodeXZUnwalkable, gridGraph));
			}
		}
	}

	private IEnumerator PrepareLosBlockerAreaCoroutine([NotNull] GridNodeBase unitNode, [NotNull] GridGraph graph)
	{
		int checkLosCallsCount = 0;
		m_CurrentUnitNode = unitNode;
		m_LosBlockerAreaTemp.Clear();
		foreach (GridNodeBase node in GridAreaHelper.GetNodesSpiralAround(unitNode, default(IntRect), m_SurfaceRenderer.losRadiusDetection))
		{
			if (!node.Walkable || node == unitNode)
			{
				continue;
			}
			if (checkLosCallsCount > m_SurfaceRenderer.losChunkSize)
			{
				checkLosCallsCount = 0;
				m_LosBlockerArea.Clear();
				m_LosBlockerArea.AddRange(m_LosBlockerAreaTemp);
				m_LosBlockerAreaSourceValid = true;
				UpdateSurfaceRenderer();
				yield return null;
			}
			checkLosCallsCount++;
			if (!unitNode.HasDirectLos(node))
			{
				using (ProfileScope.New("m_LosBlockerArea.Add(node)"))
				{
					m_LosBlockerAreaTemp.Add(node);
				}
			}
		}
		m_LosBlockerArea.AddRange(m_LosBlockerAreaTemp);
		m_LosBlockerAreaSourceValid = true;
		m_PrepareLosBlockerAreaCoroutine = null;
	}

	public static void ExtendMovementAreaByUnitSize(List<GraphNode> movementNodes, IntRect sizeRect, bool extendToNorthEast = true)
	{
		if (sizeRect.Height == 1 && sizeRect.Width == 1)
		{
			return;
		}
		int direction = 2;
		int direction2 = 1;
		Vector2Int vector2Int = Vector2Int.up;
		Vector2Int vector2Int2 = Vector2Int.right;
		if (!extendToNorthEast)
		{
			direction = 0;
			direction2 = 3;
			vector2Int = Vector2Int.down;
			vector2Int2 = Vector2Int.left;
		}
		Dictionary<Vector2Int, GridNode> dictionary = new Dictionary<Vector2Int, GridNode>();
		List<Vector2Int> list = new List<Vector2Int>();
		List<Vector2Int> list2 = new List<Vector2Int>();
		for (int i = 0; i < movementNodes.Count; i++)
		{
			if (movementNodes[i] is GridNode gridNode)
			{
				Vector2Int key = new Vector2Int(gridNode.XCoordinateInGrid, gridNode.ZCoordinateInGrid);
				dictionary[key] = gridNode;
			}
		}
		foreach (Vector2Int key2 in dictionary.Keys)
		{
			if (!dictionary.ContainsKey(key2 + vector2Int))
			{
				list.Add(key2);
			}
			if (!dictionary.ContainsKey(key2 + vector2Int2))
			{
				list2.Add(key2);
			}
		}
		for (int j = 0; j < sizeRect.Height - 1; j++)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				GridNodeBase neighbourAlongDirection = dictionary[list[num]].GetNeighbourAlongDirection(direction);
				if (neighbourAlongDirection != null)
				{
					list.RemoveAt(num);
					Vector2Int vector2Int3 = new Vector2Int(neighbourAlongDirection.XCoordinateInGrid, neighbourAlongDirection.ZCoordinateInGrid);
					dictionary[vector2Int3] = neighbourAlongDirection as GridNode;
					list.Add(vector2Int3);
					movementNodes.Add(neighbourAlongDirection);
					if (neighbourAlongDirection.GetNeighbourAlongDirection(direction2) != null && !list2.Contains(vector2Int3))
					{
						list2.Add(vector2Int3);
					}
				}
			}
		}
		for (int k = 0; k < sizeRect.Width - 1; k++)
		{
			for (int num2 = list2.Count - 1; num2 >= 0; num2--)
			{
				GridNodeBase neighbourAlongDirection2 = dictionary[list2[num2]].GetNeighbourAlongDirection(direction2);
				if (neighbourAlongDirection2 != null)
				{
					list2.RemoveAt(num2);
					Vector2Int vector2Int4 = new Vector2Int(neighbourAlongDirection2.XCoordinateInGrid, neighbourAlongDirection2.ZCoordinateInGrid);
					dictionary[vector2Int4] = neighbourAlongDirection2 as GridNode;
					list2.Add(vector2Int4);
					movementNodes.Add(neighbourAlongDirection2);
				}
			}
		}
	}

	private void PopulateAbilityRangeAreas(IntRect casterRect, int minRange, int maxRange)
	{
		if (maxRange > 0 && maxRange <= 200)
		{
			if (minRange > 0 && minRange < maxRange)
			{
				m_MinRangeAreaValid = true;
				m_WeaponEffectiveRangeAreaValid = true;
				m_MaxRangeAreaValid = true;
				m_MinRangeArea.Setup(casterRect, -1, minRange);
				m_MaxRangeArea.Setup(casterRect, minRange, maxRange);
			}
			else if (minRange > 0 && minRange < maxRange)
			{
				m_MinRangeAreaValid = true;
				m_MaxRangeAreaValid = true;
				m_MinRangeArea.Setup(casterRect, -1, minRange);
				m_MaxRangeArea.Setup(casterRect, minRange, maxRange);
			}
			else
			{
				m_MaxRangeAreaValid = true;
				m_MaxRangeArea.Setup(casterRect, -1, maxRange);
			}
		}
	}

	private void PopulateAbilityPatternAreas(OrientedPatternData pattern, OrientedPatternData haloPattern, bool buildPrimaryArea)
	{
		if (pattern.ApplicationNode == null)
		{
			return;
		}
		if (buildPrimaryArea)
		{
			m_PrimaryAoeAreaValid = true;
			m_SecondaryAoeAreaValid = true;
			OrientedPatternData.NodesWithExtraDataEnumerator enumerator = pattern.NodesWithExtraData.GetEnumerator();
			while (enumerator.MoveNext())
			{
				(GridNodeBase, PatternCellData) current = enumerator.Current;
				m_PrimaryAoeArea.Add(current.Item1);
			}
			enumerator = haloPattern.NodesWithExtraData.GetEnumerator();
			while (enumerator.MoveNext())
			{
				(GridNodeBase, PatternCellData) current2 = enumerator.Current;
				m_SecondaryAoeArea.Add(current2.Item1);
			}
			return;
		}
		m_PrimaryAoeAreaValid = false;
		m_SecondaryAoeAreaValid = true;
		foreach (GridNodeBase node in pattern.Nodes)
		{
			m_SecondaryAoeArea.Add(node);
		}
	}

	private void UpdateSurfaceRenderer()
	{
		using (ProfileScope.New("UpdateSurfaceRenderer"))
		{
			if (!(m_SurfaceRenderer == null))
			{
				if (IsDeploymentPhaseActive())
				{
					m_SurfaceRenderer.MovementAreaSource = null;
					m_SurfaceRenderer.ThreateningAreaSource = null;
					m_SurfaceRenderer.DeploymentPermittedAreaSource = (m_MovementAreaValid ? m_MovementArea : null);
					m_SurfaceRenderer.DeploymentForbiddenAreaSource = (m_ThreateningAreaValid ? m_ThreateningArea : null);
				}
				else
				{
					m_SurfaceRenderer.MovementAreaSource = (m_MovementAreaValid ? m_MovementArea : null);
					m_SurfaceRenderer.ThreateningAreaSource = (m_ThreateningAreaValid ? m_ThreateningArea : null);
					m_SurfaceRenderer.DeploymentPermittedAreaSource = null;
					m_SurfaceRenderer.DeploymentForbiddenAreaSource = null;
				}
				m_SurfaceRenderer.ActiveUnitAreaSource = (m_ActiveUnitAreaValid ? m_ActiveUnitArea : null);
				m_SurfaceRenderer.MinRangeAreaSource = (m_MinRangeAreaValid ? m_MinRangeArea : null);
				m_SurfaceRenderer.MaxRangeAreaSource = (m_MaxRangeAreaValid ? m_MaxRangeArea : null);
				m_SurfaceRenderer.PrimaryAreaSource = (m_PrimaryAoeAreaValid ? m_PrimaryAoeArea : null);
				m_SurfaceRenderer.SecondaryAreaSource = (m_SecondaryAoeAreaValid ? m_SecondaryAoeArea : null);
				m_SurfaceRenderer.EffectiveRangeAreaSource = (m_WeaponEffectiveRangeAreaValid ? m_WeaponEffectiveRangeArea : null);
				m_SurfaceRenderer.LosBlockerAreaSource = (m_LosBlockerAreaSourceValid ? m_LosBlockerArea : null);
				m_SurfaceRenderer.AbilityCommandsOverride = m_AbilityCommandsOverride;
				m_SurfaceRenderer.AllyCohesionAreaSource = (m_AllyCohesionAreaValid ? m_AllyCohesionArea : null);
				m_SurfaceRenderer.HostileCohesionAreaSource = (m_HostileCohesionAreaValid ? m_HostileCohesionArea : null);
				m_AllyDebugAreas.GetAreaDataList(m_SurfaceRenderer.AllyDebugAreaDataList);
				m_HostileDebugAreas.GetAreaDataList(m_SurfaceRenderer.HostileDebugAreaDataList);
				m_SurfaceRenderer.Display();
			}
		}
	}

	private AreaDisplayMode EvaluateAreaDisplayMode()
	{
		if (m_AbilityAreaDisplayEnabled)
		{
			return AreaDisplayMode.Ability;
		}
		if (m_MovementAreaDisplayEnabled)
		{
			return AreaDisplayMode.Movement;
		}
		return AreaDisplayMode.Default;
	}

	public void HandleTurnBasedModeResumed()
	{
		HandleTurnBasedModeSwitched(isTurnBased: true);
	}

	private void TurnBasedModeHandle(bool isTurnBased)
	{
		if (m_TurnBasedModeActive != isTurnBased)
		{
			m_TurnBasedModeActive = isTurnBased;
			switch (EvaluateAreaDisplayMode())
			{
			case AreaDisplayMode.Default:
				ShowDefaultAreas();
				break;
			case AreaDisplayMode.Movement:
				ShowMovementAreas();
				break;
			}
		}
	}

	public void HandleRoleSet(string entityId)
	{
		if (m_ActiveUnit != null && m_ActiveUnit.UniqueId == entityId)
		{
			switch (EvaluateAreaDisplayMode())
			{
			case AreaDisplayMode.Default:
				ShowDefaultAreas();
				break;
			case AreaDisplayMode.Movement:
				ShowMovementAreas();
				break;
			}
		}
	}

	private bool IsDeploymentPhaseActive()
	{
		return Game.Instance.Controllers.TurnController.IsPreparationTurn;
	}

	void IUnitCombatHandler.HandleUnitJoinCombat()
	{
		CollectCohesion(EventInvokerExtensions.BaseUnitEntity);
	}

	void IUnitCombatHandler.HandleUnitLeaveCombat()
	{
		PartCohesion optional = EventInvokerExtensions.BaseUnitEntity.Parts.GetOptional<PartCohesion>();
		if (cohesionInCombat.Contains(optional))
		{
			cohesionInCombat.Remove(optional);
		}
	}

	void IUnitFactionHandler.HandleFactionChanged()
	{
		CollectCohesion(EventInvokerExtensions.BaseUnitEntity);
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		if (Game.Instance.Controllers.TurnController.CurrentUnit is BaseUnitEntity unitEntity)
		{
			CollectCohesion(unitEntity);
		}
	}

	public void HandleEntityGainFact(EntityFact fact)
	{
		if (fact.Owner is BaseUnitEntity unitEntity)
		{
			CollectCohesion(unitEntity);
		}
	}

	private void CollectCohesion(BaseUnitEntity unitEntity)
	{
		IEnumerable<CohesionRangeTrigger> components = unitEntity.Facts.GetComponents<CohesionRangeTrigger>();
		if (components != null && components.ToList().Count > 0)
		{
			PartCohesion optional = unitEntity.Parts.GetOptional<PartCohesion>();
			if (optional != null && !cohesionInCombat.Contains(optional))
			{
				cohesionInCombat.Add(optional);
			}
		}
	}
}
