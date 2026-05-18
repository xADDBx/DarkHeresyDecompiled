using System;
using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Tutorial;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class TutorialWindowVM : ViewModel, IGameModeHandler, ISubscriber
{
	public readonly TutorialData Data;

	private readonly Action m_CallbackHide;

	private readonly ReactiveProperty<bool> m_EncyclopediaLinkExist = new ReactiveProperty<bool>();

	public List<TutorialData.Page> Pages => Data?.Pages;

	public TutorialTag? TutorialTag => Data?.Blueprint.Tag;

	public bool BanTutorInsteadOfTag => Data?.Trigger != null;

	public bool CanBeBanned
	{
		get
		{
			if (Data?.Trigger == null)
			{
				return TutorialTag != Kingmaker.Tutorial.TutorialTag.NoTag;
			}
			return true;
		}
	}

	private INode EncyclopediaReference => Data.Blueprint.EncyclopediaReference.Get();

	public float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public ReadOnlyReactiveProperty<bool> EncyclopediaLinkExist => m_EncyclopediaLinkExist;

	protected TutorialWindowVM(TutorialData data, Action callbackHide)
	{
		Data = data;
		m_CallbackHide = callbackHide;
		m_EncyclopediaLinkExist.Value = data.Blueprint.EncyclopediaReference.Get() != null;
		EventBus.Subscribe(this).AddTo(this);
		Metrics.Interface.State(InterfaceMetricsEvent.InterfaceStates.Open).Type(InterfaceMetricsEvent.InterfaceTypes.Tutorial).Id(data.Blueprint.AssetGuid)
			.Send();
	}

	protected override void OnDispose()
	{
		Hide();
		Metrics.Interface.State(InterfaceMetricsEvent.InterfaceStates.Close).Type(InterfaceMetricsEvent.InterfaceTypes.Tutorial).Id(Data.Blueprint.AssetGuid)
			.Send();
	}

	public void BanTutor()
	{
		if (BanTutorInsteadOfTag)
		{
			Game.Instance.TutorialSystem.Ban(Data.Blueprint);
		}
		else
		{
			Game.Instance.TutorialSystem.BanTag(Data.Blueprint.Tag);
		}
	}

	public void Hide()
	{
		m_CallbackHide?.Invoke();
		TutorialSystem tutorialSystem = Game.Instance.TutorialSystem;
		if (tutorialSystem.ShowingData != null)
		{
			EventBus.RaiseEvent(delegate(ITutorialWindowClosedHandler i)
			{
				i.HandleHideTutorial(tutorialSystem.ShowingData);
			});
			tutorialSystem.ShowingData.Blueprint.ActionsOnClose?.Run();
			tutorialSystem.ShowingData = null;
		}
	}

	public void TemporarilyHide()
	{
		m_CallbackHide?.Invoke();
	}

	public void GoToEncyclopedia()
	{
		TemporarilyHide();
		UIUtilityEncyclopedy.ShowEncyclopediaPage(EncyclopediaReference);
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (!(gameMode != GameModeType.GameOver))
		{
			Hide();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}
}
