using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RdwApi.Models;

// 1 ligne = 1 marque / modèle / carburant (table dbt marts.agg_reliability_by_model)
[Keyless]
[Table("agg_reliability_by_model", Schema = "marts")]
public class ModelReliability
{
    [Column("brand")] public string Brand { get; set; } = "";
    [Column("model")] public string Model { get; set; } = "";
    [Column("fuel_type")] public string FuelType { get; set; } = "";
    [Column("nb_inspections")] public long NbInspections { get; set; }
    [Column("nb_with_defect")] public long NbWithDefect { get; set; }
    [Column("defect_rate_pct")] public decimal DefectRatePct { get; set; }
    [Column("avg_age_years")] public decimal AvgAgeYears { get; set; }
    [Column("avg_defects")] public decimal AvgDefects { get; set; }
}
