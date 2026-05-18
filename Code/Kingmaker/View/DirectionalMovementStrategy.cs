using Code.Visual.Animation;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View;

public sealed class DirectionalMovementStrategy : IUnitMovementStrategy
{
	public const float Epsilon = 0.0001f;

	public const float AccelerationToRunThreshold = 0.7f;

	public const float AccelerationToSprintThreshold = 0.95f;

	private const float SlidingAngleLimit = 90f;

	private readonly UnitMovementAgent m_movementAgent;

	private Vector3 m_Velocity;

	private Vector3 m_NextVelocity;

	private float m_Speed;

	private bool m_FirstTick;

	private int m_SlidingAssistDirection;

	private float m_CurrentSlidingAngle;

	private float m_MaxWalkSpeed;

	private float m_MaxRunSpeed;

	private float m_MaxSprintSpeed;

	private bool m_SpeedValuesInitiated;

	public float Speed => m_Speed;

	public Vector3 Velocity => m_Velocity;

	public Vector3? Destination => null;

	public bool IsReallyMoving
	{
		get
		{
			if (!(DirectionFromControllerMagnitude > 0.0001f))
			{
				return m_Velocity.sqrMagnitude > 0.0001f;
			}
			return true;
		}
	}

	public bool EnableSlidingAssist { get; private set; }

	public float CurrentSlidingAngle => m_CurrentSlidingAngle;

	public int SlidingAssistDirection => m_SlidingAssistDirection;

	public GridNodeBase CurrentNode { get; private set; }

	public Vector2 DirectionFromController { get; set; }

	public float DirectionFromControllerMagnitude { get; set; }

	public DirectionalMovementStrategy(UnitMovementAgent movementAgent)
	{
		m_movementAgent = movementAgent;
	}

	public static WalkSpeedType GetMovementType(float multiplier)
	{
		if (!(multiplier > 0.95f))
		{
			if (multiplier > 0.7f)
			{
				return WalkSpeedType.Run;
			}
			return WalkSpeedType.Walk;
		}
		return WalkSpeedType.Sprint;
	}

	public static float GetSpeedByControllerStickDeflection(AbstractUnitEntityView unitView, float stickDeflection)
	{
		float a;
		float maxSpeed;
		float t;
		if (!(stickDeflection > 0.95f))
		{
			if (stickDeflection > 0.7f)
			{
				a = GetMaxSpeed(unitView, WalkSpeedType.Walk);
				maxSpeed = GetMaxSpeed(unitView, WalkSpeedType.Run);
				t = (stickDeflection - 0.7f) / 0.25f;
			}
			else
			{
				a = 0f;
				maxSpeed = GetMaxSpeed(unitView, WalkSpeedType.Walk);
				t = stickDeflection / 0.7f;
			}
		}
		else
		{
			a = GetMaxSpeed(unitView, WalkSpeedType.Run);
			maxSpeed = GetMaxSpeed(unitView, WalkSpeedType.Sprint);
			t = (stickDeflection - 0.95f) / 0.050000012f;
		}
		return Mathf.Lerp(a, maxSpeed, t);
	}

	public static void UpdateSliding(Vector3 position, Vector2 desiredDir, float deltaTime, ref int slidingAssistDirection, ref float currentSlidingAngle)
	{
		if (slidingAssistDirection == 0)
		{
			GraphNode node = ObstacleAnalyzer.GetNearestNode(position).node;
			Vector3 end = position + desiredDir.RotateAroundPoint(Vector2.zero, -90f).To3D().normalized;
			Vector3 end2 = position + desiredDir.RotateAroundPoint(Vector2.zero, 90f).To3D().normalized;
			Linecast.LinecastGrid(node.Graph, position, end, node, out var hit, ObstacleAnalyzer.DefaultXZConstraint, ref Linecast.HasConnectionTransition.Instance);
			Linecast.LinecastGrid(node.Graph, position, end2, node, out var hit2, ObstacleAnalyzer.DefaultXZConstraint, ref Linecast.HasConnectionTransition.Instance);
			slidingAssistDirection = ((!(hit.distance > hit2.distance)) ? 1 : (-1));
		}
		currentSlidingAngle = Mathf.Clamp(currentSlidingAngle + deltaTime * (float)slidingAssistDirection * 45f, -90f, 90f);
	}

