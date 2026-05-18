using System.Collections.Generic;

namespace Kingmaker.Code.Middleware.Metrics;

public class VendorDealMetricsEvent : MetricsEvent
{
	protected override string Name => "vendor_deal";

	public VendorDealMetricsEvent Bought(IEnumerable<string> bought)
	{
		AddParam("bought", bought);
		return this;
	}

	public VendorDealMetricsEvent BoughtAmounts(IEnumerable<string> bought_amounts)
	{
		AddParam("bought_amounts", bought_amounts);
		return this;
	}

	public VendorDealMetricsEvent Sold(IEnumerable<string> sold)
	{
		AddParam("sold", sold);
		return this;
	}

	public VendorDealMetricsEvent SoldAmounts(IEnumerable<string> sold_amounts)
	{
		AddParam("sold_amounts", sold_amounts);
		return this;
	}

	public VendorDealMetricsEvent Money(string money)
	{
		AddParam("money", money);
		return this;
	}

	public VendorDealMetricsEvent Price(string price)
	{
		AddParam("price", price);
		return this;
	}
}
