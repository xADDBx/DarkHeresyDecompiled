using System;
using System.Threading;

namespace Owlcat.Runtime.Visual.VirtualTexture.PostRender;

public class BackgroundThreadWorker : IDisposable
{
	private Thread m_BackgroundThread;

	private AutoResetEvent m_WorkAvailable;

	private ManualResetEvent m_WorkDone;

	private bool m_IsRunning;

	private bool m_IsBusy;

	private object m_Lock;

	private Action m_Callback;

	public bool IsBusy
	{
		get
		{
			lock (m_Lock)
			{
				return m_IsBusy;
			}
		}
	}

	public BackgroundThreadWorker(string threadName, Action callback)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		m_Callback = callback;
		m_WorkAvailable = new AutoResetEvent(initialState: false);
		m_WorkDone = new ManualResetEvent(initialState: false);
		m_IsRunning = false;
		m_IsBusy = false;
		m_Lock = new object();
		m_BackgroundThread = new Thread(BackgroundTask)
		{
			IsBackground = true,
			Name = threadName
		};
	}

	public void Dispose()
	{
		if (m_IsRunning)
		{
			m_IsRunning = false;
			m_WorkAvailable.Set();
			m_BackgroundThread.Join();
		}
		m_WorkAvailable?.Dispose();
		m_WorkDone?.Dispose();
	}

	public void Wait()
	{
		if (IsBusy)
		{
			m_WorkDone.WaitOne();
		}
	}

	private void BackgroundTask()
	{
		while (m_IsRunning)
		{
			m_WorkAvailable.WaitOne();
			if (m_IsRunning)
			{
				m_Callback?.Invoke();
				lock (m_Lock)
				{
					m_IsBusy = false;
					m_WorkDone.Set();
				}
				continue;
			}
			break;
		}
	}

	public void RunAsync()
	{
		if (!m_IsRunning)
		{
			m_IsRunning = true;
			m_BackgroundThread.Start();
		}
		lock (m_Lock)
		{
			if (m_IsBusy)
			{
				throw new Exception("BackgroundThreadWorker is busy but you're trying to run work.");
			}
			m_IsBusy = true;
			m_WorkDone.Reset();
			m_WorkAvailable.Set();
		}
	}
}
