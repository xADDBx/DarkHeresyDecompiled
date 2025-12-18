using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GeometryExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.AR;

public class UnitPathManager : MonoBehaviour, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IRoundStartHandler, IHideUIWhileActionCameraHandler, IAreaHandler, IPreparationTurnBeginHandler, IPreparationTurnEndHandler, INetRoleSetHandler, IAbilityTargetHoverUIHandler, IAbilityOwnerTargetSelectionHandler, IAbilityTargetSelectionUIHandler, IUnitMovableAreaHandler, INetPingPosition
{
	private class PathData
	{
		public Path path;

		public float PathCost;

		public bool HasThreats;
	}

	private class PingData
	{
		public IDisposable PingDelay { get; set; }
	}

	public static UnitPathManager Instance;

	[Header("Path Rendering")]
	[SerializeField]
	private CombatHudPathRenderer m_PathRenderer;

	[Header("PathDecal")]
	[SerializeField]
	private GameObject m_PathEndDecal;

	[SerializeField]
	private Color m_PathEndDecalColor;

	private GameObject m_CreatedPathEndDecal;

	[Header("PredictDecal")]
	[SerializeField]
	private PointerCellDecal m_PointerCellDecal;

	[SerializeField]
	private PointerCellDecal m_PointerCellDecalSpace;

	private PointerCellDecal m_CreatedPointerCellDecal;

	[Header("PingDecal")]
	[SerializeField]
	private GameObject m_PingDecal;

	[SerializeField]
	private GameObject m_SpacePingDecal;

	private readonly GameObject[] m_CreatedPingDecals = new GameObject[6];

	[Header("Suicide")]
	[SerializeField]
	private GameObject m_SuicideDecal;

	[SerializeField]
	private Color m_SuicideDecalColor;

	[Header("SpacePathDecal")]
	[SerializeField]
	private GameObject m_SpacePathEndDecal;

	[SerializeField]
	private Color m_SpacePathEndDecalColor;

	private GameObject m_CreatedSuicideDecal;

	private CameraRig m_CameraRig;

	private bool m_TbActive;

	private bool m_IsPlayerTurn;

	private bool m_ShouldHide;

	private bool m_TooFarForUnit;

	private bool m_DeploymentPhase;

	public GridNode CurrentNode;

	private Vector3? m_CurrentDecalPosition;

	private int m_DecalScale = 1;

	private Vector3 m_DecalOffset = Vector3.zero;

	private Vector3 m_SizeOffset = Vector3.zero;

	private readonly Dictionary<AbstractUnitEntity, PathData> m_MemorizedPaths = new Dictionary<AbstractUnitEntity, PathData>();

	private Task m_UpdateTask = Task.CompletedTask;

	private CancellationTokenSource m_UpdateTaskCancelToken;

	private Vector3? m_LastShownPosition;

	private AbstractUnitEntity m_UnitCached;

	private ForcedPath m_ForcedPathCached;

	private float[] m_ApCostPerEveryCellCached;

	private bool m_AbilityHover;

	private int m_HoverUpdateDelayCounter;

	private AbilityData m_NewHoverAbility;

	private bool? m_NewHoeverStatus;

	private bool m_PathHidden;

	private readonly Dictionary<NetPlayer, PingData> m_PlayerPingData = new Dictionary<NetPlayer, PingData>();

	public List<Vector3> PointerCellDecalCornersPositions => m_CreatedPointerCellDecal.Or(null)?.CornersPositions ?? new List<Vector3> { PointerWorldCorrectedPosition };

	private Vector3 PointerWorldCorrectedPosition
	{
		get
		{
			if (Game.Instance.Controllers.ClickEventsController == null)
			{
				return Vector3.zero;
			}
			return Game.Instance.Controllers.ClickEventsController.GroundPosition - m_DecalOffset;
		}
	}

	[CanBeNull]
	private BaseUnitEntity SelectedUnit => Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value;

	private bool ShowPointerCellDecal
	{
		get
		{
			if (m_TbActive && (m_IsPlayerTurn || m_DeploymentPhase))
			{
				return !m_ShouldHide;
			}
			return false;
		}
	}

	public float MemorizedPathCost => m_MemorizedPaths.Values.FirstOrDefault()?.PathCost ?? 0f;

	private void Awake()
	{
		Instance = this;
		EventBus.Subscribe(this);
	}

