using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Controllers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Pointer;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.View.Mechanics.Destructible;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker;

public class LosVisualizer : MonoBehaviour, IAreaHandler, ISubscriber, ITurnBasedModeHandler, ITurnBasedModeResumeHandler, IInteractionHighlightUIHandler, IUnitDirectHoverUIHandler, IAbilityTargetSelectionUIHandler, IHologramPositionChangedHandler, IHologramClearHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, ITurnEndHandler, IPreparationTurnBeginHandler, IUnitMovementHandler, ISubscriber<IBaseUnitEntity>, IEntityPositionChangedHandler, ISubscriber<IEntity>, IGridObstacleCacheHandler, IGridObstacleEnabledHandler, IGridObstacleAwakeHandler, IAbilityTargetPossibilityCheck, IDynamicCoverProviderChangedHandler, INearUnitsCoverChangedHandler
{
	private struct EndpointEntry
	{
		public ObstacleMarker Marker;

		public bool IsLeft;
	}

	[FormerlySerializedAs("LayerMaskToShowFx")]
	[Tooltip("Collider masks which we will use to find obstacle points in static assets in the scene")]
	public string[] layerMaskToShowFx;

	[FormerlySerializedAs("MouseDistanceToShowMarkers")]
	[Tooltip("Radius around cursor to show markers for obstacles, in meters.")]
	public float mouseDistanceToShowMarkers = 5f;

	[Tooltip("The radius of the sphere for spherecast for getting the coordinate of vision obstacle to spawn VFX")]
	public float sphereCastRadius = 0.15f;

	[Tooltip("If true, the markers will be shown even if the player is not in combat.")]
	public bool showOutsideCombat;

	[Tooltip("Material assigned to cover markers at instantiation")]
	public Material coverMarkerMaterial;

	[Tooltip("Material assigned to LOS blocker markers at instantiation")]
	public Material losBlockerMarkerMaterial;

	[Tooltip("Visual height of the marker quad in meters (base to top)")]
	public float markerHeight = 1.35f;

	[Tooltip("Y threshold for corner welding: neighbour vertices farther apart than this are NOT welded together")]
	public float weldYThreshold = 1f;

	[Tooltip("Unity layer assigned to procedurally created marker GameObjects")]
	public int markerLayer = 10;

	[FormerlySerializedAs("LosLine")]
	[Tooltip("Prefab of LOS line")]
	public LineRenderer losLine;

	[FormerlySerializedAs("LosInterruptedFx")]
	[Tooltip("Prefab of VFX that will be spawned in vision obstruction point")]
	public GameObject losInterruptedFx;

	[Tooltip("Prefab of grayscale and brightness FX for characters who's not seen by player")]
	public GameObject grayscaleFxPrefab;

	[Tooltip("Speed of marker fade in/out (higher = faster)")]
	public float markerFadeSpeed = 10f;

	private List<ObstacleMarker> m_obstaclesMarkers = new List<ObstacleMarker>();

	private List<GridObstacle> m_obstacles = new List<GridObstacle>();

	private CameraRig m_cameraRig;

	private Vector2 m_lastMousePos = Vector2.zero;

	private Vector3 m_lastCameraPos = Vector3.zero;

	private bool m_isInited;

	private LineRenderer m_LosLineInstance;

	private GameObject m_LosInterruptedFxInstance;

	private MechanicEntity m_SelectedUnit;

	private Dictionary<MechanicEntity, GameObject> m_UnitAndFx = new Dictionary<MechanicEntity, GameObject>();

	private Dictionary<IDynamicCoverProvider, List<ObstacleMarker>> m_DynamicCoverMarkers = new Dictionary<IDynamicCoverProvider, List<ObstacleMarker>>();

	private Dictionary<IDynamicCoverProvider, HashSet<GridNodeBase>> m_DynamicCoverNodeSnapshot = new Dictionary<IDynamicCoverProvider, HashSet<GridNodeBase>>();

	private HashSet<BaseUnitEntity> m_NearUnitsCoverTrackedUnits = new HashSet<BaseUnitEntity>();

	private Dictionary<BaseUnitEntity, List<ObstacleMarker>> m_NearUnitsCoverMarkers = new Dictionary<BaseUnitEntity, List<ObstacleMarker>>();

	private Dictionary<BaseUnitEntity, HashSet<GridNodeBase>> m_NearUnitsCoverNodeSnapshot = new Dictionary<BaseUnitEntity, HashSet<GridNodeBase>>();

	private TurnController m_TurnController;

	private int? m_CachedLayerMaskToShowFx;

	private RaycastHit[] m_RaycastHits;

	private GameModeType m_GameModeType;

	private bool m_isGlobalTabHighlight;

	private Vector3 m_eyeHeightMod = Vector3.one;

	private const float CornerQuantizeStep = 0.05f;

	private int LayerMaskToShowFx
	{
		get
		{
			int valueOrDefault = m_CachedLayerMaskToShowFx.GetValueOrDefault();
			if (!m_CachedLayerMaskToShowFx.HasValue)
			{
				valueOrDefault = LayerMask.GetMask(layerMaskToShowFx);
				m_CachedLayerMaskToShowFx = valueOrDefault;
			}
			return m_CachedLayerMaskToShowFx.Value;
		}
	}

	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	private void Init()
	{
		using (ProfileScope.New("LosVisualizer.Markers.Init"))
		{
			m_isInited = false;
			ClearAll();
			if (!InitCheckData())
			{
				PFLog.TechArt.Error(this, "Fields in LosVisualizer has null values, disabling component");
				base.enabled = false;
			}
			if (IsProperGameMode())
			{
				m_TurnController = Game.Instance.Controllers.TurnController;
				m_cameraRig = CameraRig.Instance;
				m_obstacles = GetObstacles();
				m_obstaclesMarkers = SetupMarkers(m_obstacles);
				m_LosLineInstance = UnityEngine.Object.Instantiate(losLine, base.transform);
				m_LosLineInstance.enabled = false;
				m_LosInterruptedFxInstance = UnityEngine.Object.Instantiate(losInterruptedFx, base.transform);
				m_LosInterruptedFxInstance.SetActive(value: false);
				m_isInited = true;
				RescanExistingCoverProviders();
				RefreshMarkerGeometry();
			}
		}
	}

