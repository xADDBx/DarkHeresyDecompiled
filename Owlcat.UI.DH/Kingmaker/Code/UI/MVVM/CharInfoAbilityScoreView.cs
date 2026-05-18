using Code.View.UI.Helpers;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAbilityScoreView : View<CharInfoStatVM>
{
	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_ShortName;

	[SerializeField]
	private TextMeshProUGUI m_LongName;

	[Header("Values")]
	[SerializeField]
	private TextMeshProUGUI m_Value;

	[Header("Diff")]
	[SerializeField]
	private TextMeshProUGUI m_DiffValueLabel;

	[SerializeField]
	private GameObject m_DiffState;

	[Header("Visual")]
	[SerializeField]
	protected OwlcatMultiSelectable m_Selectable;

	[SerializeField]
	private int m_SecondaryCharSizePercent;

	[SerializeField]
	private Color m_AccentCharColor;

	private bool m_HasPreviewValue;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_ShortName, m_LongName, m_Value, m_DiffValueLabel);
		}
		UISounds.Instance.SetClickSound(m_Selectable, ButtonSoundsEnum.NoSound);
		UISounds.Instance.SetHoverSound(m_Selectable, Game.Instance.IsControllerGamepad ? ButtonSoundsEnum.PaperComponentSound : ButtonSoundsEnum.NoSound);
		base.ViewModel.Name.Subscribe(delegate
		{
			SetLabels();
		}).AddTo(this);
		base.ViewModel.HasPenalties.CombineLatest(base.ViewModel.HasBonuses, (bool _, bool _) => new { }).Subscribe(_ =>
		{
			SetBonuses();
		}).AddTo(this);
		base.ViewModel.StatValue.CombineLatest(base.ViewModel.PreviewStatValue, base.ViewModel.Bonus, (int stat, int previewStat, int bonus) => new { stat, previewStat, bonus }).Subscribe(obj =>
		{
			SetValue();
			m_HasPreviewValue = obj.stat != obj.previewStat;
			m_DiffState.Or(null)?.SetActive(m_HasPreviewValue);
			SetBonuses();
			if ((bool)m_DiffValueLabel)
			{
				if (m_HasPreviewValue)
				{
					int num = obj.previewStat - obj.stat;
					m_DiffValueLabel.text = UIUtilityText.AddSign(num) ?? "";
				}
				else if (obj.bonus != 0)
				{
					m_DiffValueLabel.text = UIUtilityText.AddSign(obj.bonus) ?? "";
				}
			}
		}).AddTo(this);
		SetTooltip();
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		m_TextHelper.Dispose();
	}

	private void SetLabels()
	{
		if (m_LongName != null)
		{
			m_LongName.text = base.ViewModel.Name.CurrentValue;
		}
		if (m_ShortName != null)
		{
			m_ShortName.text = base.ViewModel.ShortName;
		}
	}

	private void SetValue()
	{
		if (!base.ViewModel.IsValueEnabled.CurrentValue)
		{
			if (m_Value != null)
			{
				m_Value.text = "-";
			}
		}
		else if (!(m_Value == null))
		{
			string text = base.ViewModel.StatValue.ToString();
			string text2;
			if (text.Length <= 1)
			{
				text2 = string.Empty;
			}
			else
			{
				string text3 = text;
				text2 = text3.Substring(1, text3.Length - 1);
			}
			string text4 = text2;
			m_Value.text = $"<color=#{ColorUtility.ToHtmlStringRGB(m_AccentCharColor)}>{text[0]}</color><size={m_SecondaryCharSizePercent}%>{text4}</size>";
		}
	}

	private void SetBonuses()
	{
		if ((bool)m_Selectable)
		{
			if (m_HasPreviewValue)
			{
				m_Selectable.SetActiveLayer("Preview");
			}
			else if (base.ViewModel.HasBonuses.CurrentValue && base.ViewModel.Bonus.CurrentValue > 0)
			{
				m_Selectable.SetActiveLayer("Bonus");
			}
			else if (base.ViewModel.HasPenalties.CurrentValue && base.ViewModel.Bonus.CurrentValue < 0)
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
		this.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
	}
}
