using System.Collections.Generic;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CombatTextMessageQueue<TMessage> : ViewModel
{
	private readonly Queue<TMessage> m_MessagesQueue = new Queue<TMessage>();

	private readonly HashSet<TMessage> m_RequestedMessages = new HashSet<TMessage>();

	private readonly ReactiveCommand m_CombatMessageEnqueued;

	private readonly ReactiveProperty<int> m_AliveMessagesCount;

	public Observable<Unit> CombatMessageEnqueued => m_CombatMessageEnqueued;

	public ReadOnlyReactiveProperty<int> AliveMessagesCount => m_AliveMessagesCount;

	public CombatTextMessageQueue()
	{
		m_CombatMessageEnqueued = new ReactiveCommand().AddTo(this);
		m_AliveMessagesCount = new ReactiveProperty<int>().AddTo(this);
	}

	public void Enqueue(TMessage messageBase)
	{
		m_MessagesQueue.Enqueue(messageBase);
		m_AliveMessagesCount.Value = GetMessagesCount();
		m_CombatMessageEnqueued.Execute(default(Unit));
	}

	public bool Dequeue(out TMessage message)
	{
		if (!m_MessagesQueue.TryDequeue(out message))
		{
			return false;
		}
		m_RequestedMessages.Add(message);
		return true;
	}

	public void ClearMessage(TMessage message)
	{
		m_RequestedMessages.Remove(message);
		m_AliveMessagesCount.Value = GetMessagesCount();
	}

	public void ClearAllMessages()
	{
		m_MessagesQueue.Clear();
		m_RequestedMessages.Clear();
		m_AliveMessagesCount.Value = 0;
	}

	private int GetMessagesCount()
	{
		return m_MessagesQueue.Count + m_RequestedMessages.Count;
	}
}