	private void RescanExistingCoverProviders()
	{
		using (ProfileScope.New("LosVisualizer.Markers.RescanProviders"))
		{
			foreach (MechanicEntity item in Game.Instance.Controllers.TurnController.EntitiesInCombat)
			{
				if (item is BaseUnitEntity baseUnitEntity)
				{
					if (baseUnitEntity.IsPlayerFaction && baseUnitEntity.GetOptional<PartNearUnitsProvideCover>() != null)
					{
						m_NearUnitsCoverTrackedUnits.Add(baseUnitEntity);
					}
					PartProvidesCover optional = baseUnitEntity.GetOptional<PartProvidesCover>();
					if (optional != null && optional.IsActive && !m_DynamicCoverMarkers.ContainsKey(optional))
					{
						m_DynamicCoverMarkers[optional] = CreateDynamicCoverMarkers(optional);
						m_DynamicCoverNodeSnapshot[optional] = ToNodeSet(optional.Nodes);
					}
				}
			}
			if (m_NearUnitsCoverTrackedUnits.Count > 0)
			{
				RebuildAllNearUnitsCoverMarkers();
			}
		}
	}

	private bool InitCheckData()
	{
		if (grayscaleFxPrefab == null)
		{
			PFLog.TechArt.Error(this, "No grayscale fx prefab in LosVisualizer");
			return false;
		}
		if (coverMarkerMaterial == null)
		{
			PFLog.TechArt.Error(this, "No cover marker material in LosVisualizer");
			return false;
		}
		if (losBlockerMarkerMaterial == null)
		{
			PFLog.TechArt.Error(this, "No los blocker marker material in LosVisualizer");
			return false;
		}
		if (losLine == null)
		{
			PFLog.TechArt.Error(this, "No los line prefab in LosVisualizer");
			return false;
		}
		if (losInterruptedFx == null)
		{
			PFLog.TechArt.Error(this, "No los interrupted FX prefab in LosVisualizer");
			return false;
		}
		return true;
	}

	private void Update()
	{
		if (!m_isInited)
		{
			return;
		}
		Game instance = Game.Instance;
		if (instance == null || GameUIState.Instance.IsLoadingProcess.Value)
		{
			return;
		}
		Player player = instance.Player;
		if (player == null || !player.IsInCombat)
		{
			return;
		}
		MechanicEntity currentUnit = instance.Controllers.TurnController.CurrentUnit;
		if (m_lastMousePos != CursorController.CursorPosition)
		{
			UpdateMarkers(currentUnit);
			m_lastMousePos = CursorController.CursorPosition;
			return;
		}
		Vector3 position = m_cameraRig.transform.position;
		if (m_lastCameraPos != position)
		{
			UpdateMarkers(currentUnit);
			m_lastCameraPos = position;
		}
	}

	private List<GridObstacle> GetObstacles()
	{
		return UnityEngine.Object.FindObjectsByType<GridObstacle>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();
	}

	private DestructionStagesViewManager[] GetDestructionManagers()
	{
		return UnityEngine.Object.FindObjectsByType<DestructionStagesViewManager>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
	}

	private List<ObstacleMarker> SetupMarkers(List<GridObstacle> obstacles)
	{
		using (ProfileScope.New("LosVisualizer.Markers.SetupStatic"))
		{
			List<ObstacleMarker> list = new List<ObstacleMarker>();
			foreach (GridObstacle obstacle in obstacles)
			{
				if (obstacle.Type != 0 && !obstacle._hideArVisual)
				{
					list.Add(SetupObstacleMarker(obstacle));
				}
			}
			return list;
		}
	}

	private ObstacleMarker SetupObstacleMarker(GridObstacle obstacle)
	{
		Quaternion quaternion = (obstacle._Rotate ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity);
		Quaternion inputRotation = Quaternion.Euler(0f, ((Component)(object)obstacle).transform.eulerAngles.y, 0f) * quaternion;
		inputRotation = GetStepRotation(inputRotation, 90f);
		ObstacleMarker obstacleMarker = CreateMarkerGameObject($"ObstacleMarker_{obstacle.Type}", ((Component)(object)obstacle).transform.position, inputRotation, obstacle.Type);
		obstacleMarker.OwnerObstacle = obstacle;
		return obstacleMarker;
	}

	private ObstacleMarker CreateMarkerGameObject(string name, Vector3 position, Quaternion rotation, LosCalculations.CoverType type)
	{
		using (ProfileScope.New("LosVisualizer.Markers.CreateOne"))
		{
			float num = 1.Cells().Meters * 0.5f;
			Material sharedMaterial = ((type == LosCalculations.CoverType.Cover) ? coverMarkerMaterial : losBlockerMarkerMaterial);
			GameObject obj = new GameObject(name);
			obj.layer = markerLayer;
			obj.transform.SetParent(base.transform);
			obj.transform.SetPositionAndRotation(position, rotation);
			MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = sharedMaterial;
			BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
			boxCollider.size = new Vector3(2f * num, markerHeight, 0.01f);
			boxCollider.center = new Vector3(0f, markerHeight * 0.5f, 0f);
			ObstacleMarker obstacleMarker = obj.AddComponent<ObstacleMarker>();
			Vector3 vector = rotation * Vector3.right;
			Vector3 leftEndpoint = position - vector * num;
			Vector3 rightEndpoint = position + vector * num;
			obstacleMarker.Initialize(meshFilter, meshRenderer, boxCollider, leftEndpoint, rightEndpoint, num, markerHeight, type);
			return obstacleMarker;
		}
	}

	public void HandleGridObstacleEnabled(GridObstacle gridObstacle, bool enabled)
	{
		using (ProfileScope.New("LosVisualizer.Markers.HandleEnabled"))
		{
			AddDynamicObstacle(gridObstacle);
			foreach (ObstacleMarker obstaclesMarker in m_obstaclesMarkers)
			{
				if (!((UnityEngine.Object)(object)obstaclesMarker.OwnerObstacle != (UnityEngine.Object)(object)gridObstacle))
				{
					obstaclesMarker.gameObject.SetActive(enabled);
					if (m_isInited)
					{
						RefreshMarkerGeometry();
					}
					break;
				}
			}
		}
	}

