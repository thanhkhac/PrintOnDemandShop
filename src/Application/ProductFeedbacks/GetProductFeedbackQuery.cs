using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;

namespace CleanArchitectureBase.Application.ProductFeedbacks;

public class FeedbackDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? Feedback { get; set; }
    public int Rating { get; set; }
    public CreatedByDto? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class GetProductFeedbackQuery : IRequest<ProductFeedbackListDto>
{
    public Guid ProductId { get; set; }
}

public class ProductFeedbackListDto
{
    public Guid ProductId { get; set; }
    public double AverageRating { get; set; }
    public int Total { get; set; }
    public List<FeedbackDto> Feedbacks { get; set; } = new();
}

public class GetProductFeedbackQueryHandler : IRequestHandler<GetProductFeedbackQuery, ProductFeedbackListDto>
{
    private readonly IApplicationDbContext _context;

    public GetProductFeedbackQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductFeedbackListDto> Handle(GetProductFeedbackQuery request, CancellationToken cancellationToken)
    {
        var feedbacks = await _context.ProductFeedbacks
        .Include(x => x.CreatedByUser)
            .Where(f => f.ProductId == request.ProductId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FeedbackDto
            {
                Id = f.Id,
                ProductId = f.ProductId,
                Feedback = f.Feedback,
                Rating = f.Rating,
                CreatedBy = new CreatedByDto
                {
                    UserId = f.CreatedBy,
                    Name = f.CreatedByUser!.FullName
                },
                CreatedAt = f.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new ProductFeedbackListDto
        {
            ProductId = request.ProductId,
            Total = feedbacks.Count,
            AverageRating = feedbacks.Any() ? Math.Round(feedbacks.Average(f => f.Rating), 2) : 0,
            Feedbacks = feedbacks
        };
    }
}
