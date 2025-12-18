using Owlcat.Runtime.Visual.XPBD.Bodies;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Authoring;

[RequireComponent(typeof(AuthoringBase))]
public class ParticleAttachment : XPBDEntity
{
	public enum AttachmentType
	{
		Static,
		Dynamic
	}

	private AuthoringBase m_Body;

	[SerializeField]
	private Transform m_Target;

	[SerializeField]
	private ParticleGroup m_ParticleGroup;

	[SerializeField]
	[HideInInspector]
	private AttachmentType m_AttachmentType;

	public AuthoringBase Body => m_Body;

	public ParticleGroup ParticleGroup
	{
		get
		{
			return m_ParticleGroup;
		}
		set
		{
			m_ParticleGroup = value;
		}
	}

	public Transform Target => m_Target;

	public override Transform GetTransform()
	{
		if (m_Target == null && base.IsValid)
		{
			return base.transform;
		}
		return m_Target;
	}

	private void OnEnable()
	{
		m_Body = GetComponent<AuthoringBase>();
		EnableAttachment();
	}

	private void OnDisable()
	{
		DisableAttachment();
	}

	private void EnableAttachment()
	{
		if (m_Target != null && m_Target.gameObject.activeInHierarchy)
		{
			XPBD.RegisterParticleAttachment(this);
		}
	}

	private void DisableAttachment()
	{
		XPBD.UnregisterParticleAttachment(this);
	}
}
