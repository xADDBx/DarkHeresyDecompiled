using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DialogVM : ViewModel, IDialogCueHandler, ISubscriber, IDialogHistoryHandler, IGameModeHandler, INetPingDialogAnswer, IFullScreenUIHandler
{
	public readonly ObservableList<DialogHistoryEntityVM> History = new ObservableList<DialogHistoryEntityVM>();

	private readonly ReactiveProperty<Sprite> m_SpeakerPortrait = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<Sprite> m_AnswerPortrait = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<string> m_SpeakerName = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_AnswerName = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_EmptySpeaker = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsSpeakerNeedGlow = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsAnswererNeedGlow = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsSpeakerNeedEqualizer = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsAnswererNeedEqualizer = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<List<AnswerVM>> m_Answers = new ReactiveProperty<List<AnswerVM>>(null);

	private readonly ReactiveProperty<AnswerVM> m_SystemAnswer = new ReactiveProperty<AnswerVM>(null);

	private readonly ReactiveProperty<CueVM> m_Cue = new ReactiveProperty<CueVM>(null);

	private readonly ReactiveProperty<bool> m_IsVisible = new ReactiveProperty<bool>(value: true);

	private readonly ReactiveCommand<Unit> m_OnCueUpdate = new ReactiveCommand<Unit>();

	private readonly ReactiveCommand<Unit> m_CheckVotesActive = new ReactiveCommand<Unit>();

	private readonly ReactiveCommand<Unit> m_OnDetachView = new ReactiveCommand<Unit>();

	public readonly DialogNotificationsVM DialogNotifications;

	public readonly DialogVotesBlockVM DialogVotesBlockVM;

	private readonly ReactiveProperty<PortraitVM> m_SpeakerFullPortraitVM = new ReactiveProperty<PortraitVM>();

	private readonly ReactiveProperty<PortraitVM> m_AnswererFullPortraitVM = new ReactiveProperty<PortraitVM>();

	private readonly ReactiveProperty<bool> m_SpeakerHasPortrait = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_AnswererHasPortrait = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_VotesIsActive = new ReactiveProperty<bool>();

	private PortraitData m_SpeakerPortraitData;

	private PortraitData m_AnswererPortraitData;

	private CueShowData m_CurrentCueShowData;

	public ReadOnlyReactiveProperty<Sprite> SpeakerPortrait => m_SpeakerPortrait;

	public ReadOnlyReactiveProperty<Sprite> AnswerPortrait => m_AnswerPortrait;

	public ReadOnlyReactiveProperty<string> SpeakerName => m_SpeakerName;

	public ReadOnlyReactiveProperty<string> AnswerName => m_AnswerName;

	public ReadOnlyReactiveProperty<bool> EmptySpeaker => m_EmptySpeaker;

	public ReadOnlyReactiveProperty<bool> IsSpeakerNeedGlow => m_IsSpeakerNeedGlow;

	public ReadOnlyReactiveProperty<bool> IsAnswererNeedGlow => m_IsAnswererNeedGlow;

	public ReadOnlyReactiveProperty<bool> IsSpeakerNeedEqualizer => m_IsSpeakerNeedEqualizer;

	public ReadOnlyReactiveProperty<bool> IsAnswererNeedEqualizer => m_IsAnswererNeedEqualizer;

	public ReadOnlyReactiveProperty<List<AnswerVM>> Answers => m_Answers;

	public ReadOnlyReactiveProperty<AnswerVM> SystemAnswer => m_SystemAnswer;

	public ReadOnlyReactiveProperty<CueVM> Cue => m_Cue;

	public ReadOnlyReactiveProperty<bool> IsVisible => m_IsVisible;

	public Observable<Unit> OnCueUpdate => m_OnCueUpdate;

	public Observable<Unit> CheckVotesActive => m_CheckVotesActive;

	public Observable<Unit> OnDetachView => m_OnDetachView;

	public ReadOnlyReactiveProperty<PortraitVM> SpeakerFullPortraitVM => m_SpeakerFullPortraitVM;

	public ReadOnlyReactiveProperty<PortraitVM> AnswererFullPortraitVM => m_AnswererFullPortraitVM;

	public ReadOnlyReactiveProperty<bool> SpeakerHasPortrait => m_SpeakerHasPortrait;

	public ReadOnlyReactiveProperty<bool> AnswererHasPortrait => m_AnswererHasPortrait;

	public ReadOnlyReactiveProperty<bool> VotesIsActive => m_VotesIsActive;

	private DialogController DialogController => Game.Instance.Controllers.DialogController;

	private BaseUnitEntity Speaker => DialogController.CurrentSpeaker;

	private BlueprintUnit Listener => DialogController.CurrentCueShowData.BlueprintCue.Listener;

	private BlueprintUnit SpeakerBlueprint => DialogController.CurrentCueShowData.BlueprintCue.Speaker.SpeakerPortrait;

	private MapObjectView MapObjectSpeaker => DialogController.MapObject?.View.AsMapObjectView();

	private BaseUnitEntity MainCharacter => Game.Instance.Player.MainCharacterEntity;

	public DialogVM()
	{
		DialogNotifications = new DialogNotificationsVM(DialogUIType.Dialog).AddTo(this);
		DialogVotesBlockVM = new DialogVotesBlockVM().AddTo(this);
		ObservableSubscribeExtensions.Subscribe(OnCueUpdate, delegate
		{
			UpdatePortrait();
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(CheckVotesActive, delegate
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				m_VotesIsActive.Value = Answers.CurrentValue.Any((AnswerVM a) => a.AnswerVotes.Any());
			}, 1);
		}).AddTo(this);
		UpdateCue();
		FillHistory();
		Metrics.Interface.Type(InterfaceMetricsEvent.InterfaceTypes.Dialogue).State(InterfaceMetricsEvent.InterfaceStates.Open).Send();
	}

	protected override void OnDispose()
	{
		m_SpeakerPortraitData = null;
		m_AnswererPortraitData = null;
		m_SpeakerHasPortrait.Value = false;
		m_AnswererHasPortrait.Value = false;
		ClearPortrait();
		DisposeCue();
		DisposeAnswers();
		Metrics.Interface.Type(InterfaceMetricsEvent.InterfaceTypes.Dialogue).State(InterfaceMetricsEvent.InterfaceStates.Close).Send();
	}

	private void FillHistory()
	{
		History.AddRange(DialogController.History.Select((IDialogShowData h) => new DialogHistoryEntityVM(h)));
	}

	private void DisposeCue()
	{
		Cue.CurrentValue?.Dispose();
		m_Cue.Value = null;
	}

	private void DisposeAnswers()
	{
		Answers.CurrentValue?.ForEach(delegate(AnswerVM v)
		{
			v.Dispose();
		});
		m_Answers.Value = null;
		SystemAnswer.CurrentValue?.Dispose();
		m_SystemAnswer.Value = null;
	}

	public void HandleOnCueShow(CueShowData data)
	{
		UpdateCue();
	}

	private void UpdateCue()
	{
		if (m_CurrentCueShowData == DialogController.CurrentCueShowData)
		{
			return;
		}
		DisposeCue();
		DisposeAnswers();
		ResetPortraitsGlow();
		CueShowData currentCueShowData = DialogController.CurrentCueShowData;
		BlueprintCue blueprintCue = DialogController.CurrentCueShowData.BlueprintCue;
		if (!blueprintCue)
		{
			return;
		}
		SetupSpeakerPortrait();
		m_SpeakerName.Value = ((SpeakerBlueprint != null) ? SpeakerBlueprint.CharacterName : Speaker?.CharacterName);
		m_EmptySpeaker.Value = SpeakerPortrait.CurrentValue == null && SpeakerName.CurrentValue == null;
		m_IsSpeakerNeedGlow.Value = blueprintCue.Speaker != null && blueprintCue.Speaker.TryGetSpeakerEntity(null, out var speaker) && Speaker == speaker && SpeakerPortrait.CurrentValue != null;
		m_IsAnswererNeedGlow.Value = Speaker?.CharacterName == blueprintCue.Listener?.CharacterName && AnswerPortrait.CurrentValue != null;
		BaseUnitEntity speaker2 = null;
		bool flag = (blueprintCue.Speaker != null && !blueprintCue.Speaker.TryGetSpeakerEntity(null, out speaker2)) || Speaker == speaker2;
		m_IsSpeakerNeedEqualizer.Value = flag && SpeakerPortrait.CurrentValue == null;
		bool flag2 = Speaker?.CharacterName == blueprintCue.Listener?.CharacterName;
		m_IsAnswererNeedEqualizer.Value = flag2 && AnswerPortrait.CurrentValue == null;
		SetupAnswererPortrait(blueprintCue);
		m_AnswerName.Value = ((blueprintCue.Listener != null && blueprintCue.Listener.name != "Player Character") ? blueprintCue.Listener.CharacterName : MainCharacter.CharacterName);
		BlueprintAnswer blueprintAnswer = DialogController.Answers.FirstOrDefault();
		if (blueprintAnswer != null && blueprintAnswer.IsSystem())
		{
			m_Answers.Value = null;
			m_SystemAnswer.Value = new AnswerVM(blueprintAnswer, Game.Instance.Controllers.DialogController, 1);
		}
		else
		{
			int index = 1;
			m_Answers.Value = (from a in DialogController.Answers
				where !a.IsSystem()
				select a into answer
				select new AnswerVM(answer, Game.Instance.Controllers.DialogController, index++)).ToList();
			m_SystemAnswer.Value = null;
		}
		m_Cue.Value = new CueVM(blueprintCue, currentCueShowData.SkillChecks, currentCueShowData.ConvictionShifts);
		if (!blueprintCue.Speaker.NoSpeaker)
		{
			GameObject target = ((Speaker != null && Speaker.View != null) ? Speaker.View.gameObject : null);
			string text = blueprintCue.Speaker.GetVoGuidRuntime();
			if (string.IsNullOrEmpty(text))
			{
				text = Speaker?.VoGuid;
			}
			if (string.IsNullOrEmpty(text))
			{
				MapObjectView mapObjectSpeaker = MapObjectSpeaker;
				if ((object)mapObjectSpeaker != null && mapObjectSpeaker.NeedsVoiceOver)
				{
					text = MapObjectSpeaker?.VoId?.Guid;
				}
			}
			Game.Instance.Controllers.VoiceOverController.PlayDialogVoiceOver(blueprintCue.Text, text, target);
		}
		m_OnCueUpdate.Execute(Unit.Default);
	}

	private void SetupAnswererPortrait(BlueprintCue cue)
	{
		Sprite value = MainCharacter.Portrait.HalfLengthPortrait;
		if (cue.Listener != null && cue.Listener.name != "Player Character")
		{
			value = (cue.Listener.PortraitSafe.InitiativePortrait ? null : cue.Listener.PortraitSafe.HalfLengthPortrait);
		}
		m_AnswerPortrait.Value = value;
	}

	private void SetupSpeakerPortrait()
	{
		try
		{
			Sprite value = null;
			if (SpeakerBlueprint != null)
			{
				value = (SpeakerBlueprint.PortraitSafe.InitiativePortrait ? null : SpeakerBlueprint.PortraitSafe.HalfLengthPortrait);
			}
			else if (Speaker != null && !DialogController.CurrentCueShowData.BlueprintCue.Speaker.NoSpeaker)
			{
				value = (Speaker.Portrait.InitiativePortrait ? null : Speaker.Portrait.HalfLengthPortrait);
			}
			m_SpeakerPortrait.Value = value;
		}
		catch (Exception value2)
		{
			System.Console.WriteLine(value2);
		}
	}

	public void HandleOnDialogHistory(IDialogShowData showData)
	{
		History.Add(new DialogHistoryEntityVM(showData));
	}

	private void ResetPortraitsGlow()
	{
		m_IsSpeakerNeedGlow.Value = false;
		m_IsAnswererNeedGlow.Value = false;
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		if (fullScreenUIType == FullScreenUIType.EscapeMenu)
		{
			m_IsVisible.Value = !state && Game.Instance.IsModeActive(GameModeType.Dialog);
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene || Game.Instance.IsPaused)
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

	public void HandleShowEscMenu()
	{
		EventBus.RaiseEvent(delegate(IEscMenuHandler h)
		{
			h.HandleOpen();
		});
	}

	public void HandleDialogAnswerHover(string answer, bool hover)
	{
	}

	public void HandleDialogAnswerVote(NetPlayer player, string answer)
	{
		m_CheckVotesActive.Execute(Unit.Default);
	}

	public void ShowHideBigScreenshotSpeaker(bool state)
	{
		if (!state || SpeakerPortrait.CurrentValue == null)
		{
			ClearPortrait();
		}
		else
		{
			m_SpeakerFullPortraitVM.Value = new PortraitVM(m_SpeakerPortraitData).AddTo(this);
		}
	}

	public void ShowHideBigScreenshotAnswerer(bool state)
	{
		if (!state || AnswerPortrait.CurrentValue == null)
		{
			ClearPortrait();
		}
		else
		{
			m_AnswererFullPortraitVM.Value = new PortraitVM(m_AnswererPortraitData).AddTo(this);
		}
	}

	private void UpdatePortrait()
	{
		CheckSpeakerPortrait();
		CheckAnswererPortrait();
		if (SpeakerFullPortraitVM.CurrentValue != null)
		{
			ShowHideBigScreenshotSpeaker(state: true);
		}
		else if (AnswererFullPortraitVM.CurrentValue != null)
		{
			ShowHideBigScreenshotAnswerer(state: true);
		}
	}

	private void CheckSpeakerPortrait()
	{
		m_SpeakerPortraitData = null;
		if (SpeakerBlueprint != null)
		{
			m_SpeakerPortraitData = (SpeakerBlueprint.PortraitSafe.InitiativePortrait ? null : SpeakerBlueprint.PortraitSafe.Data);
		}
		else if (Speaker != null)
		{
			m_SpeakerPortraitData = (Speaker.Portrait.InitiativePortrait ? null : Speaker.Portrait);
		}
		m_SpeakerHasPortrait.Value = m_SpeakerPortraitData != null && m_SpeakerPortraitData.FullLengthPortrait != null;
		if (m_SpeakerPortraitData == null || m_SpeakerPortraitData.FullLengthPortrait == null)
		{
			ClearPortrait();
		}
	}

	private void CheckAnswererPortrait()
	{
		m_AnswererPortraitData = MainCharacter.Portrait;
		if (Listener != null && Listener.name != "Player Character")
		{
			m_AnswererPortraitData = (Listener.PortraitSafe.InitiativePortrait ? null : Listener.PortraitSafe.Data);
		}
		m_AnswererHasPortrait.Value = m_AnswererPortraitData != null && m_AnswererPortraitData.FullLengthPortrait != null;
		if (m_AnswererPortraitData == null || m_AnswererPortraitData.FullLengthPortrait == null)
		{
			ClearPortrait();
		}
	}

	private void ClearPortrait()
	{
		SpeakerFullPortraitVM.CurrentValue?.Dispose();
		m_SpeakerFullPortraitVM.Value = null;
		AnswererFullPortraitVM.CurrentValue?.Dispose();
		m_AnswererFullPortraitVM.Value = null;
	}

	public void DetachView()
	{
		m_OnDetachView.Execute(Unit.Default);
	}
}