	private void Start()
	{
		m_CameraRig = UnityEngine.Object.FindObjectOfType<CameraRig>();
	}

	private void OnEnable()
	{
		if (!(Instance != null))
		{
			Instance = this;
			EventBus.Subscribe(this);
		}
	}

	private void OnDisable()
	{
		Instance = null;
		EventBus.Unsubscribe(this);
	}

	public void OnAreaBeginUnloading()
	{
		UnityEngine.Object.Destroy(m_CreatedPathEndDecal);
		UnityEngine.Object.Destroy(m_CreatedSuicideDecal);
		UnityEngine.Object.Destroy(m_CreatedPointerCellDecal.gameObject);
		m_CreatedPingDecals.ForEach(UnityEngine.Object.Destroy);
		m_UpdateTaskCancelToken.Cancel();
	}

	public void OnAreaDidLoad()
	{
		m_ShouldHide = false;
		List<Color> coopPlayersPingsColors = ConfigRoot.Instance.UIConfig.CoopPlayersPingsColors;
		int num = Math.Min(m_CreatedPingDecals.Length, coopPlayersPingsColors.Count);
		if (Game.Instance.IsSpaceCombat)
		{
			m_CreatedPathEndDecal = CreateDecal(m_SpacePathEndDecal, m_SpacePathEndDecalColor);
			for (int i = 0; i < num; i++)
			{
				m_CreatedPingDecals[i] = CreateDecal(m_SpacePingDecal, coopPlayersPingsColors[i]);
			}
		}
		else
		{
			m_CreatedPathEndDecal = CreateDecal(m_PathEndDecal, m_PathEndDecalColor);
			for (int j = 0; j < num; j++)
			{
				m_CreatedPingDecals[j] = CreateDecal(m_PingDecal, coopPlayersPingsColors[j]);
			}
		}
		m_CreatedSuicideDecal = CreateDecal(m_SuicideDecal, m_SuicideDecalColor);
		m_CreatedPointerCellDecal = UnityEngine.Object.Instantiate((Game.Instance.CurrentModeType == GameModeType.SpaceCombat) ? m_PointerCellDecalSpace : m_PointerCellDecal, base.transform, worldPositionStays: true);
		m_UpdateTaskCancelToken.Cancel();
	}

	private void Update()
	{
		OnUpdate();
		if (m_UpdateTask.IsCompleted)
		{
			m_UpdateTaskCancelToken = new CancellationTokenSource();
			m_UpdateTask = OnUpdateAsync(m_UpdateTaskCancelToken.Token);
		}
	}

	private void OnUpdate()
	{
		if (!m_CreatedPointerCellDecal)
		{
			return;
		}
		m_CreatedPointerCellDecal.SetVisible(ShowPointerCellDecal);
		if (ShowPointerCellDecal)
		{
			CameraRig cameraRig = m_CameraRig.Or(null);
			if ((object)cameraRig == null || !cameraRig.RotationByMouse)
			{
				UpdateDecalScale();
				UpdateSizeOffset();
				UpdateCurrentNode();
				UpdatePredict();
				UpdateAbilityHover();
			}
		}
	}

	private void UpdateDecalScale()
	{
		m_DecalScale = GetCellScaleByUnit(SelectedUnit);
	}

	private int GetCellScaleByUnit(BaseUnitEntity unit)
	{
		if (unit == null)
		{
			return 1;
		}
		IntRect sizeRect = unit.SizeRect;
		sizeRect.ymax = sizeRect.ymin + sizeRect.Width - 1;
		if (!(Game.Instance.CursorController.SelectedAbility != null))
		{
			return sizeRect.Width;
		}
		return 1;
	}

	private void UpdateSizeOffset()
	{
		m_SizeOffset = GetCellOffsetForUnit(SelectedUnit);
		m_DecalOffset = ((m_DecalScale == 1) ? new Vector3(0f, 0f, 0f) : m_SizeOffset);
	}

