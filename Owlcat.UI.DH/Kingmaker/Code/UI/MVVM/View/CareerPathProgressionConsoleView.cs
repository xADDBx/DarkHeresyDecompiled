using System.Collections.Generic;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CareerPathProgressionConsoleView : CareerPathProgressionCommonView, ICharInfoCanHookConfirm
{
	[Header("Console")]
	[SerializeField]
	protected CharInfoSkillsAndWeaponsConsoleView m_SkillsAndWeaponsView;

	[SerializeField]
	private HintView m_ExpandHint;

	private readonly ReactiveProperty<bool> m_CanReset = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanConfirm = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsInScreenNavigation = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsOnCareerItem = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CanHideUnavailable = new ReactiveProperty<bool>();

	private readonly CompositeDisposable m_DelayedInvokes = new CompositeDisposable();

	private readonly CompositeDisposable m_InfoDisposables = new CompositeDisposable();

	private readonly CompositeDisposable m_Disposable = new CompositeDisposable();

	private bool m_HasTooltip;

	private bool m_ShowTooltip = true;

	private TooltipConfig m_TooltipConfig;

	private IConsoleEntity m_ContentEntity;

	private CareerPathSelectionTabsConsoleView CareerPathSelectionTabsConsoleView => m_CareerPathSelectionPartCommonView as CareerPathSelectionTabsConsoleView;

	protected override void OnBind()
	{
		base.OnBind();
		m_TooltipConfig = new TooltipConfig
		{
			TooltipPlace = m_InfoSection.GetComponent<RectTransform>(),
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		};
		m_IsShown.And(base.ViewModel.HasNewValidSelections).Subscribe(delegate(bool value)
		{
			m_CanReset.Value = value;
		}).AddTo(this);
		base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Subscribe(delegate(IRankEntrySelectItem value)
		{
			if (value != null)
			{
				value.HasUnavailableFeatures.Subscribe(delegate(bool hasUnavailable)
				{
					m_CanHideUnavailable.Value = hasUnavailable;
				}).AddTo(this);
			}
			else
			{
				m_CanHideUnavailable.Value = false;
			}
		}).AddTo(this);
		base.ViewModel.UnitProgressionVM.OnRepeatedCurrentRankEntryItem.Subscribe(delegate
		{
			SwitchDescriptionShowed(true);
		}).AddTo(this);
		base.ViewModel.IsDescriptionShowed.Subscribe(delegate(bool value)
		{
			EventBus.RaiseEvent(delegate(ISetCharInfoUnitPanelState h)
			{
				h.SetUnitPanelNavigationState(!value);
			});
		}).AddTo(this);
		base.ViewModel.IsDescriptionShowed.Skip(1).Subscribe(delegate
		{
			EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
			{
				h.HandleFocus();
			});
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_DelayedInvokes?.Clear();
		m_InfoDisposables?.Clear();
		m_Disposable?.Clear();
	}

	public void AddInput()
	{
	}

	private void CreateInformationBind()
	{
	}

	private void CreateInfoTooltipInputLayer()
	{
	}

	private void Scroll(float value)
	{
		m_InfoSection.Scroll(value);
	}

	public ReadOnlyReactiveProperty<bool> GetCanHookConfirmProperty()
	{
		return m_CanConfirm.And(m_IsInScreenNavigation).And(m_IsOnCareerItem.And(base.ViewModel.CanCommit).And(base.ViewModel.AllVisited).Not()).ToReadOnlyReactiveProperty(initialValue: false);
	}
}
