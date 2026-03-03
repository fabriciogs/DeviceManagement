using DeviceManagement.Application.DTOs;
using DeviceManagement.Application.Services;
using DeviceManagement.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace DeviceManagement.API.Controllers;

/// <summary>
/// Manage Devices CRUD operations and queries.
/// </summary>
/// <param name="service"></param>
//[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[Route("api/[controller]")]
[ApiController]
public class DevicesController(IDeviceService service) : ControllerBase
{
    /// <summary>Fetches a single device by Id.</summary>
    /// <param name="id">Device Id (GUID)</param>
    /// <returns>A single device by Id</returns>
    /// <response code="200">A single device by Id</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Device not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeviceDTO>> GetById(Guid id)
    {
        var device = await service.GetByIdAsync(id);
        return device == null ? NotFound() : Ok(device);
    }

    /// <summary>Fetches all devices with pagination.</summary>
    /// <param name="page">Page number (int), min: 1.</param>
    /// <param name="size">Page size (int), max: 100.</param>
    /// <returns>Paged list od all devices.</returns>
    /// <response code="200">Paged list of all devices.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    public async Task<ActionResult<PagedResult<DeviceDTO>>> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var devices = await service.GetAllAsync(page, size);
        return Ok(devices);
    }

    /// <summary>Fetches devices by brand.</summary>
    /// <param name="brand">Device brand</param>
    /// <returns>All devices that matches the 'brand' param</returns>
    /// <response code="200">Devices that matches the 'brand' param</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">No devices found for specified brand</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("brand/{brand}")]
    public async Task<ActionResult<IEnumerable<DeviceDTO>>> GetByBrand(string brand)
    {
        var devices = await service.GetByBrandAsync(brand);
        return !devices.Any() ? NotFound() : Ok(devices);
    }

    /// <summary>Fetches devices by state.</summary>
    /// <param name="state"></param>
    /// <returns></returns>
    /// <response code="200"></response>
    /// <response code="400">Invalid state provided.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("state/{state}")]
    public async Task<ActionResult<IEnumerable<DeviceDTO>>> GetByState(string state)
    {
        if (!Enum.TryParse<DeviceState>(state, true, out var parsedState))
        {
            return BadRequest("Invalid state.");
        }
        var devices = await service.GetByStateAsync(parsedState);
        return Ok(devices);
    }

    /// <summary>Creates a new device.</summary>
    /// <param name="dto"></param>
    /// <response code="201">Device created successfully.</response>
    /// <response code="400">Bad request, check error(s).</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    public async Task<ActionResult<DeviceDTO>> Create([FromBody] CreateDeviceDTO dto)
    {
        var device = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = device.Id }, device);
    }

    /// <summary>Fully updates an existing device (PUT).</summary>
    /// <param name="id">Device ID (GUID)</param>
    /// <param name="dto"></param>
    /// <response code="204">Device updated successfully.</response>
    /// <response code="400">Bad request, check error(s).</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateDeviceDTO dto)
    {
        await service.UpdateAsync(id, dto);
        return NoContent();
    }

    /// <summary>Partially updates an existing device (PATCH).</summary>
    /// <param name="id">Device ID (GUID)</param>
    /// <param name="dto"></param>
    /// <response code="204"></response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult> PartialUpdate(Guid id, [FromBody] UpdateDeviceDTO dto)
    {
        await service.PartialUpdateAsync(id, dto);
        return NoContent();
    }

    /// <summary>Deletes a device by ID.</summary>
    /// <param name="id">Device ID (GUID)</param>
    /// <response code="204">Device deleted successfully.</response>
    /// <response code="400">Bad request, check error(s).</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}