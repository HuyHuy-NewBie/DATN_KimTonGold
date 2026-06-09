using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileApi.Contracts;
using MobileApi.Data;
using MobileApi.Models;
using MobileApi.Services;

namespace MobileApi.Controllers;

[ApiController]
[Route("api/branches")]
[Authorize(Policy = Policies.BackOffice)]
public class BranchesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BranchesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BranchDto>>> GetBranches(bool includeInactive = false)
    {
        var query = _context.Branches.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(branch => branch.IsActive);
        }

        return await query
            .OrderBy(branch => branch.BranchName)
            .Select(branch => DtoMapper.ToBranchDto(branch))
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Policy = Policies.BranchesManage)]
    public async Task<ActionResult<BranchDto>> Create(CreateBranchRequest request)
    {
        var branchName = Normalize(request.BranchName);
        if (string.IsNullOrWhiteSpace(branchName))
        {
            return BadRequest(new ApiError("Tên chi nhánh không được để trống."));
        }

        if (await _context.Branches.AnyAsync(branch => branch.BranchName.ToLower() == branchName.ToLower()))
        {
            return Conflict(new ApiError("Tên chi nhánh này đã tồn tại."));
        }

        var branch = new Branch
        {
            BranchName = branchName,
            Address = NormalizeOrNull(request.Address),
            PhoneNumber = NormalizeOrNull(request.PhoneNumber),
            IsActive = true
        };

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBranches), new { id = branch.Id }, DtoMapper.ToBranchDto(branch));
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Policy = Policies.BranchesManage)]
    public async Task<ActionResult<BranchDto>> UpdateStatus(int id, UpdateBranchStatusRequest request)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch == null)
        {
            return NotFound(new ApiError("Không tìm thấy chi nhánh."));
        }

        branch.IsActive = request.IsActive;
        await _context.SaveChangesAsync();
        return DtoMapper.ToBranchDto(branch);
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static string? NormalizeOrNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
