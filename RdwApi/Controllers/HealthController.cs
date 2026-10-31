using Microsoft.AspNetCore.Mvc;
using RdwApi.Data;

namespace RdwApi.Controllers;

[ApiController]
[Route("api/v1/health")]
public class HealthController(RdwDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var dbOk = await db.Database.CanConnectAsync(ct);
        return dbOk
            ? Ok(new { status = "ok", database = "ok" })
            : StatusCode(503, new { status = "degraded", database = "unreachable" });
    }
}
