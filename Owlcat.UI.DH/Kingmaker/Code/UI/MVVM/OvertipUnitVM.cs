using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.Parts.AdditionalCombat;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[UsedImplicitly]
public sealed class OvertipUnitVM : OvertipEntityVM
{
	private readonly ReactiveProperty<bool> m_HasSurrounding = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsChosen = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CombatBlocksCreated = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasActiveCombatMessage = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasCombatInteraction = new ReactiveProperty<bool>();

	private bool m_UnitScanned;

	private Transform m_Bone;

	private Bounds m_Bounds;

	public readonly MechanicEntityUIState MechanicEntityUIState;

	public readonly MechanicEntity Unit;

	public MechanicEntityUIWrapper MechanicEntityUIWrapper => MechanicEntityUIState.MechanicEntity;

	public OvertipBarkBlockVM BarkBlockVM { get; }

	public OvertipNameBlockVM NameBlockVM { get; }

	public EntityOvertipVisibilityVM VisibilityVM { get; }

	public OvertipCombatTextBlockVM CombatTextBlockVM { get; }

	public OvertipHealthBlockVM HealthBlockVM { get; private set; }

	public OvertipBuffBlockVM OvertipBuffBlockVM { get; private set; }

	public OvertipSpecialBuffBlockVM OvertipSpecialBuffBlockVM { get; private set; }

	public OvertipBuffsPredictionBlockVM OvertipBuffsPredictionBlockVM { get; private set; }

	public OvertipDamageBlockVM DamageBlockVM { get; private set; }

	public OvertipHitChanceBlockVM HitChanceBlockVM { get; private set; }

	public OvertipCoverBlockVM OvertipCoverBlockVM { get; private set; }

	public OvertipConcentrationActionVM SurfaceCombatActionVM { get; private set; }

	public OvertipMoraleVM OvertipMoraleVM { get; private set; }

	public OvertipCombatUnitInteractionVM CombatUnitInteractionVM { get; private set; }

	public float? DeathDelay { get; private set; }

	public ReadOnlyReactiveProperty<bool> IsBarkActive => BarkBlockVM.IsBarkActive;

	public ReadOnlyReactiveProperty<bool> HasSurrounding => m_HasSurrounding;

	public ReadOnlyReactiveProperty<bool> IsChosen => m_IsChosen;

	public ReadOnlyReactiveProperty<bool> CombatBlocksCreated => m_CombatBlocksCreated;

	public ReadOnlyReactiveProperty<bool> HasActiveCombatMessage => m_HasActiveCombatMessage;

	public ReadOnlyReactiveProperty<bool> HasCombatInteraction => m_HasCombatInteraction;

	public bool HideFromScreen
	{
		get
		{
			if (Unit != null)
			{
				if (Game.Instance.IsControllerGamepad)
				{
					if (Unit.IsInState)
					{
						if (!Unit.IsVisibleForPlayer)
						{
							return !Unit.IsDirectlyControllable;
						}
						return false;
					}
					return true;
				}
				if (Unit.IsInState && Unit is AbstractUnitEntity { IsAwake: not false })
				{
					if (!Unit.IsVisibleForPlayer)
					{
						return !Unit.IsDirectlyControllable;
					}
					return false;
				}
				return true;
			}
			return true;
		}
	}

	public bool IsInCameraFrustum => Unit?.IsInCameraFrustum ?? false;

