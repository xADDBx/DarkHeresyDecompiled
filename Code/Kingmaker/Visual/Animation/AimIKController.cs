using Kingmaker.Code.View.Visual.SceneHelpers;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using RootMotion.FinalIK;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

[RequireComponent(typeof(AimIK))]
public class AimIKController : MonoBehaviour, IIKComponent
{
	private AimIK m_AimIK;

	private float m_DurationSeconds;

	private float m_TargetValue;

	private float m_VelocityRef;

	private void Start()
	{
		m_AimIK = base.gameObject.GetComponent<AimIK>();
		m_AimIK.solver.IKPositionWeight = 0f;
		m_TargetValue = 0f;
		SetUpBones();
	}

	public void StartAim(Transform target, bool isMainHand, float turningDurationSeconds)
	{
		if (!(m_AimIK == null) && TryGetAimTransform(isMainHand, out var aimTransform))
		{
			m_AimIK.solver.target = target;
			m_AimIK.solver.transform = aimTransform;
			m_DurationSeconds = turningDurationSeconds;
			m_TargetValue = 1f;
			IKComponentsManager.Instance.RegisterComponent(this);
			PFLog.Animations.Log($"[AimIK] Start in {m_DurationSeconds} seconds");
		}
	}

	public void StopAim(float fadingDurationSeconds)
	{
		if (!(m_AimIK == null) && !(m_AimIK.solver.transform == null))
		{
			m_DurationSeconds = fadingDurationSeconds;
			m_TargetValue = 0f;
			PFLog.Animations.Log($"[AimIK] Stop in {m_DurationSeconds} seconds");
		}
	}

	void IIKComponent.DoLateUpdate()
	{
		if (!Mathf.Approximately(m_AimIK.solver.IKPositionWeight, m_TargetValue))
		{
			m_AimIK.solver.IKPositionWeight = Mathf.SmoothDamp(m_AimIK.solver.IKPositionWeight, m_TargetValue, ref m_VelocityRef, m_DurationSeconds * 0.5f);
			if (m_AimIK.solver.IKPositionWeight >= 0.999f)
			{
				m_AimIK.solver.IKPositionWeight = 1f;
			}
			if (m_AimIK.solver.IKPositionWeight <= 0.001f)
			{
				m_AimIK.solver.IKPositionWeight = 0f;
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

	private void SetUpBones()
	{
		m_AimIK.solver.bones = new IKSolver.Bone[3];
		m_AimIK.solver.bones[0] = new IKSolver.Bone(GetBoneTransform("Spine_1"));
		m_AimIK.solver.bones[1] = new IKSolver.Bone(GetBoneTransform("Spine_2"));
		m_AimIK.solver.bones[2] = new IKSolver.Bone(GetBoneTransform("Spine_3"));
	}

	private bool TryGetAimTransform(bool isMainHandAttack, out Transform aimTransform)
	{
		aimTransform = null;
		string boneName = (isMainHandAttack ? "R_Clavicle" : "L_Clavicle");
		Transform boneTransform = GetBoneTransform(boneName);
		if (boneTransform == null)
		{
			return false;
		}
		WeaponPrefab unityObject = boneTransform.gameObject.GetComponentsInChildren<WeaponPrefab>().FirstOrDefault();
		aimTransform = unityObject.Or(null)?.transform;
		return aimTransform != null;
	}

	private Transform GetBoneTransform(string boneName)
	{
		return base.gameObject.GetComponentsInChildren<Transform>().FindOrDefault((Transform t) => t.name == boneName);
	}
}
