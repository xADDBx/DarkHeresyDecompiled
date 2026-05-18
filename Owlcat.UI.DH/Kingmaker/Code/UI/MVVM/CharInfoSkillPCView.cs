using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoSkillPCView : View<CharInfoStatVM>, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate
{
	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_NameLabel;

	[Header("Values")]
	[SerializeField]
	private TextMeshProUGUI m_ValueLabel;

	[Header("Diff")]
	[SerializeField]
	private TextMeshProUGUI m_DiffValueLabel;

	[SerializeField]
	private GameObject m_DiffState;

	[Header("Source stat")]
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_SourceNameLabel;

	[Header("Visual")]
	[SerializeField]
	protected OwlcatMultiSelectable m_Selectable;

	[SerializeField]
	private Image m_TooltipArea;

	[SerializeField]
	private GameObject m_BattleSkillIcon;

	private bool m_HasPreviewValue;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		UISounds.Instance.SetClickSound(m_Selectable, ButtonSoundsEnum.NoSound);
		UISounds.Instance.SetHoverSound(m_Selectable, Game.Instance.IsControllerGamepad ? ButtonSoundsEnum.PaperComponentSound : ButtonSoundsEnum.NoSound);
		base.ViewModel.Name.Subscribe(delegate
		{
			SetLabel();
		}).AddTo(this);
		if (m_BattleSkillIcon != null)
		{
			base.ViewModel.IsBattleSkill.Subscribe(m_BattleSkillIcon.SetActive).AddTo(this);
		}
		base.ViewModel.HasPenalties.CombineLatest(base.ViewModel.HasBonuses, (bool _, bool _) => new { }).Subscribe(_ =>
		{
			SetBonuses();
		}).AddTo(this);
		base.ViewModel.StatValue.CombineLatest(base.ViewModel.PreviewStatValue, base.ViewModel.Bonus, (int stat, int previewStat, int bonus) => new { stat, previewStat, bonus }).Subscribe(obj =>
		{
			SetValues(obj.stat, obj.previewStat, obj.bonus);
		}).AddTo(this);
		SetTooltip();
	}

	protected virtual void SetValues(int statValue, int previewValue, int bonus)
	{
		m_ValueLabel.text = statValue.ToString();
		m_HasPreviewValue = statValue != previewValue;
		if ((bool)m_DiffState)
		{
			m_DiffState.SetActive(m_HasPreviewValue);
		}
		if ((bool)m_DiffValueLabel)
		{
			if (m_HasPreviewValue)
			{
				int num = previewValue - statValue;
				m_DiffValueLabel.text = UIUtilityText.AddSign(num) ?? "";
			}
			else if (bonus != 0)
			{
				m_DiffValueLabel.text = UIUtilityText.AddSign(bonus) ?? "";
			}
		}
		SetBonuses();
	}

	private void SetLabel()
	{
		if (m_SourceNameLabel != null)
		{
			m_SourceNameLabel.text = base.ViewModel.ShortName;
		}
		m_NameLabel.text = base.ViewModel.Name.CurrentValue;
	}

	private void SetBonuses()
	{
		if ((bool)m_Selectable)
		{
			if (m_HasPreviewValue)
			{
				m_Selectable.SetActiveLayer("Preview");
			}
			else if (base.ViewModel.HasBonuses.CurrentValue)
			{
				m_Selectable.SetActiveLayer("Bonus");
			}
			else if (base.ViewModel.HasPenalties.CurrentValue)
			{
				m_Selectable.SetActiveLayer("Penalty");
			}
			else
			{
				m_Selectable.SetActiveLayer("Normal");
			}
		}
	}

	private void SetTooltip()
	{
		(m_TooltipArea ? ((MonoBehaviour)m_TooltipArea) : ((MonoBehaviour)this)).SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		if (m_BattleSkillIcon != null)
		{
			m_BattleSkillIcon?.GetComponent<MonoBehaviour>()?.SetGlossaryTooltip("CombatSkill").AddTo(this);
		}
	}

	public void SetFocus(bool value)
	{
		m_Selectable.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Selectable.IsValid();
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip.CurrentValue;
	}
}
