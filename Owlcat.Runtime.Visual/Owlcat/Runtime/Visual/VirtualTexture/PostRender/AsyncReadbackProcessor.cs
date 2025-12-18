using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTexture.PostRender;

public class AsyncReadbackProcessor : IDisposable
{
	public enum RequestState
	{
		Free,
		WaitingGPU,
		ProcessingCPU
	}

	public class Request : IDisposable
	{
		private object m_Lock;

		private RequestState m_State;

		public NativeArray<uint> ReadbackData;

		public int StartFrame;

		public int LastFrameLag;

		public RequestState State
		{
			get
			{
				lock (m_Lock)
				{
					return m_State;
				}
			}
			set
			{
				lock (m_Lock)
				{
					m_State = value;
				}
			}
		}

		public Request()
		{
			m_Lock = new object();
			ReadbackData = default(NativeArray<uint>);
		}

		public void Dispose()
		{
			if (ReadbackData.IsCreated)
			{
				ReadbackData.Dispose();
			}
		}

		public void Refresh(int length)
		{
			Dispose();
			ReadbackData = new NativeArray<uint>(length, Allocator.Persistent);
		}
	}

	private const int kMaxRequests = 4;

	private Request[] m_Requests;

	private Action<AsyncGPUReadbackRequest> m_GpuCallback;

	private NativeList<int2> m_DecodedFeedback;

	internal NativeList<int2> DecodedFeedback => m_DecodedFeedback;

	public AsyncReadbackProcessor()
	{
		m_Requests = new Request[4];
		for (int i = 0; i < m_Requests.Length; i++)
		{
			m_Requests[i] = new Request();
		}
		m_GpuCallback = GpuCallback;
	}

	public void Dispose()
	{
		for (int i = 0; i < m_Requests.Length; i++)
		{
			m_Requests[i].Dispose();
		}
		m_DecodedFeedback.Dispose();
	}

	internal bool HasAnyFreeRequests()
	{
		for (int i = 0; i < m_Requests.Length; i++)
		{
			if (m_Requests[i].State == RequestState.Free)
			{
				return true;
			}
		}
		return false;
	}

	internal void RequestReadback(CommandBuffer cmd, GraphicsBuffer buffer)
	{
		for (int i = 0; i < m_Requests.Length; i++)
		{
			Request request = m_Requests[i];
			if (request.State == RequestState.Free)
			{
				request.State = RequestState.WaitingGPU;
				cmd.RequestAsyncReadback(buffer, m_GpuCallback);
				request.StartFrame = Time.frameCount;
				break;
			}
		}
	}

	private void GpuCallback(AsyncGPUReadbackRequest gpuRequest)
	{
		for (int i = 0; i < m_Requests.Length; i++)
		{
			Request request = m_Requests[i];
			if (request.State == RequestState.WaitingGPU)
			{
				if (request.ReadbackData.IsCreated && request.ReadbackData.Length == gpuRequest.layerDataSize / 4)
				{
					gpuRequest.GetData<uint>().CopyTo(request.ReadbackData);
					request.State = RequestState.ProcessingCPU;
				}
				else
				{
					request.State = RequestState.Free;
				}
				request.LastFrameLag = Time.frameCount - request.StartFrame;
				break;
			}
		}
	}

	internal void Refresh(int2 virtualAtlasResolutionInTiles)
	{
		int x = virtualAtlasResolutionInTiles.x * virtualAtlasResolutionInTiles.y;
		x = RenderingUtils.DivRoundUp(x, 32);
		for (int i = 0; i < m_Requests.Length; i++)
		{
			m_Requests[i].Refresh(x);
		}
		if (m_DecodedFeedback.IsCreated)
		{
			m_DecodedFeedback.Dispose();
		}
		m_DecodedFeedback = new NativeList<int2>(x * 32, Allocator.Persistent);
	}

	internal Request GetPendingRequest()
	{
		for (int i = 0; i < m_Requests.Length; i++)
		{
			Request request = m_Requests[i];
			if (request.State == RequestState.ProcessingCPU)
			{
				return request;
			}
		}
		return null;
	}

	internal void FreeRequest(Request request)
	{
		request.State = RequestState.Free;
	}
}
