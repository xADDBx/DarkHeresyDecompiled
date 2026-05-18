using System;
using Kingmaker.Localization;
using Owlcat.UI;
using Owlcat.UI.Commands;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.Common;

public class HintView : View<Command>
{
	[SerializeField]
	private HintGamepadIconProvider m_Icons;

	[Header("Elements")]
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private GameObject m_ProgressRoot;

	[SerializeField]
	private Image m_ProgressImage;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[Header("Styles")]
	[SerializeField]
	private HintViewLabelStyle m_LabelStyle;

	[Header("Animations")]
	[SerializeField]
	private PunchScaleAnimator m_Trigger;

	private bool TryGetIcon(string binding, out Sprite sprite)
	{
		return m_Icons.TryGetIcon(binding, out sprite);
	}

	protected override void OnBind()
	{
		Observable.Merge<Unit>(Observable.Return(Unit.Default), Observable.EveryValueChanged(base.ViewModel, (Command x) => x.Active).AsUnitObservable(), Observable.EveryValueChanged(base.ViewModel, (Command x) => x.Enabled).AsUnitObservable(), Observable.EveryValueChanged(base.ViewModel, (Command x) => x.Progress).AsUnitObservable()).Subscribe(OnChanged).AddTo(this);
		Observable.FromEvent(delegate(Action h)
		{
			base.ViewModel.Triggered += h;
		}, delegate(Action h)
		{
			base.ViewModel.Triggered -= h;
		}).Subscribe(OnTriggered).AddTo(this);
	}

	private void OnTriggered()
	{
		m_Trigger.Play(this, m_Icon.transform);
	}

	private void OnChanged(Unit _)
	{
		UpdateStyle();
		UpdateIcon();
		UpdateProgress();
		UpdateLabel();
	}

	private void UpdateStyle()
	{
		if (TryGetComponent<AutoLayoutGroup>(out var component))
		{
			component.reverseArrangement = m_LabelStyle == HintViewLabelStyle.Right;
		}
		m_Label.gameObject.SetActive(m_LabelStyle != HintViewLabelStyle.None);
	}

	private void UpdateIcon()
	{
		if (TryGetIcon(base.ViewModel.Binding, out var sprite))
		{
			m_Icon.sprite = sprite;
		}
		else
		{
			PFLog.UI.Error("Missing hint icon for '" + base.ViewModel.Binding + "'");
		}
	}

	private void UpdateProgress()
	{
		bool flag = base.ViewModel.Binding.Contains("#longpress");
		if (flag)
		{
			m_ProgressImage.fillAmount = Mathf.Lerp(0.15f, 1f, base.ViewModel.Progress);
		}
		m_ProgressRoot.SetActive(flag);
	}

	private void UpdateLabel()
	{
		if (base.ViewModel.Label is LocalizedString localizedString)
		{
			m_Label.text = localizedString;
		}
		else if (base.ViewModel.Label is string text)
		{
			m_Label.text = text;
		}
		else if (base.ViewModel.Label == null)
		{
			m_Label.text = string.Empty;
		}
	}
}
