using System.Collections.Generic;
using Animancer;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Decorators;

public class UnitAnimationDecoratorManager
{
	internal class DecoratorCacheEntry : IRuntimeDecorator
	{
		public readonly List<GameObject> GameObjects = new List<GameObject>();

		private readonly List<IDecoratorVisibilityRequest> m_Requests = new List<IDecoratorVisibilityRequest>();

		public UnitAnimationDecoratorObject DecoratorAsset { get; }

		public bool IsVisible { get; private set; }

		public IReadOnlyList<IDecoratorVisibilityRequest> Requests => m_Requests;

		public DecoratorCacheEntry(UnitAnimationDecoratorObject decoratorAsset)
		{
			DecoratorAsset = decoratorAsset;
		}

		public void AddRequest(IDecoratorVisibilityRequest request)
		{
			m_Requests.Add(request);
			UpdateVisibility();
		}

		public void RemoveRequest(IDecoratorVisibilityRequest request)
		{
			m_Requests.Remove(request);
			UpdateVisibility();
		}

		public void UpdateVisibility()
		{
			bool isVisible = IsVisible;
			int num = 0;
			foreach (IDecoratorVisibilityRequest request in m_Requests)
			{
				num += ((request.Type == IDecoratorVisibilityRequest.RequestType.Show) ? 1 : (-1));
			}
			IsVisible = num > 0;
			if (isVisible == IsVisible)
			{
				return;
			}
			foreach (GameObject gameObject in GameObjects)
			{
				gameObject.SetActive(IsVisible);
			}
		}

		public override string ToString()
		{
			return $"{DecoratorAsset.name} (visible: {IsVisible})";
		}
	}

	private UnitAnimationManager m_AnimationManager;

	private Dictionary<UnitAnimationDecoratorObject, DecoratorCacheEntry> m_DecoratorsCache = new Dictionary<UnitAnimationDecoratorObject, DecoratorCacheEntry>();

	private List<DecoratorVisibilityRequestWithDuration> m_RequestsWithDuration = new List<DecoratorVisibilityRequestWithDuration>();

	public UnitAnimationDecoratorManager(UnitAnimationManager animationManager)
	{
		m_AnimationManager = animationManager;
	}

	public void Update(float dt)
	{
		for (int num = m_RequestsWithDuration.Count - 1; num >= 0; num--)
		{
			DecoratorVisibilityRequestWithDuration decoratorVisibilityRequestWithDuration = m_RequestsWithDuration[num];
			decoratorVisibilityRequestWithDuration.Tick(dt);
			if (decoratorVisibilityRequestWithDuration.IsReleased)
			{
				m_RequestsWithDuration.RemoveAt(num);
			}
		}
	}

	public IDecoratorVisibilityRequest ShowDecorator(UnitAnimationDecoratorObject decorator, AnimationSet animationSet)
	{
		return new DecoratorVisibilityRequest(GetOrCreateDecoratorCacheEntry(decorator), animationSet, IDecoratorVisibilityRequest.RequestType.Show);
	}

	public IDecoratorVisibilityRequest ShowDecorator(UnitAnimationDecoratorObject decorator, CommandControlAnimationDecorator cutsceneCommand)
	{
		return new DecoratorVisibilityRequest(GetOrCreateDecoratorCacheEntry(decorator), cutsceneCommand, IDecoratorVisibilityRequest.RequestType.Show);
	}

	public IDecoratorVisibilityRequest HideDecorator(UnitAnimationDecoratorObject decorator, CommandControlAnimationDecorator cutsceneCommand)
	{
		return new DecoratorVisibilityRequest(GetOrCreateDecoratorCacheEntry(decorator), cutsceneCommand, IDecoratorVisibilityRequest.RequestType.Hide);
	}

	public void ShowDecorator(UnitAnimationDecoratorObject decorator, AnimancerState animancerState, float duration)
	{
		DecoratorCacheEntry orCreateDecoratorCacheEntry = GetOrCreateDecoratorCacheEntry(decorator);
		m_RequestsWithDuration.Add(new DecoratorVisibilityRequestWithDuration(orCreateDecoratorCacheEntry, animancerState, duration, IDecoratorVisibilityRequest.RequestType.Show));
	}

	private DecoratorCacheEntry GetOrCreateDecoratorCacheEntry(UnitAnimationDecoratorObject decorator)
	{
		if (m_DecoratorsCache.TryGetValue(decorator, out var value))
		{
			return value;
		}
		value = new DecoratorCacheEntry(decorator);
		DecoratorEntry[] entries = decorator.Entries;
		foreach (DecoratorEntry decoratorEntry in entries)
		{
			if (!string.IsNullOrEmpty(decoratorEntry.BoneName) && decoratorEntry.Prefab != null)
			{
				Transform transform = m_AnimationManager.transform.FindChildRecursive(decoratorEntry.BoneName);
				if (transform != null)
				{
					GameObject gameObject = Object.Instantiate(decoratorEntry.Prefab, transform, worldPositionStays: true);
					gameObject.transform.localScale = decoratorEntry.Scale;
					gameObject.transform.localPosition = decoratorEntry.Position;
					gameObject.transform.localRotation = Quaternion.Euler(decoratorEntry.Rotation);
					gameObject.name = "Decorator";
					value.GameObjects.Add(gameObject);
					m_AnimationManager.View.Or(null)?.MarkRenderersAndCollidersAreUpdated();
				}
			}
		}
		m_DecoratorsCache.Add(decorator, value);
		m_AnimationManager.Rebind();
		return value;
	}
}
