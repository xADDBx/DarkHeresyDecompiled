using System.Collections;
using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Controllers.Asks;

public class ProximityAsksController : IControllerTick, IController, IControllerEnable, IControllerDisable
{
	private List<AskPart> m_Components = new List<AskPart>();

	private IEnumerator m_NewlyUnlockedClueStudiesWatcher;

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		AskPart askPart = null;
		float num = float.MaxValue;
		AbstractUnitEntity abstractUnitEntity = null;
		foreach (AskPart component in m_Components)
		{
			foreach (UnitReference partyCharacter in Game.Instance.Player.PartyCharacters)
			{
				float num2 = Vector3.Distance(component.View.ViewTransform.position, partyCharacter.Entity.Position);
				if (num2 < component.Settings.DistanceToAsk && num2 < num && component.CanBeTriggered)
				{
					num = num2;
					askPart = component;
					abstractUnitEntity = partyCharacter.Entity as AbstractUnitEntity;
				}
			}
		}
		if (askPart != null && abstractUnitEntity != null)
		{
			askPart.Trigger();
			switch (askPart.Settings.AskType)
			{
			case ProximityAskType.DetectiveSearch:
				abstractUnitEntity.View.Asks?.DetectiveSearch.Schedule();
				break;
			case ProximityAskType.DetectiveReconstructionFind:
				abstractUnitEntity.View.Asks?.DetectiveReconstructionFound.Schedule();
				break;
			}
		}
		m_NewlyUnlockedClueStudiesWatcher.MoveNext();
	}

	void IControllerEnable.OnEnable()
	{
		m_NewlyUnlockedClueStudiesWatcher = Game.Instance.DetectiveSystem.WatchNewlyUnlockedClueStudies();
	}

	void IControllerDisable.OnDisable()
	{
		m_NewlyUnlockedClueStudiesWatcher = null;
	}

	public void Register(AskPart component)
	{
		m_Components.Add(component);
	}

	public void Unregister(AskPart component)
	{
		m_Components.Remove(component);
	}
}