	private void UpdateMarkersState()
	{
		using (ProfileScope.New("LosVisualizer.Markers.UpdateMovement"))
		{
			RemoveDestroyedMarkers();
			foreach (ObstacleMarker obstaclesMarker in m_obstaclesMarkers)
			{
				Quaternion quaternion = (obstaclesMarker.OwnerObstacle._Rotate ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity);
				Quaternion quaternion2 = Quaternion.Euler(0f, ((Component)(object)obstaclesMarker.OwnerObstacle).transform.eulerAngles.y, 0f);
				Quaternion stepRotation = GetStepRotation(quaternion2 * quaternion, 90f);
				obstaclesMarker.transform.rotation = stepRotation;
				obstaclesMarker.transform.position = ((Component)(object)obstaclesMarker.OwnerObstacle).transform.position;
				obstaclesMarker.UpdateEndpoints(stepRotation);
			}
			RefreshMarkerGeometry();
		}
	}

	private void RemoveDestroyedMarkers()
	{
		using (ProfileScope.New("LosVisualizer.Markers.RemoveDestroyed"))
		{
			for (int num = m_obstaclesMarkers.Count - 1; num >= 0; num--)
			{
				ObstacleMarker obstacleMarker = m_obstaclesMarkers[num];
				if (obstacleMarker == null || (UnityEngine.Object)(object)obstacleMarker.OwnerObstacle == null)
				{
					if (obstacleMarker != null)
					{
						UnityEngine.Object.Destroy(obstacleMarker.gameObject);
					}
					m_obstaclesMarkers.RemoveAt(num);
				}
			}
			m_obstacles.RemoveAll((GridObstacle o) => (UnityEngine.Object)(object)o == null);
		}
	}

	private Quaternion GetStepRotation(Quaternion inputRotation, float stepValue)
	{
		Vector3 eulerAngles = inputRotation.eulerAngles;
		eulerAngles = new Vector3(Mathf.Round(eulerAngles.x / stepValue) * stepValue, Mathf.Round(eulerAngles.y / stepValue) * stepValue, Mathf.Round(eulerAngles.z / stepValue) * stepValue);
		return Quaternion.Euler(eulerAngles);
	}

	private Vector3 GetMouseWorldPosition()
	{
		Vector3 zero = Vector3.zero;
		if (Physics.Raycast(m_cameraRig.Camera.ScreenPointToRay(CursorController.CursorPosition), out var hitInfo, 1000f))
		{
			return hitInfo.point;
		}
		return zero;
	}

	private bool IsProperGameMode()
	{
		m_GameModeType = Game.Instance.CurrentModeType;
		if (m_GameModeType != GameModeType.None && m_GameModeType != GameModeType.GlobalMap)
		{
			return m_GameModeType != GameModeType.CutsceneGlobalMap;
		}
		return false;
	}

	private void UpdateMarkers(MechanicEntity selectedUnit)
	{
		using (ProfileScope.New("LosVisualizer.Markers.UpdateFade"))
		{
			if (m_isGlobalTabHighlight || (m_obstaclesMarkers.Count == 0 && m_DynamicCoverMarkers.Count == 0 && m_NearUnitsCoverMarkers.Count == 0) || (!m_TurnController.IsPlayerTurn && !m_TurnController.IsPreparationTurn))
			{
				return;
			}
			Vector3 mouseWorldPosition = GetMouseWorldPosition();
			float num = mouseDistanceToShowMarkers * mouseDistanceToShowMarkers;
			bool flag = selectedUnit != null && selectedUnit.Size >= Size.Large;
			for (int i = 0; i < m_obstaclesMarkers.Count; i++)
			{
				ObstacleMarker obstacleMarker = m_obstaclesMarkers[i];
				if (obstacleMarker.Type != 0)
				{
					if (flag && obstacleMarker.Type == LosCalculations.CoverType.Cover)
					{
						obstacleMarker.EnableMarker(enable: false, markerFadeSpeed);
						continue;
					}
					bool enable = (obstacleMarker.transform.position - mouseWorldPosition).sqrMagnitude <= num;
					obstacleMarker.EnableMarker(enable, markerFadeSpeed);
				}
			}
			UpdateDynamicMarkers(m_DynamicCoverMarkers, mouseWorldPosition, num, flag);
			UpdateDynamicMarkers(m_NearUnitsCoverMarkers, mouseWorldPosition, num, flag);
		}
	}

	private void UpdateDynamicMarkers<TKey>(Dictionary<TKey, List<ObstacleMarker>> markers, Vector3 mouseWorldPosition, float distSqr, bool isLargeUnit)
	{
		foreach (KeyValuePair<TKey, List<ObstacleMarker>> marker in markers)
		{
			foreach (ObstacleMarker item in marker.Value)
			{
				if (isLargeUnit && item.Type == LosCalculations.CoverType.Cover)
				{
					item.EnableMarker(enable: false, markerFadeSpeed);
					continue;
				}
				bool enable = (item.transform.position - mouseWorldPosition).sqrMagnitude <= distSqr;
				item.EnableMarker(enable, markerFadeSpeed);
			}
		}
	}

	private void ToggleAllMarkers(bool state)
	{
		using (ProfileScope.New("LosVisualizer.Markers.ToggleAll"))
		{
			foreach (ObstacleMarker obstaclesMarker in m_obstaclesMarkers)
			{
				obstaclesMarker.EnableMarker(state, markerFadeSpeed);
			}
			ToggleDynamicMarkers(m_DynamicCoverMarkers, state);
			ToggleDynamicMarkers(m_NearUnitsCoverMarkers, state);
		}
	}

	private void ToggleDynamicMarkers<TKey>(Dictionary<TKey, List<ObstacleMarker>> markers, bool state)
	{
		foreach (KeyValuePair<TKey, List<ObstacleMarker>> marker in markers)
		{
			foreach (ObstacleMarker item in marker.Value)
			{
				item.EnableMarker(state, markerFadeSpeed);
			}
		}
	}

