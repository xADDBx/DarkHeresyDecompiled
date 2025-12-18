using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.MVVM.ServiceWindows.AbilityModification;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Code.View.UI.MVVM;

public class CharInfoAbilitiesTabView : CharInfoComponentView<CharInfoAbilitiesTabVM>
{
	private readonly Dictionary<int, Vector2> ABILITY_GRID_SPICING = new Dictionary<int, Vector2>
	{
		{
			0,
			new Vector2(0f, 0f)
		},
		{
			1,
			new Vector2(0f, 0f)
		},
		{
			2,
			new Vector2(90f, 90f)
		},
		{
			3,
			new Vector2(90f, 90f)
		},
		{
			4,
			new Vector2(10f, 40f)
		},
		{
			5,
			new Vector2(10f, 40f)
		}
	};

	private const string FILTER_BUTTON_ON = "On";

	private const string FILTER_BUTTON_OFF = "Off";

	[SerializeField]
	protected ModificationSlotPCView m_ModificationSlot;

	[SerializeField]
	protected AbilitySlotPCView m_AbilitySlot;

	[SerializeField]
	protected WidgetList m_AttackAbilitiesList;

	[SerializeField]
	protected WidgetList m_AbilitiesList;

	[SerializeField]
	protected GridLayoutGroup m_AbilitiesGrid;

	[SerializeField]
	protected WidgetList m_ModificationsList;

	[SerializeField]
	private ActionBarPartConsumablesPCView m_ConsumablesView;

	[SerializeField]
	private ActionBarPartWeaponsPCView m_WeaponsView;

	[SerializeField]
	private ActionBarPartAbilitiesPCView m_AbilitiesView;

	[SerializeField]
	private ModificationSlotPCView m_ModifierDraggableSlot;

	[SerializeField]
	protected TextMeshProUGUI m_ModificationTitle;

	[SerializeField]
	protected OwlcatMultiButton m_FilterButton;

	private RectTransform m_RectTransform;

	public override void Initialize()
	{
		base.Initialize();
		m_ConsumablesView.Initialize();
		m_WeaponsView.Initialize();
		m_AbilitiesView.Initialize();
		m_RectTransform = base.transform as RectTransform;
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.AbilitiesVMCollection.Subscribe(DrawAbilities).AddTo(this);
		base.ViewModel.AttackAbilitiesVMCollection.Subscribe(DrawAttackAbilities).AddTo(this);
		base.ViewModel.ModificationsVMCollection.Subscribe(DrawModifiers).AddTo(this);
		base.ViewModel.ModifierFilterActive.Subscribe(delegate(bool a)
		{
			m_FilterButton.SetActiveLayer(a ? "On" : "Off");
		}).AddTo(this);
		base.ViewModel.SelectedAbilitySlot.Subscribe(OnAbilitySelected).AddTo(this);
		base.ViewModel.ModifierDragData.Subscribe(OnDragModifier).AddTo(this);
		m_ConsumablesView.Bind(base.ViewModel.Consumables);
		m_WeaponsView.Bind(base.ViewModel.Weapons);
		m_AbilitiesView.Bind(base.ViewModel.Abilities);
		ObservableSubscribeExtensions.Subscribe(m_FilterButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ToggleModifierFilter();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_ConsumablesView.Unbind();
		m_WeaponsView.Unbind();
		m_AbilitiesView.Unbind();
	}

	private void OnDragModifier(PointerEventData data)
	{
		if (data == null || !data.dragging)
		{
			m_ModifierDraggableSlot.gameObject.SetActive(value: false);
			m_ModifierDraggableSlot.Bind(null);
			return;
		}
		if (!m_ModifierDraggableSlot.gameObject.activeSelf)
		{
			m_ModifierDraggableSlot.Bind(base.ViewModel.SelectedModifierSlot.CurrentValue);
		}
		RectTransformUtility.ScreenPointToLocalPointInRectangle(UIDollRooms.Instance.RectTransform ? UIDollRooms.Instance.RectTransform : m_RectTransform, data.position, UICamera.Instance, out var localPoint);
		m_ModifierDraggableSlot.transform.localPosition = localPoint;
	}

	private void OnAbilitySelected(AbilitySlotVM slot)
	{
		m_ModificationTitle.text = ((slot == null) ? UIStrings.Instance.AbilityModifications.ModificationsLabel : UIStrings.Instance.AbilityModifications.BindModificationsButtonLabel);
		m_FilterButton.gameObject.SetActive(slot != null);
	}

	private void DrawAbilities(List<AbilitySlotVM> list)
	{
		m_AbilitiesList.Clear();
		m_AbilitiesList.DrawEntries(list, m_AbilitySlot);
		int num = Mathf.CeilToInt(Mathf.Sqrt(list.Count));
		Vector2 spacing = ABILITY_GRID_SPICING[num];
		m_AbilitiesGrid.spacing = spacing;
		m_AbilitiesGrid.constraintCount = num;
	}

	private void DrawAttackAbilities(List<AbilitySlotVM> list)
	{
		m_AttackAbilitiesList.Clear();
		m_AttackAbilitiesList.DrawEntries(list, m_AbilitySlot);
	}

	private void DrawModifiers(List<ModificationSlotVM> list)
	{
		m_ModificationsList.Clear();
		m_ModificationsList.DrawEntries(list, m_ModificationSlot);
	}
}
