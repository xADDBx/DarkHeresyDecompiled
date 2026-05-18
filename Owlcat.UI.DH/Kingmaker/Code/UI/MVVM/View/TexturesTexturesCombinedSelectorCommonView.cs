using Kingmaker.UI.Common;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class TexturesTexturesCombinedSelectorCommonView : BaseCharGenAppearancePageComponentView<TextureTextureCombinedSelectorVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_Counter;

	[SerializeField]
	private ClickablePageNavigation m_Paginator;

	[SerializeField]
	private TextureSelectorCommonView m_FirstTextureSelectorPagedView;

	[FormerlySerializedAs("m_TextureSelectorPagedView")]
	[SerializeField]
	private TextureSelectorPagedView m_SecondTextureSelectorPagedView;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	private bool m_IsInputAdded;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.CurrentSlideSelector.Subscribe(m_FirstTextureSelectorPagedView.Bind));
		AddDisposable(base.ViewModel.CurrentTextureSelector.Subscribe(m_SecondTextureSelectorPagedView.Bind));
		AddDisposable(base.ViewModel.Title.Subscribe(SetTitleText));
		AddDisposable(base.ViewModel.TotalItems.CombineLatest(base.ViewModel.CurrentIndex, (int total, int current) => new { total, current }).Subscribe(value =>
		{
			m_Counter.text = $"{value.current + 1}/{value.total}";
		}));
		m_Paginator.Initialize(base.ViewModel.TotalItems.CurrentValue, delegate(int idx)
		{
			base.ViewModel.SetIndex(idx);
		});
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		m_LayoutSettings.SetDirty();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_IsInputAdded = false;
	}

	public void AddInput()
	{
	}

	public void SetTitleText(string title)
	{
		if (!(m_Label == null))
		{
			m_Label.gameObject.SetActive(!string.IsNullOrEmpty(title));
			m_Label.text = title;
		}
	}
}
