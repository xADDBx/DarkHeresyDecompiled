using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Networking;

namespace Kingmaker.Code.UI.MVVM;

public class NetRolesPlayerVM : NetLobbyPlayerVM
{
	public readonly List<NetRolesPlayerCharacterVM> Players = new List<NetRolesPlayerCharacterVM>();

	private static List<BaseUnitEntity> Characters => Game.Instance.Controllers.SelectionCharacter.ActualGroup;

	protected override void OnDispose()
	{
		base.OnDispose();
		Players.ForEach(delegate(NetRolesPlayerCharacterVM p)
		{
			p.Dispose();
		});
		Players.Clear();
	}

	public override void SetPlayer(PhotonActorNumber player, string userId, bool isActive)
	{
		base.SetPlayer(player, userId, isActive);
		for (int i = 0; i < 6; i++)
		{
			BaseUnitEntity unit = ((Characters.Count > i) ? Characters[i] : null);
			Players.Add(new NetRolesPlayerCharacterVM(UnitReference.FromIAbstractUnitEntity(unit), player));
		}
	}
}
