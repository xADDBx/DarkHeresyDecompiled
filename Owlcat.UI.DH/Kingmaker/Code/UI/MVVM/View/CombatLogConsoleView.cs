using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CombatLogConsoleView : CombatLogBaseView
{
	[SerializeField]
	protected GameObject m_ChannelPanel;

	[SerializeField]
	private GameObject m_PinBackground;

	[SerializeField]
	private TextMeshProUGUI m_ChannelText;

	[SerializeField]
	private List<float> m_YSizesList;

	[Header("Hints")]
	[SerializeField]
	private HintView m_ChangeSizeHint;

	[SerializeField]
	private HintView m_ModePinHint;

	[SerializeField]
	private HintView m_ConsoleHintFilterPrev;

	[SerializeField]
	private HintView m_ConsoleHintFilterNext;

	[SerializeField]
	private HintView m_ConsoleHintClose;

	[SerializeField]
	private HintView m_ConsoleHintOpen;

	[SerializeField]
	private HintView m_ConsoleHintOpenCombat;

	[SerializeField]
	private HintView m_ConsoleHintOpenExploration;

	[SerializeField]
	private HintView m_MoveCameraToHint;

	private readonly ReactiveProperty<bool> m_ShowModePin = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowMoveCameraTo = new ReactiveProperty<bool>();

	private bool m_InputLayerActivated;

	private TooltipConfig m_TooltipConfig;

	public bool HoldCombatLog => Game.Instance.Player.UISettings.HoldCombatLog;

	private int CurrentSizeIndex
	{
		get
		{
			if (base.ViewModel.CurrentSizeIndex.CurrentValue >= m_YSizesList.Count)
			{
				base.ViewModel.SetCurrentSizeIndex(m_YSizesList.Count - 1);
			}
			else if (base.ViewModel.CurrentSizeIndex.CurrentValue < 0)
			{
				base.ViewModel.SetCurrentSizeIndex(0);
			}
			return base.ViewModel.CurrentSizeIndex.CurrentValue;
		}
		set
		{
			if (value >= m_YSizesList.Count)
			{
				base.ViewModel.SetCurrentSizeIndex(0);
			}
			else if (value < 0)
			{
				base.ViewModel.SetCurrentSizeIndex(m_YSizesList.Count - 1);
			}
			else
			{
				base.ViewModel.SetCurrentSizeIndex(value);
			}
			Game.Instance.Player.UISettings.CombatLogSizeIndex = base.ViewModel.CurrentSizeIndex.CurrentValue;
		}
	}

	public override void Awake()
	{
		base.Awake();
		m_ChannelPanel.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.OnBind();
		SetSizeDelta(Game.Instance.Player.UISettings.LogSizeConsole);
		base.ViewModel.IsActive.Subscribe(OnVisible).AddTo(this);
		base.ViewModel.IsControlActive.Subscribe(delegate(bool value)
		{
			if (value)
			{
				ActivateControl();
			}
			else
			{
				DeactivateControl();
			}
		}).AddTo(this);
		foreach (CombatLogToggleWithCustomHint toggle in m_Toggles)
		{
			toggle.SetCustomHint(base.ViewModel.GetChannelName(m_Toggles.IndexOf(toggle)));
		}
		base.ViewModel.CurrentSizeIndex.Skip(1).Subscribe(OnSizeIndexChanged).AddTo(this);
		base.ViewModel.SetActiveState(HoldCombatLog);
		SetupChannelText();
		SetupShowMode();
		SetupSize();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		DeactivateControl();
	}

	protected virtual void OnVisible(bool value)
	{
		IsPinned.Value = value;
	}

	private void GetInputLayer()
	{
	}

	public override void SwitchPinnedState(bool pinned)
	{
		base.SwitchPinnedState(pinned);
		if (pinned)
		{
			SetContainerState(state: true);
		}
	}

	protected override void SetSizeDeltaImpl(Vector2 size)
	{
		m_PinnedContainer.sizeDelta = size;
		Game.Instance.Player.UISettings.LogSizeConsole = size;
	}

	private void OnChangeChannel(bool isPrev)
	{
		CombatLogToggleWithCustomHint combatLogToggleWithCustomHint = m_ToggleGroup.ActiveToggles().FirstOrDefault() as CombatLogToggleWithCustomHint;
		if (!(combatLogToggleWithCustomHint == null))
		{
			int num = m_Toggles.IndexOf(combatLogToggleWithCustomHint) + (isPrev ? 1 : (-1));
			if (num < 0)
			{
				num = m_Toggles.Count - 1;
			}
			else if (num >= m_Toggles.Count)
			{
				num = 0;
			}
			m_Toggles[num].Set(value: true);
			ButtonsSounds.Instance.Default.Hover.Play();
			OnToggleClick();
			SetupChannelText();
			m_VirtualList.ScrollController.ForceScrollToBottom();
		}
	}

	private void OnToggleClick()
	{
		CombatLogToggleWithCustomHint combatLogToggleWithCustomHint = m_ToggleGroup.ActiveToggles().FirstOrDefault() as CombatLogToggleWithCustomHint;
		if (!(combatLogToggleWithCustomHint == null))
		{
			CurrentSelectedIndex = m_Toggles.IndexOf(combatLogToggleWithCustomHint);
			base.ViewModel.SetCurrentChannelById(CurrentSelectedIndex);
		}
	}

	private void SetupChannelText()
	{
		if (m_ChannelText != null)
		{
			m_ChannelText.text = m_ToggleTexts[CurrentSelectedIndex].text;
		}
	}

	private void OnBack()
	{
		if (base.ViewModel.IsControlActive.CurrentValue)
		{
			base.ViewModel.CombatLogChangeState();
		}
		if (!HoldCombatLog)
		{
			base.ViewModel.Deactivate();
		}
		TooltipHelper.HideTooltip();
	}

	private void ActivateControl()
	{
		if (!m_InputLayerActivated)
		{
			m_InputLayerActivated = true;
			m_ChannelPanel.gameObject.SetActive(value: true);
			m_VirtualList.ScrollController.ForceScrollToBottom();
		}
	}

	private void DeactivateControl()
	{
		if (m_InputLayerActivated)
		{
			m_InputLayerActivated = false;
			m_ChannelPanel.gameObject.SetActive(value: false);
			m_VirtualList.ScrollController.ForceScrollToBottom();
		}
	}

	private void UpdateEntities()
	{
	}

	private void OnChangeShowMode()
	{
		Game.Instance.Player.UISettings.HoldCombatLog = !HoldCombatLog;
		SetupShowMode();
	}

	private void SetupShowMode()
	{
		if (m_PinBackground != null)
		{
			m_PinBackground.SetActive(HoldCombatLog);
		}
		m_ShowModePin.Value = HoldCombatLog;
	}

	private void SetupSize()
	{
		m_PinnedContainer.sizeDelta = new Vector2(m_PinnedContainer.sizeDelta.x, m_YSizesList[CurrentSizeIndex]);
	}

	private void OnChangeSize()
	{
		CombatSounds.Instance.CombatLog.SizeChanged.Play();
		CurrentSizeIndex++;
	}

	public void OnSizeIndexChanged(int index)
	{
		m_PinnedContainer.DOSizeDelta(new Vector2(m_PinnedContainer.sizeDelta.x, m_YSizesList[CurrentSizeIndex]), 0.2f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			UpdateEntities();
		});
	}

	public void AddInput()
	{
	}

	public void AddInputToCombat()
	{
	}

	public void AddInputToExploration()
	{
	}
}
