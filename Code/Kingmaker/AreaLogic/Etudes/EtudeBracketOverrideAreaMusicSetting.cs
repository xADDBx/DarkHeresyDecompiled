using Kingmaker.Sound;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("a2ea6d1bf82c4ae1a1b21f789d09a91a")]
public class EtudeBracketOverrideAreaMusicSetting : EtudeBracketTrigger
{
	public AkStateReference OverrideMusicSetting;

	protected override void OnExit()
	{
		if (SoundState.Instance?.MusicStateHandler != null)
		{
			SoundState.Instance?.MusicStateHandler.DisableOverrideAreaSetting();
		}
	}

	protected override void OnEnter()
	{
		if (SoundState.Instance?.MusicStateHandler != null)
		{
			SoundState.Instance?.MusicStateHandler.OverrideAreaSetting(OverrideMusicSetting);
		}
	}

	protected override void OnDispose()
	{
		if (!EtudeBracketTrigger.Etude.IsCompleted && !EtudeBracketTrigger.Etude.CompletionInProgress)
		{
			OnExit();
		}
	}

	protected override void OnResume()
	{
		OnEnter();
	}
}
