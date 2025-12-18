using System;
using System.Collections.Generic;
using Cinemachine;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units.CameraFollow;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class CameraFollowController : IControllerTick, IController, IControllerStop, IControllerEnable, IControllerDisable, ITurnBasedModeHandler, ISubscriber, IAreaHandler, ICameraMovementHandler
{
	private CombatFollowTasksProvider m_TasksProvider;

	private readonly List<ICameraFollowTask> m_ActiveTasks = new List<ICameraFollowTask>();

	private ICameraFollowTask m_DelayedTask;

	private TimeSpan m_DelayStarted;

	private TimeSpan m_DelayTime;

	private Coroutine m_ScrollToCoroutine;

	private bool m_IsTurnBased;

	private bool m_IsActionCameraInAction;

	private CinemachineBrain m_Brain;

	private List<CinemachineVirtualCamera> m_VirtualCameras;

	private CinemachineVirtualCamera m_SourceVirtualCamera;

	private AudioListenerPositionController m_AudioListenerPosition;

	public void OnEnable()
	{
		m_IsTurnBased = Game.Instance.Controllers.TurnController.TurnBasedModeActive;
		m_TasksProvider = new CombatFollowTasksProvider(TryAddTask);
	}

	public void OnDisable()
	{
		if (m_Brain.enabled)
		{
			StopActionCamera();
		}
		SetTimescale(1f, force: true);
		m_TasksProvider.Dispose();
		m_TasksProvider = null;
	}

	void IControllerStop.OnStop()
	{
		m_ActiveTasks.Clear();
		RemoveDelayedTask();
		m_IsTurnBased = false;
		CleanActionCamera();
	}

	public TickType GetTickType()
	{
		return TickType.EndOfFrame;
	}

	public void Tick()
	{
		if (!m_IsTurnBased)
		{
			return;
		}
		if (m_DelayedTask != null && Game.Instance.Controllers.TimeController.RealTime > m_DelayStarted + m_DelayTime)
		{
			m_ActiveTasks.Add(m_DelayedTask);
			RemoveDelayedTask();
		}
		ICameraFollowTask cameraFollowTask = null;
		for (int num = m_ActiveTasks.Count - 1; num >= 0; num--)
		{
			ICameraFollowTask cameraFollowTask2 = m_ActiveTasks[num];
			if (!cameraFollowTask2.IsStarted)
			{
				if (!m_Brain.enabled)
				{
					if (cameraFollowTask2.CanStartBrain)
					{
						StartActionCamera();
					}
					else
					{
						m_ActiveTasks.RemoveAt(num);
					}
					return;
				}
				StartTask(cameraFollowTask2);
			}
			if (cameraFollowTask2.IsActive)
			{
				if (cameraFollowTask == null || cameraFollowTask2.Priority >= cameraFollowTask.Priority)
				{
					cameraFollowTask = cameraFollowTask2;
				}
			}
			else
			{
				StopTask(cameraFollowTask2);
				SetTimescale(1f);
				m_ActiveTasks.RemoveAt(num);
			}
		}
		if (m_Brain.enabled && !m_Brain.IsBlending)
		{
			if (m_ActiveTasks.Count == 0)
			{
				StopActionCamera();
			}
			if (cameraFollowTask != null)
			{
				SetTimescale(cameraFollowTask.TaskParams.TimeScale);
			}
		}
	}

	private void StartTask(ICameraFollowTask task)
	{
		m_Brain.m_DefaultBlend = task.TaskParams.BlendSettings;
		CinemachineVirtualCamera cinemachineVirtualCamera = ClaimCamera();
		cinemachineVirtualCamera.gameObject.name = task.DebugName;
		PFLog.Camera.Log("start: " + task.DebugName);
		CameraRig instance = CameraRig.Instance;
		cinemachineVirtualCamera.ForceCameraPosition(instance.Camera.transform.position, instance.Camera.transform.rotation);
		cinemachineVirtualCamera.m_Lens.FieldOfView = instance.Camera.fieldOfView;
		cinemachineVirtualCamera.Priority = task.Priority;
		if (task.Target == null)
		{
			Transform transform = null;
			for (int i = 0; i < instance.CombatCameraRoot.transform.childCount; i++)
			{
				Transform child = instance.CombatCameraRoot.transform.GetChild(i);
				if (child.gameObject.name == "Camera Target")
				{
					transform = child;
					break;
				}
			}
			if (transform == null)
			{
				transform = new GameObject("Camera Target").transform;
				transform.parent = instance.CombatCameraRoot.transform;
			}
			transform.position = task.Position;
			cinemachineVirtualCamera.LookAt = transform;
			cinemachineVirtualCamera.Follow = transform;
		}
		else
		{
			cinemachineVirtualCamera.LookAt = task.Target;
			cinemachineVirtualCamera.Follow = task.Target;
		}
		task.Start(cinemachineVirtualCamera);
		cinemachineVirtualCamera.enabled = true;
	}

	private void StopTask(ICameraFollowTask task)
	{
		if (task.IsStarted)
		{
			task.VirtualCamera.enabled = false;
		}
		task.End();
		PFLog.Camera.Log("stop: " + task.DebugName);
	}

	private void TryAddTask(ICameraFollowTask task, bool delayed = false, float delaySeconds = 0f)
	{
		if (m_IsTurnBased)
		{
			if (delayed)
			{
				AddDelayedTask(task, delaySeconds);
			}
			else
			{
				m_ActiveTasks.Add(task);
			}
		}
	}

	private static void SetTimescale(float scale, bool force = false)
	{
		if ((!(scale < 1f) || !(Game.Instance.Controllers.TimeController.CameraFollowTimeScale < 1f)) && (force || !Mathf.Approximately(scale, Game.Instance.Controllers.TimeController.CameraFollowTimeScale)))
		{
			Game.Instance.GameCommandQueue.CameraFollowTimeScale(scale, force);
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		m_IsTurnBased = isTurnBased;
		if (!isTurnBased)
		{
			SetTimescale(1f, force: true);
		}
	}

	private void StartActionCamera()
	{
		if (!m_Brain.enabled)
		{
			m_Brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
			CinemachineVirtualCamera sourceVirtualCamera = m_SourceVirtualCamera;
			Transform cameraAttachPoint = CameraRig.Instance.CameraAttachPoint;
			CameraRig.Instance.ListenerUpdater.SetParent(CameraRig.Instance.Camera.transform);
			CinemachineCore.CameraUpdatedEvent.AddListener(UpdateListenerPosition);
			sourceVirtualCamera.ForceCameraPosition(cameraAttachPoint.transform.position, cameraAttachPoint.transform.rotation);
			sourceVirtualCamera.m_Lens.FieldOfView = CameraRig.Instance.Camera.fieldOfView;
			sourceVirtualCamera.Priority = CombatFollowTasksProvider.BaseCombatCameraPriority - 1;
			sourceVirtualCamera.enabled = true;
		}
		PFLog.Camera.Log("Start Combat Camera");
		m_Brain.enabled = true;
	}

	private void UpdateListenerPosition(CinemachineBrain brain)
	{
		if ((object)m_AudioListenerPosition == null)
		{
			m_AudioListenerPosition = CameraRig.Instance.ListenerUpdater.GetComponent<AudioListenerPositionController>();
		}
		m_AudioListenerPosition.PostCinemachineUpdate();
	}

	private void StopActionCamera()
	{
		foreach (ICameraFollowTask activeTask in m_ActiveTasks)
		{
			StopTask(activeTask);
		}
		m_ActiveTasks.Clear();
		Transform transform = CameraRig.Instance.Camera.transform;
		Vector3 vector = CameraRig.Instance.CameraAttachPoint.TransformDirection(transform.localPosition);
		CameraRig.Instance.SetRigPositionImmmediately(CameraRig.Instance.transform.position + vector);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		CinemachineCore.CameraUpdatedEvent.RemoveListener(UpdateListenerPosition);
		CameraRig.Instance.ListenerUpdater.SetParent(CameraRig.Instance.CameraAttachPoint);
		PFLog.Camera.Log("Stop Combat Camera");
		m_Brain.enabled = false;
		SetTimescale(1f);
	}

	private void InitializeCameras()
	{
		CameraRig instance = CameraRig.Instance;
		m_Brain = instance.Camera.gameObject.EnsureComponent<CinemachineBrain>();
		m_Brain.enabled = false;
		m_SourceVirtualCamera = instance.CombatCameraRoot.GetComponentInChildren<CinemachineVirtualCamera>();
		m_SourceVirtualCamera.enabled = false;
		if (m_SourceVirtualCamera == null)
		{
			PFLog.Default.Error("No combat camera found");
			return;
		}
		CinemachineFramingTransposer cinemachineComponent = m_SourceVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
		float cameraDistance = Vector3.Distance(CameraRig.Instance.Camera.transform.position, CameraRig.Instance.GetTargetPointPosition());
		if (cinemachineComponent != null)
		{
			cinemachineComponent.m_CameraDistance = cameraDistance;
		}
		m_VirtualCameras = new List<CinemachineVirtualCamera> { m_SourceVirtualCamera };
	}

	private void CleanActionCamera()
	{
		StopActionCamera();
		for (int num = CameraRig.Instance.CombatCameraRoot.transform.childCount - 1; num >= 0; num--)
		{
			GameObject gameObject = CameraRig.Instance.CombatCameraRoot.transform.GetChild(num).gameObject;
			if (!(gameObject == m_SourceVirtualCamera.gameObject))
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
	}

	private void AddDelayedTask(ICameraFollowTask task, float delay)
	{
		m_DelayedTask = task;
		m_DelayStarted = Game.Instance.Controllers.TimeController.RealTime;
		m_DelayTime = delay.Seconds();
	}

	private void RemoveDelayedTask()
	{
		m_DelayedTask = null;
		m_DelayStarted = default(TimeSpan);
		m_DelayTime = default(TimeSpan);
	}

	private CinemachineVirtualCamera ClaimCamera()
	{
		CinemachineVirtualCamera cinemachineVirtualCamera = m_VirtualCameras.FirstOrDefault((CinemachineVirtualCamera vcam) => vcam.IsValid && !vcam.enabled);
		if (cinemachineVirtualCamera == null)
		{
			cinemachineVirtualCamera = UnityEngine.Object.Instantiate(m_SourceVirtualCamera, CameraRig.Instance.Camera.transform.position, CameraRig.Instance.Camera.transform.rotation, CameraRig.Instance.CombatCameraRoot.transform).GetComponent<CinemachineVirtualCamera>();
			cinemachineVirtualCamera.enabled = false;
			m_VirtualCameras.Add(cinemachineVirtualCamera);
		}
		return cinemachineVirtualCamera;
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		InitializeCameras();
	}

	public void HandleCameraRotated(float angle)
	{
		if (m_Brain.enabled && angle != 0f)
		{
			StopActionCamera();
		}
	}

	public void HandleCameraTransformed(float distance)
	{
		if (m_Brain.enabled && distance != 0f)
		{
			StopActionCamera();
		}
	}
}
