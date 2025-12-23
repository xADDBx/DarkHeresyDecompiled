using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.View.Scene.DetectiveServoskull;

[Serializable]
public sealed class DetectiveServoskullVisualChaser : MonoBehaviour, IAreaLoadingStagesHandler, ISubscriber
{
	[SerializeField]
	private Transform m_VisualTransform;

	[SerializeField]
	private bool m_UseAcceleration = true;

	[SerializeField]
	private float m_Acceleration = 20f;

	[SerializeField]
	private float m_OverrideMaxSpeed;

	[SerializeField]
	private float m_OverrideAngularSpeed;

	[SerializeField]
	private float m_AlignWithAgentDistance = 0.5f;

	[Space]
	[InspectorReadOnly]
	public float VisualSpeed;

	private AbstractUnitEntityView m_UnitView;

	private float m_CurrentSpeed;

	private Vector3 m_CurrentWorldPos;

	public bool IsInMotion => Mathf.Abs(m_CurrentSpeed) > 0f;

	private AbstractUnitEntity Unit => m_UnitView.EntityData;

	private DetectiveServoskullRoot? Settings => ConfigRoot.Instance.DetectiveServoskull;

	private float MaxSpeed
	{
		get
		{
			if (!(m_OverrideMaxSpeed > 0f))
			{
				return Unit.View.MovementAgent.MaxSpeed;
			}
			return m_OverrideMaxSpeed;
		}
	}

	private float AngularSpeed
	{
		get
		{
			if (!(m_OverrideAngularSpeed > 0f))
			{
				return Unit.View.MovementAgent.AngularSpeedWhenMove;
			}
			return m_OverrideAngularSpeed;
		}
	}

	public Vector3 CurrentWorldPosition => m_CurrentWorldPos;

	private void Start()
	{
		m_UnitView = GetComponent<AbstractUnitEntityView>();
	}

	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
		ResetMovement();
	}

	private void OnDestroy()
	{
		EventBus.Unsubscribe(this);
		ResetMovement();
	}

	private void LateUpdate()
	{
		UpdateMovement();
	}

	private void ResetMovement()
	{
		if (!(m_VisualTransform == null))
		{
			base.transform.GetPositionAndRotation(out var position, out var rotation);
			m_VisualTransform.SetPositionAndRotation(position, rotation);
			m_CurrentSpeed = 0f;
			m_CurrentWorldPos = m_VisualTransform.position;
		}
	}

	private void UpdateMovement()
	{
		if (m_VisualTransform == null)
		{
			return;
		}
		Vector3 position = base.transform.position;
		float num = Vector3.Distance(m_CurrentWorldPos, position);
		if (num >= Settings.TeleportWhenDistanceToLeaderIsGreaterThan)
		{
			ResetMovement();
			return;
		}
		float num2 = (m_UseAcceleration ? (m_CurrentSpeed * m_CurrentSpeed / (2f * m_Acceleration)) : 0f);
		float target = ((num > num2) ? MaxSpeed : 0f);
		float deltaTime = Time.deltaTime;
		m_CurrentSpeed = Mathf.MoveTowards(m_CurrentSpeed, target, m_UseAcceleration ? (m_Acceleration * deltaTime) : float.MaxValue);
		VisualSpeed = m_CurrentSpeed;
		m_CurrentWorldPos = Vector3.MoveTowards(m_CurrentWorldPos, position, m_CurrentSpeed * deltaTime);
		m_VisualTransform.position = m_CurrentWorldPos;
		if (num <= m_AlignWithAgentDistance)
		{
			m_VisualTransform.rotation = Quaternion.RotateTowards(m_VisualTransform.rotation, base.transform.rotation, AngularSpeed * deltaTime);
			return;
		}
		position.y = m_VisualTransform.position.y;
		m_VisualTransform.LookAt(position, Vector3.up);
	}

	public void OnAreaScenesLoaded()
	{
	}

	public void OnAreaLoadingComplete()
	{
		ResetMovement();
	}
}
