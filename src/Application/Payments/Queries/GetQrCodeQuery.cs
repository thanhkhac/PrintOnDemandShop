using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Common.Settings;
using CleanArchitectureBase.Domain.Constants;
using Microsoft.Extensions.Options;

namespace CleanArchitectureBase.Application.Payments.Queries;

// [Authorize]
public class GetQrCodeQuery : IRequest<string>
{
    public int Amount { get; set; }
    public string? PaymentCode { get; set; }
}

public class GetQrCodeQueryValidator : AbstractValidator<GetQrCodeQuery>
{
    public GetQrCodeQueryValidator()
    {
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(2000);

        RuleFor(x => x.PaymentCode).NotEmpty();
    }
}

public class GetQrCodeQueryHandler : IRequestHandler<GetQrCodeQuery, string>
{
    private readonly IApplicationDbContext _context;
    // private readonly IUser _user;
    private readonly PaymentSettings _settings;

    public GetQrCodeQueryHandler(IApplicationDbContext context, IOptions<PaymentSettings> paymentSettings, IUser user)
    {
        this._context = context;
        // _user = user;
        _settings = paymentSettings.Value;
    }

    public async Task<string> Handle(GetQrCodeQuery request, CancellationToken cancellationToken)
    {
        // var user = await _context.DomainUsers.FirstOrDefaultAsync(x => x.Id == _user.UserId, cancellationToken: cancellationToken);
        await Task.CompletedTask;
        // if (user == null) throw new ErrorCodeException(ErrorCodes.USER_NOTFOUND);
        var bank = _settings.Bank;
        var account = _settings.Account;
        var virtualAccount = _settings.VirtualAccount;
        var url = "";
        
        if (bank!.ToLower() == "bidv")
            url = $"https://qr.sepay.vn/img?acc={account}&bank={bank}&amount={request.Amount}&des={request.PaymentCode}";

        if (bank!.ToLower() != "VPBank".ToLower())
            url = $"https://qr.sepay.vn/img?acc={account}&bank=VPBank&amount={request.Amount}&des={virtualAccount}+{request.PaymentCode}";

        
        return url;
    }
}
