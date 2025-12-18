using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenPortraitSelectorItemView : SelectionGroupEntityView<CharGenPortraitSelectorItemVM>, IFunc02ClickHandler, IConsoleEntity
{
	[SerializeField]
	private Image m_Portrait;

	public bool IsSelected => base.ViewModel.IsSelected.Value;

	public bool CanFunc02Click()
	{
		return base.ViewModel.IsCustom;
	}

	public void OnFunc02Click()
	{
		base.ViewModel.OnCustomPortraitChange();
	}

	public string GetFunc02ClickHint()
	{
		return UIStrings.Instance.CharGen.ChangePortrait;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (base.ViewModel.PortraitData.SmallPortraitHandle != null)
		{
			base.ViewModel.PortraitData.SmallPortraitHandle.Request.Loaded += RefreshView;
		}
		AddDisposable(m_Button.OnHoverAsObservable().Subscribe(delegate(bool value)
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
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		if (base.ViewModel.PortraitData.SmallPortraitHandle != null)
		{
			base.ViewModel.PortraitData.SmallPortraitHandle.Request.Loaded -= RefreshView;
		}
	}

	protected override void OnClick()
	{
		if (UtilityNet.IsControlMainCharacter())
		{
			base.OnClick();
		}
	}

	public override void RefreshView()
	{
		m_Portrait.sprite = base.ViewModel.PortraitData.SmallPortrait;
	}

	public override bool IsValid()
	{
		if (base.IsValid())
		{
			return base.gameObject.activeInHierarchy;
		}
		return false;
	}
}
