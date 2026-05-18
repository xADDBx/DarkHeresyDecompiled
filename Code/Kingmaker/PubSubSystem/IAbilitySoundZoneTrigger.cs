using Kingmaker.Framework;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface IAbilitySoundZoneTrigger : ISubscriber
{
	void TriggerSoundZone(IEvalContext context, GameObject gameObject);
}
