namespace WarehouseKG.Application.Features.Reports.Dtos;

public class OrderStatusBreakdownDto
{
    public string Status { get; set; } = string.Empty;

    public int OrderCount { get; set; }

    public decimal TotalAmount { get; set; }
}

public class SalesSummaryReportDto
{
    public int TotalOrders { get; set; }

    public decimal TotalAmount { get; set; }

    public List<OrderStatusBreakdownDto> ByStatus { get; set; } = new();
}

public class PurchaseSummaryReportDto
{
    public int TotalOrders { get; set; }

    public decimal TotalAmount { get; set; }

    public List<OrderStatusBreakdownDto> ByStatus { get; set; } = new();
}
