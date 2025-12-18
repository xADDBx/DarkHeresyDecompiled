using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICounterWindowUIHandler : ISubscriber
{
	void HandleOpen(CounterWindowType type, ItemEntity item, Action<int> command);

	void HandleCloseCounterWindow();
}
