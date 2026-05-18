using System.Collections.Generic;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Alignments;
using R3;
using UnityEngine;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM;

public class EpilogVM : BookEventVM
{
	private readonly ReactiveProperty<VideoClip> m_BackgroundClip = new ReactiveProperty<VideoClip>();

	private readonly ReactiveProperty<Sprite> m_BackgroundSprite = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<Sprite> m_Portrait = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<string> m_Title = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_SoundStart = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_SoundStop = new ReactiveProperty<string>();

	public ReadOnlyReactiveProperty<VideoClip> BackgroundClip => m_BackgroundClip;

	public ReadOnlyReactiveProperty<Sprite> BackgroundSprite => m_BackgroundSprite;

	public ReadOnlyReactiveProperty<Sprite> Portrait => m_Portrait;

	public ReadOnlyReactiveProperty<string> Title => m_Title;

	public ReadOnlyReactiveProperty<string> SoundStart => m_SoundStart;

	public ReadOnlyReactiveProperty<string> SoundStop => m_SoundStop;

	public EpilogVM()
	{
		m_Portrait.Value = null;
		m_Title.Value = UIStrings.Instance.Epilogues.EpiloguesPortraitTitle;
	}

	protected override void SetPage(BlueprintBookPage page, List<CueShowData> cues, List<BlueprintAnswer> answers)
	{
		base.SetPage(page, cues, answers);
		SetPortrait(page);
		SetBackground(page);
	}

	private void SetTitle(BlueprintBookPage page)
	{
		m_Title.Value = (page.ShowMainCharacterName ? Game.Instance.Player.MainCharacter.Entity.CharacterName : (page.Companion?.Get()?.CharacterName ?? string.Empty));
	}

	protected override void SetCues(List<CueShowData> cues)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (CueShowData cue in cues)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(cue.BlueprintCue.DisplayText);
		}
		Cues.Add(new CueVM(stringBuilder.ToString(), new List<SkillCheckResult>(), new List<AlignmentShift>()));
	}

	private void SetPortrait(BlueprintBookPage page)
	{
		if (page.ShowMainCharacter)
		{
			m_Portrait.Value = Game.Instance.Player.MainCharacterEntity.UISettings.Portrait.FullLengthPortrait;
			return;
		}
		BlueprintPortraitReference portrait = page.Portrait;
		if (portrait?.Get() != null)
		{
			m_Portrait.Value = (portrait.Get().FullLengthPortrait ? portrait.Get().FullLengthPortrait : portrait.Get().HalfLengthPortrait);
		}
		else
		{
			m_Portrait.Value = Game.Instance.Player.MainCharacterEntity.UISettings.Portrait.FullLengthPortrait;
		}
	}

	private void SetBackground(BlueprintBookPage page)
	{
		if (page.UseSound)
		{
			if (page.SoundStartEvent == null || !page.SoundStartEvent.IsValid() || page.SoundStopEvent == null || !page.SoundStopEvent.IsValid())
			{
				Debug.LogError($"{page} has Null or Invalid sound events");
			}
			m_SoundStart.Value = ((page.SoundStartEvent != null && page.SoundStartEvent.IsValid()) ? page.SoundStartEvent.Name : null);
			m_SoundStop.Value = ((page.SoundStopEvent != null && page.SoundStopEvent.IsValid()) ? page.SoundStopEvent.Name : null);
		}
		else
		{
			m_SoundStart.Value = null;
			m_SoundStop.Value = null;
		}
		if (page.UseBackgroundVideo)
		{
			VideoLink backgroundVideoLink = page.BackgroundVideoLink;
			m_BackgroundSprite.Value = null;
			m_BackgroundClip.Value = ((backgroundVideoLink != null && backgroundVideoLink.Exists()) ? backgroundVideoLink.Load() : null);
		}
		else
		{
			SpriteLink backgroundImageLink = page.BackgroundImageLink;
			m_BackgroundClip.Value = null;
			m_BackgroundSprite.Value = ((backgroundImageLink != null && backgroundImageLink.Exists()) ? backgroundImageLink.Load() : null);
		}
	}
}
