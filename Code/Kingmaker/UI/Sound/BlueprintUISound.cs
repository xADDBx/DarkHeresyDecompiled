using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UI.Sound;

[TypeId("ab051f837cfe69b4d989791539b6838c")]
public class BlueprintUISound : BlueprintScriptableObject
{
	[field: SerializeField]
	public UISound DoNothingEvent { get; private set; }

	[field: Header("Windows")]
	[field: SerializeField]
	public ServiceWindowsSounds ServiceWindowsSounds { get; private set; }

	[field: SerializeField]
	public ModalWindowsSounds ModalWindowsSounds { get; private set; }

	[field: SerializeField]
	public FullScreenSounds FullScreenSounds { get; private set; }

	[field: SerializeField]
	public FullScreenUniqueSounds FullScreenUniqueSounds { get; private set; }

	[field: Header("Notifications")]
	[field: SerializeField]
	public NotificationsSounds NotificationsSounds { get; private set; }

	[field: Header("Combat")]
	[field: SerializeField]
	public CombatSounds CombatSounds { get; private set; }

	[field: Header("System")]
	[field: SerializeField]
	public SystemSounds SystemSounds { get; private set; }

	[field: SerializeField]
	public ButtonsSounds ButtonsSounds { get; private set; }

	[field: Header("Coop")]
	[field: SerializeField]
	public CoopSounds CoopSounds { get; private set; }
}
