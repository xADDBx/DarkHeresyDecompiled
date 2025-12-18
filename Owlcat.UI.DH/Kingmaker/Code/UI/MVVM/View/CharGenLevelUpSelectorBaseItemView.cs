using Kingmaker.Blueprints;
using Kingmaker.UI.Common.DebugInformation;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenLevelUpSelectorBaseItemView<TViewModel> : SelectionGroupEntityView<TViewModel>, IHasBlueprintInfo where TViewModel : CharGenLevelUpSelectorBaseItemVM
{
	protected const int PADDING_FOR_NESTING_LEVEL = 20;

	protected const string BUTTON_LAYER_NORMAL = "Normal";

	protected const string BUTTON_LAYER_CHOSEN = "Chosen";

	protected const string BUTTON_LAYER_NOT_AVAILABLE = "NotAvailable";

	protected const string BUTTON_LAYER_NOT_AVAILABLE_TAKEN = "NotAvailableTaken";

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_SubLabel;

	[SerializeField]
	private TextMeshProUGUI m_Acronym;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TalentGroupView m_TalentGroupView;

	[SerializeField]
	private HorizontalOrVerticalLayoutGroup m_Layout;

	[SerializeField]
	private GameObject m_Recommended;

	public BlueprintScriptableObject Blueprint => base.ViewModel.Blueprint;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_Label != null)
		{
			base.ViewModel.Label.Subscribe(delegate(string l)
			{
				m_Label.text = l;
			}).AddTo(this);
		}
		if (m_SubLabel != null)
		{
			base.ViewModel.SubLabel.Subscribe(delegate(string l)
			{
				m_SubLabel.text = l;
			}).AddTo(this);
		}
		if (m_Acronym != null)
		{
			base.ViewModel.Acronym.Subscribe(delegate(string a)
			{
				m_Acronym.text = a;
			}).AddTo(this);
		}
		if (m_Icon != null)
		{
			base.ViewModel.Sprite.Subscribe(delegate(Sprite s)
			{
				m_Icon.sprite = s;
			}).AddTo(this);
			base.ViewModel.SpriteColor.Subscribe(delegate(Color s)
			{
				m_Icon.color = s;
			}).AddTo(this);
		}
		if (m_TalentGroupView != null)
		{
			base.ViewModel.TalentIconInfo.Subscribe(delegate(TalentIconInfo s)
			{
				m_TalentGroupView.SetupView(s);
			}).AddTo(this);
		}
		if (m_Recommended != null)
		{
			base.ViewModel.IsRecommended.Subscribe(delegate(bool s)
			{
				m_Recommended.SetActive(s);
			}).AddTo(this);
		}
		if (m_Layout != null)
		{
			m_Layout.padding.left = 20 * base.ViewModel.NestingLevel;
		}
		base.ViewModel.IsShowed.Subscribe(delegate(bool e)
		{
			base.gameObject.SetActive(e);
		}).AddTo(this);
		base.ViewModel.State.Subscribe(delegate
		{
			UpdateAccessibility();
		}).AddTo(this);
		m_Button.OnHoverAsObservable().Subscribe(base.ViewModel.OnHover).AddTo(this);
	}

	public override void OnChangeSelectedState(bool value)
	{
		base.OnChangeSelectedState(value);
		UpdateAccessibility();
	}

	private void UpdateAccessibility()
	{
		OwlcatMultiButton button = m_Button;
		button.SetActiveLayer(base.ViewModel.State.CurrentValue switch
		{
			LEVEL_UP_ITEM_STATE.Available => base.ViewModel.IsSelected.Value ? "Chosen" : "Normal", 
			LEVEL_UP_ITEM_STATE.NotAvailable => "NotAvailable", 
			LEVEL_UP_ITEM_STATE.AlreadyExist => "NotAvailableTaken", 
			_ => "Normal", 
		});
	}
}
