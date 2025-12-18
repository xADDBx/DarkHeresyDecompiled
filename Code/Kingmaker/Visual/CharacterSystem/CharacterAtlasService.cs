using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility.Locator;
using Owlcat.Runtime.Visual.DxtCompressor;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class CharacterAtlasService : IService
{
	private struct AtlasRebuildRequest
	{
		public List<CharacterAtlas> Atlases;

		public Material Material;

		public Action<CharacterAtlas, Texture2D> OnTextureCompressed;

		public Action<CharacterAtlas> OnTextureNotCompressed;

		public int CharacterId;

		public string ContextString;

		public int CurrentAtlasIndex;
	}

	private List<AtlasRebuildRequest> m_Requests = new List<AtlasRebuildRequest>();

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public int RequestsCount => m_Requests.Count;

	public void QueueAtlasRebuild(List<CharacterAtlas> atlases, Action<CharacterAtlas, Texture2D> onTextureCompressed, Action<CharacterAtlas> onTextureNotCompressed, int characterId, string contextString)
	{
		AtlasRebuildRequest atlasRebuildRequest = default(AtlasRebuildRequest);
		atlasRebuildRequest.Atlases = new List<CharacterAtlas>(atlases);
		atlasRebuildRequest.OnTextureCompressed = onTextureCompressed;
		atlasRebuildRequest.OnTextureNotCompressed = onTextureNotCompressed;
		atlasRebuildRequest.CharacterId = characterId;
		atlasRebuildRequest.ContextString = contextString;
		AtlasRebuildRequest atlasRebuildRequest2 = atlasRebuildRequest;
		int num = m_Requests.FindIndex((AtlasRebuildRequest r) => r.CharacterId == characterId);
		if (num != -1)
		{
			ClearRequestBecauseAtlasDestroyed(m_Requests[num]);
			m_Requests[num] = atlasRebuildRequest2;
		}
		else
		{
			m_Requests.Add(atlasRebuildRequest2);
			Update();
		}
	}

	public void Update()
	{
		if (m_Requests.Count == 0)
		{
			return;
		}
		AtlasRebuildRequest request = m_Requests[0];
		DxtCompressorService2 instance = Services.GetInstance<DxtCompressorService2>();
		if (instance != null && instance.RequestsCount > 0)
		{
			return;
		}
		m_Requests.RemoveAt(0);
		if (request.Atlases.Count != 0)
		{
			request.Atlases[0].CompressAsync(delegate(CharacterAtlas a, Texture2D t)
			{
				OnOneAtlasCompressed(request, a, t);
			}, delegate(CharacterAtlas a)
			{
				OnOneAtlasNotCompressed(request, a);
			});
		}
	}

	private void OnOneAtlasCompressed(AtlasRebuildRequest request, CharacterAtlas atlas, Texture2D texture)
	{
		request.OnTextureCompressed(atlas, texture);
		if (request.CurrentAtlasIndex < request.Atlases.Count - 1)
		{
			CharacterAtlas characterAtlas = request.Atlases[++request.CurrentAtlasIndex];
			if (characterAtlas.Destroyed)
			{
				ClearRequestBecauseAtlasDestroyed(request);
				return;
			}
			characterAtlas.CompressAsync(delegate(CharacterAtlas a, Texture2D t)
			{
				OnOneAtlasCompressed(request, a, t);
			}, delegate(CharacterAtlas a)
			{
				OnOneAtlasNotCompressed(request, a);
			});
		}
		else
		{
			request.Atlases.Clear();
		}
	}

	private void OnOneAtlasNotCompressed(AtlasRebuildRequest request, CharacterAtlas atlas)
	{
		request.OnTextureNotCompressed(atlas);
		if (request.CurrentAtlasIndex < request.Atlases.Count - 1)
		{
			CharacterAtlas characterAtlas = request.Atlases[++request.CurrentAtlasIndex];
			if (characterAtlas.Destroyed)
			{
				ClearRequestBecauseAtlasDestroyed(request);
				return;
			}
			characterAtlas.CompressAsync(delegate(CharacterAtlas a, Texture2D t)
			{
				OnOneAtlasCompressed(request, a, t);
			}, delegate(CharacterAtlas a)
			{
				OnOneAtlasNotCompressed(request, a);
			});
		}
		else
		{
			request.Atlases.Clear();
		}
	}

	private void ClearRequestBecauseAtlasDestroyed(AtlasRebuildRequest request)
	{
		foreach (CharacterAtlas atlase in request.Atlases)
		{
			atlase.ClearTempValues();
		}
		request.Atlases.Clear();
	}
}
