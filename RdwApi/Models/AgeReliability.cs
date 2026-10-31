using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RdwApi.Models;

// 1 ligne = 1 marque / modèle / carburant / âge (table dbt marts.agg_reliability_by_age)
[Keyless]
[Table("agg_reliability_by_age", Schema = "marts")]
public class AgeReliability
{
    [Column("brand")] public string Brand { get; set; } = "";
    [Column("model")] public string Model { get; set; } = "";
    [Column("fuel_type")] public string FuelType { get; set; } = "";
    [Column("age_years")] public int AgeYears { get; set; }
    [Column("nb_inspections")] public long NbInspections { get; set; }
    [Column("defect_rate_pct")] public decimal DefectRatePct { get; set; }
    [Column("avg_defects")] public decimal AvgDefects { get; set; }
}
