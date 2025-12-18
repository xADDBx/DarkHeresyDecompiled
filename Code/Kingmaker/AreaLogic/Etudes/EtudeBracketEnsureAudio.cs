using System;
using Kingmaker.Sound;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("6ffcc131f2fad1a4bb989f06a54e8eb8")]
public class EtudeBracketEnsureAudio : EtudeBracketTrigger
{
	public AudioFilePackagesSettings.AudioChunk Chunk;

	protected override void OnEnter()
	{
		AudioFilePackagesSettings.Instance.LoadPackagesChunk(Chunk);
		AudioFilePackagesSettings.Instance.LoadBanksChunk(Chunk);
	}

	protected override void OnExit()
	{
		AudioFilePackagesSettings.Instance.UnloadBanksChunk(Chunk);
		AudioFilePackagesSettings.Instance.UnloadPackagesChunk(Chunk);
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
