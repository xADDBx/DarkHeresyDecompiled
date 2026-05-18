using Core.Cheats;

namespace Kingmaker.Cheats;

public static class CheatsSoundRagdoll
{
	[Cheat(Name = "sound_ragdoll", Description = "0 = off, 1 = receiver logs (play/block reasons), 2 = verbose (+ every bone collision from sender)")]
	public static int SoundRagdollDebug { get; set; }
}
