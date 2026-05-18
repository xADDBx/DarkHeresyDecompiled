using System.Collections.Generic;
using System.Linq;
using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CareerPathsListCommonView : View<CareerPathsListVM>
{
	private enum CareerState
	{
		Locked,
		Selected,
		HasUpgrades,
		Finished,
		Unlocked
	}

	[Header("Common")]
	[SerializeField]
	private OwlcatMultiSelectable m_MainSelectable;

	[Header("View")]
	[SerializeField]
	private TextMeshProUGUI m_LockPavelLabel;

	[SerializeField]
	private TextMeshProUGUI m_UnlockPavelLabel;

	[SerializeField]
	private OwlcatMultiSelectable m_CareerStateSelectable;

	[Header("Selected Career")]
	[SerializeField]
	private CareerPathListSelectedCareerCommonView m_SelectedCareerCommonView;

	[Header("Careers List")]
	[SerializeField]
	private CareerPathListItemCommonView m_CareerPathListItemCommonView;

	[SerializeField]
	private WidgetList m_WidgetList;

	private ReactiveProperty<int> m_CurrentRank;

	private AccessibilityTextHelper m_TextHelper;

	public IEnumerable<CareerPathListItemCommonView> ItemViews => m_WidgetList.Entries.Cast<CareerPathListItemCommonView>();

	public void Initialize()
	{
		m_SelectedCareerCommonView.Initialize();
		m_TextHelper = new AccessibilityTextHelper(m_LockPavelLabel, m_UnlockPavelLabel);
	}

	protected override void OnBind()
	{
		base.ViewModel.SelectedCareer.Subscribe(delegate(CareerPathVM career)
		{
			m_SelectedCareerCommonView.Bind(career);
			if (career != null)
			{
				ObservableSubscribeExtensions.Subscribe(career.OnUpdateSelected, delegate
				{
					UpdateSelectedCareerState();
				}).AddTo(this);
				career.CanUpgrade.Subscribe(delegate
				{
					UpdateSelectedCareerState();
				}).AddTo(this);
			}
			string activeLayer = ((base.ViewModel.SelectedCareer.CurrentValue == null) ? "Default" : "Career");
			m_MainSelectable.SetActiveLayer(activeLayer);
			UpdateSelectedCareerState();
		}).AddTo(this);
		SetupView();
		DrawEntries();
		base.ViewModel.IsUnlocked.Subscribe(delegate
		{
			UpdateSelectedCareerState();
		}).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		m_WidgetList.Clear();
		m_TextHelper.Dispose();
	}

	private void UpdateSelectedCareerState()
	{
		CareerPathVM careerPathVM = base.ViewModel.SelectedCareer?.CurrentValue;
		CareerState careerState = CareerState.Locked;
		if (careerPathVM != null)
		{
			careerState = CareerState.Selected;
			if (careerPathVM.IsFinished)
			{
				careerState = CareerState.Finished;
			}
			else if (careerPathVM.IsAvailableToUpgrade)
			{
				careerState = CareerState.HasUpgrades;
			}
		}
		else if (base.ViewModel.IsUnlocked.CurrentValue)
		{
			careerState = CareerState.Unlocked;
		}
		m_CareerStateSelectable.SetActiveLayer(careerState.ToString());
	}

	private void DrawEntries()
	{
		m_WidgetList.DrawEntries(base.ViewModel.CareerPathVMs, m_CareerPathListItemCommonView);
	}

	private void SetupView()
	{
		string text = base.ViewModel.GetLevelToUnlock() + " " + UIStrings.Instance.CharacterSheet.LvlShort.Text;
		m_LockPavelLabel.text = text;
		m_UnlockPavelLabel.text = text;
	}

	public void CreateLines(List<CareerPathListItemCommonView> allCareers)
	{
		ItemViews.ToList().ForEach(delegate(CareerPathListItemCommonView c)
		{
			c.CreateLines(allCareers);
		});
	}
}
