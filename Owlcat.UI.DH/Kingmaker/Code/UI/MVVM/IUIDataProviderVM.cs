using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UIDataProvider;
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
		Icon = mIuiDataProvider.Icon.GetDefaultIfNull(DefaultImageType.Ability);
	}

	protected override void OnDispose()
	{
	}
}
