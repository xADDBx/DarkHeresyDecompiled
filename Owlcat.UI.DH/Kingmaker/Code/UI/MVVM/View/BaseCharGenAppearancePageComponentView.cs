using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class BaseCharGenAppearancePageComponentView<TViewModel> : VirtualListElementViewBase<TViewModel>, ICharGenAppearancePageComponent, IConsoleNavigationEntity, IConsoleEntity where TViewModel : BaseCharGenAppearancePageComponentVM
{
	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	protected readonly ReactiveProperty<bool> IsFocused = new ReactiveProperty<bool>();

	public virtual void AddInput(ref InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
	}

	public virtual void RemoveInput()
	{
	}

	public virtual void SetFocus(bool value)
	{
		if (m_Selectable != null)
		{
			m_Selectable.SetFocus(value);
		}
		IsFocused.Value = value;
		if (value)
		{
			base.ViewModel.Focused();
		}
	}

	public bool IsValid()
	{
		return base.ViewModel.IsAvailable.CurrentValue;
	}

	protected override void BindViewImplementation()
	{
	}

	protected override void DestroyViewImplementation()
	{
		IsFocused.Value = false;
	}

	protected void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	protected void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
