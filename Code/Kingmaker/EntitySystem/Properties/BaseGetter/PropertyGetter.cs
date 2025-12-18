using System;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.BaseGetter;

[Serializable]
[TypeId("36fbb8639be34e619c481ac1adc134e2")]
public abstract class PropertyGetter : Element
{
	public PropertyGetterSettings Settings;

	protected PropertyContext PropertyContext
	{
		get
		{
			if (!SimpleContextData<PropertyContext, PropertyContext.Scope>.TryGetCurrent(out var result))
			{
				throw new Exception("PropertyContext is missing");
			}
			return result;
		}
	}

	[NotNull]
	protected MechanicEntity CurrentEntity => PropertyContext.CurrentEntity;

	public virtual bool AddBracketsAroundInnerCaption => true;

	public abstract bool IsBool { get; }

	public virtual bool IsSimple => true;

	protected abstract int GetBaseValueInternal();

	protected abstract string GetInnerCaption(bool useLineBreaks);

	public sealed override string GetCaption()
	{
		return MakeCaption(useLineBreaks: false);
	}

	public sealed override string GetCaption(bool useLineBreaks)
	{
		return MakeCaption(useLineBreaks);
	}

	public int GetValue(PropertyCalculator calculator)
	{
		using ElementsDebugger elementsDebugger = ElementsDebugger.Scope(calculator, this);
		try
		{
			int baseValueInternal = GetBaseValueInternal();
			int result = Settings?.Apply(baseValueInternal, calculator, this) ?? baseValueInternal;
			elementsDebugger?.SetResult(result);
			return result;
		}
		catch (Exception exception)
		{
			Element.LogException(exception);
			elementsDebugger?.SetException(exception);
			throw;
		}
	}

	private string MakeCaption(bool useLineBreaks)
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		string text = "";
		int? num = null;
		int? num2 = null;
		if (Settings.Limit == PropertyGetterSettings.LimitType.Min)
		{
			text = "Min";
			num = Settings.Min;
		}
		else if (Settings.Limit == PropertyGetterSettings.LimitType.Max)
		{
			text = "Max";
			num2 = Settings.Max;
		}
		else if (Settings.Limit == PropertyGetterSettings.LimitType.MinMax)
		{
			text = "Clamp";
			num = Settings.Min;
			num2 = Settings.Max;
		}
		bool flag = true;
		if (text != "")
		{
			string text2 = (flag ? "cyan" : "#888888");
			text = "<color='" + text2 + "'>" + text + "</color>";
		}
		string value = "";
		if (Settings.Negate)
		{
			value = ((IsBool && !Settings.AsInt) ? "NOT " : "-");
		}
		string text3 = "";
		if (Settings.Progression != 0)
		{
			text3 = Settings.ProgressionToString();
		}
		if (text3 != "")
		{
			string text4 = (flag ? "lightblue" : "#6677EE");
			text3 = "<color='" + text4 + "'>" + text3 + "</color>";
		}
		bool flag2 = (text != "" || text3 != "") && !IsSimple && useLineBreaks;
		if (flag2)
		{
			builder.Append('\n');
			builder.Append(FormulaScope.Indent);
		}
		builder.Append(text);
		if (text != "")
		{
			builder.Append('(');
		}
		if (num.HasValue)
		{
			builder.Append(num);
			builder.Append(", ");
		}
		if (num2.HasValue)
		{
			builder.Append(num2);
			builder.Append(", ");
		}
		builder.Append(value);
		builder.Append(text3);
		if (AddBracketsAroundInnerCaption)
		{
			if (IsSimple)
			{
				builder.Append('[');
			}
			else
			{
				builder.AppendIndentedFormula('[');
			}
		}
		using (FormulaScope.Enter((AddBracketsAroundInnerCaption || flag2) && useLineBreaks))
		{
			builder.Append(GetInnerCaption(useLineBreaks));
		}
		if (AddBracketsAroundInnerCaption)
		{
			if (IsSimple)
			{
				builder.Append(']');
			}
			else
			{
				builder.AppendIndentedFormula(']');
			}
		}
		if (Settings.Limit != 0)
		{
			if (IsSimple)
			{
				builder.Append(')');
			}
			else
			{
				builder.AppendIndentedFormula(')');
				if (useLineBreaks)
				{
					builder.Append('\n');
				}
			}
		}
		return builder.ToString();
	}
}
