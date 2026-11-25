using Microsoft.AspNetCore.Mvc;
using QBuild.Business.Interfaces;
using QBuild.Models;

namespace QBuild.API.Controllers;

[ApiController]
[Route("api/bom")]
public sealed class BomController : ControllerBase
{
    private readonly IBomService _bomService;

    public BomController(IBomService bomService)
    {
        _bomService = bomService;
    }

    [HttpGet("tree")]
    [ProducesResponseType(typeof(IReadOnlyList<BomTreeNode>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<BomTreeNode>> GetTree()
    {
        return Ok(_bomService.GetTree());
    }

    [HttpGet("components/{parentName}")]
    [ProducesResponseType(typeof(IReadOnlyList<ComponentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IReadOnlyList<ComponentDto>> GetComponents(string parentName)
    {
        if (string.IsNullOrWhiteSpace(parentName))
        {
            return BadRequest(new { message = "parentName is required." });
        }

        if (!_bomService.Exists(parentName))
        {
            return NotFound(new { message = $"Component '{parentName}' was not found in the BOM." });
        }

        return Ok(_bomService.GetDirectComponents(parentName));
    }
}
