using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.View;
using Kingmaker.View.Covers;
using UnityEngine;

namespace Kingmaker.UI.Selection;

public class LosTypeDisplay : MonoBehaviour
{
	public GameObject InvisibleMarker;

	public GameObject FullCoverMarker;

	public GameObject HalfCoverMarker;

	public GameObject NoCoverMarker;

	private readonly Dictionary<LosCalculations.CoverType, GameObject> m_MarkerPrefabs = new Dictionary<LosCalculations.CoverType, GameObject>();

	private readonly Dictionary<LosCalculations.CoverType, Queue<GameObject>> m_UnusedMarkers = new Dictionary<LosCalculations.CoverType, Queue<GameObject>>();

	private Vector3? m_LastUpdatedPosition;

	private readonly Dictionary<BaseUnitEntity, (LosCalculations.CoverType coverType, GameObject marker)> m_ActiveMarkers = new Dictionary<BaseUnitEntity, (LosCalculations.CoverType, GameObject)>();

	private CameraRig m_CameraRig;

	private void Awake()
	{
		m_MarkerPrefabs.Add(LosCalculations.CoverType.Obstacle, NoCoverMarker);
		m_MarkerPrefabs.Add(LosCalculations.CoverType.Cover, HalfCoverMarker);
		m_MarkerPrefabs.Add(LosCalculations.CoverType.LosBlocker, InvisibleMarker);
		m_UnusedMarkers.Add(LosCalculations.CoverType.Obstacle, new Queue<GameObject>());
		m_UnusedMarkers.Add(LosCalculations.CoverType.Cover, new Queue<GameObject>());
		m_UnusedMarkers.Add(LosCalculations.CoverType.LosBlocker, new Queue<GameObject>());
		m_CameraRig = Object.FindAnyObjectByType<CameraRig>();
	}

	private void OnDisable()
	{
		ClearAllMarkers();
	}

	private void Update()
	{
		UpdateAllMarkers();
	}

	private void AddMarker(BaseUnitEntity viewer, BaseUnitEntity target, Vector3 viewerPosition)
	{
		if (!target.CombatGroup.IsAlly(viewer) && !target.IsDeadAndHasLoot && target.IsInCombat)
		{
			LosCalculations.CoverType warhammerLos = LosCalculations.GetWarhammerLos(viewer, viewerPosition, target);
			GameObject marker = GetMarker(warhammerLos);
			marker.transform.position = target.Position;
			if (m_CameraRig != null)
			{
				base.transform.rotation = Quaternion.Euler(new Vector3(m_CameraRig.transform.rotation.eulerAngles.x, m_CameraRig.transform.rotation.eulerAngles.y, m_CameraRig.transform.rotation.eulerAngles.z));
			}
			m_ActiveMarkers.Add(target, (warhammerLos, marker));
		}
	}

	private void RemoveMarker(BaseUnitEntity unit)
	{
		if (m_ActiveMarkers.TryGetValue(unit, out (LosCalculations.CoverType, GameObject) value))
		{
			m_ActiveMarkers.Remove(unit);
			ReleaseMarker(value.Item1, value.Item2);
		}
	}

	private void UpdateMarker(BaseUnitEntity viewer, BaseUnitEntity target, bool forceRecalc, Vector3 viewerPosition)
	{
		if (m_ActiveMarkers.TryGetValue(target, out (LosCalculations.CoverType, GameObject) value))
		{
			GameObject gameObject = value.Item2;
			if (forceRecalc || !((gameObject.transform.position - target.Position).sqrMagnitude < 0.1f))
			{
				LosCalculations.CoverType warhammerLos = LosCalculations.GetWarhammerLos(viewer, viewerPosition, target);
				if (value.Item1 != warhammerLos)
				{
					RemoveMarker(target);
					gameObject = GetMarker(warhammerLos);
					m_ActiveMarkers.Add(target, (warhammerLos, gameObject));
				}
				gameObject.transform.position = target.Position;
			}
		}
		else
		{
			AddMarker(viewer, target, viewerPosition);
		}
	}

	private void UpdateAllMarkers()
	{
		if (Game.Instance.CurrentModeType == GameModeType.None || Game.Instance.CurrentModeType == GameModeType.GlobalMap || Game.Instance.CurrentModeType == GameModeType.CutsceneGlobalMap)
		{
			ClearAllMarkers();
			return;
		}
		if (!Game.Instance.Controllers.SelectionCharacter.IsSingleSelected.Value)
		{
			ClearAllMarkers();
			return;
		}
		BaseUnitEntity firstSelectedUnit = Game.Instance.Controllers.SelectionCharacter.FirstSelectedUnit;
		Vector3 position = firstSelectedUnit.Position;
		Vector3 value = position;
		Vector3? lastUpdatedPosition = m_LastUpdatedPosition;
		bool flag = ((value - lastUpdatedPosition)?.sqrMagnitude ?? 1f) > 0.2f;
		if (flag)
		{
			m_LastUpdatedPosition = position;
		}
		List<BaseUnitEntity> allBaseAwakeUnits = Game.Instance.EntityPools.AllBaseAwakeUnits;
		if (allBaseAwakeUnits.Count != m_ActiveMarkers.Count)
		{
			ClearAllMarkers();
		}
		foreach (BaseUnitEntity item in allBaseAwakeUnits)
		{
			UpdateMarker(firstSelectedUnit, item, flag, position);
		}
	}

	private void ClearAllMarkers()
	{
		for (BaseUnitEntity baseUnitEntity = m_ActiveMarkers.Keys.FirstOrDefault(); baseUnitEntity != null; baseUnitEntity = m_ActiveMarkers.Keys.FirstOrDefault())
		{
			RemoveMarker(baseUnitEntity);
		}
	}

	private GameObject GetMarker(LosCalculations.CoverType coverType)
	{
		if (m_UnusedMarkers[coverType].Count > 0)
		{
			GameObject obj = m_UnusedMarkers[coverType].Dequeue();
			obj.SetActive(value: true);
			return obj;
		}
		GameObject obj2 = Object.Instantiate(m_MarkerPrefabs[coverType], base.transform);
		obj2.SetActive(value: true);
		return obj2;
	}

	private void ReleaseMarker(LosCalculations.CoverType coverType, GameObject obj)
	{
		obj.SetActive(value: false);
		m_UnusedMarkers[coverType].Enqueue(obj);
	}
}
