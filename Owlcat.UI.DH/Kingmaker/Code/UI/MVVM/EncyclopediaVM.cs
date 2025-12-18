using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class EncyclopediaVM : ViewModel
{
	private readonly ReactiveProperty<EncyclopediaPageVM> m_Page = new ReactiveProperty<EncyclopediaPageVM>();

	public readonly EncyclopediaNavigationVM NavigationVM;

	public ReadOnlyReactiveProperty<EncyclopediaPageVM> Page => m_Page;

	public EncyclopediaVM(INode node = null)
	{
		NavigationVM = new EncyclopediaNavigationVM().AddTo(this);
		HandleEncyclopediaPage(node ?? Game.Instance.Player.UISettings.CurrentEncyclopediaPage);
		Metrics.Interface.InterfaceState(InterfaceMetricsEvent.InterfaceStates.Open).InterfaceType(InterfaceMetricsEvent.InterfaceTypes.Encyclopedia).Send();
	}

	public void HandleEncyclopediaPage(INode node)
	{
		if (node != null)
		{
			IPage page = node as IPage;
			if (page != Page.CurrentValue?.Page)
			{
				Page.CurrentValue?.Dispose();
				NavigationVM.HandleEncyclopediaPage(node);
				m_Page.Value = new EncyclopediaPageVM(page).AddTo(this);
			}
		}
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		if (Game.Instance.TutorialSystem.HasShownData)
		{
			EventBus.RaiseEvent(delegate(INewTutorialUIHandler h)
			{
				h.ShowTutorial(Game.Instance.TutorialSystem.ShowingData);
			});
		}
		Metrics.Interface.InterfaceState(InterfaceMetricsEvent.InterfaceStates.Close).InterfaceType(InterfaceMetricsEvent.InterfaceTypes.Encyclopedia).Send();
	}
}
