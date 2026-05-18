using Kingmaker.Cheats;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker;

public class RagdollSender : MonoBehaviour
{
	public RagdollRecieverMain Receiver;

	private void OnCollisionEnter(Collision collision)
	{
		if ((bool)Receiver)
		{
			SurfaceType? surfaceType = collision.gameObject.GetComponent<SoundSurfaceMapObject>()?.Switch;
			if (CheatsSoundRagdoll.SoundRagdollDebug >= 2)
			{
				PFLog.SoundRagdoll.Log($"[Sender] bone:{base.gameObject.name} impulse:{collision.relativeVelocity.magnitude:F1} " + $"surface:{surfaceType} collider:{collision.collider.name}");
			}
			Receiver.Send(base.gameObject.name, collision.relativeVelocity.magnitude, surfaceType);
		}
	}
}
