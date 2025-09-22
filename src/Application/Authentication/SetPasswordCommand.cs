﻿using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;

namespace CleanArchitectureBase.Application.Authentication;

[Authorize]
public class SetPasswordCommand : IRequest
{
    public required string Password { get; set; }
}


public class SetPasswordCommandValidator : AbstractValidator<SetPasswordCommand>
{
    public SetPasswordCommandValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);
    }
}

public class SetPasswordCommandHandler : IRequestHandler<SetPasswordCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public SetPasswordCommandHandler(IIdentityService identityService, IUser user)
    {
        _identityService = identityService;
        _user = user;
    }
    
    public async Task Handle(SetPasswordCommand request, CancellationToken cancellationToken)
    {
        await _identityService.TrySetPasswordAsync(_user.UserId!.Value, request.Password);
    }
}