	private void ClearAll()
	{
		using (ProfileScope.New("LosVisualizer.Markers.ClearAll"))
		{
			foreach (ObstacleMarker obstaclesMarker in m_obstaclesMarkers)
			{
				UnityEngine.Object.Destroy(obstaclesMarker.gameObject);
			}
			ClearLosVisual();
			m_obstaclesMarkers.Clear();
			m_obstacles = null;
			ClearAllDynamicCoverMarkers();
			ClearAllNearUnitsCoverMarkers();
			ClearGrayscaleFxs();
		}
	}

	private void ClearLosVisual()
	{
		if ((bool)m_LosLineInstance)
		{
			UnityEngine.Object.Destroy(m_LosLineInstance.gameObject);
		}
		if ((bool)m_LosInterruptedFxInstance)
		{
			UnityEngine.Object.Destroy(m_LosInterruptedFxInstance.gameObject);
		}
	}

	private void DisableAllMarkers()
	{
		using (ProfileScope.New("LosVisualizer.Markers.DisableAll"))
		{
			foreach (ObstacleMarker obstaclesMarker in m_obstaclesMarkers)
			{
				obstaclesMarker.EnableMarker(enable: false, markerFadeSpeed);
			}
			ToggleDynamicMarkers(m_DynamicCoverMarkers, state: false);
			ToggleDynamicMarkers(m_NearUnitsCoverMarkers, state: false);
		}
	}

	public void OnAreaBeginUnloading()
	{
		m_isInited = false;
		ClearAll();
	}

	public void OnAreaDidLoad()
	{
		Init();
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		DisableAllMarkers();
		if (!isTurnBased)
		{
			ClearAllDynamicCoverMarkers();
			ClearAllNearUnitsCoverMarkers();
			ClearGrayscaleFxs();
			DisableLosVisual();
		}
	}

	private void DisableLosVisual()
	{
		m_LosInterruptedFxInstance.SetActive(value: false);
		m_LosLineInstance.enabled = false;
	}

	public void HandleHighlightChange(bool isOn)
	{
		if (showOutsideCombat || Game.Instance.Player.IsInCombat)
		{
			m_isGlobalTabHighlight = isOn;
			ToggleAllMarkers(isOn);
		}
	}

	public void HandleHoverChange(AbstractUnitEntityView unitEntityView, bool isHover, bool isDirect)
	{
		if (isHover && Game.Instance.Player.IsInCombat)
		{
			DrawLosFx(unitEntityView);
			return;
		}
		m_LosInterruptedFxInstance.SetActive(value: false);
		m_LosLineInstance.enabled = false;
	}

	private Vector3 CalculateEyeShiftToCellBorder(Vector3 from, Vector3 to)
	{
		m_eyeHeightMod = LosCalculations.EyeShift;
		Vector3 vector = from + m_eyeHeightMod;
		Vector3 vector2 = (to + m_eyeHeightMod - vector).ToXZ();
		float num = 1.Cells().Meters * 0.5f;
		float num2 = Math.Max(Math.Abs(vector2.x), Math.Abs(vector2.z));
		float num3 = num / num2;
		return vector2 * num3 * 0.99f;
	}

	private void DrawLosFxBegin(LosDescription losDescription, Vector3 playerEyePosition, Vector3 targetEyePosition)
	{
		using (ProfileScope.NewScope("DrawLosFxBegin"))
		{
			if ((LosCalculations.CoverType)losDescription == LosCalculations.CoverType.LosBlocker)
			{
				float num = sphereCastRadius;
				m_RaycastHits = new RaycastHit[10];
				m_LosLineInstance.enabled = true;
				m_LosLineInstance.SetPosition(0, playerEyePosition);
				Vector3 vector = targetEyePosition;
				Vector3 normalized = (targetEyePosition - playerEyePosition).normalized;
				float maxDistance = Vector3.Distance(playerEyePosition, targetEyePosition);
				int num2 = Physics.RaycastNonAlloc(playerEyePosition, normalized, m_RaycastHits, maxDistance, LayerMaskToShowFx);
				if (num2 == 0)
				{
					for (float num3 = 0.05f; num3 <= 2f; num3 += 0.05f)
					{
						num += num3;
						num2 = Physics.SphereCastNonAlloc(playerEyePosition, num, normalized, m_RaycastHits, maxDistance, LayerMaskToShowFx);
						if (num2 > 0)
						{
							break;
						}
					}
				}
				bool active = num2 > 0;
				vector = m_RaycastHits[0].point;
				float num4 = Vector3.Dot(vector - playerEyePosition, normalized);
				Vector3 b = playerEyePosition + normalized * num4;
				vector = Vector3.Lerp(vector, b, 0.5f * num);
				m_LosInterruptedFxInstance.transform.position = vector;
				m_LosInterruptedFxInstance.SetActive(active);
				m_LosLineInstance.SetPosition(1, vector);
			}
			else
			{
				m_LosInterruptedFxInstance.SetActive(value: false);
				m_LosLineInstance.enabled = false;
			}
		}
	}

	private void DrawLosFx(Vector3? targetPoint)
	{
		UnitHologram unitHologram = UnitPredictionManager.Instance?.GetHologram();
		MechanicEntity value = Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value;
		IntRect rectForSize = SizePathfindingHelper.GetRectForSize(Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value.Size);
		LosDescription warhammerLos;
		if (unitHologram?.HologramEntityView == null)
		{
			if (value == null)
			{
				return;
			}
			warhammerLos = LosCalculations.GetWarhammerLos(value.View.transform.position, rectForSize, targetPoint.Value, rectForSize);
		}
		else
		{
			warhammerLos = LosCalculations.GetWarhammerLos(unitHologram.HologramEntityView.transform.position, rectForSize, targetPoint.Value, rectForSize);
		}
		m_eyeHeightMod = LosCalculations.EyeShift;
		Vector3 vector = ((unitHologram?.HologramEntityView != null) ? unitHologram.HologramEntityView.transform.position : Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value.ViewPosition);
		Vector3 playerEyePosition = vector + m_eyeHeightMod;
		Vector3 targetEyePosition = targetPoint.Value + m_eyeHeightMod;
		playerEyePosition += CalculateEyeShiftToCellBorder(vector, targetPoint.Value);
		DrawLosFxBegin(warhammerLos, playerEyePosition, targetEyePosition);
	}

