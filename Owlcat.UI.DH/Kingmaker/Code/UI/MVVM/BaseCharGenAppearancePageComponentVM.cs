using Kingmaker.Blueprints.Base;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class BaseCharGenAppearancePageComponentVM : VirtualListElementVMBase, ICharGenDollStateHandler, ISubscriber
{
	private readonly ReactiveCommand<CharGenAppearancePageComponent> m_OnChanged = new ReactiveCommand<CharGenAppearancePageComponent>();

	public Observable<CharGenAppearancePageComponent> OnChanged => m_OnChanged;

	public CharGenAppearancePageComponent Type { get; set; }

	public void HandleSetGender(Gender gender, int index)
	{
		SetSelectUIGender(gender, index);
	}

	public void HandleSetHead(EquipmentEntityLink head, int index)
	{
		SetSelectUIHead(head, index);
	}

	public void HandleSetRace(BlueprintRaceVisualPreset blueprint, int index)
	{
		SetSelectUIRace(blueprint, index);
	}

	public void HandleSetSkinColor(int index)
	{
		SetSelectUISkinColor(index);
	}

	public void HandleSetHair(EquipmentEntityLink equipmentEntityLink, int index)
	{
		SetSelectUIHair(equipmentEntityLink, index);
	}

	public void HandleSetHairColor(int index)
	{
		SetSelectUIHairColor(index);
	}

	public void HandleSetEyebrows(EquipmentEntityLink equipmentEntityLink, int index)
	{
		SetUIEyebrows(equipmentEntityLink, index);
	}

	public void HandleSetEyebrowsColor(int index)
	{
		SetUIEyebrowsColor(index);
	}

	public void HandleSetBeard(EquipmentEntityLink equipmentEntityLink, int index)
	{
		SetUIBeard(equipmentEntityLink, index);
	}

	public void HandleSetBeardColor(int index)
	{
		SetUIBeardColor(index);
	}

	public void HandleSetScar(EquipmentEntityLink equipmentEntityLink, int index)
	{
		SetUIScar(equipmentEntityLink, index);
	}

	public void HandleSetTattoo(EquipmentEntityLink equipmentEntityLink, int index, int tattooTabIndex)
	{
		SetUITattoo(equipmentEntityLink, index, tattooTabIndex);
	}

	public void HandleSetTattooColor(int rampIndex, int index)
	{
		SetUITattooColor(rampIndex, index);
	}

	public void HandleSetPort(EquipmentEntityLink equipmentEntityLink, int index, int portNumber)
	{
		SetUIPort(equipmentEntityLink, index, portNumber);
	}

	public void HandleSetEquipmentColor(int primaryIndex, int secondaryIndex)
	{
	}

	void ICharGenDollStateHandler.HandleShowCloth(bool showCloth)
	{
	}

	public void Changed()
	{
		m_OnChanged.Execute(Type);
		EventBus.RaiseEvent(delegate(ICharGenAppearancePageComponentHandler h)
		{
			h.HandleComponentChanged(Type);
		});
	}

	public void Focused()
	{
		EventBus.RaiseEvent(delegate(ICharGenAppearancePageComponentHandler h)
		{
			h.HandleComponentChanged(Type);
		});
	}

	public virtual void OnBeginView()
	{
	}

	protected virtual void SetSelectUIGender(Gender gender, int index)
	{
	}

	protected virtual void SetSelectUIHead(EquipmentEntityLink head, int index)
	{
	}

	protected virtual void SetSelectUIRace(BlueprintRaceVisualPreset blueprint, int index)
	{
	}

	protected virtual void SetSelectUISkinColor(int index)
	{
	}

	protected virtual void SetSelectUIHair(EquipmentEntityLink equipmentEntityLink, int index)
	{
	}

	protected virtual void SetSelectUIHairColor(int index)
	{
	}

	protected virtual void SetUIEyebrows(EquipmentEntityLink equipmentEntityLink, int index)
	{
	}

	protected virtual void SetUIEyebrowsColor(int index)
	{
	}

	protected virtual void SetUIBeard(EquipmentEntityLink equipmentEntityLink, int index)
	{
	}

	protected virtual void SetUIBeardColor(int index)
	{
	}

	protected virtual void SetUIScar(EquipmentEntityLink equipmentEntityLink, int index)
	{
	}

	protected virtual void SetUITattoo(EquipmentEntityLink equipmentEntityLink, int index, int tattooTabIndex)
	{
	}

	protected virtual void SetUITattooColor(int rampIndex, int index)
	{
	}

	protected virtual void SetUIPort(EquipmentEntityLink equipmentEntityLink, int index, int portNumber)
	{
	}
}
