using Kingmaker.Settings;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhaseDifficultyBaseView : View<NewGamePhaseDifficultyVM>
{
	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	protected VirtualListVertical m_VirtualList;

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	public VirtualListVertical VirtualList => m_VirtualList;

	public InfoSectionView InfoView => m_InfoView;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> ReactiveTooltipTemplate => m_ReactiveTooltipTemplate;

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_InfoView.Bind(base.ViewModel.InfoVM);
		base.ViewModel.IsEnabled.Subscribe(delegate(bool value)
		{
			SettingsController.Instance.RevertAllTempValues();
			base.gameObject.SetActive(value);
			m_VirtualList.ScrollController.ForceScrollToTop();
			if (value)
			{
				base.ViewModel.HandleItemChanged(string.Empty);
			}
		}).AddTo(this);
		base.ViewModel.ReactiveTooltipTemplate.Subscribe(delegate(TooltipBaseTemplate value)
		{
			m_ReactiveTooltipTemplate.Value = value;
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}

	public void StrollInfoViewToTop()
	{
		m_InfoView.Or(null)?.ScrollRectExtended.ScrollToTop();
	}
}
