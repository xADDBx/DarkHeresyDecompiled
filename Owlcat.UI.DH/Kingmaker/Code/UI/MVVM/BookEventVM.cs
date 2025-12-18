using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.VO;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.GameModes;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Sound;
using Kingmaker.UI.Sound.Base;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BookEventVM : ViewModel, IBookPageHandler, ISubscriber, IBookEventUIHandler, IGameModeHandler, INetPingDialogAnswer
{
	private readonly ReactiveProperty<bool> m_ChooseCharacterActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<Sprite> m_EventPicture = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<BlueprintBookPage> m_BlueprintBookPage = new ReactiveProperty<BlueprintBookPage>();

	private readonly ReactiveProperty<List<AnswerVM>> m_Answers = new ReactiveProperty<List<AnswerVM>>();

	private readonly ReactiveProperty<Sprite> m_Mirror = new ReactiveProperty<Sprite>(null);

	private readonly ReactiveProperty<bool> m_IsVisible = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveProperty<string> m_ChoosedAnswer = new ReactiveProperty<string>();

	private readonly ReactiveCommand<Unit> m_CheckVotesActive = new ReactiveCommand<Unit>();

	private BookEventChooseCharacterVM m_BookEventChooseCharacter;

	[CanBeNull]
	private VoiceOverStatus m_PlayingVoiceOver;

	[NotNull]
	private readonly Queue<BlueprintCue> m_VoiceOverQueue = new Queue<BlueprintCue>();

	private bool m_IsFirstCueInAllBookEvent;

	public readonly ObservableList<CueVM> Cues = new ObservableList<CueVM>();

	public readonly ObservableList<CueVM> HistoryCues = new ObservableList<CueVM>();

	private Sprite DefaultSprite => ConfigRoot.Instance.Dialog.DefaultBookEventPicture;

	public ReadOnlyReactiveProperty<Sprite> EventPicture => m_EventPicture;

	public ReadOnlyReactiveProperty<List<AnswerVM>> Answers => m_Answers;

	public ReadOnlyReactiveProperty<bool> IsVisible => m_IsVisible;

	public ReadOnlyReactiveProperty<string> ChoosedAnswer => m_ChoosedAnswer;

	public BookEventVM()
	{
		m_IsFirstCueInAllBookEvent = true;
		EventBus.Subscribe(this).AddTo(this);
		new DialogNotificationsVM().AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			OnUpdate();
		}).AddTo(this);
		AnswerVM.ChoosedAnswer.Skip(1).Subscribe(delegate
		{
			m_ChoosedAnswer.Value = AnswerVM.ChoosedAnswer.Value;
		}).AddTo(this);
	}

	protected override void OnDispose()
	{
		ClearAnswers();
		ClearCues();
		ClearHistoryCues();
	}

	private void OnUpdate()
	{
		VoiceOverStatus playingVoiceOver = m_PlayingVoiceOver;
		if (playingVoiceOver == null || playingVoiceOver.IsEnded)
		{
			m_PlayingVoiceOver = null;
			while (m_PlayingVoiceOver == null && m_VoiceOverQueue.Count > 0)
			{
				BlueprintCue blueprintCue = m_VoiceOverQueue.Dequeue();
				string voGuidByBlueprintName = VOSettings.Instance.GetVoGuidByBlueprintName(blueprintCue.Speaker.Blueprint?.name);
				m_PlayingVoiceOver = Game.Instance.Controllers.VoiceOverController.PlayVoiceOver(blueprintCue.Text, voGuidByBlueprintName, VoiceOverType.Dialog);
			}
		}
	}

	protected virtual void SetPage(BlueprintBookPage page, List<CueShowData> cues, List<BlueprintAnswer> answers)
	{
		m_BlueprintBookPage.Value = page;
		SetCues(cues);
		SetVoiceCues(cues);
		SetAnswers(answers);
		SetPicture(page);
		SetMirror(page);
	}

	private void ClearCues()
	{
		Cues.ForEach(delegate(CueVM d)
		{
			d.Dispose();
		});
		Cues.Clear();
		m_PlayingVoiceOver?.Stop();
		m_PlayingVoiceOver = null;
		m_VoiceOverQueue.Clear();
	}

	private void ClearHistoryCues()
	{
		HistoryCues.ForEach(delegate(CueVM d)
		{
			d.Dispose();
		});
		HistoryCues.Clear();
	}

	private void SetVoiceCues(List<CueShowData> cues)
	{
		foreach (CueShowData cue in cues)
		{
			m_VoiceOverQueue.Enqueue(cue.BlueprintCue);
		}
	}

	protected virtual void SetCues(List<CueShowData> cues)
	{
		ClearCues();
		foreach (CueShowData cue in cues)
		{
			Cues.Add(new CueVM(cue.BlueprintCue, cue.SkillChecks, cue.ConvictionShifts, m_IsFirstCueInAllBookEvent));
			HistoryCues.Add(new CueVM(cue.BlueprintCue, cue.SkillChecks, cue.ConvictionShifts, m_IsFirstCueInAllBookEvent));
			m_IsFirstCueInAllBookEvent = false;
		}
	}

	private void ClearAnswers()
	{
		Answers.CurrentValue?.ForEach(delegate(AnswerVM d)
		{
			d.Dispose();
		});
		m_Answers.Value = null;
	}

	private void SetAnswers(List<BlueprintAnswer> answers)
	{
		ClearAnswers();
		int index = 1;
		m_Answers.Value = answers.Select((BlueprintAnswer answer) => new AnswerVM(answer, Game.Instance.Controllers.DialogController, index++)).ToList();
	}

	private void SetPicture(BlueprintBookPage page)
	{
		if (TryGetSprite(page.ImageLink, out var sprite))
		{
			m_EventPicture.Value = sprite;
		}
		else
		{
			m_EventPicture.Value = DefaultSprite;
		}
	}

	private bool TryGetSprite(SpriteLink link, out Sprite sprite)
	{
		if (link != null && link.Exists())
		{
			sprite = link.Load();
			return sprite;
		}
		sprite = null;
		return false;
	}

	private void SetMirror(BlueprintBookPage page)
	{
		m_Mirror.Value = Game.Instance.Player.MainCharacterEntity.Portrait.FullLengthPortrait;
	}

	public void HandleOnBookPageShow(BlueprintBookPage page, List<CueShowData> cues, List<BlueprintAnswer> answers)
	{
		SetPage(page, cues, answers);
	}

	public void HandleChooseCharacter(BlueprintAnswer answer)
	{
		m_BookEventChooseCharacter = new BookEventChooseCharacterVM(answer).AddTo(this);
		m_ChooseCharacterActive.Value = true;
	}

	public void HandleChooseCharacterEnd()
	{
		m_BookEventChooseCharacter?.Dispose();
		m_BookEventChooseCharacter = null;
		m_ChooseCharacterActive.Value = false;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			m_IsVisible.Value = false;
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			m_IsVisible.Value = true;
		}
	}

	public void HandleDialogAnswerHover(string answer, bool hover)
	{
	}

	public void HandleDialogAnswerVote(NetPlayer player, string answer)
	{
		m_CheckVotesActive.Execute();
	}
}
