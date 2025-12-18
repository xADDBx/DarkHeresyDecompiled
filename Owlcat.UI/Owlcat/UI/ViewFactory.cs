using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Owlcat.UI.Lookup;
using UnityEngine;

namespace Owlcat.UI;

public class ViewFactory : IViewFactory
{
	private static readonly Dictionary<Type, Type> sCache = new Dictionary<Type, Type>();

	private readonly Transform m_Root;

	private readonly ILookupTable m_Lookup;

	private readonly ViewFactoryPolicy m_CommonPolicy;

	public ViewFactory(Transform root, ILookupTable lookup, ViewFactoryPolicy commonPolicy = null)
	{
		m_Root = root;
		m_Lookup = lookup;
		m_CommonPolicy = commonPolicy;
	}

	public async Task<IBindable<T>> Retain<T>(T data, CancellationToken cancellationToken)
	{
		MonoBehaviour prefab = await m_Lookup.FirstOrDefault(typeof(T));
		if (prefab != null)
		{
			ViewFactoryPolicy policy = GetPolicy(prefab.GetType());
			Transform parent = (string.IsNullOrEmpty(policy.Path) ? m_Root : m_Root.Find(policy.Path));
			if (parent == null)
			{
				parent = m_Root;
			}
			MonoBehaviour monoBehaviour = ((!policy.Flags.HasFlag(ViewFactoryPolicyFlag.DontInstantiateAsync)) ? (await WidgetPool.RetainAsync(prefab, parent, cancellationToken)) : WidgetPool.Retain(prefab, parent));
			MonoBehaviour monoBehaviour2 = monoBehaviour;
			if (cancellationToken.IsCancellationRequested)
			{
				WidgetPool.Release(monoBehaviour2);
				cancellationToken.ThrowIfCancellationRequested();
			}
			monoBehaviour2.name = prefab.name;
			monoBehaviour2.transform.SetSiblingIndex(FindSiblingIndexFor<T>(parent));
			sCache.TryAdd(monoBehaviour2.GetType(), typeof(T));
			if (monoBehaviour2 is IBindable<T> bindable)
			{
				bindable.Bind(data);
				return bindable;
			}
			throw new Exception($"Prefab {prefab} doesn't implement {typeof(IBindable<T>)}");
		}
		throw new Exception($"Prefab for {typeof(T)} not found");
	}

	private int FindSiblingIndexFor<T>(Transform parent)
	{
		for (int num = parent.childCount - 1; num >= 0; num--)
		{
			if (parent.GetChild(num).TryGetComponent<IBindable>(out var component) && !(component is IBindable<T>) && sCache.TryGetValue(component.GetType(), out var value) && m_Lookup.Compare(typeof(T), value) >= 0)
			{
				return num + 1;
			}
		}
		return 0;
	}

	public void Release(IBindable view)
	{
		if (!(view is MonoBehaviour monoBehaviour) || !(monoBehaviour == null))
		{
			view.Unbind();
			MonoBehaviour monoBehaviour2 = view as MonoBehaviour;
			ViewFactoryPolicyFlag flags = GetPolicy(view.GetType()).Flags;
			if (flags.HasFlag(ViewFactoryPolicyFlag.DontPool) || m_Root == null || !m_Root.gameObject.activeInHierarchy)
			{
				UnityEngine.Object.Destroy(monoBehaviour2.gameObject);
			}
			else
			{
				WidgetPool.Release(monoBehaviour2, !flags.HasFlag(ViewFactoryPolicyFlag.DontReparent));
			}
		}
	}

	private ViewFactoryPolicy GetPolicy(Type type)
	{
		ViewFactoryPolicy customAttribute = ViewFactoryPolicy.GetCustomAttribute(type);
		if (m_CommonPolicy == null)
		{
			return customAttribute;
		}
		if (customAttribute != ViewFactoryPolicy.Default)
		{
			return customAttribute;
		}
		return m_CommonPolicy;
	}
}
