namespace WarehouseKG.Application.Features.Reports.Dtos;

public class OperationStatusCountsDto
{
    public string Operation { get; set; } = string.Empty;

    public int Draft { get; set; }

    public int Completed { get; set; }

    public int Cancelled { get; set; }

    public int Total { get; set; }
}

public class StockMovementSummaryReportDto
{
    public List<OperationStatusCountsDto> Operations { get; set; } = new();
}
