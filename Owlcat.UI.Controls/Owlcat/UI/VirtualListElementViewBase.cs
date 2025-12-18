using System;
using R3;
using UnityEngine;

namespace Owlcat.UI;

[RequireComponent(typeof(RectTransform))]
public abstract class VirtualListElementViewBase<TViewModel> : View<TViewModel>, IVirtualListElementView where TViewModel : VirtualListElementVMBase
{
	private RectTransform m_RectTransform;

	private Vector2 m_RectSizes;

	public RectTransform RectTransform => m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>());

	public virtual VirtualListLayoutElementSettings LayoutSettings => VirtualListLayoutElementSettings.None;

	public bool NeedRebuildToGetSize => true;

	public TViewModel GetViewModel()
	{
		return base.ViewModel;
	}

	public void BindVirtualList(IVirtualListElementData data)
	{
		if (data is TViewModel source)
		{
			Bind(source);
			base.ViewModel.HasView = true;
		}
	}

	public void UnbindVirtualList()
	{
		if (base.ViewModel != null)
		{
			base.ViewModel.HasView = false;
		}
		Unbind();
	}

	protected override void OnBind()
	{
		BindViewImplementation();
	}

	protected override void OnUnbind()
	{
		DestroyViewImplementation();
	}

	protected virtual void BindViewImplementation()
	{
	}

	protected virtual void DestroyViewImplementation()
	{
	}

	protected virtual void AddDisposable(IDisposable disposable)
	{
		disposable.AddTo(this);
	}

	public IVirtualListElementView Instantiate()
	{
		return UnityEngine.Object.Instantiate(this);
	}

	public void Update()
	{
		if (LayoutSettings.OverrideType == VirtualListLayoutElementSettings.LayoutOverrideType.UnityLayout && !m_RectSizes.Equals(RectTransform.rect.size))
		{
			LayoutSettings.SetDirty();
			m_RectSizes = RectTransform.rect.size;
		}
	}
}
