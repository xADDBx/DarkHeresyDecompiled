using System.Threading;
using System.Threading.Tasks;
using Kingmaker.Code.View.UI.Components.Camera;
using Kingmaker.Code.View.UI.MVVM.ServiceWindows;
using Kingmaker.UI.DragNDrop;
using Owlcat.UI;
using Owlcat.UI.Commands;
using Owlcat.UI.Commands.InputSystem;
using Owlcat.UI.Navigation;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class RootView : View<RootVM>
{
	private class DefaultComposerRule : IViewComposerRule
	{
		public bool IsForbidden(object candidate, object alreadyOnScreen)
		{
			if (alreadyOnScreen is TermsOfUseVM { TermsOfUseAccepted: not false } && candidate is MessageBoxVM)
			{
				return true;
			}
			return false;
		}
	}

	[SerializeField]
	private PrefabLinkLookupTable m_Lookup;

	[SerializeField]
	private Transform m_Root;

	[SerializeField]
	private UIPostProcessSpace m_UIPostProcessSpace;

	[SerializeField]
	private DragNDropManager m_DragNDropManager;

	[SerializeField]
	private UIVisibilityView[] m_VisibilityViews;

	private InputController m_InputController;

	private UnityInputSystemSource m_Input;

	protected override void OnBind()
	{
		new ViewService(new ViewComposer(new DefaultComposerRule()), new ViewFactory(m_Root, m_Lookup), new ViewTransitor()).AddTo(this).SubscribeAll(base.ViewModel).AddTo(this);
		m_UIPostProcessSpace.Bind();
		WarmUpView<ServiceWindowsPanelVM>(m_Lookup);
		base.ViewModel.MainMenuVM.Subscribe(delegate(MainMenuVM value)
		{
			if (value != null)
			{
				WarmUpView<CharGenVM>(m_Lookup);
			}
		}).AddTo(this);
		m_InputController = new InputController().AddTo(this);
		m_Input = new UnityInputSystemSource(CommandLayerStack.Current);
		Observable.EveryUpdate(UnityFrameProvider.PreUpdate).Subscribe(OnUpdate).AddTo(this);
		NavigationSettings.DefaultPointer = new WH2PointerProvider(Game.Instance.CursorController);
		UIVisibilityView[] visibilityViews = m_VisibilityViews;
		for (int i = 0; i < visibilityViews.Length; i++)
		{
			visibilityViews[i].Bind(base.ViewModel.VisibilityVM);
		}
	}

	private void OnUpdate(Unit _)
	{
		m_Input.Update();
	}

	private static async Task WarmUpView<T>(PrefabLinkLookupTable table)
	{
		WidgetPool.Release(await WidgetPool.RetainAsync(await table.FirstOrDefault(typeof(T)), null, CancellationToken.None));
	}

	protected override void OnUnbind()
	{
		m_UIPostProcessSpace.Dispose();
		m_DragNDropManager?.Dispose();
		UIVisibilityView[] visibilityViews = m_VisibilityViews;
		for (int i = 0; i < visibilityViews.Length; i++)
		{
			visibilityViews[i].Unbind();
		}
	}

	[ContextMenu("Test")]
	private void Test()
	{
		base.ViewModel.Test();
	}
}
