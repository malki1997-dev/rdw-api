namespace RdwApi.Models;

public record PagedResult<T>(int Page, int PageSize, int Total, IReadOnlyList<T> Items);

public record ModelReliabilityDto(string Brand, string Model, string FuelType, long NbInspections,
    decimal DefectRatePct, decimal AvgAgeYears, decimal AvgDefects);

public record BrandReliabilityDto(string Brand, long NbInspections, decimal DefectRatePct, decimal AvgAgeYears);

public record AgePointDto(int AgeYears, long NbInspections, decimal DefectRatePct, decimal AvgDefects);

public record AgeCurveDto(string Brand, string Model, string FuelType, IReadOnlyList<AgePointDto> Points);