	public Vector3 GetCellOffsetForUnit(MechanicEntity unit)
	{
		if (unit == null)
		{
			return Vector3.zero;
		}
		if (CurrentNode == null)
		{
			return Vector3.zero;
		}
		Vector3 zero = Vector3.zero;
		if (unit.Size == Size.Cruiser_2x4)
		{
			float num = GraphParamsMechanicsCache.GridCellSize / 2f;
			if (PointerWorldCorrectedPosition.x >= CurrentNode.Vector3Position().x)
			{
				zero.x += num;
			}
			else
			{
				zero.x -= num;
			}
			if (PointerWorldCorrectedPosition.z >= CurrentNode.Vector3Position().z)
			{
				zero.z += num;
			}
			else
			{
				zero.z -= num;
			}
		}
		else if (unit.Size.IsBigAndEvenUnit())
		{
			float num2 = GraphParamsMechanicsCache.GridCellSize / 2f;
			zero.x += num2;
			zero.z += num2;
		}
		return zero;
	}

	private void UpdateCurrentNode()
	{
		CurrentNode = (GridNode)PointerWorldCorrectedPosition.GetNearestNodeXZ();
	}

	private async Task OnUpdateAsync(CancellationToken token)
	{
		await DrawPathToPoint(m_CurrentDecalPosition, token);
	}

	private void UpdatePredict()
	{
		if (CurrentNode == null)
		{
			return;
		}
		if (InvalidNode(CurrentNode, PointerWorldCorrectedPosition))
		{
			UpdateInvalid(PointerWorldCorrectedPosition);
		}
		else if (SelectedUnit == null)
		{
			MechanicEntity currentUnit = Game.Instance.Controllers.TurnController.CurrentUnit;
			if (currentUnit != null && currentUnit.IsDirectlyControllable)
			{
				UpdateInactiveState();
			}
		}
		else
		{
			m_CurrentDecalPosition = CurrentNode.Vector3Position() + m_DecalOffset;
			SetDecalPosition(m_CreatedPointerCellDecal.transform, CurrentNode, m_CurrentDecalPosition);
			UpdatePredictState(CurrentNode);
		}
	}

	private bool InvalidNode(GridNode node, Vector3 worldPosition)
	{
		if (node == null)
		{
			return true;
		}
		if (!node.ContainsPoint(worldPosition))
		{
			return true;
		}
		if (Game.Instance.CursorController.SelectedAbility == null && WarhammerBlockManager.Instance.NodeContainsAny(node))
		{
			BaseUnitEntity selectedUnit = SelectedUnit;
			if (selectedUnit == null)
			{
				return true;
			}
			NodeList occupiedNodes = selectedUnit.GetOccupiedNodes();
			GridNodeBase nearestNodeXZUnwalkable = selectedUnit.Position.GetNearestNodeXZUnwalkable();
			if (nearestNodeXZUnwalkable == null)
			{
				return true;
			}
			Vector2Int coordinatesInGrid = nearestNodeXZUnwalkable.CoordinatesInGrid;
			Vector2Int coordinatesInGrid2 = node.CoordinatesInGrid;
			if (occupiedNodes.Contains(node))
			{
				return coordinatesInGrid2 == coordinatesInGrid;
			}
			return true;
		}
		return false;
	}

	private void UpdateInvalid(Vector3 worldPosition)
	{
		m_CreatedPointerCellDecal.transform.position = worldPosition;
		m_CurrentDecalPosition = null;
		if (Game.Instance.CursorController.SelectedAbility == null)
		{
			Game.Instance.CursorController.SetCursor(CursorType.Restricted);
		}
		UpdatePredictState(null);
	}

	private void UpdateInactiveState()
	{
		m_CurrentDecalPosition = CurrentNode.Vector3Position() + m_DecalOffset;
		SetDecalPosition(m_CreatedPointerCellDecal.transform, CurrentNode, m_CurrentDecalPosition);
		bool flag = m_TbActive && (m_IsPlayerTurn || m_DeploymentPhase) && !m_ShouldHide;
		m_CreatedPointerCellDecal.SetVisible(flag);
		if (flag)
		{
			m_CreatedPointerCellDecal.SetTargetType(PointerCellDecal.TargetType.Ground);
			UpdateMoveMarker();
			m_CreatedPointerCellDecal.SetActionType(PointerCellDecal.ActionType.Unable);
			UnitPredictionManager.Instance.Or(null)?.ResetVirtualHoverPosition();
		}
	}

