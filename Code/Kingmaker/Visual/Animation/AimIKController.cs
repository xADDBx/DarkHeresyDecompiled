using System;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using RootMotion.FinalIK;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

[RequireComponent(typeof(AimIK))]
public class AimIKController : MonoBehaviour, IIKComponent
{
	private const float MinDistanceToTarget = 1.8f;

	private const string HeadBoneName = "Head";

	private const string MainHandRootBoneName = "R_Clavicle";

	private const string OffHandRootBoneName = "L_Clavicle";

	private const string MechadendriteRootBoneName = "BP_Mechadendrites_Ballistic";

	private static readonly string[] SpineBoneNames = new string[3] { "Spine_1", "Spine_2", "Spine_3" };

	private static readonly string[] MechadendriteBoneNames = new string[3] { "Mechadendrite01_1_R", "Mechadendrite01_2_R", "Mechadendrite01_3_R" };

	private AimIK m_AimIK;

	private float m_DurationSeconds;

	private float m_TargetValue;

	private float m_ValueVelocityRef;

	private Vector3 m_TargetPosition;

	private Vector3 m_PositionVelocityRef;

	private Transform m_Head;

	private IKSolver.Bone[] m_SpineBones;

	private IKSolver.Bone[] m_MechadendriteBones;

	private void Start()
	{
		m_AimIK = base.gameObject.GetComponent<AimIK>();
		m_AimIK.solver.IKPositionWeight = 0f;
		m_AimIK.fixTransforms = false;
		m_TargetValue = 0f;
		m_Head = GetBoneTransform("Head");
		m_SpineBones = new IKSolver.Bone[SpineBoneNames.Length];
		for (int i = 0; i < SpineBoneNames.Length; i++)
		{
			m_SpineBones[i] = new IKSolver.Bone(GetBoneTransform(SpineBoneNames[i]));
		}
		Transform boneTransform = GetBoneTransform("BP_Mechadendrites_Ballistic");
		if (boneTransform != null)
		{
			m_MechadendriteBones = new IKSolver.Bone[MechadendriteBoneNames.Length];
			for (int j = 0; j < MechadendriteBoneNames.Length; j++)
			{
				m_MechadendriteBones[j] = new IKSolver.Bone(GetBoneTransform(MechadendriteBoneNames[j], boneTransform.gameObject));
			}
		}
		else
		{
			m_MechadendriteBones = Array.Empty<IKSolver.Bone>();
		}
	}

	public void StartAim(Transform target, AttackingLimb attackingLimb, float turningDurationSeconds)
	{
		StartAim(target.position, attackingLimb, turningDurationSeconds);
	}

	public void StartAim(Vector3 targetPosition, AttackingLimb attackingLimb, float turningDurationSeconds)
	{
		if (!(m_AimIK == null) && TryGetAimTransform(attackingLimb, out var aimTransform))
		{
			m_TargetPosition = GetActualTargetPosition(targetPosition);
			if (m_AimIK.solver.IKPositionWeight < 0.25f || Mathf.Approximately(m_TargetValue, 0f))
			{
				m_AimIK.solver.IKPosition = m_TargetPosition;
				m_ValueVelocityRef = 0f;
				m_PositionVelocityRef = Vector3.zero;
			}
			m_AimIK.solver.bones = ((attackingLimb != AttackingLimb.Mechadendrite) ? m_SpineBones : m_MechadendriteBones);
			m_AimIK.solver.transform = aimTransform;
			m_AimIK.solver.axis = Vector3.forward;
			m_DurationSeconds = turningDurationSeconds;
			m_TargetValue = 1f;
			IKComponentsManager.Instance.RegisterComponent(this);
			PFLog.Animations.Log($"[AimIK] Start in {m_DurationSeconds} seconds");
		}
	}

	private Vector3 GetActualTargetPosition(Vector3 targetPosition)
	{
		if ((targetPosition - m_Head.position).magnitude < 1.8f)
		{
			Vector3 position = m_Head.position;
			Vector3 normalized = (targetPosition - position).normalized;
			return position + 1.8f * normalized;
		}
		return targetPosition;
	}

	public void StopAim(float fadingDurationSeconds)
	{
		if (!(m_AimIK == null) && !(m_AimIK.solver.transform == null))
		{
			Transform transform = m_AimIK.solver.bones.LastItem()?.transform;
			if (!(transform == null))
			{
				m_TargetPosition = transform.position + 1.8f * transform.right;
				m_AimIK.solver.transform = transform;
				m_AimIK.solver.IKPosition = m_TargetPosition;
				m_AimIK.solver.axis = Vector3.right;
				m_DurationSeconds = fadingDurationSeconds;
				m_TargetValue = 0f;
				m_ValueVelocityRef = 0f;
				m_PositionVelocityRef = Vector3.zero;
				PFLog.Animations.Log($"[AimIK] Stop in {m_DurationSeconds} seconds");
			}
		}
	}

	void IIKComponent.DoLateUpdate()
	{
		if (!Mathf.Approximately(m_AimIK.solver.IKPositionWeight, m_TargetValue) || !Mathf.Approximately((m_AimIK.solver.IKPosition - m_TargetPosition).sqrMagnitude, 0f))
		{
			m_AimIK.solver.IKPositionWeight = Mathf.SmoothDamp(m_AimIK.solver.IKPositionWeight, m_TargetValue, ref m_ValueVelocityRef, m_DurationSeconds * 0.5f);
			m_AimIK.solver.IKPosition = Vector3.SmoothDamp(m_AimIK.solver.IKPosition, m_TargetPosition, ref m_PositionVelocityRef, m_DurationSeconds * 0.5f);
			if (m_AimIK.solver.IKPositionWeight >= 0.999f)
			{
				m_AimIK.solver.IKPositionWeight = 1f;
			}
			if (m_AimIK.solver.IKPositionWeight <= 0.001f)
			{
				m_AimIK.solver.IKPositionWeight = 0f;
			}
			if ((m_AimIK.solver.IKPosition - m_TargetPosition).sqrMagnitude < 0.001f)
			{
				m_AimIK.solver.IKPosition = m_TargetPosition;
			}
			if (m_AimIK.solver.IKPositionWeight == 0f && m_TargetValue == 0f)
			{
				IKComponentsManager.Instance.UnregisterComponent(this);
			}
		}
	}

	public void DisableIKAiming()
	{
		m_TargetValue = 0f;
		m_AimIK.solver.IKPositionWeight = 0f;
		IKComponentsManager.Instance.UnregisterComponent(this);
		PFLog.Animations.Log("[AimIK] Disable");
	}

	private bool TryGetAimTransform(AttackingLimb attackingLimb, out Transform aimTransform)
	{
		aimTransform = null;
		Transform boneTransform = GetBoneTransform(attackingLimb switch
		{
			AttackingLimb.MainHand => "R_Clavicle", 
			AttackingLimb.OffHand => "L_Clavicle", 
			_ => "BP_Mechadendrites_Ballistic", 
		});
		if (boneTransform == null)
		{
			return false;
		}
		FxLocator unityObject = boneTransform.gameObject.GetComponentsInChildren<FxLocator>().FindOrDefault((FxLocator f) => f.Group == FxRoot.Instance.LocatorGroupMuzzle);
		aimTransform = unityObject.Or(null)?.transform;
		return aimTransform != null;
	}

	private Transform GetBoneTransform(string boneName, GameObject root = null)
	{
		if ((object)root == null)
		{
			root = base.gameObject;
		}
		return root.GetComponentsInChildren<Transform>().FindOrDefault((Transform t) => t.name == boneName);
	}
}
