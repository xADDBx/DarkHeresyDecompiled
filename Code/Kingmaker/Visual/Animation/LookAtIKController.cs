using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using RootMotion.FinalIK;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

[RequireComponent(typeof(LookAtIK))]
public class LookAtIKController : MonoBehaviour, IIKComponent
{
	private class LookAtCommandParams
	{
		public Vector3? TargetPosition;

		public float TurningSeconds;

		public float DurationSeconds;
	}

	private class LookAtCommand
	{
		private readonly LookAtCommandParams m_Params;

		private float m_Time;

		public bool IsStarted { get; private set; }

		public bool IsFinished { get; private set; }

		public bool ResetAfterFinished => m_Params.DurationSeconds > 0f;

		public LookAtCommand(LookAtCommandParams @params)
		{
			m_Params = @params;
		}

		public void Start(LookAtIKController controller)
		{
			IsStarted = true;
			if (m_Params.TargetPosition.HasValue)
			{
				controller.StartLookAtInternal(m_Params.TargetPosition.Value, m_Params.TurningSeconds);
			}
			else
			{
				controller.StopLookAtInternal(m_Params.TurningSeconds);
			}
		}

		public void Update(float deltaTime)
		{
			m_Time += deltaTime;
			if (m_Time >= m_Params.TurningSeconds + m_Params.DurationSeconds || (m_Time >= m_Params.TurningSeconds && !m_Params.TargetPosition.HasValue))
			{
				IsFinished = true;
			}
		}
	}

	public const float MaxDeltaAngle = 80f;

	public const float DefaultTurningTime = 0.3f;

	private const float Epsilon = 0.0001f;

	public static Vector3 EyeShift = Vector3.up * 1.7f;

	private LookAtIK m_LookAtIK;

	private float m_InitialOrientation;

	private float m_TimeSinceTurnStart;

	private float m_TurningSeconds;

	private Vector3 m_InitialPosition;

	private Vector3 m_TargetPosition;

	private Vector3 m_PositionVelocityRef;

	private float m_InitialWeight;

	private float m_TargetWeight;

	private float m_WeightVelocityRef;

	private readonly Queue<LookAtCommandParams> m_CommandsQueue = new Queue<LookAtCommandParams>();

	private LookAtCommand m_CurrentCommand;

	private bool m_ResetAfterCommandFinished;

	private void Start()
	{
		m_LookAtIK = base.gameObject.GetComponent<LookAtIK>();
		m_LookAtIK.solver.IKPositionWeight = 0f;
		m_TargetWeight = 0f;
		SetUpBones();
	}

	public void StartLookAt(Vector3 position, float turningSeconds = 0.3f, float durationSeconds = 0f)
	{
		if (!(m_LookAtIK == null))
		{
			m_CommandsQueue.Enqueue(new LookAtCommandParams
			{
				TargetPosition = position,
				TurningSeconds = turningSeconds,
				DurationSeconds = durationSeconds
			});
		}
	}

	public void StopLookAt(float turningSeconds = 0.3f)
	{
		if (!(m_LookAtIK == null) && m_LookAtIK.solver.IKPositionWeight != 0f)
		{
			m_CommandsQueue.Enqueue(new LookAtCommandParams
			{
				TargetPosition = null,
				TurningSeconds = turningSeconds
			});
		}
	}

	private void StartLookAtInternal(Vector3 position, float turningDurationSeconds)
	{
		if (!(m_LookAtIK == null))
		{
			m_InitialWeight = m_LookAtIK.solver.IKPositionWeight;
			m_InitialPosition = m_LookAtIK.solver.IKPosition;
			m_TargetPosition = position;
			m_TurningSeconds = turningDurationSeconds;
			m_TargetWeight = 1f;
			m_TimeSinceTurnStart = 0f;
			IKComponentsManager.Instance.RegisterComponent(this);
		}
	}

	private void StopLookAtInternal(float fadingDurationSeconds)
	{
		if (!(m_LookAtIK == null) && m_LookAtIK.solver.IKPositionWeight != 0f)
		{
			m_InitialWeight = m_LookAtIK.solver.IKPositionWeight;
			m_InitialPosition = m_LookAtIK.solver.IKPosition;
			m_TurningSeconds = fadingDurationSeconds;
			m_TargetWeight = 0f;
			m_TimeSinceTurnStart = 0f;
		}
	}