	private void UpdatePredictState(GraphNode node)
	{
		bool flag = m_TbActive && (m_IsPlayerTurn || m_DeploymentPhase) && !m_ShouldHide;
		m_CreatedPointerCellDecal.SetVisible(flag);
		if (!flag)
		{
			return;
		}
		BaseUnitEntity selectedUnit = SelectedUnit;
		if (selectedUnit == null)
		{
			UpdateMoveMarker();
			return;
		}
		bool flag2 = false;
		Vector2Int coordinatesInGrid = selectedUnit.Position.GetNearestNodeXZUnwalkable().CoordinatesInGrid;
		Vector2Int? vector2Int = (node as GridNode)?.CoordinatesInGrid;
		if (node == null || coordinatesInGrid == vector2Int)
		{
			flag2 = true;
		}
		bool flag3 = (m_DeploymentPhase ? (flag2 || !ClickSurfaceDeploymentHandler.CanDeployUnit(node, selectedUnit.SizeRect)) : (flag2 || m_TooFarForUnit || !WarhammerBlockManager.Instance.CanUnitStandOnNode(selectedUnit, node as GridNode)));
		AbilityData selectedAbility = Game.Instance.CursorController.SelectedAbility;
		bool flag4 = selectedAbility != null && selectedAbility.TargetAnchor == AbilityTargetAnchor.Unit;
		m_CreatedPointerCellDecal.SetTargetType(flag4 ? PointerCellDecal.TargetType.Unit : PointerCellDecal.TargetType.Ground);
		PointerCellDecal.ActionType actionType;
		if (selectedAbility == null)
		{
			actionType = (flag3 ? PointerCellDecal.ActionType.Unable : PointerCellDecal.ActionType.Move);
			List<Vector3> list = m_MemorizedPaths.Values.FirstOrDefault()?.path?.vectorPath;
			bool state = false;
			if (list != null)
			{
				Vector3 vector = list[list.Count - 1];
				state = node != null && vector == node.Vector3Position();
			}
			Game.Instance.CursorController.TrySetMoveCursor(state, flag3);
		}
		else
		{
			PointerController clickEventsController = Game.Instance.Controllers.ClickEventsController;
			TargetWrapper targetForDesiredPosition = Game.Instance.Controllers.SelectedAbilityHandler.GetTargetForDesiredPosition(clickEventsController.PointerOn, clickEventsController.WorldPosition);
			actionType = ((targetForDesiredPosition != null && selectedAbility.CanTargetFromDesiredPosition(targetForDesiredPosition)) ? PointerCellDecal.ActionType.Attack : PointerCellDecal.ActionType.Unable);
		}
		UpdateMoveMarker();
		m_CreatedPointerCellDecal.SetActionType(actionType);
		if (flag3)
		{
			UnitPredictionManager.Instance?.ResetVirtualHoverPosition();
		}
		else
		{
			UnitPredictionManager.Instance?.SetVirtualHoverPosition(node.Vector3Position());
		}
	}

	private void UpdateMoveMarker()
	{
		m_CreatedPointerCellDecal.ShowPathEndMarker((m_PathRenderer.PathShown && Game.Instance.CursorController.SelectedAbility == null && !Game.Instance.Controllers.VirtualPositionController.HasVirtualPosition) || Game.Instance.Controllers.TurnController.IsPreparationTurn);
	}

