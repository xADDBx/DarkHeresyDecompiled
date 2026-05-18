using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenPortraitSelectorItemView : SelectionGroupEntityView<CharGenPortraitSelectorItemVM>
{
	[SerializeField]
	private Image m_Portrait;

	public bool IsSelected => base.ViewModel.IsSelected.Value;

	protected override void OnBind()
	{
		base.OnBind();
		if (base.ViewModel.PortraitData?.SmallPortraitHandle != null)
		{
			base.ViewModel.PortraitData.SmallPortraitHandle.Request.Loaded += RefreshView;
		}
		m_Button.OnHoverAsObservable().Subscribe(delegate(bool value)
		{
			if (value)
			{
				EventBus.RaiseEvent(delegate(ICharGenPortraitSelectorHoverHandler h)
				{
					h.HandleHoverStart(base.ViewModel.PortraitData);
				});
			}
			else
			{
				EventBus.RaiseEvent(delegate(ICharGenPortraitSelectorHoverHandler h)
				{
					h.HandleHoverStop();
				});
			}
		}).AddTo(this);
		RefreshView();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		if (base.ViewModel.PortraitData?.SmallPortraitHandle != null)
		{
			base.ViewModel.PortraitData.SmallPortraitHandle.Request.Loaded -= RefreshView;
		}
	}

	public override void RefreshView()
	{
		m_Portrait.sprite = base.ViewModel.PortraitData?.SmallPortrait;
	}
}
