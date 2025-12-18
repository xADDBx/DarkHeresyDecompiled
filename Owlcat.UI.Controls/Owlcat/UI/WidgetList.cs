using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.Pool;

namespace Owlcat.UI;

public class WidgetList : MonoBehaviour
{
	private delegate IBindable LookupDelegate<T>(T data);

	private readonly struct Lookup
	{
		private readonly List<IBindable> m_Prefabs;

		public Lookup(List<IBindable> prefabs)
		{
			m_Prefabs = prefabs;
		}

		public IBindable Find<T>(T data)
		{
			Type type = data.GetType();
			foreach (IBindable prefab in m_Prefabs)
			{
				if (prefab.GetType().CanBindTo(type, out var _))
				{
					return prefab;
				}
			}
			return null;
		}
	}

	[SerializeField]
	private Transform m_Container;

	private readonly List<object> m_Datas = new List<object>();

	private readonly List<IBindable> m_Views = new List<IBindable>();

	public Transform Container
	{
		get
		{
			if (!m_Container)
			{
				return base.transform;
			}
			return m_Container;
		}
	}

	public List<IBindable> Entries => m_Views;

	private void Awake()
	{
		for (int i = 0; i < Container.childCount; i++)
		{
			if (Container.GetChild(i).TryGetComponent<IBindable>(out var component) && !m_Views.Contains(component))
			{
				WidgetPool.Release(component as MonoBehaviour);
			}
		}
	}

	private IDisposable Draw<TData>(List<TData> datas, LookupDelegate<TData> lookup)
	{
		for (int num = m_Datas.Count - 1; num >= 0; num--)
		{
			if (!datas.Contains((TData)m_Datas[num]))
			{
				IBindable bindable = m_Views[num];
				m_Datas.RemoveAt(num);
				m_Views.RemoveAt(num);
				WidgetPool.Release(bindable as MonoBehaviour, reparent: false);
			}
		}
		for (int i = 0; i < datas.Count; i++)
		{
			TData val = datas[i];
			int num2 = m_Datas.IndexOf(val, i);
			IBindable bindable2 = null;
			if (num2 == i)
			{
				bindable2 = m_Views[i];
			}
			else if (num2 == -1)
			{
				IBindable bindable3 = lookup(val);
				if (bindable3 != null)
				{
					bindable2 = (IBindable)WidgetPool.Retain(bindable3 as MonoBehaviour, Container);
					bindable2.BindDynamic(val);
				}
				else
				{
					UIKitLogger.Warning($"WidgetList: No view found for {val.GetType()}");
				}
				m_Datas.Insert(i, val);
				m_Views.Insert(i, bindable2);
			}
			else if (num2 >= 0)
			{
				bindable2 = m_Views[num2];
				m_Datas.RemoveAt(num2);
				m_Views.RemoveAt(num2);
				m_Datas.Insert(i, val);
				m_Views.Insert(i, bindable2);
			}
			if (bindable2 is MonoBehaviour monoBehaviour)
			{
				monoBehaviour.transform.SetAsLastSibling();
			}
		}
		return Disposable.Create(Clear);
	}

	public void Clear()
	{
		for (int num = m_Datas.Count - 1; num >= 0; num--)
		{
			IBindable bindable = m_Views[num];
			m_Datas.RemoveAt(num);
			m_Views.RemoveAt(num);
			WidgetPool.Release(bindable as MonoBehaviour, reparent: false);
		}
	}

	public void Remove(IBindable view)
	{
		int num = m_Views.IndexOf(view);
		if (num == -1)
		{
			UIKitLogger.Warning($"WidgetList: Attempt to remove a view that is not in the list: {view}");
			return;
		}
		m_Datas.RemoveAt(num);
		m_Views.RemoveAt(num);
		WidgetPool.Release(view as MonoBehaviour, reparent: false);
	}

	public IDisposable DrawEntries<TData, TWidget>(IEnumerable<TData> datas, TWidget prefab, bool unused = false) where TWidget : MonoBehaviour, IBindable<TData>
	{
		List<IBindable> value;
		using (CollectionPool<List<IBindable>, IBindable>.Get(out value))
		{
			value.Add(prefab);
			return Draw(datas, value);
		}
	}

	public IDisposable DrawMultiEntries<TData, TView>(IEnumerable<TData> datas, IEnumerable<TView> prefabs) where TView : MonoBehaviour, IBindable
	{
		return Draw(datas, prefabs);
	}

	public IDisposable DrawMultiEntries<TData>(IEnumerable<TData> datas, IEnumerable<MonoBehaviour> prefabs)
	{
		return Draw(datas, prefabs.Cast<IBindable>());
	}

	private IDisposable Draw<TData>(IEnumerable<TData> datas, IEnumerable<IBindable> views)
	{
		List<TData> value;
		using (CollectionPool<List<TData>, TData>.Get(out value))
		{
			value.AddRange(datas);
			List<IBindable> value2;
			using (CollectionPool<List<IBindable>, IBindable>.Get(out value2))
			{
				value2.AddRange(views);
				Lookup lookup = new Lookup(value2);
				return Draw(value, ((Lookup)lookup).Find);
			}
		}
	}

	public List<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return m_Views.Select((IBindable x) => x as IConsoleNavigationEntity).ToList();
	}
}
