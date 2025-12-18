using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Localization;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SelectorWindowConsoleView<TEntityView, TEntityVM> : SelectorWindowBaseView<TEntityView, TEntityVM> where TEntityView : VirtualListElementViewBase<TEntityVM>, IHasTooltipTemplate where TEntityVM : SelectionGroupEntityVM
{
	[SerializeField]
	protected ConsoleHintsWidget m_ConsoleHintsWidget;

	protected GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected InputLayer m_InputLayer;

	private readonly ReactiveProperty<IConsoleEntity> m_SelectedEntity = new ReactiveProperty<IConsoleEntity>();

	public ReadOnlyReactiveProperty<IConsoleEntity> SelectedEntity => m_SelectedEntity;

	protected virtual LocalizedString ConfirmText => UIStrings.Instance.CommonTexts.Accept;

	protected virtual bool ShouldCloseOnConfirm => true;

	protected override void OnBind()
	{
		base.OnBind();
		SetupInput();
	}

	protected override void OnUnbind()
	{
		m_FadeAnimator.DisappearAnimation();
	}

	private void SetupInput()
	{
		m_NavigationBehaviour = m_VirtualList.GetNavigationBehaviour();
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "SelectorWindowConsoleView"
		});
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(base.OnClose, 9), UIStrings.Instance.CommonTexts.Back).AddTo(this);
		m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(OnConfirm, 8, CanEquip), ConfirmText).AddTo(this);
		m_InputLayer.AddAxis(Scroll, 3, repeat: true);
		m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(EntityFocused).AddTo(this);
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
		m_SortingComponent.PushView().AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			m_NavigationBehaviour.FocusOnFirstValidEntity();
		}).AddTo(this);
	}

	private void OnConfirm(InputActionEventData inputActionEventData)
	{
		TEntityVM entityVM = ((IHasViewModel)m_NavigationBehaviour.DeepestFocusAsObservable.Value).GetViewModel() as TEntityVM;
		base.ViewModel.Confirm(entityVM);
		if (ShouldCloseOnConfirm)
		{
			OnClose(inputActionEventData);
		}
	}

	private void Scroll(InputActionEventData obj, float x)
	{
		m_InfoSectionView.Scroll(x);
	}

	protected virtual void EntityFocused(IConsoleEntity entity)
	{
		base.ViewModel.InfoSectionVM.SetTemplate((entity as IHasTooltipTemplate)?.TooltipTemplate());
	}
}
