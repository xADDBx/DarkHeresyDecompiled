using System;
using System.Collections.Generic;
using System.Linq;
using Code.Enums;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UnitLogic.Buffs.Components;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoBuffBlockView : View<UnitBuffBlockVM>
{
	[SerializeField]
	private BuffPCView m_BuffView;

	[SerializeField]
	private UnitInfoBuffBlockGroupView[] m_Groups;

	[SerializeField]
	private BuffsGroupWidget m_CritGroup;

	[SerializeField]
	private DOTGroupWidget m_DOTGroup;

	private readonly List<BuffPCView> m_BuffList = new List<BuffPCView>();

	public bool HasBuffs => m_BuffList.Any();

	private void Awake()
	{
		UnitInfoBuffBlockGroupView[] groups = m_Groups;
		foreach (UnitInfoBuffBlockGroupView unitInfoBuffBlockGroupView in groups)
		{
			unitInfoBuffBlockGroupView.SetHeader(GetGroupHeader(unitInfoBuffBlockGroupView.Group));
		}
	}

	private string GetGroupHeader(BuffGroupType group)
	{
		return group switch
		{
			BuffGroupType.Positive => UIStrings.Instance.Inspect.EffectsPositive.Text, 
			BuffGroupType.Negative => UIStrings.Instance.Inspect.EffectsNegative.Text, 
			BuffGroupType.DOT => UIStrings.Instance.Inspect.EffectsDOT.Text, 
			BuffGroupType.CriticalEffect => UIStrings.Instance.Inspect.EffectsCritical.Text, 
			_ => throw new ArgumentOutOfRangeException("group", group, null), 
		};
	}

	protected override void OnBind()
	{
		DrawBuffs();
		base.ViewModel.Buffs.ObserveAdd().Subscribe(delegate
		{
			DrawBuffs();
		}).AddTo(this);
		base.ViewModel.Buffs.ObserveRemove().Subscribe(delegate
		{
			DrawBuffs();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.Buffs.ObserveReset(), delegate
		{
			Clear();
		}).AddTo(this);
		base.ViewModel.CriticalEffects.Subscribe(HandleCriticalEffectsChanged).AddTo(this);
		base.ViewModel.DOTEffects.Subscribe(HandleDOTEffectsChanged).AddTo(this);
	}

	private void HandleCriticalEffectsChanged(CriticalEffectsUIData data)
	{
		m_CritGroup.SetActive(data.Count > 0);
		m_CritGroup.SetActiveLayer(GetLayerName(data.Count, data.HighestRank));
		static string GetLayerName(int count, int rank)
		{
			if (count == 1)
			{
				return $"Single_{rank}";
			}
			return $"Multiple_{rank}";
		}
	}

	private void HandleDOTEffectsChanged(DOTEffectsUIData data)
	{
		int count = data.DotEffects.Count;
		m_DOTGroup.SetEffectsCount(count);
		if (count == 1)
		{
			DOT item = data.DotEffects.First().dotType;
			m_DOTGroup.SetActiveLayerSingle(item.ToString());
			return;
		}
		int num = 0;
		foreach (var dotEffect in data.DotEffects)
		{
			DOT item2 = dotEffect.dotType;
			if (num >= m_DOTGroup.MaxEffectsCount)
			{
				break;
			}
			m_DOTGroup.SetActiveLayerMultiple(item2.ToString(), num);
			num++;
		}
	}

	private void DrawBuffs()
	{
		base.ViewModel.SortBuffs();
		DrawBuffsInternal(base.ViewModel.Buffs.ToList(), m_BuffList);
		int num = m_Groups.Length;
		for (int i = 0; i < num; i++)
		{
			UnitInfoBuffBlockGroupView groupView = m_Groups[i];
			groupView.SetActive(m_BuffList.Any((BuffPCView b) => b.ViewModel.Group == groupView.Group));
			groupView.SetSeparator(i + 1 < num);
		}
	}

	private void DrawBuffsInternal(List<BuffVM> buffs, List<BuffPCView> views)
	{
		for (int i = 0; i < views.Count; i++)
		{
			BuffVM viewModel = views[i].ViewModel;
			bool flag = false;
			foreach (BuffVM buff in buffs)
			{
				if (buff == viewModel)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				WidgetFactory.DisposeWidget(views[i]);
				views.RemoveAt(i);
				i--;
			}
		}
		for (int j = 0; j < buffs.Count; j++)
		{
			BuffVM buffVM = buffs[j];
			bool flag2 = false;
			foreach (BuffPCView view in views)
			{
				if (view.ViewModel == buffVM)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				BuffPCView widget = WidgetFactory.GetWidget(m_BuffView);
				widget.Bind(buffVM);
				RectTransform buffParent = GetBuffParent(buffVM);
				widget.transform.SetParent(buffParent, worldPositionStays: false);
				views.Add(widget);
			}
		}
	}

	private RectTransform GetBuffParent(BuffVM vm)
	{
		return m_Groups.FirstOrDefault((UnitInfoBuffBlockGroupView g) => g.Group == vm.Group)?.Container;
	}

	private void Clear()
	{
		m_BuffList.ForEach(WidgetFactory.DisposeWidget);
		m_BuffList.Clear();
	}
}
