using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenPhaseRoadmapView<TViewModel> : SelectionGroupEntityView<TViewModel>, ICharGenPhaseRoadmapView, IInitializable where TViewModel : CharGenPhaseBaseVM
{
	private const string BUTTON_STATE_ACTIVE = "Active";

	private const string BUTTON_STATE_AVAILABLE = "Available";

	private const string BUTTON_STATE_NOT_AVAILABLE = "NotAvailable";

	private const string BUTTON_STATE_DONE = "Done";

	private const string BUTTON_STATE_WARNING = "Warning";

	private const string BUTTON_STATE_WARNING_ACTIVE = "WarningActive";

	[SerializeField]
	protected TextMeshProUGUI m_Label;

	[SerializeField]
	private GameObject m_LevelSeparator;

	[SerializeField]
	protected TextMeshProUGUI m_LevelLabel;

	private AccessibilityTextHelper m_AccessibilityTextHelper;

	public RectTransform ViewRectTransform => base.transform as RectTransform;

	public void SetParentTransform(Transform parent, int siblingIndex = 0)
	{
		base.transform.SetParent(parent, worldPositionStays: false);
		base.transform.SetSiblingIndex(siblingIndex);
	}

	public CharGenPhaseBaseVM GetPhaseBaseVM()
	{
		return base.ViewModel;
	}

	protected override void BindViewImplementation()
	{
		m_AccessibilityTextHelper = new AccessibilityTextHelper(m_Label);
		ClearState();
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.PhaseName.Subscribe(delegate(string value)
		{
			m_Label.text = value;
		}));
		AddDisposable(base.ViewModel.IsAvailable.Subscribe(delegate
		{
			UpdateSelectableState();
		}));
		AddDisposable(base.ViewModel.IsCompletedAndAvailable.Subscribe(delegate
		{
			UpdateSelectableState();
		}));
		m_Label.text = UIStrings.Instance.CharGen.GetPhaseName(base.ViewModel.PhaseType);
		m_AccessibilityTextHelper.UpdateTextSize();
		m_LevelSeparator.SetActive(base.ViewModel.Rank > 0);
		m_LevelLabel.text = base.ViewModel.Rank.ToString();
	}

	protected override void DestroyViewImplementation()
	{
		ClearState();
	}

	protected override void OnClick()
	{
		if (UtilityNet.IsControlMainCharacter())
		{
			base.OnClick();
		}
	}

	public override void OnChangeSelectedState(bool value)
	{
		base.OnChangeSelectedState(value);
		UpdateSelectableState();
	}

	private void UpdateSelectableState()
	{
		m_Button.SetActiveLayer(GetButtonState());
	}

	private string GetButtonState()
	{
		if (!base.ViewModel.IsSelected.Value)
		{
			if (!base.ViewModel.IsCompletedAndAvailable.CurrentValue)
			{
				if (!base.ViewModel.IsAvailable.CurrentValue)
				{
					return "NotAvailable";
				}
				return "Available";
			}
			return "Done";
		}
		return "Active";
	}

	private void ClearState()
	{
		m_Button.Interactable = false;
		m_Button.SetActiveLayer("Available");
		m_LevelSeparator.SetActive(value: false);
	}

	void IInitializable.Initialize()
	{
		Initialize();
	}
}