	private async Task DrawPathToPoint(Vector3? position, CancellationToken token)
	{
		if (!ShowPointerCellDecal)
		{
			ClearActivePath();
		}
		else
		{
			if ((m_CameraRig.Or(null)?.RotationByMouse ?? false) || m_DeploymentPhase || !m_MemorizedPaths.Empty())
			{
				return;
			}
			BaseUnitEntity unit = SelectedUnit;
			if (unit == null || unit.CombatState.MovementPoints == 0f || unit.IsCastingAbility() || !position.HasValue || !TurnController.IsInTurnBasedCombat() || !Game.Instance.Controllers.TurnController.IsPlayerTurn || Game.Instance.CursorController.SelectedAbility != null || Game.Instance.CursorController.OnGui)
			{
				ClearActivePath();
			}
			else
			{
				if (position == m_LastShownPosition)
				{
					return;
				}
				m_LastShownPosition = position;
				if (!WarhammerBlockManager.Instance.CanUnitStandOnNode(unit, position.Value.GetNearestNodeXZUnwalkable()) || !unit.IsMyNetRole())
				{
					return;
				}
				Vector3 unitSizeOffset = m_SizeOffset;
				UnitMovementAgentBase unitMovementAgent = unit.View.MovementAgent;
				using PathDisposable<WarhammerPathPlayer> pathDisposable = await PathfindingService.Instance.FindPathTB_Task(unitMovementAgent, position.Value - unitSizeOffset, -1, this);
				token.ThrowIfCancellationRequested();
				if (!m_MemorizedPaths.Empty())
				{
					return;
				}
				WarhammerPathPlayer warhammerPathPlayer = pathDisposable?.Path;
				List<GraphNode> list = ((warhammerPathPlayer != null && !warhammerPathPlayer.error) ? warhammerPathPlayer.path : null);
				float cost = warhammerPathPlayer.CalculatedPath[^1].Length;
				if (list == null || list.Count < 2 || cost >= ConfigRoot.Instance.CombatRoot.ForbiddenNodesTraverseCost)
				{
					ClearActivePath();
					return;
				}
				m_TooFarForUnit = cost > unit.CombatState.MovementPoints;
				bool threateningStatus = warhammerPathPlayer.CalculatedPath[^1].IsOneWayPath || HasAttackOfOpportunityOnPath(unit, warhammerPathPlayer) || HasHarmfulEffectsOnPath(warhammerPathPlayer);
				SetupActivePath(list, m_TooFarForUnit, threateningStatus, unitSizeOffset, unitMovementAgent);
				UpdateMoveMarker();
				EventBus.RaiseEvent((IMechanicEntity)unit, (Action<IUnitPathManagerHandler>)delegate(IUnitPathManagerHandler h)
				{
					h.HandleCurrentNodeChanged(cost);
				}, isCheckRuntime: true);
			}
		}
	}

	public void AddPath(BaseUnitEntity unit, Path path, float apPerCell, float actionPointsBlue, bool oddDiagonal, float[] apCostPerEveryCell = null)
	{
		m_UnitCached = unit;
		m_ForcedPathCached = (path as ForcedPath)?.Clone();
		m_ApCostPerEveryCellCached = apCostPerEveryCell;
		PathData pathData = CreatePathData(unit, path);
		IntRect sizeRect = unit.SizeRect;
		sizeRect.ymax = sizeRect.ymin + sizeRect.Width - 1;
		Vector3 sizePositionOffset = SizePathfindingHelper.GetSizePositionOffset(sizeRect);
		float num = actionPointsBlue;
		int num2 = (oddDiagonal ? 1 : 0);
		for (int i = 1; i < path.vectorPath.Count; i++)
		{
			Vector2 a = path.vectorPath[i - 1].To2D();
			Vector2 b = path.vectorPath[i].To2D();
			bool num3 = a.IsDiagonal(b);
			if (num3)
			{
				num2++;
			}
			int num4 = ((!num3 || num2 % 2 != 0) ? 1 : 2);
			float num5 = ((apCostPerEveryCell != null && i < apCostPerEveryCell.Length) ? apCostPerEveryCell[i] : (apPerCell * (float)num4));
			num -= num5;
		}
		pathData.PathCost = actionPointsBlue - num;
		bool hasAttackOfOpportunity;
		List<BaseUnitEntity> enemiesAoO = GetAttackOfOpportunityOnPath(unit, path, out hasAttackOfOpportunity);
		pathData.HasThreats = hasAttackOfOpportunity || HasOneWayNodeLinkOnPath(path) || HasHarmfulEffectsOnPath(path);
		List<Vector3> vectorPath = path.vectorPath;
		Vector3 vector = vectorPath[vectorPath.Count - 1];
		GridNodeBase node = (GridNodeBase)AstarPath.active.GetNearest(vector).node;
		BaseUnitEntity firstUnit = node.GetFirstUnit();
		GameObject decal = ((firstUnit != null && !firstUnit.IsDead && !firstUnit.IsInPlayerParty) ? m_CreatedSuicideDecal : m_CreatedPathEndDecal);
		DrawDecalAt(decal, node, vector + sizePositionOffset);
		UpdatePredictState(node);
		UpdatePathRenderer();
		EventBus.RaiseEvent((IMechanicEntity)unit, (Action<IUnitPathManagerHandler>)delegate(IUnitPathManagerHandler h)
		{
			h.HandlePathAdded(path, pathData.PathCost, enemiesAoO);
		}, isCheckRuntime: true);
	}

