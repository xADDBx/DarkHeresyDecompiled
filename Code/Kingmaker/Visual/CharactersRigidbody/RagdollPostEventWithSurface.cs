using Kingmaker.View;
using Kingmaker.Visual.Animation;
using UnityEngine;

namespace Kingmaker.Visual.CharactersRigidbody;

public class RagdollPostEventWithSurface : MonoBehaviour
{
	[SerializeField]
	private UnitEntityView m_View;

	[SerializeField]
	private RigidbodyCreatureController m_Controller;

	public string SoundString = "BodyfallsRagDoll_Play";

	public void GetInfoAboutCharacter()
	{
		m_View = GetComponentInParent<UnitEntityView>();
		if (!(m_View == null))
		{
			m_Controller = m_View.GetComponentInChildren<RigidbodyCreatureController>();
			_ = m_Controller == null;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.layer != 29 && !(m_View == null) && !(m_Controller == null) && m_Controller.PostEventWithSurface)
		{
			m_View.PlayBodyFall(SoundString);
			m_Controller.PostEventWithSurface = false;
		}
	}
}