	private void DrawLosFx(AbstractUnitEntityView unitEntityView)
	{
		UnitHologram unitHologram = UnitPredictionManager.Instance?.GetHologram();
		MechanicEntity value = Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value;
		MechanicEntity data = unitEntityView.Data;
		LosDescription warhammerLos;
		if (unitHologram?.HologramEntityView == null)
		{
			if (value == null)
			{
				return;
			}
			warhammerLos = LosCalculations.GetWarhammerLos(value, data);
		}
		else
		{
			IntRect rectForSize = SizePathfindingHelper.GetRectForSize(Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value.Size);
			warhammerLos = LosCalculations.GetWarhammerLos(unitHologram.HologramEntityView.transform.position, rectForSize, data.View.transform.position, rectForSize);
		}
		m_eyeHeightMod = LosCalculations.EyeShift;
		Vector3 vector = ((unitHologram?.HologramEntityView != null) ? unitHologram.HologramEntityView.transform.position : Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value.View.transform.position);
		Vector3 playerEyePosition = vector + m_eyeHeightMod;
		Vector3 targetEyePosition = unitEntityView.transform.position + m_eyeHeightMod;
		playerEyePosition += CalculateEyeShiftToCellBorder(vector, unitEntityView.transform.position);
		DrawLosFxBegin(warhammerLos, playerEyePosition, targetEyePosition);
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
	}

	public void HandleHologramPositionChanged()
	{
		UnitHologram unitHologram = UnitPredictionManager.Instance?.GetHologram();
		if (unitHologram != null)
		{
			ApplyGrayscaleFx(unitHologram.HologramEntityView.transform.position);
		}
	}

	private void ApplyGrayscaleFx(Vector3 losCalculateFrom)
	{
		IntRect rectForSize = SizePathfindingHelper.GetRectForSize(Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value.Size);
		foreach (MechanicEntity item in Game.Instance.Controllers.TurnController.EntitiesInCombat)
		{
			if (!item.IsPlayerEnemy)
			{
				continue;
			}
			LosDescription warhammerLos = LosCalculations.GetWarhammerLos(losCalculateFrom, rectForSize, item.ViewPosition, rectForSize);
			if (!m_UnitAndFx.ContainsKey(item) || (LosCalculations.CoverType)warhammerLos != LosCalculations.CoverType.LosBlocker)
			{
				if (!m_UnitAndFx.ContainsKey(item) && (LosCalculations.CoverType)warhammerLos == LosCalculations.CoverType.LosBlocker)
				{
					m_UnitAndFx.Add(item, FxHelper.SpawnFxOnEntity(grayscaleFxPrefab, item.View));
				}
				if (m_UnitAndFx.ContainsKey(item) && (LosCalculations.CoverType)warhammerLos != LosCalculations.CoverType.LosBlocker)
				{
					FxHelper.Destroy(m_UnitAndFx[item]);
					m_UnitAndFx.Remove(item);
				}
			}
		}
	}

	private bool IsValidUnit()
	{
		if (Game.Instance.Controllers.TurnController.CurrentUnit == null || Game.Instance.Controllers.TurnController.CurrentUnit.IsPlayerEnemy || Game.Instance.Controllers.SelectionCharacter == null || Game.Instance.Controllers.SelectionCharacter.SelectedUnit == null || Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value == null)
		{
			return false;
		}
		return true;
	}

	public void HandleHologramClear()
	{
		if (IsValidUnit())
		{
			ApplyGrayscaleFx(Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value.Position);
		}
	}

	private void ClearGrayscaleFxs()
	{
		foreach (KeyValuePair<MechanicEntity, GameObject> item in m_UnitAndFx)
		{
			FxHelper.Destroy(item.Value);
		}
		m_UnitAndFx.Clear();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (Game.Instance.Controllers.TurnController.CurrentUnit != null)
		{
			if (Game.Instance.Controllers.TurnController.CurrentUnit.IsPlayerEnemy)
			{
				ClearGrayscaleFxs();
			}
			else if (Game.Instance.Controllers.SelectionCharacter != null && Game.Instance.Controllers.SelectionCharacter.SelectedUnit != null && Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value != null)
			{
				ApplyGrayscaleFx(Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value.Position);
			}
		}
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
	}

	public void HandleTurnBasedModeResumed()
	{
		TurnController turnController = Game.Instance.Controllers.TurnController;
		if (turnController.IsPreparationTurn)
		{
			HandleBeginPreparationTurn(turnController.IsDeploymentAllowed);
		}
	}

	public void HandleBeginPreparationTurn(bool canDeploy)
	{
		if (IsValidUnit())
		{
			ApplyGrayscaleFx(Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value.Position);
		}
	}

	public void HandleMovementComplete()
	{
	}

	public void HandleEntityPositionChanged()
	{
		if (m_isInited)
		{
			MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
			if (mechanicEntity != null && mechanicEntity.IsInCombat)
			{
				if (m_DynamicCoverMarkers.Count > 0)
				{
					RebuildAllDynamicCoverMarkers();
				}
				if (m_NearUnitsCoverTrackedUnits.Count > 0)
				{
					RebuildAllNearUnitsCoverMarkers();
				}
			}
		}
		if (IsValidUnit())
		{
			ApplyGrayscaleFx(Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value.Position);
		}
	}

	public void HandleWaypointUpdate()
	{
	}

	public void HandleGridObstacleCacheUpdated()
	{
		if (m_isInited)
		{
			UpdateMarkersState();
		}
	}

	public void HandleGridObstacleAwake(GridObstacle gridObstacle)
	{
		AddDynamicObstacle(gridObstacle);
	}

