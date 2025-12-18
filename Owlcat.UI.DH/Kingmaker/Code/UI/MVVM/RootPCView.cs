using System.Threading;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.Components.Camera;
using Kingmaker.Code.View.UI.MVVM.ServiceWindows;
using Kingmaker.UI.Pointer;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class RootPCView : View<RootVM>
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
	private Transform m_Root;

	[SerializeField]
	private UIPostProcessSpace m_UIPostProcessSpace;

	[SerializeField]
	private UIPostProcessingAnimator m_UIPostProcessingAnimator;

	[SerializeField]
	private PCCursor m_Cursor;

	protected override void OnBind()
	{
		PrefabLinkLookupTable prefabLinkLookupTable = (PrefabLinkLookupTable)UIConfig.Instance.ViewConfigs.RootPCLookupTable;
		new ViewService(new ViewComposer(new DefaultComposerRule()), new ViewFactory(m_Root, prefabLinkLookupTable), new ViewTransitor()).AddTo(this).SubscribeAll(base.ViewModel).AddTo(this);
		m_UIPostProcessSpace.Bind();
		m_UIPostProcessingAnimator.Bind();
		WarmUpView<ServiceWindowsPanelVM>(prefabLinkLookupTable);
		m_Cursor.Bind().AddTo(this);
		m_Cursor.SetActive(base.ViewModel.IsCursorActive);
	}

	private static async Task WarmUpView<T>(PrefabLinkLookupTable table)
	{
		WidgetPool.Release(await WidgetPool.RetainAsync(await table.FirstOrDefault(typeof(T)), null, CancellationToken.None));
	}

	protected override void OnUnbind()
	{
		m_UIPostProcessSpace.Dispose();
		m_UIPostProcessingAnimator.Dispose();
	}
}
