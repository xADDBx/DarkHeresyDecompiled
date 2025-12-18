using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Common.PageNavigation;
using Kingmaker.Utility.DotNetExtensions;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class TextureSelectorPagedGroupView : TextureSelectorGroupView
{
	[SerializeField]
	protected Paginator m_Paginator;

	[SerializeField]
	private LayoutElement m_LayoutElement;

	[SerializeField]
	private int m_RowNumber = 1;

	[SerializeField]
	private float m_RowHeight = 43f;

	public void OnValidate()
	{
	}

	protected override void OnBind()
	{
		m_Paginator.AddTo(this);
		SetRowNumber(m_RowNumber);
		base.ViewModel.SelectedEntity.Subscribe(delegate
		{
			UpdatePageIndex();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Paginator.UpdateViewTrigger, delegate
		{
			UpdatePageIndex();
		}).AddTo(this);
		base.OnBind();
	}

	public void SetRowNumber(int n)
	{
		m_LayoutElement.minHeight = (float)n * m_RowHeight;
		m_LayoutElement.preferredHeight = (float)n * m_RowHeight;
	}

	protected override void DrawEntities()
	{
		base.DrawEntities();
		m_Paginator.UpdateView();
	}

	private void UpdatePageIndex()
	{
		int num = base.ViewModel.EntitiesCollection.FindIndex((TextureSelectorItemVM e) => e == base.ViewModel.SelectedEntity.Value);
		if (num == -1)
		{
			base.ViewModel.SelectedEntity.Value = base.ViewModel.EntitiesCollection.FirstItem();
		}
		else
		{
			m_Paginator.SetPageIndex(num / m_ItemsPerRow);
		}
	}

	public override bool HandleUp()
	{
		return false;
	}

	public override bool HandleDown()
	{
		return false;
	}

	public override bool HandleLeft()
	{
		if (!UtilityNet.IsControlMainCharacter())
		{
			return false;
		}
		return base.ViewModel.SelectPrevValidEntity();
	}

	public override bool HandleRight()
	{
		if (!UtilityNet.IsControlMainCharacter())
		{
			return false;
		}
		return base.ViewModel.SelectNextValidEntity();
	}

	public override void SetFocus(bool value)
	{
	}

	public override bool CanConfirmClick()
	{
		return false;
	}

	public override void OnConfirmClick()
	{
	}
}