	private void AddDynamicObstacle(GridObstacle gridObstacle)
	{
		using (ProfileScope.New("LosVisualizer.Markers.AddDynamicObstacle"))
		{
			if (m_isInited && !m_obstacles.Contains(gridObstacle))
			{
				m_obstacles.Add(gridObstacle);
				if (!gridObstacle._hideArVisual)
				{
					m_obstaclesMarkers.Add(SetupObstacleMarker(gridObstacle));
					RefreshMarkerGeometry();
				}
			}
		}
	}

	private List<ObstacleMarker> CreateBoundaryEdgeMarkers(NodeList nodes, LosCalculations.CoverType coverType)
	{
		using (ProfileScope.New("LosVisualizer.Markers.CreateBoundary"))
		{
			List<ObstacleMarker> list = new List<ObstacleMarker>();
			if (nodes.IsEmpty)
			{
				return list;
			}
			HashSet<GridNodeBase> hashSet = new HashSet<GridNodeBase>();
			foreach (GridNodeBase item in nodes)
			{
				hashSet.Add(item);
			}
			foreach (GridNodeBase item2 in nodes)
			{
				for (int i = 0; i < 4; i++)
				{
					GridNodeBase neighbourAlongDirection = item2.GetNeighbourAlongDirection(i, checkConnectivity: false);
					if (neighbourAlongDirection != null && !hashSet.Contains(neighbourAlongDirection))
					{
						Vector3 vector = item2.Vector3Position();
						Vector3 vector2 = neighbourAlongDirection.Vector3Position();
						Vector3 position = (vector + vector2) * 0.5f;
						Quaternion rotation = ((i == 1 || i == 3) ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity);
						list.Add(CreateMarkerGameObject($"BoundaryMarker_{coverType}", position, rotation, coverType));
					}
				}
			}
			return list;
		}
	}

	private void RefreshMarkerGeometry()
	{
		using (ProfileScope.New("LosVisualizer.Markers.Refresh"))
		{
			DeduplicateMarkers();
			WeldAllCorners();
		}
	}

	private void DeduplicateMarkers()
	{
		using (ProfileScope.New("LosVisualizer.Markers.Deduplicate"))
		{
			Dictionary<Vector3Int, List<ObstacleMarker>> dictionary = new Dictionary<Vector3Int, List<ObstacleMarker>>();
			CollectMarkersByEdge(m_obstaclesMarkers, dictionary);
			foreach (KeyValuePair<IDynamicCoverProvider, List<ObstacleMarker>> dynamicCoverMarker in m_DynamicCoverMarkers)
			{
				CollectMarkersByEdge(dynamicCoverMarker.Value, dictionary);
			}
			foreach (KeyValuePair<BaseUnitEntity, List<ObstacleMarker>> nearUnitsCoverMarker in m_NearUnitsCoverMarkers)
			{
				CollectMarkersByEdge(nearUnitsCoverMarker.Value, dictionary);
			}
			HashSet<ObstacleMarker> hashSet = new HashSet<ObstacleMarker>();
			foreach (KeyValuePair<Vector3Int, List<ObstacleMarker>> item in dictionary)
			{
				List<ObstacleMarker> value = item.Value;
				if (value.Count < 2)
				{
					continue;
				}
				ObstacleMarker obstacleMarker = value[0];
				for (int i = 1; i < value.Count; i++)
				{
					if (GetMarkerPriority(value[i].Type) > GetMarkerPriority(obstacleMarker.Type))
					{
						obstacleMarker = value[i];
					}
				}
				foreach (ObstacleMarker item2 in value)
				{
					if (item2 != obstacleMarker)
					{
						hashSet.Add(item2);
					}
				}
			}
			if (hashSet.Count == 0)
			{
				return;
			}
			RemoveDeduplicated(m_obstaclesMarkers, hashSet);
			foreach (KeyValuePair<IDynamicCoverProvider, List<ObstacleMarker>> dynamicCoverMarker2 in m_DynamicCoverMarkers)
			{
				RemoveDeduplicated(dynamicCoverMarker2.Value, hashSet);
			}
			foreach (KeyValuePair<BaseUnitEntity, List<ObstacleMarker>> nearUnitsCoverMarker2 in m_NearUnitsCoverMarkers)
			{
				RemoveDeduplicated(nearUnitsCoverMarker2.Value, hashSet);
			}
		}
	}

	private static void CollectMarkersByEdge(List<ObstacleMarker> source, Dictionary<Vector3Int, List<ObstacleMarker>> byEdge)
	{
		foreach (ObstacleMarker item in source)
		{
			if (!(item == null))
			{
				Vector3Int key = QuantizeCornerXZ(item.transform.position);
				if (!byEdge.TryGetValue(key, out var value))
				{
					value = (byEdge[key] = new List<ObstacleMarker>());
				}
				value.Add(item);
			}
		}
	}

	private static int GetMarkerPriority(LosCalculations.CoverType type)
	{
		return type switch
		{
			LosCalculations.CoverType.LosBlocker => 2, 
			LosCalculations.CoverType.Cover => 1, 
			LosCalculations.CoverType.Obstacle => 0, 
			_ => -1, 
		};
	}

