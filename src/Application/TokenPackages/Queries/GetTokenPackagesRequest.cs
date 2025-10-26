using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.TokenPackages.Queries;

// Request DTO
public class GetTokenPackagesRequest
{
    // public int? MinTokenAmount { get; set; }
    // public int? MaxTokenAmount { get; set; }
    // public int? MinPrice { get; set; }
    // public int? MaxPrice { get; set; }
}

// Query Handler
public class GetTokenPackagesQueryHandler
{
    private readonly IApplicationDbContext _context;

    public GetTokenPackagesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TokenPackage>> Handle(GetTokenPackagesRequest request)
    {
        var query = _context.TokenPackages.AsQueryable();

        // Chỉ lấy package chưa bị xóa
        query = query.Where(tp => !tp.IsDeleted);


        // Order by TokenAmount
        query = query.OrderBy(tp => tp.TokenAmount);

        return await query.ToListAsync();
    }
}
