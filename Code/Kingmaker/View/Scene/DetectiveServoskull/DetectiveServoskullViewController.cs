using System;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.Networking.Sync;
using Kingmaker.Code.Gameplay.Features.DetectiveClues.View;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View.Scene.DetectiveServoskull;

[Serializable]
public sealed class DetectiveServoskullViewController : MonoBehaviour, IDetectiveServoskullDelegate, IAreaLoadingStagesHandler, ISubscriber
{
	public enum State
	{
		Idle,
		Scan
	}

	[SerializeField]
	private Transform m_ScannerBone;

	private AbstractUnitEntityView m_UnitView;

	private MapObjectEntity? m_ScanTargetEntity;

	private DetectiveServoskullVisualChaser m_Chaser;

	public State CurrentState { get; private set; }

	private AbstractUnitEntity Unit => m_UnitView.EntityData;

	private PartDetectiveServoSkull Servoskull => Unit.GetRequired<PartDetectiveServoSkull>();

	private DetectiveServoskullRoot? Settings => ConfigRoot.Instance.DetectiveServoskull;

	bool IDetectiveServoskullDelegate.IsVisualSyncedToAgent
	{
		get
		{
			if (!(m_Chaser == null) && m_Chaser.isActiveAndEnabled)
			{
				return !m_Chaser.IsInMotion;
			}
			return true;
		}
	}

	public Vector3 CurrentVisualPosition
	{
		get
		{
			if (!(m_Chaser != null))
			{
				return base.transform.position;
			}
			return m_Chaser.CurrentWorldPosition;
		}
	}

	private void Start()
	{
		m_UnitView = GetComponent<AbstractUnitEntityView>();
		m_Chaser = GetComponent<DetectiveServoskullVisualChaser>();
	}

	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	private void OnDestroy()
	{
		EventBus.Unsubscribe(this);
	}

	private void LateUpdate()
	{
		switch (CurrentState)
		{
		case State.Idle:
			UpdateIdle();
			break;
		case State.Scan:
			UpdateScan();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void UpdateIdle()
	{
	}

	private void UpdateScan()
	{
		if (m_ScanTargetEntity != null)
		{
			if (m_ScanTargetEntity.View is IScanTargetOverride scanTargetOverride && scanTargetOverride.ScanTargetOverride != null)
			{
				m_ScannerBone.LookAt(scanTargetOverride.ScanTargetOverride);
			}
			else
			{
				m_ScannerBone.LookAt(m_ScanTargetEntity.Position);
			}
		}
	}

	private async Task PlayScanAnimation(MapObjectEntity target)
	{
		if (Settings == null)
		{
			return;
		}
		if (target is DetectiveTraceEntity detectiveTraceEntity)
		{
			FxHelper.SpawnFxOnPoint(Settings?.FxOnScanTracePrefab?.Load(), detectiveTraceEntity.Position);
		}
		if (target is DetectiveTraceRootEntity detectiveTraceRootEntity)
		{
			FxHelper.SpawnFxOnPoint(Settings?.FxOnScanTracePrefab?.Load(), detectiveTraceRootEntity.Position);
		}
		GameObject gameObject = FxHelper.SpawnFxOnGameObject(Settings?.ScanFxPrefab?.Load(), m_ScannerBone.gameObject);
		if (!(gameObject == null))
		{
			float scanFxDurationSeconds = Settings.ScanFxDurationSeconds;
			float scanFxFadeSeconds = Settings.ScanFxFadeSeconds;
			FxFadeOut ensuredFadeOut = gameObject.EnsureComponent<FxFadeOut>();
			ensuredFadeOut.Duration = scanFxFadeSeconds;
			gameObject.transform.SetParent(m_ScannerBone.transform, worldPositionStays: true);
			TimeSpan scanEndTime = Game.Instance.Controllers.TimeController.GameTime + TimeSpan.FromSeconds(scanFxDurationSeconds);
			TimeSpan scanFadeEndTime = scanEndTime + TimeSpan.FromSeconds(scanFxFadeSeconds);
			while (Game.Instance.Controllers.TimeController.GameTime < scanEndTime)
			{
				await NextTickAwaiter.New();
			}
			ensuredFadeOut.StartForceFadeOut = true;
			while (Game.Instance.Controllers.TimeController.GameTime < scanFadeEndTime)
			{
				await NextTickAwaiter.New();
			}
		}
	}

	async Task IDetectiveServoskullDelegate.PlayScanAnimation(MapObjectEntity target)
	{
		await PlayScanAnimation(target);
	}

	void IDetectiveServoskullDelegate.SetScanTargetEntity(MapObjectEntity? targetEntity)
	{
		CurrentState = ((targetEntity != null) ? State.Scan : State.Idle);
		m_ScanTargetEntity = targetEntity;
	}

	void IAreaLoadingStagesHandler.OnAreaScenesLoaded()
	{
	}

	void IAreaLoadingStagesHandler.OnAreaLoadingComplete()
	{
		Servoskull.Delegate = this;
	}
}
