using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View.MapObjects;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipTransitionVM : BaseOvertipMapObjectVM
{
	private readonly ReactiveProperty<string> m_Title = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<bool> m_HasCharactersMovingToHere = new ReactiveProperty<bool>(value: false);

	public readonly bool EnableInCombat;

	public ReadOnlyReactiveProperty<string> Title => m_Title;

	public ReadOnlyReactiveProperty<bool> HasCharactersMovingToHere => m_HasCharactersMovingToHere;

	protected override bool UpdateEnabled => MapObjectEntity?.IsInGame ?? false;

	protected override Vector3 GetEntityPosition()
	{
		return MapObjectEntity.Position;
	}

	public OvertipTransitionVM(MapObjectEntity mapObjectData)
		: base(mapObjectData)
	{
		AreaTransitionSettings areaTransitionSettings = mapObjectData.View.GetComponent<AreaTransition>()?.Settings;
		EnableInCombat = areaTransitionSettings?.EnableInCombat ?? true;
		m_Title.Value = areaTransitionSettings?.AreaEnterPoint?.TooltipDescription ?? areaTransitionSettings?.AreaEnterPoint?.Area?.AreaDisplayName;
	}

	protected override void OnUpdateHandler()
	{
		m_HasCharactersMovingToHere.Value = IsCharactersMove();
		base.OnUpdateHandler();
	}

	private bool IsCharactersMove()
	{
		if (!UpdateEnabled)
		{
			return false;
		}
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			UnitAreaTransition current = partyAndPet.Commands.GetCurrent<UnitAreaTransition>();
			if (current != null && current.TransitionPart.Owner == MapObjectEntity)
			{
				return true;
			}
		}
		return false;
	}

	public void OnClick()
	{
		if (!LoadingProcess.Instance.IsLoadingInProcess)
		{
			StartAreaTransition();
		}
	}

	private void StartAreaTransition()
	{
		if (!Game.Instance.Player.IsInCombat && !(Game.Instance.CurrentModeType == GameModeType.Dialog) && MapObjectEntity.GetOptional<AreaTransitionPart>() != null && UtilityNet.IsControlMainCharacterWithWarning())
		{
			AreaTransitionHelper.StartAreaTransition(MapObjectEntity);
		}
	}

	public void HighlightChanged()
	{
		m_MapObjectIsHighlighted.Value = MapObjectEntity?.View.Highlighted ?? false;
	}
}
