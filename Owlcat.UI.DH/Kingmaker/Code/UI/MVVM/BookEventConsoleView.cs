using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Controllers.Dialog;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BookEventConsoleView : BookEventBaseView
{
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private OwlcatMultiButton m_FirstGlossaryFocus;

	[SerializeField]
	private OwlcatMultiButton m_SecondGlossaryFocus;

	[Header("Navigations")]
	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_NavigationParameters;

	private const string GlossaryInputLayerContextName = "DialogGlossary";

	private const string VotesInputLayerContextName = "DialogVotes";

	private FloatConsoleNavigationBehaviour m_GlossaryNavigationBehavior;

	private readonly ReactiveProperty<bool> m_HasGlossaryPoints = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_GlossaryMode = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_VotesMode = new ReactiveProperty<bool>();

	private InputLayer m_GlossaryInputLayer;

	private InputLayer m_VotesInputLayer;

	private GridConsoleNavigationBehaviour m_VotesNavigationBehavior;

	private IDisposable m_GlossaryDisposable;

	public override void Awake()
	{
		base.Awake();
		m_FirstGlossaryFocus.Or(null)?.gameObject.SetActive(value: false);
		m_SecondGlossaryFocus.Or(null)?.gameObject.SetActive(value: false);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		CloseGlossary(default(InputActionEventData));
		m_GlossaryDisposable?.Dispose();
		m_GlossaryDisposable = null;
	}

	protected override void CreateInputImpl(InputLayer inputLayer, GridConsoleNavigationBehaviour behaviour)
	{
		base.CreateInputImpl(inputLayer, behaviour);
		m_GlossaryNavigationBehavior = new FloatConsoleNavigationBehaviour(m_NavigationParameters).AddTo(this);
		m_GlossaryInputLayer = m_GlossaryNavigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = "DialogGlossary"
		});
		m_GlossaryNavigationBehavior.DeepestFocusAsObservable.Subscribe(OnGlossaryFocusChanged).AddTo(this);
		behaviour.Focus.Subscribe(ScrollMenu).AddTo(this);
		m_ConsoleHintsWidget.BindHint(Layer.AddButton(ShowGlossary, 11, m_HasGlossaryPoints.And(m_VotesMode.Not()).ToReadOnlyReactiveProperty(initialValue: false)), UIStrings.Instance.Dialog.OpenGlossary).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_GlossaryInputLayer.AddButton(CloseGlossary, 9, m_GlossaryMode.And(m_VotesMode.Not()).ToReadOnlyReactiveProperty(initialValue: false)), UIStrings.Instance.Dialog.CloseGlossary).AddTo(this);
		m_ConsoleHintsWidget.BindHint(Layer.AddButton(delegate
		{
			ShowVotes();
		}, 19, m_VotesMode.Not().And(VotesIsActive).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustReleased), UIStrings.Instance.Dialog.ShowVotes).AddTo(this);
		Layer.AddButton(OnShowEscMenu, 16, InputActionEventType.ButtonJustReleased).AddTo(this);
		AddVotesInput();
	}

	private void AddVotesInput()
	{
		m_VotesNavigationBehavior = new GridConsoleNavigationBehaviour().AddTo(this);
		m_VotesInputLayer = m_VotesNavigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = "DialogVotes"
		});
		m_ConsoleHintsWidget.BindHint(m_VotesInputLayer.AddButton(delegate
		{
			CloseVotes();
		}, 19, m_VotesMode, InputActionEventType.ButtonJustReleased), UIStrings.Instance.Dialog.HideVotes).AddTo(this);
		m_VotesInputLayer.AddButton(delegate
		{
			CloseVotes();
		}, 9, m_VotesMode).AddTo(this);
	}

	private void ShowVotes()
	{
		m_VotesMode.Value = true;
		SetVotesNavigation();
		GamePad.Instance.PushLayer(m_VotesInputLayer);
		m_VotesNavigationBehavior.FocusOnFirstValidEntity();
	}

	private void CloseVotes()
	{
		m_VotesNavigationBehavior.UnFocusCurrentEntity();
		m_VotesMode.Value = false;
		TooltipHelper.HideTooltip();
		GamePad.Instance.PopLayer(m_VotesInputLayer);
	}

	private void SetVotesNavigation()
	{
		m_VotesNavigationBehavior.Clear();
		List<OwlcatSelectable> list = (from block in new List<IConsoleNavigationEntity>().OfType<OwlcatSelectable>()
			where block.IsValid()
			select block).ToList();
		if (list.Any())
		{
			m_VotesNavigationBehavior.SetEntitiesVertical(list);
		}
		if (m_VotesMode.Value)
		{
			TooltipHelper.HideTooltip();
		}
	}

	private void ShowGlossary(InputActionEventData data)
	{
		IFloatConsoleNavigationEntity floatConsoleNavigationEntity = CalculateGlossary();
		m_GlossaryMode.Value = true;
		m_GlossaryDisposable = GamePad.Instance.PushLayer(m_GlossaryInputLayer);
		if (floatConsoleNavigationEntity != null)
		{
			m_GlossaryNavigationBehavior.FocusOnEntityManual(floatConsoleNavigationEntity);
		}
		else
		{
			m_GlossaryNavigationBehavior.FocusOnFirstValidEntity();
		}
	}

	private void CloseGlossary(InputActionEventData data)
	{
		m_GlossaryNavigationBehavior.UnFocusCurrentEntity();
		m_GlossaryDisposable?.Dispose();
		m_GlossaryDisposable = null;
		TooltipHelper.HideTooltip();
		m_GlossaryMode.Value = false;
	}

	protected override void UpdateFocusLinks()
	{
		base.UpdateFocusLinks();
		CalculateGlossary();
	}

	private IFloatConsoleNavigationEntity CalculateGlossary()
	{
		m_GlossaryNavigationBehavior.Clear();
		List<IFloatConsoleNavigationEntity> list = new List<IFloatConsoleNavigationEntity>();
		if (!base.IsShowHistory.CurrentValue)
		{
			foreach (BookEventCueView currentCuesView in CurrentCuesViews)
			{
				list.AddRange(TMPLinkNavigationGenerator.GenerateEntityList(currentCuesView.Text, m_FirstGlossaryFocus, m_SecondGlossaryFocus, null, OnFocusCueLink, currentCuesView.SkillChecks, TooltipHelper.GetLinkTooltipTemplate));
			}
		}
		if (base.IsShowHistory.CurrentValue)
		{
			for (int num = MemorizedCuesViews.Count - 1; num >= 0; num--)
			{
				list.AddRange(TMPLinkNavigationGenerator.GenerateEntityList(MemorizedCuesViews[num].Text, m_FirstGlossaryFocus, m_SecondGlossaryFocus, null, OnFocusHistoryLink, TooltipHelper.GetLinkTooltipTemplate));
			}
		}
		if (!AnswersEntities.Empty())
		{
			_ = base.IsShowHistory.CurrentValue;
		}
		if (list.Count > 0)
		{
			m_GlossaryNavigationBehavior.AddEntities(list);
		}
		m_HasGlossaryPoints.Value = list.Count > 0;
		return list.FirstItem();
	}

	private void OnFocusHistoryLink(string key)
	{
		if (!(GamePad.Instance.CurrentInputLayer.ContextName != "DialogGlossary"))
		{
			ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
			{
				m_FirstGlossaryFocus.ShowLinkTooltip(key);
			}).AddTo(this);
		}
	}

	private void OnFocusCueLink(string key, List<SkillCheckResult> skillCheckResults)
	{
		if (!(GamePad.Instance.CurrentInputLayer.ContextName != "DialogGlossary"))
		{
			ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
			{
				m_FirstGlossaryFocus.ShowLinkTooltip(key, null, skillCheckResults);
			}).AddTo(this);
		}
	}

	private void OnFocusAnswerLink(string key, List<SkillCheckDC> skillCheckDcs)
	{
		if (!(GamePad.Instance.CurrentInputLayer.ContextName != "DialogGlossary"))
		{
			ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
			{
				m_FirstGlossaryFocus.ShowLinkTooltip(key, skillCheckDcs);
			}).AddTo(this);
		}
	}

	protected override void OnCloseGlossaryMode()
	{
		base.OnCloseGlossaryMode();
		CloseGlossary(default(InputActionEventData));
	}

	private void OnGlossaryFocusChanged(IConsoleEntity focus)
	{
		if (focus == null)
		{
			return;
		}
		RectTransform rect = ((focus as MonoBehaviour) ?? (focus as IMonoBehaviour)?.MonoBehaviour)?.transform as RectTransform;
		bool num = !base.IsShowHistory.CurrentValue && !m_CuesScrollRect.IsInViewport(rect);
		Action action = delegate
		{
			if (!base.IsShowHistory.CurrentValue)
			{
				m_CuesScrollRect.SnapToCenter(rect);
			}
		};
		if (num)
		{
			action();
		}
	}

	public void OnShowEscMenu(InputActionEventData data)
	{
		EventBus.RaiseEvent(delegate(IEscMenuHandler h)
		{
			h.HandleOpen();
		});
	}

	private void ScrollMenu(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_AnswersScrollRect.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}
}
