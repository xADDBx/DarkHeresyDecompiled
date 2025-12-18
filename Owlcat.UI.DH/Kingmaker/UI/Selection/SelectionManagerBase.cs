using System;
using System.Collections.Generic;
using System.Linq;
using Code.Framework.Utility.UnityExtensions;
using Code.View.UI.UIUtils;
using JetBrains.Annotations;
using Kingmaker.Code.View.Bridge.Interfaces;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Selection.UnitMark;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using ObservableCollections;
using R3;
using UnityEngine;

namespace Kingmaker.UI.Selection;

public abstract class SelectionManagerBase : MonoBehaviour, ISelectionManager, INetRoleSetHandler, ISubscriber, IAreaHandler, ITurnBasedModeHandler, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, INetStopPlayingHandler
{
	[SerializeField]
	protected BaseUnitMark m_SelectionMarkPrefab;

	protected readonly List<BaseUnitMark> m_UnitMarks = new List<BaseUnitMark>();

	private readonly List<BaseUnitEntity> m_UnitsToUnselect = new List<BaseUnitEntity>();

	private static readonly Predicate<BaseUnitMark> TryDestroyMarkPredicate = TryDestroyMark;

	public static SelectionManagerBase Instance { get; protected set; }

	protected ObservableList<BaseUnitEntity> SelectedUnits => Game.Instance.Controllers.SelectionCharacter.SelectedUnits;

	protected ReactiveProperty<BaseUnitEntity> SelectedUnit => Game.Instance.Controllers.SelectionCharacter.SelectedUnit;

	protected virtual bool CanMultiSelect => true;

	public abstract void SelectUnit(UnitEntityView unit, bool single = true, bool sendSelectionEvent = true, bool ask = true);

	public void SelectAll(IEnumerable<BaseUnitEntity> characters = null)
	{
		if (characters == null)
		{
			characters = Game.Instance.EntityPools.AllBaseUnits.Where(UIUtilityUnit.IsViewActiveUnit);
		}
		IEnumerable<BaseUnitEntity> selectableUnits = GetSelectableUnits(characters);
		SelectAllImpl(selectableUnits);
	}

	protected abstract void SelectAllImpl(IEnumerable<BaseUnitEntity> units);

	public abstract void UnselectUnit(BaseUnitEntity data);

	public abstract void UpdateSelectedUnits();

	public abstract void ChangeNetRole(string entityId);

	public abstract void MultiSelect(IEnumerable<UnitEntityView> views, bool canAddToSelection = true);

	public virtual void Stop()
	{
		Game.Instance.GameCommandQueue.StopUnits(SelectedUnits);
	}

	public void Hold()
	{
		Game.Instance.GameCommandQueue.HoldUnits(SelectedUnits);
	}

	public bool IsSelected(AbstractUnitEntity unit)
	{
		if (!SelectedUnits.Contains(unit))
		{
			return SelectedUnit.Value == unit;
		}
		return true;
	}

	public BaseUnitEntity GetNearestSelectedUnit(Vector3 point)
	{
		if (SelectedUnits.Count <= 1)
		{
			return SelectedUnits.FirstOrDefault();
		}
		return SelectedUnits.Aggregate((BaseUnitEntity u1, BaseUnitEntity u2) => (!(u1.DistanceTo(point) <= u2.DistanceTo(point))) ? u2 : u1);
	}

	public void ForceCreateMarks()
	{
		UpdateUnitMarks();
	}

