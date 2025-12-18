using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DetectiveIngameMenuNotificatorView : View<DetectiveIngameMenuNotificatorVM>
{
	[Header("Elements")]
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private MoveAnimator m_MoveAnimator;

	[Header("Values")]
	[SerializeField]
	private TextStyle m_HintStyle;

	private IDisposable m_HintDisposable;

	protected override void OnBind()
	{
		base.ViewModel.BadgeType.CombineLatest(base.ViewModel.IsInCombat, (DetectiveBadgeType badge, bool inCombat) => new { badge, inCombat }).Subscribe(value =>
		{
			m_Button.SetActiveLayer(value.ToString());
			SetState(value.badge != 0 && !value.inCombat);
			UpdateHint();
		}).AddTo(this);
		base.ViewModel.OperationCase.Subscribe(delegate
		{
			UpdateHint();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnBadgeClick();
		}).AddTo(this);
	}

	private void UpdateHint()
	{
		m_HintDisposable?.Dispose();
		m_HintDisposable = m_Button.SetHint(GetHint(base.ViewModel.BadgeType.CurrentValue)).AddTo(this);
	}

	private string GetHint(DetectiveBadgeType type)
	{
		if (type == DetectiveBadgeType.None)
		{
			return string.Empty;
		}
		using (GameLogContext.Scope)
		{
			GameLogContext.Case = base.ViewModel.OperationCase.CurrentValue;
			GameLogContext.TextStyle = m_HintStyle;
			string text = UIStrings.Instance.DetectiveJournal.ToCaseCommonHint.Text;
			GameLogContext.TextStyle = UIConfig.Instance.DefaultTextStyle;
			return text;
		}
	}

	private void SetState(bool state)
	{
		m_Button.gameObject.SetActive(state);
		if (state)
		{
			Appear();
		}
		else
		{
			Disappear();
		}
	}

	private void Appear()
	{
		m_FadeAnimator.AppearAnimation();
		m_MoveAnimator.AppearAnimation();
	}

	private void Disappear()
	{
		m_FadeAnimator.DisappearAnimation();
		m_MoveAnimator.DisappearAnimation();
	}
}
