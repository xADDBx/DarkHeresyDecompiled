using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.EntitySystem.Interfaces.Config;
using Kingmaker.Framework.EntitySystem.Interfaces.View;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using R3;
using UnityEngine;
using UnityEngine.VFX;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[Serializable]
[KDB("Элемент в цепочке следов. Цепочки могут ветвиться. Каждый следующий элемент становится доступен после взаимодействия с предыдущим. В конце цепочки - улика для детективного дела (или что угодно еще, что захочет воткнуть сюда левелдизайнер). Следы могут находиться в одном из состояний: None, Found, Followed, FollowedToDeadEnd.")]
[KnowledgeDatabaseID("293237afebf2491e80e610b0e29f240f")]
[RequireComponent(typeof(VisualEffect))]
[RequireComponent(typeof(InteractionDetectiveTrace))]
public class DetectiveTraceView : MapObjectView, IDetectiveTraceView, IEntityView, IDetectiveTraceConfig, IMapObjectEntityConfig, IMechanicEntityConfig, IEntityConfig
{
	public const string PointGameObjectName = "TracePoint";

	public const string FX_DetectiveFootstepsVisualEffectAssetGUID = "ccd3d778be8d5aa4ba17c996368bba70";

	public static int DecorOnBoolId = Shader.PropertyToID("DecorOn");

	public static int AliveParticlesBoolId = Shader.PropertyToID("AliveParticles");

	public static int FalseStateBoolId = Shader.PropertyToID("FootStepFalseState");

	public static int SkullPosVector2Id = Shader.PropertyToID("SkullPos");

	public static int FootStepsAliveParticlesBoolId = Shader.PropertyToID("FootStepsAlive");

	public static int WindowAliveParticlesBoolId = Shader.PropertyToID("WindowAlive");

	public static int FootStepFoundColorVector4Id = Shader.PropertyToID("FootStepFoundColor");

	public static int FootStepFollowedBySkullColorVector4Id = Shader.PropertyToID("FootStepFollowedColor");

	public static int FootStepAlphaMultiplyFloatId = Shader.PropertyToID("AlphaMult");

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

	private readonly ReactiveProperty<DetectiveTraceStatus> m_CurrentStatus = new ReactiveProperty<DetectiveTraceStatus>();

	public new DetectiveTraceEntity Data => (DetectiveTraceEntity)base.Data;

	IReadOnlyList<DetectiveTracePoint> IDetectiveTraceConfig.PointsData => PointsData;

	bool IDetectiveTraceConfig.TrueEnd => TrueEnd;

	bool IDetectiveTraceConfig.HideInteract => HideInteract;

	bool IDetectiveTraceConfig.HideInteractionIfFollowed => HideInteractionIfFollowed;

	bool IDetectiveTraceConfig.HideInteractionIfFollowedToDeadEnd => HideInteractionIfFollowedToDeadEnd;

	bool IDetectiveTraceConfig.Found => Found;

	IEnumerable<EntityRef<DetectiveTraceEntity>> IDetectiveTraceConfig.Continuations => Continuations.Select((DetectiveTraceView i) => new EntityRef<DetectiveTraceEntity>(i.UniqueId));

	public ReadOnlyReactiveProperty<DetectiveTraceStatus> CurrentStatus => m_CurrentStatus;

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new DetectiveTraceEntity(this));
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
		OnStatusLoad(Game.Instance.Controllers.TurnController.InCombat);
	}

	protected override void OnVisibilityChanged()
	{
		base.OnVisibilityChanged();
		UpdateMarkers();
	}

	public override void OnInFogOfWarChanged()
	{
		base.OnInFogOfWarChanged();
		UpdateMarkers();
	}

	public void OnStatusChanged(bool inCombat)
	{
		m_ServoSkullPart = PartDetectiveServoSkull.Find();
		switch (Data.Status)
		{
		case DetectiveTraceStatus.None:
			m_TraceStepsVFX.enabled = false;
			m_TraceStepsVFX.SetBool(AliveParticlesBoolId, b: true);
			m_TraceStepsVFX.SetBool(WindowAliveParticlesBoolId, b: true);
			m_TraceStepsVFX.SetBool(FootStepsAliveParticlesBoolId, b: true);
			m_TraceStepsVFX.SetBool(FalseStateBoolId, b: false);
			break;
		case DetectiveTraceStatus.Found:
			m_TraceStepsVFX.enabled = !inCombat;
			m_TraceStepsVFX.SetBool(WindowAliveParticlesBoolId, b: true);
			break;
		case DetectiveTraceStatus.Followed:
			m_TraceStepsVFX.enabled = !inCombat;
			m_TraceStepsVFX.SetBool(WindowAliveParticlesBoolId, b: false);
			break;
		case DetectiveTraceStatus.FollowedToDeadEnd:
			m_TraceStepsVFX.enabled = !inCombat;
			m_TraceStepsVFX.SetBool(WindowAliveParticlesBoolId, b: false);
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
		m_CurrentStatus.Value = Data.Status;
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

	public void OnStatusLoad(bool inCombat)
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
			m_TraceStepsVFX.enabled = !inCombat;
			m_TraceStepsVFX.SetBool(AliveParticlesBoolId, b: true);
			m_TraceStepsVFX.SetBool(FalseStateBoolId, b: false);
			m_TraceStepsVFX.SetBool(WindowAliveParticlesBoolId, b: false);
			break;
		case DetectiveTraceStatus.Followed:
			m_TraceStepsVFX.enabled = !inCombat;
			m_TraceStepsVFX.SetBool(AliveParticlesBoolId, b: true);
			m_TraceStepsVFX.SetBool(FalseStateBoolId, b: false);
			m_TraceStepsVFX.SetBool(WindowAliveParticlesBoolId, b: false);
			break;
		case DetectiveTraceStatus.FollowedToDeadEnd:
			m_TraceStepsVFX.enabled = !inCombat;
			m_TraceStepsVFX.SetBool(AliveParticlesBoolId, b: true);
			m_TraceStepsVFX.SetBool(FalseStateBoolId, b: true);
			m_TraceStepsVFX.SetBool(WindowAliveParticlesBoolId, b: false);
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
		m_CurrentStatus.Value = Data.Status;
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
		if (!base.IsVisible || base.IsInFogOfWar)
		{
			return;
		}
		if (Data.Status != DetectiveTraceStatus.Followed)
		{
			InteractionPartDetectiveTrace? interaction = Data.Interaction;
			if (interaction != null && interaction.ShowNotFollowedOnMap)
			{
				DetectiveTraceStatus status = Data.Status;
				if (status == DetectiveTraceStatus.None || status == DetectiveTraceStatus.Found)
				{
					goto IL_004b;
				}
			}
			RemoveMarkers();
			return;
		}
		goto IL_004b;
		IL_004b:
		CreateMarkers();
	}

	private void CreateMarkers()
	{
		LocalMapMarkerPart orCreate = Data.GetOrCreate<LocalMapMarkerPart>();
		orCreate.IsRuntimeCreated = true;
		orCreate.OverridePosition = PointsData.LastOrDefault()?.Position;
		orCreate.Settings.Type = LocalMapMarkType.DetectiveTrace;
	}

	private void RemoveMarkers()
	{
		Data.Remove<LocalMapMarkerPart>();
	}

	string IEntityView.get_name()
	{
		return base.name;
	}
}