	public void RemoveAllPaths()
	{
		foreach (AbstractUnitEntity item in new List<AbstractUnitEntity>(m_MemorizedPaths.Keys.ToList()))
		{
			RemovePathInternal(item);
		}
		UpdatePathRenderer();
	}

	public void RemovePath(AbstractUnitEntity unit)
	{
		if (RemovePathInternal(unit))
		{
			UpdatePathRenderer();
		}
	}

	private bool RemovePathInternal(AbstractUnitEntity unit)
	{
		if (m_MemorizedPaths.Remove(unit))
		{
			SetDecalVisibility(m_CreatedPathEndDecal, isVisible: false);
			SetDecalVisibility(m_CreatedSuicideDecal, isVisible: false);
			m_CreatedPointerCellDecal.SetVisible(visible: false);
			EventBus.RaiseEvent((IMechanicEntity)unit, (Action<IUnitPathManagerHandler>)delegate(IUnitPathManagerHandler h)
			{
				h.HandlePathRemoved();
			}, isCheckRuntime: true);
			m_UpdateTaskCancelToken.Cancel();
			if (!m_AbilityHover)
			{
				m_UnitCached = null;
				m_ForcedPathCached = null;
				m_ApCostPerEveryCellCached = null;
			}
			return true;
		}
		return false;
	}

	public Path GetPath(AbstractUnitEntity unit)
	{
		if (unit != null && m_MemorizedPaths.TryGetValue(unit, out var value))
		{
			return value.path;
		}
		return null;
	}

	public void DrawDecalAt(GameObject decal, GraphNode node, Vector3? overridePosition = null)
	{
		if (!(decal == null))
		{
			SetDecalVisibility(decal, isVisible: true);
			SetDecalPosition(decal.transform, node, overridePosition);
		}
	}