	public OvertipUnitVM(AbstractUnitEntity unit)
	{
		Unit = unit;
		MechanicEntityUIState = UnitUIStateHolder.Instance.GetOrCreateUnitState(unit);
		BarkBlockVM = new OvertipBarkBlockVM().AddTo(this);
		NameBlockVM = new OvertipNameBlockVM(MechanicEntityUIState).AddTo(this);
		CombatTextBlockVM = new OvertipCombatTextBlockVM(MechanicEntityUIState, m_HasActiveCombatMessage).AddTo(this);
		MechanicEntityUIState.IsInCombat.Subscribe(CreateCombatBlocks).AddTo(this);
		VisibilityVM = new EntityOvertipVisibilityVM(MechanicEntityUIState, BarkBlockVM.IsBarkActive, m_HasActiveCombatMessage).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUpdateHandler()
	{
		base.OnUpdateHandler();
		m_HasCombatInteraction.Value = GameUIState.Instance.IsInCombat.CurrentValue && UtilityUnit.GetUnitInteractionFrom(Unit) != null;
	}

	public void ShowBark(string text)
	{
		BarkBlockVM.ShowBark(text);
	}

	public void HideBark()
	{
		BarkBlockVM.HideBark();
	}

	public void SetDeathDelay(float val)
	{
		DeathDelay = val;
	}

	public void HandleSurroundingObjectsChanged(bool moreThenOne, bool isChosen)
	{
		m_HasSurrounding.Value = moreThenOne;
		m_IsChosen.Value = isChosen;
	}

	protected override void OnDispose()
	{
		if (Unit != null)
		{
			UnitUIStateHolder.Instance.RemoveUnitState(Unit);
		}
		else
		{
			UnitUIStateHolder.Instance.RemoveUnitState(MechanicEntityUIWrapper.UniqueId);
		}
	}

	protected override Vector3 GetEntityPosition()
	{
		return GetEntityPositionInternal();
	}

	private Vector3 GetEntityPositionInternal()
	{
		if (MechanicEntityUIState == null)
		{
			return Vector3.zero;
		}
		if (!m_UnitScanned && Unit?.View != null)
		{
			m_Bone = Unit.View.ViewTransform.FindChildRecursive("UI_Overtip_Bone");
			m_UnitScanned = true;
		}
		MechanicEntityUIWrapper mechanicEntity = MechanicEntityUIState.MechanicEntity;
		if (m_Bone != null && !mechanicEntity.IsDeadOrUnconscious)
		{
			return m_Bone.position;
		}
		if (mechanicEntity.IsDeadOrUnconscious && mechanicEntity.IsDeadAndHasAttachedDroppedLoot)
		{
			return mechanicEntity.MechanicEntity.GetOptional<PartInventory>()?.AttachedDroppedLootData.Position ?? Vector3.zero;
		}
		if (MechanicEntityUIState.MechanicEntity.IsDeadOrUnconscious && Unit is BaseUnitEntity baseUnitEntity)
		{
			return baseUnitEntity.View?.CorpseOvertipPosition ?? baseUnitEntity.Position;
		}
		if (Unit is AbstractUnitEntity { Position: var position } abstractUnitEntity)
		{
			position.y += abstractUnitEntity.View?.CameraOrientedBoundsSize.y ?? Vector2.zero.y;
			return position;
		}
		return Vector3.zero;
	}

	private void CreateCombatBlocks(bool isInCombat)
	{
		if (isInCombat && !m_CombatBlocksCreated.Value)
		{
			HitChanceBlockVM = new OvertipHitChanceBlockVM(MechanicEntityUIState).AddTo(this);
			HealthBlockVM = new OvertipHealthBlockVM(MechanicEntityUIState, HitChanceBlockVM.IsVisible).AddTo(this);
			OvertipCoverBlockVM = new OvertipCoverBlockVM(MechanicEntityUIState).AddTo(this);
			UnitBuffBlockVM buffBlockVM = new UnitBuffBlockVM(Unit).AddTo(this);
			OvertipBuffBlockVM = new OvertipBuffBlockVM(Unit, buffBlockVM).AddTo(this);
			OvertipSpecialBuffBlockVM = new OvertipSpecialBuffBlockVM(Unit, buffBlockVM, OvertipCoverBlockVM).AddTo(this);
			OvertipBuffsPredictionBlockVM = new OvertipBuffsPredictionBlockVM(MechanicEntityUIState).AddTo(this);
			DamageBlockVM = new OvertipDamageBlockVM(MechanicEntityUIState).AddTo(this);
			SurfaceCombatActionVM = new OvertipConcentrationActionVM(MechanicEntityUIState).AddTo(this);
			OvertipMoraleVM = new OvertipMoraleVM(MechanicEntityUIState).AddTo(this);
			CombatUnitInteractionVM = new OvertipCombatUnitInteractionVM(MechanicEntityUIState).AddTo(this);
			m_CombatBlocksCreated.Value = true;
		}
	}
}
