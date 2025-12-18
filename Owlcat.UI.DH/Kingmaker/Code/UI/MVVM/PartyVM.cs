using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Selection;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class PartyVM : ViewModel, IBarkHandler, ISubscriber<IEntity>, ISubscriber
{
	private int m_StartIndex;

	private List<BaseUnitEntity> m_ActualGroupCopy = new List<BaseUnitEntity>();

	private readonly ReactiveProperty<bool> m_NextEnable = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_PrevEnable = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_AllSelected = new ReactiveProperty<bool>(value: false);

	public readonly List<PartyCharacterVM> CharactersVM = new List<PartyCharacterVM>();

	private List<BaseUnitEntity> ActualGroup => Game.Instance.Controllers.SelectionCharacter.ActualGroup;

	private int StartIndex
	{
		get
		{
			return m_StartIndex;
		}
		set
		{
			int num = ActualGroup.Count - 6;
			if (num < 0)
			{
				num = 0;
			}
			value = Mathf.Clamp(value, 0, num);
			m_StartIndex = value;
			m_PrevEnable.Value = value > 0;
			m_NextEnable.Value = ActualGroup.Count > m_StartIndex + 6;
			for (int i = 0; i < CharactersVM.Count; i++)
			{
				int num2 = m_StartIndex + i;
				CharactersVM[i].SetUnitData((ActualGroup.Count > num2) ? ActualGroup[num2] : null);
			}
		}
	}

	public ReadOnlyReactiveProperty<bool> NextEnable => m_NextEnable;

	public ReadOnlyReactiveProperty<bool> PrevEnable => m_PrevEnable;

	public ReadOnlyReactiveProperty<bool> AllSelected => m_AllSelected;

	public PartyVM()
	{
		for (int i = 0; i < 6; i++)
		{
			CharactersVM.Add(new PartyCharacterVM(NextPrev, i));
		}
		ObservableSubscribeExtensions.Subscribe(Game.Instance.Controllers.SelectionCharacter.ActualGroupUpdated, delegate
		{
			m_AllSelected.Value = ActualGroup.All((BaseUnitEntity u) => u.IsSelected);
			if (!m_ActualGroupCopy.SequenceEqual(ActualGroup))
			{
				SetGroup();
				m_ActualGroupCopy = new List<BaseUnitEntity>(ActualGroup);
			}
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		Disposable.Create(DisposeImplementation).AddTo(this);
	}

	private void DisposeImplementation()
	{
		CharactersVM.ForEach(delegate(PartyCharacterVM vm)
		{
			vm.Dispose();
		});
		CharactersVM.Clear();
	}

	private void SetGroup()
	{
		StartIndex = 0;
	}

	private void NextPrev(bool dir)
	{
		if (dir)
		{
			Next();
		}
		else
		{
			Prev();
		}
	}

	public void Next()
	{
		StartIndex++;
	}

	public void Prev()
	{
		StartIndex--;
	}

	public void SelectNeighbour(bool next)
	{
		GetNeighbour(next)?.HandleUnitClick(isDoubleClick: true);
	}

	private PartyCharacterVM GetNeighbour(bool next)
	{
		int num = CharactersVM.FindIndex((PartyCharacterVM c) => c.IsSingleSelected.CurrentValue);
		if (num == -1)
		{
			return null;
		}
		int neighbourIndex = GetNeighbourIndex(num, CharactersVM.Count, next);
		while (neighbourIndex != num && !CharactersVM[neighbourIndex].UnitEntityData.IsDirectlyControllable())
		{
			neighbourIndex = GetNeighbourIndex(neighbourIndex, CharactersVM.Count, next);
		}
		return CharactersVM[neighbourIndex];
	}

	private static int GetNeighbourIndex(int current, int total, bool next)
	{
		return (current + total + (next ? 1 : (-1))) % total;
	}

	public void SwitchCharacter(bool next)
	{
		PartyCharacterVM partyCharacterVM = CharactersVM.FirstItem((PartyCharacterVM c) => c.IsSingleSelected.CurrentValue);
		if (partyCharacterVM != null)
		{
			PartyCharacterVM neighbour = GetNeighbour(next);
			if (partyCharacterVM != neighbour && neighbour != null)
			{
				SwitchCharacter(partyCharacterVM.UnitEntityData, neighbour.UnitEntityData);
			}
		}
	}

	public void SelectPrevCharacter()
	{
		SelectShiftedCharacter(-1);
	}

	public void SelectNextCharacter()
	{
		SelectShiftedCharacter(1);
	}

	public void SelectFirstCharacter()
	{
		if (!(SelectionManagerBase.Instance == null) && (!TurnController.IsInTurnBasedCombat() || Game.Instance.Controllers.TurnController.IsPreparationTurn))
		{
			SelectionManagerBase.Instance.SelectUnit(Game.Instance.Controllers.SelectionCharacter.ActualGroup.FirstOrDefault()?.View);
		}
	}

	private void SelectShiftedCharacter(int shift)
	{
		if (SelectionManagerBase.Instance == null || (TurnController.IsInTurnBasedCombat() && !Game.Instance.Controllers.TurnController.IsPreparationTurn))
		{
			return;
		}
		ReactiveProperty<BaseUnitEntity> selectedUnit = Game.Instance.Controllers.SelectionCharacter.SelectedUnit;
		List<BaseUnitEntity> list = GetSelectableUnits(Game.Instance.Controllers.SelectionCharacter.ActualGroup).ToList();
		if (!list.Empty())
		{
			int num = (list.IndexOf(selectedUnit.Value) + shift) % list.Count;
			if (num < 0)
			{
				num += list.Count;
			}
			SelectionManagerBase.Instance.SelectUnit(list[num].View);
		}
	}

	private static IEnumerable<BaseUnitEntity> GetSelectableUnits(IEnumerable<BaseUnitEntity> units)
	{
		if (TurnController.IsInTurnBasedCombat() && !Game.Instance.Controllers.TurnController.IsPreparationTurn)
		{
			return Enumerable.Empty<BaseUnitEntity>();
		}
		if (Game.Instance.CurrentlyLoadedArea.IsGlobalmapArea)
		{
			units = units.Where((BaseUnitEntity u) => u.IsMainCharacter);
		}
		return units.Where((BaseUnitEntity u) => u.IsInGame && u.IsDirectlyControllable());
	}

	public void SwitchCharacter(BaseUnitEntity unit1, BaseUnitEntity unit2)
	{
		Game.Instance.GameCommandQueue.AddCommand(new SwitchPartyCharactersGameCommand(unit1, unit2));
	}

	public void HandleOnShowBark(string text)
	{
		Entity entity = EventInvokerExtensions.Entity;
		CharactersVM.FindOrDefault((PartyCharacterVM o) => o.UnitEntityData == entity)?.ShowBark(text);
	}

	public void HandleOnShowLinkedBark(string text, string encyclopediaLink)
	{
	}

	public void HandleOnHideBark()
	{
		Entity entity = EventInvokerExtensions.Entity;
		CharactersVM.FindOrDefault((PartyCharacterVM o) => o.UnitEntityData == entity)?.HideBark();
	}

	public void SetMassLink()
	{
		SelectionManagerConsole.Instance.SetMassLink((from c in CharactersVM
			where c.UnitEntityData.IsDirectlyControllable()
			select c.UnitEntityData).ToList());
		CharactersVM.Where((PartyCharacterVM c) => c.UnitEntityData.IsDirectlyControllable()).ForEach(delegate(PartyCharacterVM c)
		{
			c.UpdateLink();
		});
	}

	public void UpdateConsoleGroup()
	{
		for (int i = CharactersVM.Count; i < ActualGroup.Count; i++)
		{
			CharactersVM.Add(new PartyCharacterVM(NextPrev, i));
			CharactersVM[i].SetUnitData(ActualGroup[i]);
		}
	}
}
