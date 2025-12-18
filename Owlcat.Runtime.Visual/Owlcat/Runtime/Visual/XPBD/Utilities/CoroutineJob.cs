using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Utilities;

public class CoroutineJob
{
	public class ProgressInfo
	{
		public string UserReadableInfo;

		public float Progress;

		public ProgressInfo(string userReadableInfo, float progress)
		{
			UserReadableInfo = userReadableInfo;
			Progress = progress;
		}
	}

	private object m_Result;

	private bool m_IsDone;

	private bool m_RaisedException;

	private bool m_Stop;

	private Exception m_Exception;

	public int AsyncThreshold;

	public object Result
	{
		get
		{
			if (m_Exception != null)
			{
				throw m_Exception;
			}
			return m_Result;
		}
	}

	public bool IsDone => m_IsDone;

	public bool RaisedException => m_RaisedException;

	private void Init()
	{
		m_IsDone = false;
		m_RaisedException = false;
		m_Stop = false;
		m_Result = null;
	}

	public static object RunSynchronously(IEnumerator coroutine)
	{
		List<object> list = new List<object>();
		if (coroutine == null)
		{
			return list;
		}
		try
		{
			while (coroutine.MoveNext())
			{
				list.Add(coroutine.Current);
			}
			return list;
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	public IEnumerator Start(IEnumerator coroutine)
	{
		Init();
		if (coroutine == null)
		{
			m_IsDone = true;
			yield break;
		}
		Stopwatch sw = new Stopwatch();
		sw.Start();
		while (!m_Stop)
		{
			try
			{
				if (!coroutine.MoveNext())
				{
					m_IsDone = true;
					sw.Stop();
					break;
				}
			}
			catch (Exception exception)
			{
				Exception exception2 = (m_Exception = exception);
				m_RaisedException = true;
				UnityEngine.Debug.LogException(exception2);
				m_IsDone = true;
				sw.Stop();
				break;
			}
			m_Result = coroutine.Current;
			if (sw.ElapsedMilliseconds > AsyncThreshold)
			{
				yield return m_Result;
			}
		}
	}

	public void Stop()
	{
		m_Stop = true;
	}
}
