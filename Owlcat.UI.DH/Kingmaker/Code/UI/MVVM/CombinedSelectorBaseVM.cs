using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CombinedSelectorBaseVM<TFirstVM, TSecondVM> : BaseCharGenAppearancePageComponentVM where TFirstVM : BaseCharGenAppearancePageComponentVM where TSecondVM : BaseCharGenAppearancePageComponentVM
{
	private readonly ReactiveProperty<int> m_CurrentIndex = new ReactiveProperty<int>();

	private readonly ReactiveProperty<TFirstVM> m_CurrentSlideSelector = new ReactiveProperty<TFirstVM>();

	private readonly ReactiveProperty<TSecondVM> m_CurrentTextureSelector = new ReactiveProperty<TSecondVM>();

	private readonly AutoDisposingList<TFirstVM> m_SlideSequentialSelectorVms = new AutoDisposingList<TFirstVM>();

	private readonly AutoDisposingList<TSecondVM> m_TextureSelectorVms = new AutoDisposingList<TSecondVM>();

	private readonly ReactiveProperty<string> m_Title = new ReactiveProperty<string>();

	private readonly ReactiveCommand<Unit> m_OnSetValues = new ReactiveCommand<Unit>();

	private readonly ReactiveProperty<int> m_TotalItems = new ReactiveProperty<int>();

	public ReadOnlyReactiveProperty<int> CurrentIndex => m_CurrentIndex;

	public ReadOnlyReactiveProperty<TFirstVM> CurrentSlideSelector => m_CurrentSlideSelector;

	public ReadOnlyReactiveProperty<TSecondVM> CurrentTextureSelector => m_CurrentTextureSelector;

	public Observable<Unit> OnSetValues => m_OnSetValues;

	public ReadOnlyReactiveProperty<int> TotalItems => m_TotalItems;

	public ReadOnlyReactiveProperty<string> Title => m_Title;

	public override void CaptureDefaults()
	{
		m_DefaultIndex = m_CurrentIndex.Value;
		m_SlideSequentialSelectorVms.ForEach(delegate(TFirstVM vm)
		{
			vm.CaptureDefaults();
		});
		m_TextureSelectorVms.ForEach(delegate(TSecondVM vm)
		{
			vm.CaptureDefaults();
		});
	}

	public override void Randomize()
	{
		m_SlideSequentialSelectorVms.ForEach(delegate(TFirstVM vm)
		{
			vm.Randomize();
		});
		m_TextureSelectorVms.ForEach(delegate(TSecondVM vm)
		{
			vm.Randomize();
		});
	}

	public override void ResetToDefault()
	{
		if (m_DefaultIndex >= 0 && m_DefaultIndex < m_SlideSequentialSelectorVms.Count && m_DefaultIndex < m_TextureSelectorVms.Count)
		{
			SetIndex(m_DefaultIndex);
		}
		m_SlideSequentialSelectorVms.ForEach(delegate(TFirstVM vm)
		{
			vm.ResetToDefault();
		});
		m_TextureSelectorVms.ForEach(delegate(TSecondVM vm)
		{
			vm.ResetToDefault();
		});
	}

	protected override void DisposeImplementation()
	{
		Clear();
	}

	private void Clear()
	{
		m_SlideSequentialSelectorVms.Clear();
		m_TextureSelectorVms.Clear();
	}

	public void SetTitle(string title)
	{
		m_Title.Value = title;
	}

	public void SetValues(IEnumerable<TFirstVM> slideSelectors, IEnumerable<TSecondVM> textureSelectors)
	{
		if (slideSelectors.Any())
		{
			m_SlideSequentialSelectorVms.Clear();
			foreach (TFirstVM slideSelector in slideSelectors)
			{
				m_SlideSequentialSelectorVms.Add(slideSelector);
				AddDisposable(slideSelector.OnChanged.Subscribe(delegate
				{
					Changed();
				}));
			}
		}
		if (textureSelectors.Any())
		{
			m_TextureSelectorVms.Clear();
			foreach (TSecondVM textureSelector in textureSelectors)
			{
				m_TextureSelectorVms.Add(textureSelector);
				AddDisposable(textureSelector.OnChanged.Subscribe(delegate
				{
					Changed();
				}));
			}
		}
		m_TotalItems.Value = m_SlideSequentialSelectorVms.Count;
		SetIndex((CurrentIndex.CurrentValue < TotalItems.CurrentValue) ? CurrentIndex.CurrentValue : 0);
		m_OnSetValues.Execute(Unit.Default);
	}

	public void SetIndex(int index)
	{
		if (index < 0 || index >= m_SlideSequentialSelectorVms.Count || index >= m_TextureSelectorVms.Count)
		{
			throw new ArgumentOutOfRangeException();
		}
		m_CurrentSlideSelector.Value = m_SlideSequentialSelectorVms[index];
		m_CurrentTextureSelector.Value = m_TextureSelectorVms[index];
		m_CurrentIndex.Value = index;
	}

	public void SetNextIndex()
	{
		SetIndex((CurrentIndex.CurrentValue + 1) % m_SlideSequentialSelectorVms.Count);
	}
}
