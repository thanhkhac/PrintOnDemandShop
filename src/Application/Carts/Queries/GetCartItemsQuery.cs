using CleanArchitectureBase.Application.Carts.Dtos.Response;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;

namespace CleanArchitectureBase.Application.Carts.Queries;

[Authorize]
public class GetCartItemsQuery : PaginatedQuery, IRequest<List<CartItemResponseDto>>
{

}

public class GetCartItemsQueryHandler : IRequestHandler<GetCartItemsQuery, List<CartItemResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetCartItemsQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    //TODO: Nếu có bản design thì sẽ để hình ảnh của bản design
    public async Task<List<CartItemResponseDto>> Handle(GetCartItemsQuery request, CancellationToken cancellationToken)
    {
        // @formatter:off
        var cartItems = await _context.CartItems
            .Include(x=>x.ProductDesign)
                .ThenInclude(y=>y!.DesignTemplates)
            .Include(ci => ci.ProductVariant)
                .ThenInclude(pv => pv!.Product)
            .Include(ci => ci.ProductVariant)
                .ThenInclude(pv => pv!.VariantValues)
                    .ThenInclude(vv => vv.ProductOptionValue)
                        .ThenInclude(pov => pov!.ProductOption)
            .Include(ci => ci.ProductVariant)
            .ThenInclude(pv => pv!.VariantValues)
                .ThenInclude(vv => vv.ProductOptionValue)
                    .ThenInclude(pov => pov!.Images)
            .Include(ci => ci.ProductDesign)
            .Where(ci => ci.CreatedBy == _user.UserId
                         && !ci.ProductVariant!.IsDeleted
                         && !ci.ProductVariant.Product!.IsDeleted)
            .OrderByDescending(ci => ci.LastModifiedAt)
            .ToListAsync(cancellationToken);
        // @formatter:on

        var imageDic = new Dictionary<Guid, string>();

        foreach (var item in cartItems)
        {
            if (item.ProductDesign != null)
            {
               var image =  item.ProductDesign.DesignTemplates.First().DesignImageUrl;
               if (image != null)
               {
                   imageDic.Add(item.Id, image);
                   break;
               }
            }
            else
            {
                foreach (var variantValue in item.ProductVariant!.VariantValues)
                {
                    if (variantValue.ProductOptionValue?.ProductOption?.Name == "COLOR")
                    {
                        var image = variantValue.ProductOptionValue.Images.First().ImageUrl;
                        if (image != null)
                        {
                            imageDic.Add(item.Id, image);
                            break;
                        }
                    }
                }
            }
        }


        return cartItems.Select(ci => new CartItemResponseDto
        {
            CartId = ci.Id,
            ProductId = ci.ProductVariant!.ProductId,
            ProductName = ci.ProductVariant.Product!.Name,
            ProductImageUrl =  imageDic.TryGetValue(ci.Id, out string? value)? value :  ci.ProductVariant.Product.ImageUrl ,
            ProductVariantId = ci.ProductVariantId,
            ProductVariantSku = ci.ProductVariant.Sku,
            UnitPrice = ci.ProductVariant.UnitPrice,
            Stock = ci.ProductVariant.Stock,
            Quantity = ci.Quantity,
            TotalPrice = ci.ProductVariant.UnitPrice * ci.Quantity,
            ProductDesignId = ci.ProductDesignId,
            VariantOptions = ci.ProductVariant.VariantValues.Select(vv => new ProductVariantOptionDto
            {
                OptionName = vv.ProductOptionValue?.ProductOption?.Name,
                OptionValue = vv.ProductOptionValue?.Value
            }).ToList(),
            CreatedAt = ci.CreatedAt
        }).ToList();
    }
}