	public bool IsLookingAt(Vector3 position)
	{
		if (m_LookAtIK == null || m_LookAtIK.solver.IKPositionWeight == 0f)
		{
			return false;
		}
		if ((m_LookAtIK.solver.IKPosition - position).sqrMagnitude < 0.0001f)
		{
			return Mathf.Approximately(m_LookAtIK.solver.IKPositionWeight, 1f);
		}
		return false;
	}

	private void Update()
	{
		LookAtCommand nextCommand = GetNextCommand();
		if (nextCommand != null)
		{
			m_ResetAfterCommandFinished = nextCommand.ResetAfterFinished;
			ProcessCommand(nextCommand, Time.deltaTime);
		}
		else if (m_LookAtIK.solver.IKPositionWeight != 0f && m_ResetAfterCommandFinished)
		{
			m_ResetAfterCommandFinished = false;
			StopLookAt();
		}
	}

	private LookAtCommand GetNextCommand()
	{
		if (m_CurrentCommand == null || m_CurrentCommand.IsFinished)
		{
			m_CurrentCommand = (m_CommandsQueue.TryDequeue(out var result) ? new LookAtCommand(result) : null);
		}
		return m_CurrentCommand;
	}

	private void ProcessCommand(LookAtCommand command, float deltaTime)
	{
		m_TimeSinceTurnStart += deltaTime;
		if (!command.IsStarted)
		{
			command.Start(this);
		}
		else
		{
			command.Update(deltaTime);
		}
	}

	void IIKComponent.DoLateUpdate()
	{
		if (!Mathf.Approximately(m_LookAtIK.solver.IKPositionWeight, m_TargetWeight) || !((m_LookAtIK.solver.IKPosition - m_TargetPosition).sqrMagnitude < 0.0001f))
		{
			float t = ((m_TimeSinceTurnStart < m_TurningSeconds) ? (m_TimeSinceTurnStart / m_TurningSeconds) : 1f);
			InterpolateTargetPosition(t);
			InterpolateWeight(t);
			if (m_LookAtIK.solver.IKPositionWeight == 0f && m_TargetWeight == 0f)
			{
				IKComponentsManager.Instance.UnregisterComponent(this);
			}
		}
	}

	private void InterpolateTargetPosition(float t)
	{
		if (!((m_LookAtIK.solver.IKPosition - m_TargetPosition).sqrMagnitude < 0.0001f))
		{
			if (m_TargetWeight < 0.5f)
			{
				m_InitialPosition = m_TargetPosition;
				m_LookAtIK.solver.IKPosition = m_TargetPosition;
			}
			else
			{
				m_LookAtIK.solver.IKPosition = Vector3.Slerp(m_InitialPosition, m_TargetPosition, t);
			}
		}
	}

	private void InterpolateWeight(float t)
	{
		m_LookAtIK.solver.IKPositionWeight = Mathf.Lerp(m_InitialWeight, m_TargetWeight, t);
	}

	public void DisableIKAiming()
	{
		m_TargetWeight = 0f;
		m_LookAtIK.solver.IKPositionWeight = 0f;
		IKComponentsManager.Instance.UnregisterComponent(this);
	}

	private void SetUpBones()
	{
		m_LookAtIK.solver.head = new IKSolverLookAt.LookAtBone(GetBoneTransform("Head"));
		m_LookAtIK.solver.spine = new IKSolverLookAt.LookAtBone[3]
		{
			new IKSolverLookAt.LookAtBone(GetBoneTransform("Spine_2")),
			new IKSolverLookAt.LookAtBone(GetBoneTransform("Spine_3")),
			new IKSolverLookAt.LookAtBone(GetBoneTransform("Neck"))
		};
	}

	private Transform GetBoneTransform(string boneName)
	{
		return base.gameObject.GetComponentsInChildren<Transform>().FindOrDefault((Transform t) => t.name == boneName);
	}
}
