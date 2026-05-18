using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IServiceWindowUIHandler : IInventoryUIHandler, ISubscriber, IJournalUIHandler, IDetectiveJournalUIHandler, IEncyclopediaHandler
{
	void HandleCloseAll();

	void HandleOpenLocalMap();

	void HandleOpenCharacterInfo();

	void HandleOpenCharacterInfo(CharInfoPageType pageType, BaseUnitEntity unitEntity);
}
