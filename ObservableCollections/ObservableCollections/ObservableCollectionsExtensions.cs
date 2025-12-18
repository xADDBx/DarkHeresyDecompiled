using System;
using System.Collections.Generic;
using ObservableCollections.Internal;

namespace ObservableCollections;

public static class ObservableCollectionsExtensions
{
	private class AnonymousComparer<T, TCompare> : IComparer<T>
	{
		private readonly Func<T, TCompare> selector;

		private readonly int f;

		public AnonymousComparer(Func<T, TCompare> selector, bool ascending)
		{
			this.selector = selector;
			f = (ascending ? 1 : (-1));
		}

		public int Compare(T? x, T? y)
		{
			if (x == null && y == null)
			{
				return 0;
			}
			if (x == null)
			{
				return f;
			}
			if (y == null)
			{
				return -1 * f;
			}
			return Comparer<TCompare>.Default.Compare(selector(x), selector(y)) * f;
		}
	}

	public static ISynchronizedView<T, TView> CreateSortedView<T, TKey, TView>(this IObservableCollection<T> source, Func<T, TKey> identitySelector, Func<T, TView> transform, IComparer<T> comparer) where TKey : notnull
	{
		return new SortedView<T, TKey, TView>(source, identitySelector, transform, comparer);
	}

	public static ISynchronizedView<T, TView> CreateSortedView<T, TKey, TView>(this IObservableCollection<T> source, Func<T, TKey> identitySelector, Func<T, TView> transform, IComparer<TView> viewComparer) where TKey : notnull
	{
		return new SortedViewViewComparer<T, TKey, TView>(source, identitySelector, transform, viewComparer);
	}

	public static ISynchronizedView<T, TView> CreateSortedView<T, TKey, TView, TCompare>(this IObservableCollection<T> source, Func<T, TKey> identitySelector, Func<T, TView> transform, Func<T, TCompare> compareSelector, bool ascending = true) where TKey : notnull
	{
		return source.CreateSortedView(identitySelector, transform, new AnonymousComparer<T, TCompare>(compareSelector, ascending));
	}

	public static ISortableSynchronizedView<T, TView> CreateSortableView<T, TView>(this IFreezedCollection<T> source, Func<T, TView> transform, IComparer<T> initialSort)
	{
		ISortableSynchronizedView<T, TView> sortableSynchronizedView = source.CreateSortableView(transform);
		sortableSynchronizedView.Sort(initialSort);
		return sortableSynchronizedView;
	}

	public static ISortableSynchronizedView<T, TView> CreateSortableView<T, TView>(this IFreezedCollection<T> source, Func<T, TView> transform, IComparer<TView> initialViewSort)
	{
		ISortableSynchronizedView<T, TView> sortableSynchronizedView = source.CreateSortableView(transform);
		sortableSynchronizedView.Sort(initialViewSort);
		return sortableSynchronizedView;
	}

	public static ISortableSynchronizedView<T, TView> CreateSortableView<T, TView, TCompare>(this IFreezedCollection<T> source, Func<T, TView> transform, Func<T, TCompare> initialCompareSelector, bool ascending = true)
	{
		ISortableSynchronizedView<T, TView> sortableSynchronizedView = source.CreateSortableView(transform);
		sortableSynchronizedView.Sort(initialCompareSelector, ascending);
		return sortableSynchronizedView;
	}

	public static void Sort<T, TView, TCompare>(this ISortableSynchronizedView<T, TView> source, Func<T, TCompare> compareSelector, bool ascending = true)
	{
		source.Sort(new AnonymousComparer<T, TCompare>(compareSelector, ascending));
	}
}
