using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoExperiencePCView : CharInfoComponentView<CharInfoExperienceVM>
{
	[Header("Fields")]
	[SerializeField]
	private TextMeshProUGUI m_LevelLabel;

	[SerializeField]
	private TextMeshProUGUI m_Level;

	[Header("Psy Rating")]
	[SerializeField]
	private GameObject m_PsyRatingGroup;

	[SerializeField]
	private Image m_PsyRatingBgr;

	[SerializeField]
	private TextMeshProUGUI m_PsyRatingLabel;

	[SerializeField]
	private TextMeshProUGUI m_PsyRating;

	[Header("Progress Bar")]
	[SerializeField]
	private Image m_ExpRoundImage;

	[SerializeField]
	private OwlcatMultiButton m_LevelUpButton;

	public override void Initialize()
	{
		base.Initialize();
		m_LevelLabel.text = UIStrings.Instance.CharacterSheet.LvlShort;
		if (m_PsyRatingLabel != null)
		{
			m_PsyRatingLabel.text = UIStrings.Instance.CharacterSheet.PsyRatingShort;
		}
		ObservableSubscribeExtensions.Subscribe(m_LevelUpButton?.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.LevelUp();
		}).AddTo(this);
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.Level.Subscribe(delegate(int level)
		{
			m_Level.text = $"{level}";
		}).AddTo(this);
		if (m_PsyRatingGroup != null)
		{
			base.ViewModel.HasPsyRating.Subscribe(m_PsyRatingGroup.SetActive).AddTo(this);
		}
		if (m_PsyRating != null)
		{
			base.ViewModel.PsyRating.Subscribe(delegate(int psyRating)
			{
				m_PsyRating.text = $"{psyRating}";
			}).AddTo(this);
		}
		if (m_LevelUpButton != null)
		{
			base.ViewModel.CanLevelup.Subscribe(delegate
			{
				m_LevelUpButton.gameObject.SetActive(base.ViewModel.CanLevelup.CurrentValue);
			}).AddTo(this);
		}
		if (m_ExpRoundImage != null)
		{
			base.ViewModel.CurrentLevelExpRatio.Subscribe(delegate(float value)
			{
				m_ExpRoundImage.fillAmount = value;
			}).AddTo(this);
		}
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		base.gameObject.SetActive(value: false);
	}

	protected override void RefreshView()
	{
		SetTooltips();
	}

	private void SetTooltips()
	{
		this.SetTooltip(new TooltipTemplateLevelExp(base.ViewModel)).AddTo(this);
		if ((bool)m_PsyRatingBgr)
		{
			m_PsyRatingBgr.SetTooltip(base.ViewModel.PsyRatingTooltip).AddTo(this);
		}
	}
}
