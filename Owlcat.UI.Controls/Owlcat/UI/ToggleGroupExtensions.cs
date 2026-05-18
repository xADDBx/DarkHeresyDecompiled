using System;
using System.Collections.Generic;
using System.Linq;
using R3;

namespace Owlcat.UI;

public static class ToggleGroupExtensions
{
	public static IDisposable SubscribeToToggleGroup<T>(this ReactiveProperty<T> selection, OwlcatToggleGroup group)
	{
		return selection.SubscribeToToggleGroup(group, (OwlcatToggle toggle) => toggle.GetComponent<View<T>>().ViewModel);
	}

	public static IDisposable SubscribeToToggleGroup<T>(this ReactiveProperty<T> selection, OwlcatToggleGroup group, Func<OwlcatToggle, T> selector)
	{
		CompositeDisposable compositeDisposable = new CompositeDisposable();
		bool sync = false;
		selection.Where((T _) => !sync).Subscribe(delegate(T value)
		{
			sync = true;
			foreach (OwlcatToggle toggle in group.Toggles)
			{
				toggle.Set(Equals(toggle, value));
			}
			sync = false;
		}).AddTo(compositeDisposable);
		(from _ in @group.ActiveToggle
			where !sync
			select _ into t
			where t != null || HasAnyToggleWithValue(selection.Value)
			select t).Subscribe(delegate(OwlcatToggle toggle)
		{
			sync = true;
			selection.Value = (toggle ? selector(toggle) : default(T));
			sync = false;
		}).AddTo(compositeDisposable);
		Observable.FromEvent(delegate(Action<OwlcatToggle> h)
		{
			group.ToggleRegistered += h;
		}, delegate(Action<OwlcatToggle> h)
		{
			group.ToggleRegistered -= h;
		}).ObserveOn(UnityFrameProvider.PreLateUpdate).Subscribe(delegate(OwlcatToggle toggle)
		{
			sync = true;
			toggle.Set(Equals(toggle, selection.Value));
			sync = false;
		})
			.AddTo(compositeDisposable);
		return compositeDisposable;
		bool Equals(OwlcatToggle toggle, T value)
		{
			return EqualityComparer<T>.Default.Equals(selector(toggle), value);
		}
		bool HasAnyToggleWithValue(T value)
		{
			return group.Toggles.Select(selector).Any((T value) => EqualityComparer<T>.Default.Equals(value, selection.Value));
		}
	}
}
