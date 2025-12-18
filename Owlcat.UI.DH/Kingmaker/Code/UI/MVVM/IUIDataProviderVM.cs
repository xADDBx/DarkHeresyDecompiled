using Kingmaker.Blueprints.Root;
using Kingmaker.UIDataProvider;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class IUIDataProviderVM : ViewModel
{
	private IUIDataProvider m_IUIDataProvider;

	public readonly Sprite Icon;

	public IUIDataProvider IUIDataProvider => m_IUIDataProvider;

	public string Name => m_IUIDataProvider.Name;

	public string Description => m_IUIDataProvider.Description;

	public IUIDataProviderVM(IUIDataProvider mIuiDataProvider)
	{
		m_IUIDataProvider = mIuiDataProvider;
		Icon = mIuiDataProvider.Icon.Or(UIConfig.Instance.UIIcons.DefaultAbilityIcon);
	}

	protected override void OnDispose()
	{
	}
}
