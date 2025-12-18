using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UtilityParty
{
	public static BaseUnitEntity GetCurrentSelectedUnit()
	{
		return Game.Instance.Controllers.SelectionCharacter.SelectedUnitInUI.Value;
	}

	public static List<BaseUnitEntity> GetGroup(bool withRemote = false, bool withPet = false)
	{
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		GetGroup(list, withRemote, withPet);
		return list;
	}

	public static void GetGroup(List<BaseUnitEntity> characters, bool withRemote = false, bool withPet = false)
	{
		characters.Clear();
		if (withRemote)
		{
			characters.AddRange(Game.Instance.Player.Party);
			IEnumerable<BaseUnitEntity> collection = Game.Instance.Player.RemoteCompanions.Reverse();
			characters.AddRange(collection);
		}
		else
		{
			characters.AddRange(Game.Instance.Player.Party.Where(UtilityUnit.IsViewActiveUnit));
		}
		if (!withPet)
		{
			return;
		}
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			if (partyAndPet.IsPet)
			{
				BaseUnitEntity master = partyAndPet.Master;
				int num = characters.FindIndex((BaseUnitEntity m) => m == master);
				if (num < 0 || num + 1 >= characters.Count)
				{
					characters.Add(partyAndPet);
				}
				else
				{
					characters.Insert(num + 1, partyAndPet);
				}
			}
		}
	}
}
