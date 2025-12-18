using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.UI.Workarounds;
using ObservableCollections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Assets.Code.View.UI.MVVM;

public class CharInfoArchetypeTabView : CharInfoComponentView<CharInfoArchetypeTabVM>
{
	[Header("Career")]
	[SerializeField]
	private CharInfoArchetypeFeatureView m_FirstCareerFeatureView;

	[SerializeField]
	private TextMeshProUGUI m_FirstCareerLabel;

	[SerializeField]
	private CharInfoArchetypeFeatureView m_SecondCareerFeatureView;

	[SerializeField]
	private TextMeshProUGUI m_SecondCareerLabel;

	[Header("Homeworld")]
	[SerializeField]
	private CharInfoArchetypeFeatureView m_HomeworldFeatureView;

	[SerializeField]
	private TextMeshProUGUI m_HomeworldLabel;

	[Header("Background")]
	[SerializeField]
	private CharInfoArchetypeFeatureView m_BackgroundFeatureView;

	[SerializeField]
	private TextMeshProUGUI m_BackgroundLabel;

	[Header("Specializations")]
	[SerializeField]
	private WidgetList m_SpecializationsList;

	[SerializeField]
	private CharInfoArchetypeFeatureView m_SpecializationsView;

	[SerializeField]
	private WidgetList m_TalentGroupList;

	[SerializeField]
	private CharInfoTalentGroupView m_TalentGroupView;

	[SerializeField]
	private TMP_InputField m_SearchInputField;

	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_BackgroundTitle;

	[SerializeField]
	private TextMeshProUGUI m_SpecializationTitle;

	[SerializeField]
	private TextMeshProUGUI m_TalentTitle;

	[SerializeField]
	private TextMeshProUGUI m_PlanLevelUpButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_NoTalentsLabel;

	protected override void OnBind()
	{
		base.OnBind();
		m_FirstCareerLabel.text = "I " + UIStrings.Instance.CharacterSheet.Career;
		m_SecondCareerLabel.text = "II " + UIStrings.Instance.CharacterSheet.Career;
		m_BackgroundLabel.text = UIStrings.Instance.CharGen.Background;
		m_HomeworldLabel.text = UIStrings.Instance.CharGen.Homeworld;
		m_BackgroundTitle.text = UIStrings.Instance.CharGen.Background;
		m_SpecializationTitle.text = UIStrings.Instance.CharGen.LevelUpSpecialization;
		m_TalentTitle.text = UIStrings.Instance.CharacterSheet.TalentFeatureGroupLabel;
		m_PlanLevelUpButtonLabel.text = UIStrings.Instance.CharacterSheet.PlanLevelUp;
		m_NoTalentsLabel.text = UIStrings.Instance.CharacterSheet.NoTalentsYet;
		base.ViewModel.FirstCareer.Subscribe(delegate(CareerPathVM value)
		{
			CharInfoArchetypeFeatureVM source2 = new CharInfoArchetypeFeatureVM(value?.CareerPath, value?.CareerTooltip, base.ViewModel.Unit.CurrentValue).AddTo(this);
			m_FirstCareerFeatureView.Bind(source2);
		}).AddTo(this);
		base.ViewModel.SecondCareer.Subscribe(delegate(CareerPathVM value)
		{
			CharInfoArchetypeFeatureVM source = new CharInfoArchetypeFeatureVM(value?.CareerPath, value?.CareerTooltip, base.ViewModel.Unit.CurrentValue).AddTo(this);
			m_SecondCareerFeatureView.Bind(source);
		}).AddTo(this);
		base.ViewModel.Homeworld.Subscribe(m_HomeworldFeatureView.Bind).AddTo(this);
		base.ViewModel.Background.Subscribe(m_BackgroundFeatureView.Bind).AddTo(this);
		base.ViewModel.SpecializationList.SubscribeToWidgetList(m_SpecializationsList, m_SpecializationsView).AddTo(this);
		base.ViewModel.TalentGroupList.SubscribeToWidgetList(m_TalentGroupList, m_TalentGroupView).AddTo(this);
		base.ViewModel.TalentGroupList.ObserveCountChanged().Subscribe(delegate(int c)
		{
			m_NoTalentsLabel.gameObject.SetActive(c == 0);
		}).AddTo(this);
		m_NoTalentsLabel.gameObject.SetActive(base.ViewModel.TalentGroupList.Count == 0);
		m_SearchInputField.OnValueChangedAsObservable().Subscribe(base.ViewModel.SearchChanged).AddTo(this);
	}
}
