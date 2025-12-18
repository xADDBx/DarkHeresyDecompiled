using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public abstract class WaaaghResourceDataBase : ContextItem
{
	internal enum ActiveID
	{
		Camera,
		BackBuffer
	}

	internal bool isAccessible { get; set; }

	internal void InitFrame()
	{
		isAccessible = true;
	}

	internal void EndFrame()
	{
		isAccessible = false;
	}

	protected void CheckAndSetTextureHandle(ref TextureHandle handle, TextureHandle newHandle)
	{
		if (CheckAndWarnAboutAccessibility())
		{
			handle = newHandle;
		}
	}

	protected TextureHandle CheckAndGetTextureHandle(ref TextureHandle handle)
	{
		if (!CheckAndWarnAboutAccessibility())
		{
			return TextureHandle.nullHandle;
		}
		return handle;
	}

	protected void CheckAndSetBufferHandle(ref BufferHandle handle, BufferHandle newHandle)
	{
		if (CheckAndWarnAboutAccessibility())
		{
			handle = newHandle;
		}
	}

	protected BufferHandle CheckAndGetBufferHandle(ref BufferHandle handle)
	{
		if (!CheckAndWarnAboutAccessibility())
		{
			return BufferHandle.nullHandle;
		}
		return handle;
	}

	protected void CheckAndSetTextureHandle(ref TextureHandle[] handle, TextureHandle[] newHandle)
	{
		if (CheckAndWarnAboutAccessibility())
		{
			if (handle == null || handle.Length != newHandle.Length)
			{
				handle = new TextureHandle[newHandle.Length];
			}
			for (int i = 0; i < newHandle.Length; i++)
			{
				handle[i] = newHandle[i];
			}
		}
	}

	protected TextureHandle[] CheckAndGetTextureHandle(ref TextureHandle[] handle)
	{
		if (!CheckAndWarnAboutAccessibility())
		{
			return new TextureHandle[1] { TextureHandle.nullHandle };
		}
		return handle;
	}

	protected bool CheckAndWarnAboutAccessibility()
	{
		if (!isAccessible)
		{
			Debug.LogError("Trying to access Waaagh Resources outside of the current frame setup.");
		}
		return isAccessible;
	}
}
