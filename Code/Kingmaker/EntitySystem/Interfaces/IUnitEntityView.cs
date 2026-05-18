using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.View;
using Kingmaker.View.Equipment;
using Kingmaker.Visual;
using Kingmaker.Visual.CharacterSystem;
using UnityEngine;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IUnitEntityView : IAbstractUnitEntityView, IMechanicEntityView, IEntityView
{
	UnitViewHandsEquipment HandsEquipment { get; }

	Character CharacterAvatar { get; }

	UnitHitFxManager HitFxManager { get; }

	Vector3 CorpseOvertipPosition { get; }

	bool MouseHoverHighlighting { get; set; }

	bool SecondaryHighlighting { get; set; }

	bool DoNotAdjustScale { get; }

	bool IsHighlighted { get; }

	FogOfWarRevealerSettings FogOfWarRevealer { get; }

	bool IsCommandsPreventMovement { get; }

	UnitViewMechadendritesEquipment MechadendritesEquipment { get; }

	float GetSizeScale();

	void MarkRenderersAndCollidersAreUpdated();

	void HandleHoverChange(bool value);

	void UpdateAsks();

	FogOfWarRevealerSettings SureFogOfWarRevealer();

	void HideOffWeapon(bool value);

	void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem);

	void OnMovementStarted(Vector3 pathDestination, bool preview = false);

	void TryShowPointer(Vector3 pathDestination, bool preview = false);

	bool IsMoving();

	void UpdateEquipmentColor();

	void UpdateCombatSwitch();

	T GetComponent<T>();

	T GetComponentInChildren<T>();
}
