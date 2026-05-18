using Code.View.UI.Helpers;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenPhaseRoadmapView<TViewModel> : SelectionGroupEntityView<TViewModel>, ICharGenPhaseRoadmapView, IInitializable where TViewModel : CharGenPhaseBaseVM
{
	private const string ButtonStateActive = "Active";

	private const string ButtonStateAvailable = "Available";

	private const string ButtonStateNotAvailable = "NotAvailable";

	private const string ButtonStateDone = "Done";

	private const string ButtonStateWarning = "Warning";

	private const string ButtonStateWarningActive = "WarningActive";

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
		m_Label.text = base.ViewModel.PhaseName.CurrentValue;
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
		if (base.ViewModel.IsSelected.Value)
		{
			return "Active";
		}
		if (base.ViewModel.IsCompletedAndAvailable.CurrentValue)
		{
			return "Done";
		}
		if (!base.ViewModel.IsAvailable.CurrentValue)
		{
			return "NotAvailable";
		}
		return "Available";
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
