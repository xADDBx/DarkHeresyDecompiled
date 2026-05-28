using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Paths;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenContext : IDisposable, ICharGenDollStateHandler, ISubscriber, ICharGenTextureSelectorTabChangeHandler
{
	private readonly ReactiveProperty<bool> m_IsCustomCharacter = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<LevelUpManager> m_LevelUpManager = new ReactiveProperty<LevelUpManager>();

	private readonly CompositeDisposable m_Disposables = new CompositeDisposable();

	private readonly SerialDisposable m_NextFrameSubscription = new SerialDisposable();

	public readonly CharGenConfig CharGenConfig;

	private readonly ReactiveProperty<BaseUnitEntity> m_CurrentUnit = new ReactiveProperty<BaseUnitEntity>();

	public DollState Doll { get; private set; }

	public int CurrentTattooSet { get; private set; }

	public ReadOnlyReactiveProperty<BaseUnitEntity> CurrentUnit => m_CurrentUnit;

	public ReadOnlyReactiveProperty<bool> IsCustomCharacter => m_IsCustomCharacter;

	public ReadOnlyReactiveProperty<LevelUpManager> LevelUpManager => m_LevelUpManager;

	private BaseUnitEntity BaseChargenUnit => CharGenConfig.Unit;

	public CharGenContext(CharGenConfig config)
	{
		CharGenConfig = config;
		EventBus.Subscribe(this).AddTo(m_Disposables);
		m_NextFrameSubscription.AddTo(m_Disposables);
	}

	public void SetPregenUnit(BaseUnitEntity unit)
	{
		bool flag = unit != null;
		BaseUnitEntity charGenUnit = (flag ? unit : BaseChargenUnit);
		Doll = new DollState();
		m_IsCustomCharacter.Value = !flag;
		m_CurrentUnit.Value = charGenUnit;
		if (flag)
		{
			RaceAppearanceOverride component = charGenUnit.Blueprint.GetComponent<RaceAppearanceOverride>();
			if (component != null)
			{
				Doll.Setup(charGenUnit, component);
			}
		}
		else
		{
			Doll.SetTrackPortrait(state: true);
			m_NextFrameSubscription.Disposable = ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
			{
				Doll.UpdateMechanicsEntities(charGenUnit);
			});
		}
		BlueprintPath originPath = GetOriginPath(flag);
		if (originPath == null)
		{
			m_LevelUpManager.Value = new LevelUpManager(charGenUnit, autoCommit: false);
			return;
		}
		m_LevelUpManager.Value?.Dispose();
		m_LevelUpManager.Value = new LevelUpManager(charGenUnit, originPath, autoCommit: false);
	}

	public void EnsureDollSet()
	{
		PartUnitViewSettings viewSettings = m_LevelUpManager.Value.TargetUnit.ViewSettings;
		if (viewSettings != null && viewSettings.Doll == null)
		{
			DollData dollData = Doll?.CreateData();
			if (dollData != null)
			{
				viewSettings.SetDoll(dollData);
			}
		}
	}

	public void CommitLevelUp()
	{
		if (m_LevelUpManager.Value != null)
		{
			Game.Instance.GameCommandQueue.CommitLvlUp(m_LevelUpManager.Value);
			m_LevelUpManager.Value = null;
		}
	}

	private BlueprintPath GetOriginPath(bool isPregen)
	{
		if (CharGenConfig.Mode == CharGenMode.LevelUp)
		{
			return CurrentUnit.CurrentValue.Progression.AllCareerPaths.FirstOrDefault().Blueprint;
		}
		if (CharGenConfig.Mode == CharGenMode.NewGame)
		{
			if (!isPregen)
			{
				return BlueprintCharGenRoot.Instance.NewGameCustomChargenPath;
			}
			return BlueprintCharGenRoot.Instance.NewGamePregenChargenPath;
		}
		if (CharGenConfig.Mode == CharGenMode.NewCompanion)
		{
			return CharGenConfig.CompanionType switch
			{
				CharGenCompanionType.Common => isPregen ? BlueprintCharGenRoot.Instance.NewCompanionPregenChargenPath : BlueprintCharGenRoot.Instance.NewCompanionCustomChargenPath, 
				CharGenCompanionType.Navigator => isPregen ? BlueprintCharGenRoot.Instance.NewCompanionNavigatorPregenChargenPath : BlueprintCharGenRoot.Instance.NewCompanionNavigatorCustomChargenPath, 
				_ => null, 
			};
		}
		return null;
	}

	public void RequestSetGender(Gender gender, int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetGender(gender, index);
	}

	void ICharGenDollStateHandler.HandleSetGender(Gender gender, int index)
	{
		Doll.SetGender(gender);
	}

	public void RequestSetHead([NotNull] EquipmentEntityLink head, int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetHead(head, index);
	}

	void ICharGenDollStateHandler.HandleSetHead(EquipmentEntityLink head, int index)
	{
		Doll.SetHead(head);
	}

	public void RequestSetRace([NotNull] BlueprintRaceVisualPreset blueprint, int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetRace(blueprint, index);
	}

	void ICharGenDollStateHandler.HandleSetRace(BlueprintRaceVisualPreset blueprint, int index)
	{
		Doll.SetRacePreset(blueprint);
	}

	public void RequestSetSkinColor(int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetSkinColor(index);
	}

	void ICharGenDollStateHandler.HandleSetSkinColor(int index)
	{
		Doll.SetSkinColor(index);
	}

	public void RequestSetHair([NotNull] EquipmentEntityLink equipmentEntityLink, int index)
	{
		Doll.SetHair(equipmentEntityLink);
		Game.Instance.GameCommandQueue.CharGenSetHair(equipmentEntityLink, index);
	}

	void ICharGenDollStateHandler.HandleSetHair(EquipmentEntityLink equipmentEntityLink, int index)
	{
		Doll.SetHair(equipmentEntityLink);
	}

	public void RequestSetHairColor(int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetHairColor(index);
	}

	void ICharGenDollStateHandler.HandleSetHairColor(int index)
	{
		Doll.SetHairColor(index);
	}

	public void RequestSetEyebrows([NotNull] EquipmentEntityLink equipmentEntityLink, int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetEyebrows(equipmentEntityLink, index);
	}

	void ICharGenDollStateHandler.HandleSetEyebrows(EquipmentEntityLink equipmentEntityLink, int index)
	{
		Doll.SetEyebrows(equipmentEntityLink);
	}

	public void RequestSetEyebrowsColor(int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetEyebrowsColor(index);
	}

	void ICharGenDollStateHandler.HandleSetEyebrowsColor(int index)
	{
		Doll.SetEyebrowsColor(index);
	}

	public void RequestSetBeard([NotNull] EquipmentEntityLink equipmentEntityLink, int index)
	{
		Doll.SetBeard(equipmentEntityLink);
		Game.Instance.GameCommandQueue.CharGenSetBeard(equipmentEntityLink, index);
	}

	void ICharGenDollStateHandler.HandleSetBeard(EquipmentEntityLink equipmentEntityLink, int index)
	{
		Doll.SetBeard(equipmentEntityLink);
	}

	public void RequestSetBeardColor(int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetBeardColor(index);
	}

	void ICharGenDollStateHandler.HandleSetBeardColor(int index)
	{
		Doll.SetBeardColor(index);
	}

	public void RequestSetScar([NotNull] EquipmentEntityLink equipmentEntityLink, int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetScar(equipmentEntityLink, index);
	}

	void ICharGenDollStateHandler.HandleSetScar(EquipmentEntityLink equipmentEntityLink, int index)
	{
		Doll.SetScar(equipmentEntityLink);
	}

	public void RequestSetTattoo([NotNull] EquipmentEntityLink equipmentEntityLink, int index, int tattooTabIndex)
	{
		Game.Instance.GameCommandQueue.CharGenSetTattoo(equipmentEntityLink, index, tattooTabIndex);
	}

	void ICharGenDollStateHandler.HandleSetTattoo(EquipmentEntityLink equipmentEntityLink, int index, int tattooTabIndex)
	{
		CurrentTattooSet = tattooTabIndex;
		Doll.SetTattoo(equipmentEntityLink, tattooTabIndex);
		EventBus.RaiseEvent(delegate(ICharGenAppearanceComponentUpdateHandler h)
		{
			h.HandleAppearanceComponentUpdate(CharGenAppearancePageComponent.Tattoo);
		});
	}

	public void RequestSetTattooColor(int rampIndex, int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetTattooColor(rampIndex, index);
	}

	void ICharGenDollStateHandler.HandleSetTattooColor(int rampIndex, int index)
	{
		Doll.SetTattooColor(rampIndex, index);
	}

	public void RequestSetPort([NotNull] EquipmentEntityLink equipmentEntityLink, int index, int portNumber)
	{
		Game.Instance.GameCommandQueue.CharGenSetPort(equipmentEntityLink, index, portNumber);
	}

	void ICharGenDollStateHandler.HandleSetPort(EquipmentEntityLink equipmentEntityLink, int index, int portNumber)
	{
		Doll.SetPort(equipmentEntityLink, portNumber);
	}

	void ICharGenDollStateHandler.HandleSetEquipmentColor(int primaryIndex, int secondaryIndex)
	{
		Doll.SetEquipColors(primaryIndex, secondaryIndex);
	}

	void ICharGenDollStateHandler.HandleShowCloth(bool showCloth)
	{
		Doll.ShowCloth = showCloth;
	}

	public void Dispose()
	{
		LevelUpManager.CurrentValue?.Dispose();
		m_Disposables.Dispose();
	}

	public void HandleTextureSelectorTabChange(CharGenAppearancePageComponent type, int tabIndex)
	{
		if (type == CharGenAppearancePageComponent.Tattoo)
		{
			CurrentTattooSet = tabIndex;
		}
	}
}
