using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.GameModes;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
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

public class LosVisualizer : MonoBehaviour, IAreaHandler, ISubscriber, ITurnBasedModeHandler, IInteractionHighlightUIHandler, IUnitDirectHoverUIHandler, IAbilityTargetSelectionUIHandler, IHologramPositionChangedHandler, IHologramClearHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, ITurnEndHandler, IPreparationTurnBeginHandler, IUnitMovementHandler, ISubscriber<IBaseUnitEntity>, IEntityPositionChangedHandler, ISubscriber<IEntity>, IGridObstacleCacheHandler, IGridObstacleEnabledHandler, IGridObstacleAwakeHandler, IAbilityTargetPossibilityCheck
{
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

	[FormerlySerializedAs("CoverMarker")]
	[Tooltip("Prefab of cover marker")]
	public ObstacleMarker coverMarker;

	[FormerlySerializedAs("LosBlockerMarker")]
	[Tooltip("Prefab of LOS blocker marker")]
	public ObstacleMarker losBlockerMarker;

	[FormerlySerializedAs("LosLine")]
	[Tooltip("Prefab of LOS line")]
	public LineRenderer losLine;

	[FormerlySerializedAs("LosInterruptedFx")]
	[Tooltip("Prefab of VFX that will be spawned in vision obstruction point")]
	public GameObject losInterruptedFx;

	[Tooltip("Prefab of grayscale and brightness FX for characters who's not seen by player")]
	public GameObject grayscaleFxPrefab;

	private List<ObstacleMarker> m_obstaclesMarkers = new List<ObstacleMarker>();

	private List<GridObstacle> m_obstacles = new List<GridObstacle>();

	private CameraRig m_cameraRig;

	private Vector3 m_lastMousePos = Vector3.zero;

	private Vector3 m_lastCameraPos = Vector3.zero;

	private bool m_isInited;

	private LineRenderer m_LosLineInstance;

	private GameObject m_LosInterruptedFxInstance;

	private MechanicEntity m_SelectedUnit;

	private Dictionary<MechanicEntity, GameObject> m_UnitAndFx = new Dictionary<MechanicEntity, GameObject>();

	private TurnController m_TurnController;

	private int? m_CachedLayerMaskToShowFx;

	private RaycastHit[] m_RaycastHits;

	private GameModeType m_GameModeType;

	private bool m_isGlobalTabHighlight;

