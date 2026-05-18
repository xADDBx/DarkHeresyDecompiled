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
		public readonly float TurningSeconds;

		public float DurationSeconds;

		public IVector3PositionProvider TargetPositionProvider { get; private set; }

		public RotatedBonesSet RotatedBonesSet { get; private set; }

		private LookAtCommandParams(float turningSeconds)
		{
			TurningSeconds = turningSeconds;
		}

		public static LookAtCommandParams StartLookAt(IVector3PositionProvider positionProvider, RotatedBonesSet rotatedBonesSet, float turningSeconds, float durationSeconds)
		{
			return new LookAtCommandParams(turningSeconds)
			{
				TargetPositionProvider = positionProvider,
				RotatedBonesSet = rotatedBonesSet,
				DurationSeconds = durationSeconds
			};
		}

		public static LookAtCommandParams StopLookAt(float turningSeconds)
		{
			return new LookAtCommandParams(turningSeconds);
		}
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
			if (m_Params.TargetPositionProvider != null)
			{
				controller.StartLookAtInternal(m_Params.TargetPositionProvider, m_Params.RotatedBonesSet, m_Params.TurningSeconds);
			}
			else
			{
				controller.StopLookAtInternal(m_Params.TurningSeconds);
			}
		}

		public void Update(float deltaTime)
		{
			m_Time += deltaTime;
			if (m_Time >= m_Params.TurningSeconds + m_Params.DurationSeconds || (m_Time >= m_Params.TurningSeconds && m_Params.TargetPositionProvider == null))
			{
				IsFinished = true;
			}
		}
	}

	public const float MaxDeltaAngle = 80f;

	public const float DefaultTurningTime = 0.3f;

	private const float Epsilon = 0.0001f;

	public static readonly Vector3 EyeShift = Vector3.up * 1.7f;

	private LookAtIK m_LookAtIK;

	private float m_InitialOrientation;

	private float m_TimeSinceTurnStart;

	private float m_TurningSeconds;

	private Vector3 m_PositionVelocityRef;

	private IVector3PositionProvider m_TargetPositionProvider;

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
		m_LookAtIK.fixTransforms = false;
		m_TargetWeight = 0f;
		SetUpBones();
	}

	public void PushLookAtCommand(IVector3PositionProvider positionProvider, RotatedBonesSet rotatedBonesSet = RotatedBonesSet.HeadAndSpine, float turningSeconds = 0.3f, float durationSeconds = 0f)
	{
		if (!(m_LookAtIK == null))
		{
			m_CommandsQueue.Enqueue(LookAtCommandParams.StartLookAt(positionProvider, rotatedBonesSet, turningSeconds, durationSeconds));
		}
	}

	public void PushResetCommand(float turningSeconds = 0.3f)
	{
		if (!(m_LookAtIK == null))
		{
			m_CommandsQueue.Enqueue(LookAtCommandParams.StopLookAt(turningSeconds));
		}
	}

	public void LookAtImmediately(IVector3PositionProvider positionProvider, RotatedBonesSet rotatedBonesSet = RotatedBonesSet.HeadAndSpine, float turningSeconds = 0.3f, float durationSeconds = 0f)
	{
		CancelAll();
		PushLookAtCommand(positionProvider, rotatedBonesSet, turningSeconds, durationSeconds);
	}

	public void ResetImmediately(float turningSeconds = 0.3f)
	{
		CancelAll();
		PushResetCommand(turningSeconds);
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

	public bool HasCommandToProcess()
	{
		if (m_CurrentCommand == null)
		{
			return m_CommandsQueue.Count > 0;
		}
		return true;
	}

	public void DisableIKAiming()
	{
		m_TargetWeight = 0f;
		m_LookAtIK.solver.IKPositionWeight = 0f;
		IKComponentsManager.Instance.UnregisterComponent(this);
	}

	private void StartLookAtInternal(IVector3PositionProvider positionProvider, RotatedBonesSet rotatedBonesSet, float turningDurationSeconds)
	{
		if (!(m_LookAtIK == null))
		{
			m_InitialWeight = m_LookAtIK.solver.IKPositionWeight;
			m_TargetPositionProvider = new ClampedToSphereSegmentPositionProvider(base.gameObject.transform, m_LookAtIK.solver.head.transform, 80f, positionProvider);
			m_TurningSeconds = turningDurationSeconds;
			m_TargetWeight = 1f;
			m_TimeSinceTurnStart = 0f;
			m_PositionVelocityRef = Vector3.zero;
			m_LookAtIK.solver.headWeight = 1f;
			IKSolverLookAt solver = m_LookAtIK.solver;
			float bodyWeight = ((rotatedBonesSet != RotatedBonesSet.OnlyHead) ? 0.5f : 0f);
			solver.bodyWeight = bodyWeight;
			IKComponentsManager.Instance.RegisterComponent(this);
		}
	}

	private void StopLookAtInternal(float fadingDurationSeconds)
	{
		if (!(m_LookAtIK == null) && m_LookAtIK.solver.IKPositionWeight != 0f)
		{
			m_InitialWeight = m_LookAtIK.solver.IKPositionWeight;
			m_TurningSeconds = fadingDurationSeconds;
			m_TargetWeight = 0f;
			m_TimeSinceTurnStart = 0f;
			m_PositionVelocityRef = Vector3.zero;
		}
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
			PushResetCommand();
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
		if (!Mathf.Approximately(m_LookAtIK.solver.IKPositionWeight, m_TargetWeight) || !((m_LookAtIK.solver.IKPosition - m_TargetPositionProvider.Position).sqrMagnitude < 0.0001f))
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
		if (!((m_LookAtIK.solver.IKPosition - m_TargetPositionProvider.Position).sqrMagnitude < 0.0001f))
		{
			if (m_TargetWeight < 0.5f)
			{
				m_LookAtIK.solver.IKPosition = m_TargetPositionProvider.Position;
			}
			else
			{
				m_LookAtIK.solver.IKPosition = Vector3.SmoothDamp(m_LookAtIK.solver.IKPosition, m_TargetPositionProvider.Position, ref m_PositionVelocityRef, m_TurningSeconds * 0.5f);
			}
		}
	}

	private void InterpolateWeight(float t)
	{
		m_LookAtIK.solver.IKPositionWeight = Mathf.Lerp(m_InitialWeight, m_TargetWeight, t);
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

	private void CancelAll()
	{
		m_CommandsQueue.Clear();
		m_CurrentCommand = null;
	}
}
