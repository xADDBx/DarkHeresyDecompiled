using R3;
using UnityEngine;

namespace Owlcat.UI;

public class SelectionGroupEntityView<TViewModel> : VirtualListElementViewBase<TViewModel>, IConfirmClickHandler, IConsoleEntity, IConsolePointerLeftClickEvent, IConsoleNavigationEntity where TViewModel : SelectionGroupEntityVM
{
	private bool m_IsInit;

	[SerializeField]
	protected OwlcatMultiButton m_Button;

	public ReactiveCommand<Unit> PointerLeftClickCommand { get; } = new ReactiveCommand<Unit>();


	public IConsoleEntity ConsoleEntityProxy => m_Button;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			ClearView();
			DoInitialize();
			m_IsInit = true;
		}
	}

	public virtual void DoInitialize()
	{
	}

	protected override void OnBind()
	{
		base.OnBind();
		ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			PointerLeftClickCommand.Execute();
			OnClick();
		}).AddTo(this);
		m_Button.OnConfirmClickAsObservable().Subscribe(OnClick).AddTo(this);
		base.ViewModel.RefreshView.Subscribe(RefreshView).AddTo(this);
		RefreshView();
		base.ViewModel.IsSelected.Subscribe(OnChangeSelectedState).AddTo(this);
		base.ViewModel.IsAvailable.Subscribe(delegate
		{
			OnChangeSelectedState(base.ViewModel.IsSelected.Value);
		}).AddTo(this);
	}

	protected virtual void OnClick()
	{
		base.ViewModel.SetSelectedFromView(!base.ViewModel.IsSelected.Value);
		if (base.ViewModel != null && !base.ViewModel.AllowSwitchOff)
		{
			OnChangeSelectedState(base.ViewModel.IsSelected.Value);
		}
	}

	public virtual void OnChangeSelectedState(bool value)
	{
		if (base.ViewModel.IsAvailable.CurrentValue)
		{
			m_Button.SetInteractable(state: true);
			m_Button.SetActiveLayer(value ? "Selected" : "Normal");
			m_Button.CanConfirm = !value || base.ViewModel.AllowSwitchOff;
		}
		else
		{
			m_Button.SetInteractable(state: false);
			m_Button.SetActiveLayer(value ? "Selected" : "Normal");
			m_Button.CanConfirm = !value;
		}
	}

	public virtual void RefreshView()
	{
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_Button.SetActiveLayer("Normal");
		m_Button.CanConfirm = true;
		ClearView();
	}

	protected virtual void ClearView()
	{
	}

	public virtual void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public virtual bool IsValid()
	{
		return m_Button.Interactable;
	}

	public bool CanConfirmClick()
	{
		return m_Button.CanConfirm;
	}

	public string GetConfirmClickHint()
	{
		return GetConfirmActionName();
	}

	protected virtual string GetConfirmActionName()
	{
		return string.Empty;
	}

	public void OnConfirmClick()
	{
		m_Button.OnConfirmClick();
	}
}
