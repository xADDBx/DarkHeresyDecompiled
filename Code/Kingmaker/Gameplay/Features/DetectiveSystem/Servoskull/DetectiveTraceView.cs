using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.VFX;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[Serializable]
[KDB("Элемент в цепочке следов. Цепочки могут ветвиться. Каждый следующий элемент становится доступен после взаимодействия с предыдущим. В конце цепочки - улика для детективного дела (или что угодно еще, что захочет воткнуть сюда левелдизайнер). Следы могут находиться в одном из состояний: None, Found, Followed, FollowedToDeadEnd.")]
[KnowledgeDatabaseID("293237afebf2491e80e610b0e29f240f")]
[RequireComponent(typeof(VisualEffect))]
[RequireComponent(typeof(InteractionDetectiveTrace))]
public class DetectiveTraceView : MapObjectView
{
	[Serializable]
	public class DetectiveTracePoint
	{
		public Vector3 Position;

		public bool IsInterruption;
	}

	[Serializable]
	public class DetectiveTraceStep
	{
		public Vector3 Position;

		public Quaternion Rotation;

		public DetectiveTraceStep(Vector3 position, Quaternion rotation)
		{
			Position = position;
			Rotation = rotation;
		}
	}

	public const string PointGameObjectName = "TracePoint";

	public const string FX_DetectiveFootstepsVisualEffectAssetGUID = "ccd3d778be8d5aa4ba17c996368bba70";

	public static int DecorOnBoolId = Shader.PropertyToID("DecorOn");

	public static int AliveParticlesBoolId = Shader.PropertyToID("AliveParticles");

	public static int FalseStateBoolId = Shader.PropertyToID("FootStepFalseState");

	public static int SkullPosVector2Id = Shader.PropertyToID("SkullPos");

	public static int FootStepMapTextureId = Shader.PropertyToID("FootStepMap");

	public static int PositionMapTextureId = Shader.PropertyToID("PositionMap");

	public static int AngleXYZMapTextureId = Shader.PropertyToID("AngleXYZMap");

	public static int PositionCountIntId = Shader.PropertyToID("PositionCount");

	public static int StartEndVector2Id = Shader.PropertyToID("StartEnd");

	public static int BoundsCenterVector3Id = Shader.PropertyToID("BoundsCenter");

	public static int BoundsSizeVector3Id = Shader.PropertyToID("BoundsSize");

	[HideInInspector]
	[ValidateNoNullEntries]
	public List<DetectiveTracePoint> PointsData = new List<DetectiveTracePoint>();

	[HideInInspector]
	public List<DetectiveTraceStep> StepsData = new List<DetectiveTraceStep>();

	public VisualEffect m_TraceStepsVFX;

	[HideInInspector]
	public bool m_VFXNeedInit;

	[KDB("Тип визуала следов.")]
	public DetectiveTraceVisualType VisualType;

	[KDB("Следующие следы в цепочке. Если указано несколько - это развилка. Один и тот же след может быть следующим у нескольких других следов.")]
	[ValidateNoNullEntries]
	public List<DetectiveTraceView> Continuations = new List<DetectiveTraceView>();

	[KDB("Не показывать интеракт со следами в конце цепочки")]
	public bool HideInteract;

	[KDB("Когда игрок поинтерактит с этими следами все следы, связанные с этими, перейдут в состояние FollowedToDeadEnd.")]
	public bool TrueEnd;

	[KDB("Перевести следы в статус Found на старте. Без этой галки следы должны быть найдены при интеракте с DetectiveTraceRootView или с предыдущими в цепочке следами.")]
	public bool Found;

	[KDB("Нужно ли прятать интеракт со следами если игрок поинтерактил с ними.")]
	public bool HideInteractionIfFollowed = true;

	[KDB("Нужно ли прятать интеракт с этими следами если игрок поинтерактил с последними следами в цепочке.")]
	public bool HideInteractionIfFollowedToDeadEnd = true;

	[KDB("Активировать объекты, когда следы найдены (звук)")]
	public GameObject[] ActivateObjectsOnFound = Array.Empty<GameObject>();

	private PartDetectiveServoSkull m_ServoSkullPart;