	protected void UpdateUnitMarks()
	{
		m_UnitMarks.RemoveAll(TryDestroyMarkPredicate);
		foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.EntityPools.AllBaseAwakeUnits)
		{
			bool flag = false;
			foreach (BaseUnitMark unitMark in m_UnitMarks)
			{
				if (unitMark.Unit == allBaseAwakeUnit)
				{
					flag = true;
					break;
				}
			}
			if (!(!allBaseAwakeUnit.View || (!allBaseAwakeUnit.IsDirectlyControllable && !allBaseAwakeUnit.Faction.IsPlayer) || flag))
			{
				BaseUnitMark baseUnitMark = UnityEngine.Object.Instantiate(m_SelectionMarkPrefab, allBaseAwakeUnit.View.ViewTransform, worldPositionStays: true);
				baseUnitMark.transform.localPosition = Vector3.zero;
				baseUnitMark.transform.localRotation = Quaternion.identity;
				baseUnitMark.Initialize(allBaseAwakeUnit);
				m_UnitMarks.Add(baseUnitMark);
			}
		}
	}

	protected void ClearUnitMarks()
	{
		foreach (BaseUnitMark item in m_UnitMarks.Where((BaseUnitMark mark) => mark))
		{
			item.Dispose();
			Utils.EditorSafeDestroy(item.gameObject);
		}
		m_UnitMarks.Clear();
	}

	private static bool TryDestroyMark(BaseUnitMark mark)
	{
		if (!mark || !mark.Unit.View || (!mark.Unit.IsDirectlyControllable && !mark.Unit.IsPlayerFaction))
		{
			if ((bool)mark)
			{
				UnityEngine.Object.Destroy(mark.gameObject);
			}
			return true;
		}
		return false;
	}

	public void RemoveSelection(BaseUnitEntity data)
	{
		if (IsSelected(data) && !SelectedUnits.All((BaseUnitEntity s) => s == data))
		{
			UnselectUnit(data);
		}
	}

	public void OnEnable()
	{
		InternalEnable();
	}

	public void OnDisable()
	{
		InternalDisable();
	}

	protected virtual void InternalEnable()
	{
		EventBus.Subscribe(this);
	}

	protected virtual void InternalDisable()
	{
		EventBus.Unsubscribe(this);
		if (Application.isPlaying)
		{
			ClearUnitMarks();
		}
	}

	public void SwitchSelectionUnitInGroup(BaseUnitEntity data, bool canAddToSelection = true, bool force = false)
	{
		if (!force && !SelectionCharacterController.CanSelectUnit)
		{
			return;
		}
		canAddToSelection = canAddToSelection && CanMultiSelect;
		canAddToSelection = canAddToSelection && MultiplySelection.Instance.ShouldMultiSelect;
		if (canAddToSelection && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
		{
			if (IsSelected(data))
			{
				RemoveSelection(data);
			}
			else
			{
				SelectUnit(data.View, single: false);
			}
		}
		else
		{
			SelectUnit(data.View);
		}
	}

	[UsedImplicitly]
	private void Awake()
	{
		Instance = this;
	}

	[UsedImplicitly]
	private void LateUpdate()
	{
		foreach (BaseUnitEntity selectedUnit in SelectedUnits)
		{
			if (selectedUnit.IsDisposed || !selectedUnit.IsDirectlyControllable())
			{
				m_UnitsToUnselect.Add(selectedUnit);
			}
		}
		foreach (BaseUnitEntity item in m_UnitsToUnselect)
		{
			UnselectUnit(item);
		}
		m_UnitsToUnselect.Clear();
		UpdateUnitMarks();
	}

	public void HandleRoleSet(string entityId)
	{
		ChangeNetRole(entityId);
	}

	void INetStopPlayingHandler.HandleStopPlaying()
	{
		if (SelectedUnits.Count == 0)
		{
			SelectAll();
		}
	}

	protected static IEnumerable<BaseUnitEntity> GetSelectableUnits(IEnumerable<BaseUnitEntity> units)
	{
		if (TurnController.IsInTurnBasedCombat() || Game.Instance.RootUIContext.IsInMainMenu || !Game.Instance.CurrentlyLoadedArea)
		{
			return Enumerable.Empty<BaseUnitEntity>();
		}
		if (Game.Instance.CurrentlyLoadedArea.IsGlobalmapArea)
		{
			units = units.Where((BaseUnitEntity u) => u.IsMainCharacter);
		}
		return units.Where((BaseUnitEntity u) => u.IsInGame && u.IsDirectlyControllable() && u.IsViewActive);
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		if (SelectedUnits.Empty())
		{
			SelectAll();
		}
	}

	public virtual void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
	}

	public virtual void HandleTurnBasedModeResumed()
	{
	}

	public virtual void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			UnselectUnit(SelectedUnit.Value);
		}
	}

	public static void SelectPrevCharacter()
	{
		SelectShiftedCharacter(-1);
	}

	public static void SelectNextCharacter()
	{
		SelectShiftedCharacter(1);
	}

	private static void SelectShiftedCharacter(int shift)
	{
		if (Instance == null || (TurnController.IsInTurnBasedCombat() && !Game.Instance.Controllers.TurnController.IsPreparationTurn))
		{
			return;
		}
		ReactiveProperty<BaseUnitEntity> selectedUnit = Game.Instance.Controllers.SelectionCharacter.SelectedUnit;
		if (!Game.Instance.Controllers.SelectionCharacter.ActualGroup.Empty())
		{
			int num = (Game.Instance.Controllers.SelectionCharacter.ActualGroup.IndexOf(selectedUnit.Value) + shift) % Game.Instance.Controllers.SelectionCharacter.ActualGroup.Count;
			if (num < 0)
			{
				num += Game.Instance.Controllers.SelectionCharacter.ActualGroup.Count;
			}
			Instance?.SelectUnit(Game.Instance.Controllers.SelectionCharacter.ActualGroup[num].View);
		}
	}
}
