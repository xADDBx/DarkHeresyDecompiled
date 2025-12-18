using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Owlcat.UI;

public static class TransitionAsyncExtensions
{
	public class TransitionAwaiter : INotifyCompletion
	{
		private static readonly List<TransitionAwaiter> sAwaiters = new List<TransitionAwaiter>();

		private readonly Transition m_Transition;

		private Action m_Continuation;

		public bool IsCompleted => m_Transition.Completed;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Initialize()
		{
			PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
			PlayerLoopSystem[] array = currentPlayerLoop.subSystemList ?? Array.Empty<PlayerLoopSystem>();
			PlayerLoopSystem[] array2 = new PlayerLoopSystem[array.Length + 1];
			Array.Copy(array, array2, array.Length);
			int num = array.Length;
			array2[num].type = typeof(TransitionAwaiter);
			array2[num].updateDelegate = OnUpdate;
			currentPlayerLoop.subSystemList = array2;
			PlayerLoop.SetPlayerLoop(currentPlayerLoop);
		}

		private static void OnUpdate()
		{
			for (int i = 0; i < sAwaiters.Count; i++)
			{
				TransitionAwaiter transitionAwaiter = sAwaiters[i];
				if (transitionAwaiter.m_Transition.Completed)
				{
					List<TransitionAwaiter> list = sAwaiters;
					int index = i--;
					List<TransitionAwaiter> list2 = sAwaiters;
					list[index] = list2[list2.Count - 1];
					sAwaiters.RemoveAt(sAwaiters.Count - 1);
					transitionAwaiter.Complete();
				}
			}
		}

		public TransitionAwaiter(Transition transition)
		{
			m_Transition = transition;
			sAwaiters.Add(this);
		}

		private void Complete()
		{
			m_Continuation?.Invoke();
			m_Continuation = null;
		}

		public void GetResult()
		{
		}

		public void OnCompleted(Action continuation)
		{
			if (IsCompleted)
			{
				continuation();
			}
			else
			{
				m_Continuation = (Action)Delegate.Combine(m_Continuation, continuation);
			}
		}
	}

	public static TransitionAwaiter GetAwaiter(this Transition transition)
	{
		return new TransitionAwaiter(transition);
	}
}
