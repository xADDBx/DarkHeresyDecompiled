using Kingmaker.EntitySystem.Entities;
using Kingmaker.Formations;
using Kingmaker.GameCommands;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class FormationCharacterVM : ViewModel
{
	public readonly Sprite PortraitSprite;

	private readonly ReactiveCommand<Unit> m_FormationUpdated = new ReactiveCommand<Unit>();

	private readonly ReactiveProperty<bool> m_IsInteractable = new ReactiveProperty<bool>();

	private readonly int m_Index;

	private readonly BaseUnitEntity m_Unit;

	public readonly Vector2 OffsetPosition = new Vector2(0f, 138f);

	public Observable<Unit> FormationUpdated => m_FormationUpdated;

	public ReadOnlyReactiveProperty<bool> IsInteractable => m_IsInteractable;

	public FormationCharacterVM(int index, BaseUnitEntity unit, ReactiveCommand<Unit> formationUpdated)
	{
		m_Index = index;
		m_Unit = unit;
		m_FormationUpdated = formationUpdated;
		PortraitSprite = unit.Portrait.SmallPortrait;
		SetupCharacter();
		ObservableSubscribeExtensions.Subscribe(formationUpdated, delegate
		{
			SetupCharacter();
		}).AddTo(this);
	}

	private void SetupCharacter()
	{
		PartyFormationManager formationManager = Game.Instance.Player.FormationManager;
		m_IsInteractable.Value = formationManager.IsCustomFormation;
	}

	public Vector2 GetOffset()
	{
		return Game.Instance.Player.FormationManager.CurrentFormation.GetOffset(m_Index, m_Unit);
	}

	public Vector3 GetLocalPosition()
	{
		return GetOffset() * 40f + OffsetPosition;
	}

	public void MoveCharacter(Vector2 vector)
	{
		Game.Instance.GameCommandQueue.PartyFormationOffset(m_Index, m_Unit, vector);
	}
}