	private void RemoveDeduplicated(List<ObstacleMarker> list, HashSet<ObstacleMarker> toDestroy)
	{
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (toDestroy.Contains(list[num]))
			{
				UnityEngine.Object.Destroy(list[num].gameObject);
				list.RemoveAt(num);
			}
		}
	}

	private void WeldAllCorners()
	{
		using (ProfileScope.New("LosVisualizer.Markers.Weld"))
		{
			Dictionary<Vector3Int, List<EndpointEntry>> dictionary = new Dictionary<Vector3Int, List<EndpointEntry>>();
			List<ObstacleMarker> list = new List<ObstacleMarker>();
			CollectMarkersForWelding(m_obstaclesMarkers, list, dictionary);
			foreach (KeyValuePair<IDynamicCoverProvider, List<ObstacleMarker>> dynamicCoverMarker in m_DynamicCoverMarkers)
			{
				CollectMarkersForWelding(dynamicCoverMarker.Value, list, dictionary);
			}
			foreach (KeyValuePair<BaseUnitEntity, List<ObstacleMarker>> nearUnitsCoverMarker in m_NearUnitsCoverMarkers)
			{
				CollectMarkersForWelding(nearUnitsCoverMarker.Value, list, dictionary);
			}
			Dictionary<ObstacleMarker, Vector4> dictionary2 = new Dictionary<ObstacleMarker, Vector4>(list.Count);
			foreach (ObstacleMarker item in list)
			{
				dictionary2[item] = new Vector4(item.BaseY, item.TopY, item.BaseY, item.TopY);
			}
			foreach (KeyValuePair<Vector3Int, List<EndpointEntry>> item2 in dictionary)
			{
				List<EndpointEntry> value = item2.Value;
				if (value.Count >= 2)
				{
					value.Sort(CompareByBaseY);
					ApplyClusterWelding(value, dictionary2, isBot: true);
					value.Sort(CompareByTopY);
					ApplyClusterWelding(value, dictionary2, isBot: false);
				}
			}
			foreach (ObstacleMarker item3 in list)
			{
				Vector4 vector = dictionary2[item3];
				item3.SetCornerYs(vector.x, vector.y, vector.z, vector.w);
			}
		}
	}

	private static int CompareByBaseY(EndpointEntry a, EndpointEntry b)
	{
		return a.Marker.BaseY.CompareTo(b.Marker.BaseY);
	}

	private static int CompareByTopY(EndpointEntry a, EndpointEntry b)
	{
		return a.Marker.TopY.CompareTo(b.Marker.TopY);
	}

	private static void CollectMarkersForWelding(List<ObstacleMarker> source, List<ObstacleMarker> allMarkers, Dictionary<Vector3Int, List<EndpointEntry>> bucket)
	{
		foreach (ObstacleMarker item in source)
		{
			if (!(item == null) && item.gameObject.activeSelf)
			{
				allMarkers.Add(item);
				AddEndpointToBucket(bucket, item, isLeft: true);
				AddEndpointToBucket(bucket, item, isLeft: false);
			}
		}
	}

	private static void AddEndpointToBucket(Dictionary<Vector3Int, List<EndpointEntry>> bucket, ObstacleMarker m, bool isLeft)
	{
		Vector3Int key = QuantizeCornerXZ(isLeft ? m.LeftEndpoint : m.RightEndpoint);
		if (!bucket.TryGetValue(key, out var value))
		{
			value = (bucket[key] = new List<EndpointEntry>());
		}
		value.Add(new EndpointEntry
		{
			Marker = m,
			IsLeft = isLeft
		});
	}

	private static Vector3Int QuantizeCornerXZ(Vector3 worldPos)
	{
		return new Vector3Int(Mathf.RoundToInt(worldPos.x / 0.05f), 0, Mathf.RoundToInt(worldPos.z / 0.05f));
	}

	private void ApplyClusterWelding(List<EndpointEntry> entries, Dictionary<ObstacleMarker, Vector4> resolved, bool isBot)
	{
		int num = 0;
		while (num < entries.Count)
		{
			int i;
			for (i = num + 1; i < entries.Count; i++)
			{
				float clusterY = GetClusterY(entries[i - 1].Marker, isBot);
				if (GetClusterY(entries[i].Marker, isBot) - clusterY > weldYThreshold)
				{
					break;
				}
			}
			if (i - num >= 2)
			{
				float num2 = 0f;
				for (int j = num; j < i; j++)
				{
					num2 += GetClusterY(entries[j].Marker, isBot);
				}
				float num3 = num2 / (float)(i - num);
				for (int k = num; k < i; k++)
				{
					EndpointEntry endpointEntry = entries[k];
					Vector4 value = resolved[endpointEntry.Marker];
					if (isBot)
					{
						if (endpointEntry.IsLeft)
						{
							value.x = num3;
						}
						else
						{
							value.z = num3;
						}
					}
					else if (endpointEntry.IsLeft)
					{
						value.y = num3;
					}
					else
					{
						value.w = num3;
					}
					resolved[endpointEntry.Marker] = value;
				}
			}
			num = i;
		}
	}

	private static float GetClusterY(ObstacleMarker m, bool isBot)
	{
		if (!isBot)
		{
			return m.TopY;
		}
		return m.BaseY;
	}

	public void HandleDynamicCoverProviderRegistered(IDynamicCoverProvider provider)
	{
		using (ProfileScope.New("LosVisualizer.Markers.RegisterDynamicCover"))
		{
			if (m_isInited && !m_DynamicCoverMarkers.ContainsKey(provider))
			{
				m_DynamicCoverMarkers[provider] = CreateDynamicCoverMarkers(provider);
				m_DynamicCoverNodeSnapshot[provider] = ToNodeSet(provider.Nodes);
				RefreshMarkerGeometry();
			}
		}
	}

	public void HandleDynamicCoverProviderUnregistered(IDynamicCoverProvider provider)
	{
		using (ProfileScope.New("LosVisualizer.Markers.UnregisterDynamicCover"))
		{
			if (m_DynamicCoverMarkers.TryGetValue(provider, out var value))
			{
				DestroyMarkers(value);
				m_DynamicCoverMarkers.Remove(provider);
				m_DynamicCoverNodeSnapshot.Remove(provider);
				if (m_isInited)
				{
					RefreshMarkerGeometry();
				}
			}
		}
	}

	private List<ObstacleMarker> CreateDynamicCoverMarkers(IDynamicCoverProvider provider)
	{
		return CreateBoundaryEdgeMarkers(provider.Nodes, provider.CoverType);
	}

	private void RebuildAllDynamicCoverMarkers()
	{
		using (ProfileScope.New("LosVisualizer.Markers.RebuildDynamic"))
		{
			List<IDynamicCoverProvider> list = new List<IDynamicCoverProvider>(m_DynamicCoverMarkers.Count);
			foreach (KeyValuePair<IDynamicCoverProvider, List<ObstacleMarker>> dynamicCoverMarker in m_DynamicCoverMarkers)
			{
				list.Add(dynamicCoverMarker.Key);
			}
			bool flag = false;
			foreach (IDynamicCoverProvider item in list)
			{
				HashSet<GridNodeBase> hashSet = ToNodeSet(item.Nodes);
				if (!m_DynamicCoverNodeSnapshot.TryGetValue(item, out var value) || !value.SetEquals(hashSet))
				{
					if (m_DynamicCoverMarkers.TryGetValue(item, out var value2))
					{
						DestroyMarkers(value2);
					}
					m_DynamicCoverMarkers[item] = CreateDynamicCoverMarkers(item);
					m_DynamicCoverNodeSnapshot[item] = hashSet;
					flag = true;
				}
			}
			if (flag)
			{
				RefreshMarkerGeometry();
			}
		}
	}

	private void ClearAllDynamicCoverMarkers()
	{
		using (ProfileScope.New("LosVisualizer.Markers.ClearDynamic"))
		{
			foreach (KeyValuePair<IDynamicCoverProvider, List<ObstacleMarker>> dynamicCoverMarker in m_DynamicCoverMarkers)
			{
				DestroyMarkers(dynamicCoverMarker.Value);
			}
			m_DynamicCoverMarkers.Clear();
			m_DynamicCoverNodeSnapshot.Clear();
		}
	}

	public void HandleNearUnitsCoverProviderRegistered(BaseUnitEntity unit)
	{
		using (ProfileScope.New("LosVisualizer.Markers.RegisterNearUnits"))
		{
			if (m_isInited && unit.IsPlayerFaction && m_NearUnitsCoverTrackedUnits.Add(unit))
			{
				RebuildAllNearUnitsCoverMarkers();
			}
		}
	}

	public void HandleNearUnitsCoverProviderUnregistered(BaseUnitEntity unit)
	{
		using (ProfileScope.New("LosVisualizer.Markers.UnregisterNearUnits"))
		{
			if (m_NearUnitsCoverTrackedUnits.Remove(unit))
			{
				RebuildAllNearUnitsCoverMarkers();
			}
		}
	}

	private void RebuildAllNearUnitsCoverMarkers()
	{
		using (ProfileScope.New("LosVisualizer.Markers.RebuildNearUnits"))
		{
			HashSet<BaseUnitEntity> hashSet = new HashSet<BaseUnitEntity>();
			foreach (BaseUnitEntity nearUnitsCoverTrackedUnit in m_NearUnitsCoverTrackedUnits)
			{
				PartNearUnitsProvideCover optional = nearUnitsCoverTrackedUnit.GetOptional<PartNearUnitsProvideCover>();
				if (optional == null)
				{
					continue;
				}
				foreach (MechanicEntity item in Game.Instance.Controllers.TurnController.EntitiesInCombat)
				{
					if (item != nearUnitsCoverTrackedUnit && item is BaseUnitEntity baseUnitEntity && optional.IsSuitableCover(baseUnitEntity))
					{
						hashSet.Add(baseUnitEntity);
					}
				}
			}
			List<BaseUnitEntity> list = new List<BaseUnitEntity>();
			foreach (BaseUnitEntity key in m_NearUnitsCoverMarkers.Keys)
			{
				if (!hashSet.Contains(key))
				{
					list.Add(key);
				}
			}
			foreach (BaseUnitEntity item2 in list)
			{
				DestroyMarkers(m_NearUnitsCoverMarkers[item2]);
				m_NearUnitsCoverMarkers.Remove(item2);
				m_NearUnitsCoverNodeSnapshot.Remove(item2);
			}
			bool flag = list.Count > 0;
			foreach (BaseUnitEntity item3 in hashSet)
			{
				HashSet<GridNodeBase> hashSet2 = ToNodeSet(item3.GetOccupiedNodes());
				if (!m_NearUnitsCoverNodeSnapshot.TryGetValue(item3, out var value) || !value.SetEquals(hashSet2))
				{
					if (m_NearUnitsCoverMarkers.TryGetValue(item3, out var value2))
					{
						DestroyMarkers(value2);
					}
					m_NearUnitsCoverMarkers[item3] = CreatePerimeterCoverMarkers(item3);
					m_NearUnitsCoverNodeSnapshot[item3] = hashSet2;
					flag = true;
				}
			}
			if (flag)
			{
				RefreshMarkerGeometry();
			}
		}
	}

	private List<ObstacleMarker> CreatePerimeterCoverMarkers(BaseUnitEntity provider)
	{
		return CreateBoundaryEdgeMarkers(provider.GetOccupiedNodes(), LosCalculations.CoverType.Cover);
	}

	private void ClearAllNearUnitsCoverMarkers()
	{
		using (ProfileScope.New("LosVisualizer.Markers.ClearNearUnits"))
		{
			foreach (KeyValuePair<BaseUnitEntity, List<ObstacleMarker>> nearUnitsCoverMarker in m_NearUnitsCoverMarkers)
			{
				DestroyMarkers(nearUnitsCoverMarker.Value);
			}
			m_NearUnitsCoverMarkers.Clear();
			m_NearUnitsCoverNodeSnapshot.Clear();
			m_NearUnitsCoverTrackedUnits.Clear();
		}
	}

	private static HashSet<GridNodeBase> ToNodeSet(NodeList nodes)
	{
		HashSet<GridNodeBase> hashSet = new HashSet<GridNodeBase>();
		foreach (GridNodeBase item in nodes)
		{
			hashSet.Add(item);
		}
		return hashSet;
	}

	private void DestroyMarkers(List<ObstacleMarker> markers)
	{
		foreach (ObstacleMarker marker in markers)
		{
			if (marker != null)
			{
				UnityEngine.Object.Destroy(marker.gameObject);
			}
		}
	}

	public void HandleAbilityTargetPossibilityCheck(AbilityData ability, TargetWrapper target, Vector3? pointerPosition, bool targetingIsPossible)
	{
		if (ability.IsAoe)
		{
			if (!targetingIsPossible)
			{
				DrawLosFx(pointerPosition);
			}
			else if (m_LosInterruptedFxInstance.activeSelf)
			{
				m_LosInterruptedFxInstance.SetActive(value: false);
				m_LosLineInstance.enabled = false;
			}
		}
	}
}
