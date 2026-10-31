using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RdwApi.Data;
using RdwApi.Models;

namespace RdwApi.Controllers;

[ApiController]
[Route("api/v1")]
public class ReliabilityController(RdwDbContext db) : ControllerBase
{
    /// <summary>Liste paginée des modèles (voitures particulières, contrôles APK).</summary>
    [HttpGet("models")]
    public async Task<ActionResult<PagedResult<ModelReliabilityDto>>> GetModels(
        [FromQuery] string? brand, [FromQuery] string? model, [FromQuery] string? fuelType,
        [FromQuery] int minInspections = 100, [FromQuery] string sort = "defect_rate",
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest("page doit être >= 1 et pageSize compris entre 1 et 100.");
        if (sort != "defect_rate" && sort != "inspections")
            return BadRequest("sort doit valoir 'defect_rate' ou 'inspections'.");

        var q = db.ModelReliabilities.AsNoTracking().Where(x => x.NbInspections >= minInspections);
        if (!string.IsNullOrWhiteSpace(brand))
            q = q.Where(x => x.Brand.ToUpper() == brand.ToUpper());
        if (!string.IsNullOrWhiteSpace(model))
            q = q.Where(x => x.Model.ToUpper().Contains(model.ToUpper()));
        if (!string.IsNullOrWhiteSpace(fuelType))
            q = q.Where(x => x.FuelType.ToUpper() == fuelType.ToUpper());

        var ordered = sort == "inspections"
            ? q.OrderByDescending(x => x.NbInspections)
            : q.OrderBy(x => x.DefectRatePct).ThenByDescending(x => x.NbInspections);
        var stable = ordered.ThenBy(x => x.Brand).ThenBy(x => x.Model).ThenBy(x => x.FuelType);

        var total = await q.CountAsync(ct);
        var items = await stable.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new ModelReliabilityDto(x.Brand, x.Model, x.FuelType, x.NbInspections,
                x.DefectRatePct, x.AvgAgeYears, x.AvgDefects))
            .ToListAsync(ct);

        return new PagedResult<ModelReliabilityDto>(page, pageSize, total, items);
    }

    /// <summary>Classement par marque. Attention : comparer aussi l'âge moyen (biais d'âge).</summary>
    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<BrandReliabilityDto>>> GetBrands(
        [FromQuery] int minInspections = 10000, CancellationToken ct = default)
    {
        // Les agrégats par modèle contiennent des comptes : somme pondérée = résultat exact par marque.
        var rows = await db.ModelReliabilities.AsNoTracking()
            .GroupBy(x => x.Brand)
            .Select(g => new
            {
                Brand = g.Key,
                Inspections = g.Sum(x => x.NbInspections),
                WithDefect = g.Sum(x => x.NbWithDefect),
                AgeWeighted = g.Sum(x => x.AvgAgeYears * x.NbInspections)
            })
            .Where(x => x.Inspections >= minInspections)
            .ToListAsync(ct);

        var result = rows
            .Select(r => new BrandReliabilityDto(r.Brand, r.Inspections,
                Math.Round(100m * r.WithDefect / r.Inspections, 1),
                Math.Round(r.AgeWeighted / r.Inspections, 1)))
            .OrderBy(r => r.DefectRatePct)
            .ToList();
        return Ok(result);
    }

    /// <summary>Taux de défaut par âge pour un modèle, une courbe par carburant.</summary>
    [HttpGet("models/{brand}/{model}/age-curve")]
    public async Task<ActionResult<IReadOnlyList<AgeCurveDto>>> GetAgeCurve(
        string brand, string model, [FromQuery] string? fuelType, CancellationToken ct = default)
    {
        var q = db.AgeReliabilities.AsNoTracking()
            .Where(x => x.Brand.ToUpper() == brand.ToUpper() && x.Model.ToUpper() == model.ToUpper());
        if (!string.IsNullOrWhiteSpace(fuelType))
            q = q.Where(x => x.FuelType.ToUpper() == fuelType.ToUpper());

        var rows = await q.OrderBy(x => x.FuelType).ThenBy(x => x.AgeYears).ToListAsync(ct);
        if (rows.Count == 0)
            return NotFound($"Aucune donnée pour {brand} {model}.");

        var curves = rows.GroupBy(x => x.FuelType)
            .Select(g => new AgeCurveDto(brand.ToUpper(), model.ToUpper(), g.Key,
                g.Select(p => new AgePointDto(p.AgeYears, p.NbInspections, p.DefectRatePct, p.AvgDefects)).ToList()))
            .ToList();
        return Ok(curves);
    }
}
