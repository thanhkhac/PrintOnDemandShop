using System.Linq.Dynamic.Core;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Mappings;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;

namespace CleanArchitectureBase.Application.TokenPackages.Queries.Admin;


public class PaidTokenPackageDto
{
    public Guid Id { get; set; }
    public string? PaymentCode { get; set; }
    public long Price { get; set; }
    public int TokenAmount { get; set; }
    public CreatedByDto? CreatedByDto { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}

[Authorize(Roles = Domain.Constants.Roles.Administrator + "," + Domain.Constants.Roles.Moderator)]
public class AdminGetTokenPackageHistoryQuery : PaginatedQuery, IRequest<PaginatedList<PaidTokenPackageDto>>
{
    
}


public class AdminGetTokenPackageHistoryQueryHandler 
    : IRequestHandler<AdminGetTokenPackageHistoryQuery, PaginatedList<PaidTokenPackageDto>>
{
    private readonly IApplicationDbContext _context;

    public AdminGetTokenPackageHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<PaidTokenPackageDto>> Handle(
        AdminGetTokenPackageHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var query = from p in _context.UserTokenPackages
            join u in _context.DomainUsers on p.UserId equals u.Id
            where p.IsPaid && !u.IsDeleted
            orderby p.TimeEnd descending
            select new PaidTokenPackageDto
            {
                Id = p.Id,
                PaymentCode = p.PaymentCode,
                Price = p.Price,
                TokenAmount = p.TokenAmount,
                CreatedAt = p.TimeEnd,
                CreatedByDto = new CreatedByDto
                {
                    UserId = u.Id,
                    Name = u.FullName,
                    Email = u.Email
                }
            };

        return await query.PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