	public void TickMovement(float deltaTime, MovementStrategyInput input, out MovementStrategyProcessingResult result)
	{
		result = new MovementStrategyProcessingResult
		{
			IsPositionChanged = false,
			IsTraverseInProgress = false,
			EstimatedTimeLeft = 0f,
			FaceDirection = input.FaceDirection,
			MoveDirection = input.MoveDirection,
			State = MovementStrategyProcessingResult.MovementState.InProgress
		};
		if (Game.Instance.CurrentModeType != GameModeType.Default)
		{
			return;
		}
		if (!(m_movementAgent.Unit?.AnimationManager?.IsPreventingMovement).GetValueOrDefault())
		{
			AbstractUnitEntityView unit = m_movementAgent.Unit;
			if ((object)unit == null || !unit.IsCommandsPreventMovement)
			{
				using (ProfileScope.New("Tick Movement Continuous"))
				{
					AbstractUnitEntity abstractUnitEntity = m_movementAgent.Unit?.Data;
					if (abstractUnitEntity != null)
					{
						m_movementAgent.Position = abstractUnitEntity.Position;
					}
					if (!IsReallyMoving)
					{
						result.State = MovementStrategyProcessingResult.MovementState.Completed;
					}
					else
					{
						bool firstTick = m_FirstTick;
						m_FirstTick = false;
						float num = m_movementAgent.MaxSpeedOverride ?? GetSpeedByControllerStickDeflection(DirectionFromControllerMagnitude);
						if (firstTick)
						{
							m_Speed = num;
							Vector3 position = AstarPath.active.graphs[0].GetNearest(m_movementAgent.Position).position;
							m_movementAgent.Position = position;
						}
						if (DirectionFromController.SqrMagnitude() < Mathf.Epsilon)
						{
							result.State = MovementStrategyProcessingResult.MovementState.Completed;
						}
						Vector2 vector = CalcMoveDirection(deltaTime, abstractUnitEntity);
						Vector2 faceDirection = CalcFaceDirection(input.FaceDirection, vector, deltaTime);
						m_Speed = ((Mathf.Abs(num - m_Speed) < m_movementAgent.Acceleration * deltaTime) ? num : (m_Speed + Mathf.Sign(num - m_Speed) * m_movementAgent.Acceleration * deltaTime));
						m_NextVelocity = vector.To3D() * m_Speed;
						m_Velocity = m_NextVelocity;
						GridNodeBase targetNode;
						Vector3 vector2 = UnitMovementAgent.Move(m_movementAgent.Position, m_NextVelocity * deltaTime, m_movementAgent.Corpulence, out targetNode);
						Vector3 vector3 = vector2 - (m_movementAgent.Position + m_NextVelocity * deltaTime);
						result.IsPositionChanged = Mathf.Abs(vector3.x) < 0.01f && Mathf.Abs(vector3.z) < 0.01f;
						if (result.IsPositionChanged)
						{
							EnableSlidingAssist = false;
							m_CurrentSlidingAngle = 0f;
							m_SlidingAssistDirection = 0;
						}
						else
						{
							EnableSlidingAssist = true;
						}
						if (!NodeLinksExtensions.AreConnected(CurrentNode, targetNode, out var _))
						{
							m_movementAgent.Position = vector2;
							CurrentNode = targetNode;
						}
						result.MoveDirection = vector;
						result.FaceDirection = faceDirection;
					}
					return;
				}
			}
		}
		result.State = MovementStrategyProcessingResult.MovementState.Completed;
	}

	public void Stop()
	{
		m_FirstTick = true;
		m_Velocity = Vector3.zero;
	}

	private static UnitAnimationActionLocomotion GetLocomotion(AbstractUnitEntityView unitView)
	{
		return unitView.AnimationManager.Or(null)?.GetAction(UnitAnimationType.LocoMotion) as UnitAnimationActionLocomotion;
	}

	private static float GetMaxSpeed(AbstractUnitEntityView unitView, WalkSpeedType walkSpeedType)
	{
		UnitAnimationActionLocomotion locomotion = GetLocomotion(unitView);
		if (!(locomotion != null))
		{
			return 0.01f;
		}
		return GetMaxSpeed(locomotion, walkSpeedType);
	}

	private static float GetMaxSpeed(UnitAnimationActionLocomotion locomotion, WalkSpeedType walkSpeedType)
	{
		if (locomotion == null)
		{
			return 0.01f;
		}
		UnitAnimationActionLocomotion.WalkingTypeData walkingTypeData = locomotion.GetWalkingTypeData(locomotion.NonCombatLocomotionData, walkSpeedType);
		return Mathf.Max(0.01f, walkingTypeData.Parameters.Speed);
	}

	private void CalcSpeedValues()
	{
		if (!m_SpeedValuesInitiated)
		{
			UnitAnimationActionLocomotion locomotion = GetLocomotion(m_movementAgent.Unit);
			m_MaxWalkSpeed = GetMaxSpeed(locomotion, WalkSpeedType.Walk);
			m_MaxRunSpeed = GetMaxSpeed(locomotion, WalkSpeedType.Run);
			m_MaxSprintSpeed = GetMaxSpeed(locomotion, WalkSpeedType.Sprint);
			m_SpeedValuesInitiated = true;
		}
	}

	private float GetSpeedByControllerStickDeflection(float stickDeflection)
	{
		CalcSpeedValues();
		float a;
		float b;
		float t;
		if (!(stickDeflection > 0.95f))
		{
			if (stickDeflection > 0.7f)
			{
				a = m_MaxWalkSpeed;
				b = m_MaxRunSpeed;
				t = (stickDeflection - 0.7f) / 0.25f;
			}
			else
			{
				a = 0f;
				b = m_MaxWalkSpeed;
				t = stickDeflection / 0.7f;
			}
		}
		else
		{
			a = m_MaxRunSpeed;
			b = m_MaxSprintSpeed;
			t = (stickDeflection - 0.95f) / 0.050000012f;
		}
		return Mathf.Lerp(a, b, t);
	}

	private Vector2 CalcMoveDirection(float deltaTime, AbstractUnitEntity unit)
	{
		if (unit == null || !EnableSlidingAssist)
		{
			return DirectionFromController;
		}
		Vector2 directionFromController = DirectionFromController;
		UpdateSliding(m_movementAgent.Position, directionFromController, deltaTime, ref m_SlidingAssistDirection, ref m_CurrentSlidingAngle);
		directionFromController = directionFromController.RotateAroundPoint(Vector2.zero, m_CurrentSlidingAngle);
		return Vector2.Lerp(DirectionFromController, directionFromController, 0.5f);
	}

	private Vector2 CalcFaceDirection(Vector2 faceDir, Vector2 desiredFaceDir, float deltaTime)
	{
		float currentAngularSpeed = m_movementAgent.CurrentAngularSpeed;
		float f = Vector2.SignedAngle(faceDir, desiredFaceDir);
		float b = Mathf.Abs(f);
		float num = Mathf.Sign(f);
		return (Quaternion.AngleAxis(Mathf.Min(currentAngularSpeed * deltaTime, b), Vector3.down * num) * faceDir.To3D()).To2D();
	}
}