	public new DetectiveTraceEntity Data => (DetectiveTraceEntity)base.Data;

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new DetectiveTraceEntity(UniqueId, base.IsInGameBySettings));
	}

	protected override void Awake()
	{
		if (m_TraceStepsVFX == null)
		{
			m_TraceStepsVFX = GetComponent<VisualEffect>();
		}
		if (m_TraceStepsVFX != null && Application.isPlaying)
		{
			m_TraceStepsVFX.SetBool(DecorOnBoolId, b: true);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying)
		{
			m_ServoSkullPart = PartDetectiveServoSkull.Find();
		}
		if (m_TraceStepsVFX != null && Application.isPlaying)
		{
			m_TraceStepsVFX.SetBool(DecorOnBoolId, b: true);
		}
	}

	protected override void OnDidAttachToData()
	{
		base.OnDidAttachToData();
		OnStatusLoad();
	}

	public void OnStatusChanged()
	{
		m_ServoSkullPart = PartDetectiveServoSkull.Find();
		switch (Data.Status)
		{
		case DetectiveTraceStatus.None:
			m_TraceStepsVFX.enabled = false;
			m_TraceStepsVFX.SetBool(AliveParticlesBoolId, b: true);
			m_TraceStepsVFX.SetBool(FalseStateBoolId, b: false);
			break;
		case DetectiveTraceStatus.Found:
			m_TraceStepsVFX.enabled = true;
			break;
		case DetectiveTraceStatus.FollowedToDeadEnd:
			m_TraceStepsVFX.SetBool(FalseStateBoolId, b: true);
			break;
		case DetectiveTraceStatus.FollowedToTrueEnd:
			m_TraceStepsVFX.SetBool(AliveParticlesBoolId, b: false);
			break;
		}
		GameObject[] activateObjectsOnFound = ActivateObjectsOnFound;
		foreach (GameObject gameObject in activateObjectsOnFound)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(Data.Status == DetectiveTraceStatus.Found);
			}
		}
		UpdateMarkers();
	}

	private void RepositionTargetTransform()
	{
		Vector3 zero = Vector3.zero;
		foreach (DetectiveTracePoint pointsDatum in PointsData)
		{
			zero += pointsDatum.Position;
		}
		zero /= (float)PointsData.Count;
		base.transform.position = zero;
	}

	public void OnStatusLoad()
	{
		RepositionTargetTransform();
		m_ServoSkullPart = PartDetectiveServoSkull.Find();
		switch (Data.Status)
		{
		case DetectiveTraceStatus.None:
			m_TraceStepsVFX.enabled = false;
			m_TraceStepsVFX.SetBool(AliveParticlesBoolId, b: true);
			m_TraceStepsVFX.SetBool(FalseStateBoolId, b: false);
			break;
		case DetectiveTraceStatus.Found:
			m_TraceStepsVFX.enabled = true;
			m_TraceStepsVFX.SetBool(AliveParticlesBoolId, b: true);
			m_TraceStepsVFX.SetBool(FalseStateBoolId, b: false);
			break;
		case DetectiveTraceStatus.Followed:
			m_TraceStepsVFX.enabled = true;
			m_TraceStepsVFX.SetBool(AliveParticlesBoolId, b: true);
			m_TraceStepsVFX.SetBool(FalseStateBoolId, b: false);
			break;
		case DetectiveTraceStatus.FollowedToDeadEnd:
			m_TraceStepsVFX.enabled = true;
			m_TraceStepsVFX.SetBool(AliveParticlesBoolId, b: true);
			m_TraceStepsVFX.SetBool(FalseStateBoolId, b: true);
			break;
		case DetectiveTraceStatus.FollowedToTrueEnd:
			m_TraceStepsVFX.enabled = false;
			m_TraceStepsVFX.SetBool(AliveParticlesBoolId, b: false);
			break;
		}
		m_TraceStepsVFX.Reinit();
		GameObject[] activateObjectsOnFound = ActivateObjectsOnFound;
		foreach (GameObject gameObject in activateObjectsOnFound)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(Data.Status == DetectiveTraceStatus.Found);
			}
		}
		UpdateMarkers();
	}

	private void LateUpdate()
	{
		if (m_ServoSkullPart?.Delegate != null && m_TraceStepsVFX != null && m_ServoSkullPart.Owner?.View != null && m_ServoSkullPart?.Delegate != null && m_TraceStepsVFX.HasVector3(SkullPosVector2Id))
		{
			m_TraceStepsVFX.SetVector3(SkullPosVector2Id, m_ServoSkullPart.Delegate.CurrentVisualPosition);
		}
	}

	public void UpdateMarkers()
	{
		if (Data.Status == DetectiveTraceStatus.Followed)
		{
			CreateMarkers();
		}
		else
		{
			RemoveMarkers();
		}
	}

	private void CreateMarkers()
	{
		LocalMapMarkerPart orCreate = Data.GetOrCreate<LocalMapMarkerPart>();
		orCreate.IsRuntimeCreated = true;
		orCreate.Settings.Type = LocalMapMarkType.DetectiveTrace;
	}

	private void RemoveMarkers()
	{
		Data.Remove<LocalMapMarkerPart>();
	}
}
