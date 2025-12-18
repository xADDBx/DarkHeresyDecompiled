using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NestedSelectionGroupEntityPCView<TViewModel> : VirtualListElementViewBase<TViewModel> where TViewModel : NestedSelectionGroupEntityVM
{
	[Header("Add to button layers: Collapsed | Expanded")]
	[SerializeField]
	private OwlcatMultiButton m_CollapseButton;

	private bool m_IsInit;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			ClearView();
			DoInitialize();
			m_IsInit = true;
		}
	}

	public void DoInitialize()
	{
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			OnClick();
		}));
		AddDisposable(ObservableSubscribeExtensions.Subscribe(base.ViewModel.RefreshView.DebounceFrame(1, UnityFrameProvider.PreLateUpdate), delegate
		{
			RefreshView();
		}));
		RefreshView();
		AddDisposable(base.ViewModel.IsSelected.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(OnChangeSelectedState));
		AddDisposable(base.ViewModel.IsAvailable.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate
		{
			OnChangeSelectedState(base.ViewModel.IsSelected.CurrentValue);
		}));
		if (!m_CollapseButton)
		{
			return;
		}
		if (base.ViewModel.HasNesting)
		{
			m_CollapseButton.gameObject.SetActive(value: true);
			AddDisposable(ObservableSubscribeExtensions.Subscribe(m_CollapseButton.OnLeftClickAsObservable(), delegate
			{
				OnExpandClick();
			}));
			AddDisposable(base.ViewModel.IsExpanded.DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(delegate
			{
				OnChangeSelectedState(base.ViewModel.IsSelected.CurrentValue);
			}));
		}
		else
		{
			m_CollapseButton.gameObject.SetActive(value: false);
		}
	}

	private void OnClick()
	{
		base.ViewModel.SetSelectedFromView(!base.ViewModel.IsSelected.CurrentValue);
		if (!base.ViewModel.AllowSwitchOff)
		{
			OnChangeSelectedState(base.ViewModel.IsSelected.CurrentValue);
		}
	}

	public void OnExpandClick()
	{
		base.ViewModel.SetExpanded(!base.ViewModel.IsExpanded.CurrentValue);
	}

	public void OnChangeSelectedState(bool value)
	{
		if (base.ViewModel.IsAvailable.CurrentValue)
		{
			m_Button.SetInteractable(state: true);
			m_Button.SetActiveLayer(value ? "Selected" : "Normal");
		}
		else
		{
			m_Button.SetActiveLayer("Normal");
			m_Button.SetInteractable(state: false);
		}
		if ((bool)m_CollapseButton)
		{
			if (base.ViewModel.IsAvailable.CurrentValue)
			{
				m_CollapseButton.SetInteractable(state: true);
				m_CollapseButton.SetActiveLayer(base.ViewModel.IsExpanded.CurrentValue ? "Expanded" : "Collapsed");
			}
			else
			{
				m_CollapseButton.SetActiveLayer("Collapsed");
				m_CollapseButton.SetInteractable(state: false);
			}
		}
	}

	public void RefreshView()
	{
	}

	private void ClearView()
	{
	}

	protected override void DestroyViewImplementation()
	{
		m_Button.SetActiveLayer("Normal");
		ClearView();
		if ((bool)m_CollapseButton)
		{
			m_CollapseButton.SetActiveLayer("Collapsed");
		}
	}
}