	private Vector3 m_eyeHeightMod = Vector3.one;

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
		}
	}

	private bool InitCheckData()
	{
		if (grayscaleFxPrefab == null)
		{
			PFLog.TechArt.Error(this, "No grayscale fx prefab in LosVisualizer");
			return false;
		}
		if (coverMarker == null)
		{
			PFLog.TechArt.Error(this, "No cover marker prefab in LosVisualizer");
			return false;
		}
		if (losBlockerMarker == null)
		{
			PFLog.TechArt.Error(this, "No los blocker marker prefab in LosVisualizer");
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
		if (!m_isInited || GameUIState.Instance.IsLoadingProcess.Value)
		{
			return;
		}
		Player player = Game.Instance.Player;
		if (player == null || player.IsInCombat)
		{
			m_SelectedUnit = Game.Instance.Controllers.TurnController.CurrentUnit;
			if (m_lastMousePos != Input.mousePosition)
			{
				UpdateMarkers(m_SelectedUnit);
				m_lastMousePos = Input.mousePosition;
			}
			else if (m_lastCameraPos != m_cameraRig.transform.position)
			{
				UpdateMarkers(m_SelectedUnit);
				m_lastCameraPos = m_cameraRig.transform.position;
			}
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
		List<ObstacleMarker> list = new List<ObstacleMarker>();
		foreach (GridObstacle obstacle in obstacles)
		{
			if (obstacle.Type != 0)
			{
				list.Add(SetupObstacleMarker(obstacle));
			}
		}
		return list;
	}

	private ObstacleMarker SetupObstacleMarker(GridObstacle obstacle)
	{
		ObstacleMarker original = ((obstacle.Type == LosCalculations.CoverType.Cover) ? coverMarker : losBlockerMarker);
		Quaternion inputRotation = (obstacle._Rotate ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity) * ((Component)(object)obstacle).transform.rotation;
		ObstacleMarker obj = UnityEngine.Object.Instantiate(rotation: GetStepRotation(inputRotation, 90f), original: original, position: ((Component)(object)obstacle).transform.position);
		obj.Type = obstacle.Type;
		obj.OwnerObstacle = obstacle;
		obj.ObstacleRenderer = obj.GetComponentInChildren<Renderer>();
		obj.RaycastCollider = obj.GetComponentInChildren<Collider>();
		obj.gameObject.transform.SetParent(base.transform);
		return obj;
	}

	public void HandleGridObstacleEnabled(GridObstacle gridObstacle, bool enabled)
	{
		AddDynamicObstacle(gridObstacle);
		foreach (ObstacleMarker obstaclesMarker in m_obstaclesMarkers)
		{
			if ((UnityEngine.Object)(object)obstaclesMarker.OwnerObstacle == (UnityEngine.Object)(object)gridObstacle)
			{
				obstaclesMarker.gameObject.SetActive(enabled);
			}
		}
	}

	private void UpdateMarkersState()
	{
		using (ProfileScope.New("LosVisualizer.UpdateMarkersState"))
		{
			foreach (ObstacleMarker obstaclesMarker in m_obstaclesMarkers)
			{
				Quaternion quaternion = (obstaclesMarker.OwnerObstacle._Rotate ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity);
				Quaternion inputRotation = ((Component)(object)obstaclesMarker.OwnerObstacle).transform.rotation * quaternion;
				obstaclesMarker.transform.rotation = GetStepRotation(inputRotation, 90f);
				obstaclesMarker.transform.position = ((Component)(object)obstaclesMarker.OwnerObstacle).transform.position;
			}
		}
	}

	private Quaternion GetStepRotation(Quaternion inputRotation, float stepValue)
	{
		Vector3 eulerAngles = inputRotation.eulerAngles;
		eulerAngles = new Vector3(Mathf.Round(eulerAngles.x / stepValue) * stepValue, Mathf.Round(eulerAngles.y / stepValue) * stepValue, Mathf.Round(eulerAngles.z / stepValue) * stepValue);
		return Quaternion.Euler(eulerAngles);
	}

	private List<GameObject> GetObstaclesMarkersInDistance(List<ObstacleMarker> obstaclesMarkers, Vector3 position, float distance)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (ObstacleMarker obstaclesMarker in obstaclesMarkers)
		{
			if (obstaclesMarker.Type != 0 && Vector3.Distance(obstaclesMarker.gameObject.transform.position, position) <= distance)
			{
				list.Add(obstaclesMarker.gameObject);
			}
		}
		return list;
	}

	private Vector3 GetMouseWorldPosition()
	{
		Vector3 zero = Vector3.zero;
		if (Physics.Raycast(m_cameraRig.Camera.ScreenPointToRay(Input.mousePosition), out var hitInfo, float.PositiveInfinity))
		{
			return hitInfo.point;
		}
		return zero;
	}

	private bool IsProperGameMode()
	{
		m_GameModeType = Game.Instance.CurrentModeType;
		if (m_GameModeType != GameModeType.None && m_GameModeType != GameModeType.GlobalMap && m_GameModeType != GameModeType.SpaceCombat && m_GameModeType != GameModeType.StarSystem)
		{
			return m_GameModeType != GameModeType.CutsceneGlobalMap;
		}
		return false;
	}

	private void UpdateMarkers(MechanicEntity selectedUnit)
	{
		if (m_isGlobalTabHighlight || m_obstaclesMarkers.Count == 0 || (!m_TurnController.IsPlayerTurn && !m_TurnController.IsPreparationTurn))
		{
			return;
		}
		Vector3 mouseWorldPosition = GetMouseWorldPosition();
		List<GameObject> obstaclesMarkersInDistance = GetObstaclesMarkersInDistance(m_obstaclesMarkers, mouseWorldPosition, mouseDistanceToShowMarkers);
		foreach (ObstacleMarker obstaclesMarker in m_obstaclesMarkers)
		{
			if (obstaclesMarker.Type != 0)
			{
				if (selectedUnit != null && selectedUnit.Size >= Size.Large && obstaclesMarker.Type == LosCalculations.CoverType.Cover)
				{
					obstaclesMarker.EnableMarker(enable: false);
				}
				else
				{
					obstaclesMarker.EnableMarker(obstaclesMarkersInDistance.Contains(obstaclesMarker.gameObject));
				}
			}
		}
	}

	private void ToggleAllMarkers(bool state)
	{
		foreach (ObstacleMarker obstaclesMarker in m_obstaclesMarkers)
		{
			obstaclesMarker.EnableMarker(state);
		}
	}

	private void ClearAll()
	{
		foreach (ObstacleMarker obstaclesMarker in m_obstaclesMarkers)
		{
			UnityEngine.Object.Destroy(obstaclesMarker.gameObject);
		}
		ClearLosVisual();
		m_obstaclesMarkers.Clear();
		m_obstacles = null;
		ClearGrayscaleFxs();
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
		foreach (ObstacleMarker obstaclesMarker in m_obstaclesMarkers)
		{
			obstaclesMarker.EnableMarker(enable: false);
		}
	}

	public void OnAreaBeginUnloading()
	{
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
				m_LosLineInstance.enabled = true;
				m_LosLineInstance.SetPosition(0, playerEyePosition);
				Vector3 vector = targetEyePosition;
				Vector3 normalized = (targetEyePosition - playerEyePosition).normalized;
				float maxDistance = Vector3.Distance(playerEyePosition, targetEyePosition);
				int num = Physics.RaycastNonAlloc(playerEyePosition, normalized, m_RaycastHits, maxDistance, LayerMaskToShowFx);
				if (num == 0)
				{
					for (float num2 = 0.05f; num2 <= 2f; num2 += 0.05f)
					{
						sphereCastRadius += num2;
						num = Physics.SphereCastNonAlloc(playerEyePosition, sphereCastRadius, normalized, m_RaycastHits, maxDistance, LayerMaskToShowFx);
						if (num > 0)
						{
							break;
						}
					}
				}
				bool active = num > 0;
				vector = m_RaycastHits[0].point;
				float num3 = Vector3.Dot(vector - playerEyePosition, normalized);
				Vector3 b = playerEyePosition + normalized * num3;
				vector = Vector3.Lerp(vector, b, 0.5f * sphereCastRadius);
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
		Vector3 vector = ((unitHologram?.HologramEntityView != null) ? unitHologram.HologramEntityView.transform.position : Game.Instance.Controllers.SelectionCharacter.SelectedUnit.Value.View.transform.position);
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
		foreach (MechanicEntity item in Game.Instance.Controllers.TurnController.UnitsInCombat)
		{
			if (!item.IsPlayerEnemy)
			{
				continue;
			}
			LosDescription warhammerLos = LosCalculations.GetWarhammerLos(losCalculateFrom, rectForSize, item.View.transform.position, rectForSize);
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
		UpdateMarkersState();
	}

	public void HandleGridObstacleAwake(GridObstacle gridObstacle)
	{
		AddDynamicObstacle(gridObstacle);
	}

	private void AddDynamicObstacle(GridObstacle gridObstacle)
	{
		using (ProfileScope.New("LosVisualizer.AddNewObstacleVisual"))
		{
			if (m_isInited && !m_obstacles.Contains(gridObstacle))
			{
				m_obstacles.Add(gridObstacle);
				m_obstaclesMarkers.Add(SetupObstacleMarker(gridObstacle));
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