	private GameObject CreateDecal(GameObject prefab, Color color, bool active = false)
	{
		if (prefab == null)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab, base.transform, worldPositionStays: true);
		SetDecalColor(gameObject, color);
		SetDecalVisibility(gameObject, active);
		return gameObject;
	}

	private void SetDecalPosition(Transform decalTransform, GraphNode node, Vector3? overridePosition = null)
	{
		GridGraph obj = (GridGraph)node.Graph;
		Vector3 position = overridePosition ?? node.Vector3Position();
		obj.collision.CheckHeight(position, out var hit, out var _, 200f);
		decalTransform.position = position;
		Vector3 localScale = decalTransform.localScale;
		localScale.x = 1.35f * (float)m_DecalScale;
		localScale.z = 1.35f * (float)m_DecalScale;
		decalTransform.localScale = localScale;
		if (hit.normal != Vector3.zero)
		{
			decalTransform.rotation = Quaternion.LookRotation(decalTransform.forward, hit.normal.normalized);
		}
	}

	private void SetDecalColor(GameObject decal, Color color)
	{
		decal.GetComponentInChildren<Renderer>().material.color = color;
	}

	private void SetDecalVisibility(GameObject decal, bool isVisible)
	{
		if (decal != null)
		{
			decal.SetActive(isVisible);
		}
	}

	private PathData CreatePathData(BaseUnitEntity unit, Path path)
	{
		if (!m_MemorizedPaths.TryGetValue(unit, out var value))
		{
			value = new PathData();
			m_MemorizedPaths.Add(unit, value);
		}
		value.path = path;
		return value;
	}

	private void UpdatePathRenderer()
	{
		if (m_PathRenderer == null)
		{
			return;
		}
		if (true & m_TbActive)
		{
			foreach (KeyValuePair<AbstractUnitEntity, PathData> memorizedPath in m_MemorizedPaths)
			{
				AbstractUnitEntity key = memorizedPath.Key;
				if (key != null)
				{
					PathData value = memorizedPath.Value;
					List<GraphNode> list = value?.path?.path;
					if (list != null && list.Count >= 2)
					{
						SetupActivePath(list, pathUnableStatus: false, value.HasThreats, m_SizeOffset, key.MovementAgent);
						return;
					}
				}
			}
		}
		ClearActivePath();
	}

	private static bool HasOneWayNodeLinkOnPath(Path path)
	{
		foreach (GraphNode item in path.path)
		{
			if (item is LinkNode linkNode && linkNode.linkSource.component is WarhammerOneWayNodeLink)
			{
				return true;
			}
		}
		return false;
	}

	private bool HasHarmfulEffectsOnPath(Path path)
	{
		foreach (GraphNode item in path.path)
		{
			if (!(item is GridNodeBase node))
			{
				continue;
			}
			foreach (AreaEffectEntity areaEffect in node.GetAreaEffects())
			{
				if (areaEffect.IsHarmful())
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool HasAttackOfOpportunityOnPath(BaseUnitEntity unit, Path path)
	{
		return unit.GetEngagedByUnits().Any((BaseUnitEntity enemy) => CheckAttackOfOpportunityThreat(unit, path, enemy));
	}

	private List<BaseUnitEntity> GetAttackOfOpportunityOnPath(BaseUnitEntity unit, Path path, out bool hasAttackOfOpportunity)
	{
		hasAttackOfOpportunity = false;
		IEnumerable<BaseUnitEntity> engagedByUnitsAlongPath = unit.GetEngagedByUnitsAlongPath(path.path);
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		foreach (BaseUnitEntity item in engagedByUnitsAlongPath)
		{
			if (CheckAttackOfOpportunityThreat(unit, path, item))
			{
				list.Add(item);
				hasAttackOfOpportunity = true;
			}
		}
		return list;
	}

	private static bool CheckAttackOfOpportunityThreat(BaseUnitEntity unit, Path path, BaseUnitEntity enemy)
	{
		if (!enemy.CanAct)
		{
			return false;
		}
		if (enemy.IsInvisible)
		{
			return false;
		}
		if (!enemy.CanMakeAttackOfOpportunity(unit))
		{
			return false;
		}
		List<GraphNode> path2 = path.path;
		if (path2.Count < 2)
		{
			return false;
		}
		HashSet<GraphNode> threateningArea = enemy.GetThreateningArea();
		if (threateningArea == null)
		{
			return false;
		}
		bool num = path2.Take(path2.Count - 1).Any((GraphNode node) => threateningArea.Contains(node));
		bool flag = !threateningArea.Contains(path2.LastItem());
		if (!num || !flag)
		{
			return false;
		}
		return true;
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		TurnBasedModeHandle(isTurnBased);
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		UpdatePlayerTurn();
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		UpdatePlayerTurn();
	}

	public void HandleRoundStart(bool isTurnBased)
	{
		UpdatePlayerTurn();
	}

	private void UpdatePlayerTurn()
	{
		m_IsPlayerTurn = Game.Instance.Controllers.TurnController.IsPlayerTurn;
		m_UnitCached = null;
		m_ForcedPathCached = null;
		m_ApCostPerEveryCellCached = null;
	}

	public void HandleTurnBasedModeResumed()
	{
		TurnBasedModeHandle(isTurnBased: true);
	}

	private void TurnBasedModeHandle(bool isTurnBased)
	{
		m_TbActive = isTurnBased;
		if (m_CreatedPointerCellDecal != null)
		{
			m_CreatedPointerCellDecal.SetVisible(m_TbActive && !Game.Instance.Controllers.TurnController.IsPreparationTurn);
		}
		if (!m_TbActive)
		{
			RemoveAllPaths();
		}
		UpdatePlayerTurn();
	}

	public void HandleHideUI()
	{
		m_ShouldHide = true;
	}

	public void HandleShowUI()
	{
		DelayedInvoker.InvokeInTime(delegate
		{
			m_ShouldHide = false;
		}, 2.5f);
	}

	public void HandleBeginPreparationTurn(bool canDeploy)
	{
		m_DeploymentPhase = canDeploy;
		UpdatePlayerTurn();
	}

	public void HandleEndPreparationTurn()
	{
		m_DeploymentPhase = false;
		UpdatePlayerTurn();
	}

	public void HandleRoleSet(string entityId)
	{
		UpdatePathRenderer();
	}

	private void UpdateAbilityHover()
	{
		if (!m_NewHoeverStatus.HasValue)
		{
			return;
		}
		if (m_HoverUpdateDelayCounter > 0)
		{
			m_HoverUpdateDelayCounter--;
			if (m_HoverUpdateDelayCounter > 0)
			{
				return;
			}
		}
		bool value = m_NewHoeverStatus.Value;
		m_NewHoeverStatus = null;
		AbilityData newHoverAbility = m_NewHoverAbility;
		m_NewHoverAbility = null;
		m_AbilityHover = value;
		if (!value)
		{
			m_UpdateTaskCancelToken.Cancel();
		}
		if (newHoverAbility.TargetAnchor != 0)
		{
			return;
		}
		if (value)
		{
			if (!m_PathHidden)
			{
				m_PathHidden = true;
				Game.Instance.GameCommandQueue.ClearMovePrediction();
				UnitHelper.ClearPrediction();
			}
		}
		else if (m_PathHidden)
		{
			m_PathHidden = false;
			if (m_UnitCached is BaseUnitEntity unit && m_ForcedPathCached != null)
			{
				MoveCommandSettings moveCommandSettings = default(MoveCommandSettings);
				List<Vector3> vectorPath = m_ForcedPathCached.vectorPath;
				moveCommandSettings.Destination = vectorPath[vectorPath.Count - 1];
				moveCommandSettings.IsControllerGamepad = Game.Instance.IsControllerGamepad;
				MoveCommandSettings settings = moveCommandSettings;
				unit.TryCreateMoveCommandTB(settings, showMovePrediction: true);
			}
		}
	}

	public void HandleAbilityTargetHover(AbilityData ability, bool hover)
	{
		m_NewHoeverStatus = hover;
		m_NewHoverAbility = ability;
		m_HoverUpdateDelayCounter = 2;
	}

	public void HandleOwnerAbilitySelected(AbilityData ability)
	{
		m_UnitCached = null;
		m_ForcedPathCached = null;
		m_ApCostPerEveryCellCached = null;
		m_LastShownPosition = null;
	}

	public void HandleSetUnitMovableArea(List<GraphNode> nodes)
	{
		m_UpdateTaskCancelToken.Cancel();
	}

	public void HandleRemoveUnitMovableArea()
	{
		m_UpdateTaskCancelToken.Cancel();
	}

	public void HandlePingPosition(NetPlayer player, Vector3 position)
	{
		if (Game.Instance.CurrentModeType == GameModeType.GlobalMap || Game.Instance.CurrentModeType == GameModeType.StarSystem)
		{
			return;
		}
		int playerIndex = player.Index - 1;
		if (m_PlayerPingData.ContainsKey(player))
		{
			m_PlayerPingData[player].PingDelay?.Dispose();
			EventBus.RaiseEvent(delegate(INetAddPingMarker h)
			{
				h.HandleRemovePingPositionMarker(m_CreatedPingDecals[playerIndex]);
			});
		}
		else
		{
			m_PlayerPingData[player] = new PingData();
		}
		GridNodeBase node = (GridNodeBase)AstarPath.active.GetNearest(position).node;
		if (TurnController.IsInTurnBasedCombat())
		{
			position = node.Vector3Position();
		}
		PingData pingData = m_PlayerPingData[player];
		DrawDecalAt(m_CreatedPingDecals[playerIndex], node, position);
		EventBus.RaiseEvent(delegate(INetPingPosition h)
		{
			h.HandlePingPositionSound(m_CreatedPingDecals[playerIndex]);
		});
		EventBus.RaiseEvent(delegate(INetAddPingMarker h)
		{
			h.HandleAddPingPositionMarker(m_CreatedPingDecals[playerIndex]);
		});
		pingData.PingDelay = DelayedInvoker.InvokeInTime(delegate
		{
			SetDecalVisibility(m_CreatedPingDecals[playerIndex], isVisible: false);
			EventBus.RaiseEvent(delegate(INetAddPingMarker h)
			{
				h.HandleRemovePingPositionMarker(m_CreatedPingDecals[playerIndex]);
			});
		}, 7.5f);
	}

	public void HandlePingPositionSound(GameObject gameObject)
	{
	}

	private void ClearActivePath()
	{
		m_PathRenderer.Show(null, null, pathUnableStatus: false, threateningStatus: false, default(Vector3));
		m_LastShownPosition = null;
		EventBus.RaiseEvent(delegate(IUnitPathManagerHandler h)
		{
			h.HandlePathRemoved();
		});
	}

	private void SetupActivePath(List<GraphNode> nodes, bool pathUnableStatus, bool threateningStatus, Vector3 meshOffset, UnitMovementAgentBase movementAgent)
	{
		m_PathRenderer.Show(nodes, (movementAgent != null) ? movementAgent.transform : null, pathUnableStatus, threateningStatus, meshOffset);
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		m_LastShownPosition = null;
	}
}
