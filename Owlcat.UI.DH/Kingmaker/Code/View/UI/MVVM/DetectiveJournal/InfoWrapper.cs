using System;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public abstract class InfoWrapper : IEquatable<InfoWrapper>
{
	protected abstract int HashCode { get; }

	public abstract void MarkAsViewed();

	public abstract void RefreshData();

	public abstract AddendumState GetAddendumState();

	public abstract bool Equals(InfoWrapper other);

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((InfoWrapper)obj);
	}

	public override int GetHashCode()
	{
		return HashCode;
	}
}
