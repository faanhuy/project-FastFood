using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Addresses.Commands.DeleteAddress;

public class DeleteAddressCommandHandler(
    IUserAddressRepository addressRepository,
    IUnitOfWork unitOfWork,
    ILocalizationService localization,
    ICurrentLanguageService languageService) : IRequestHandler<DeleteAddressCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(DeleteAddressCommand command, CancellationToken cancellationToken)
    {
        var address = await addressRepository.GetByIdAsync(command.AddressId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserAddress), command.AddressId);

        if (address.UserId != command.UserId)
            throw new UnauthorizedException("error.address_delete_unauthorized", null);

        // If deleting the default address, promote the oldest remaining address
        if (address.IsDefault)
        {
            var remaining = await addressRepository.GetByUserIdAsync(command.UserId, cancellationToken);
            var oldest = remaining
                .Where(a => a.Id != command.AddressId)
                .OrderBy(a => a.CreatedAt)
                .FirstOrDefault();

            if (oldest is not null)
                oldest.SetAsDefault();
        }

        addressRepository.Remove(address);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true,
            localization.GetMessage("success.address_deleted", languageService.Language));
    }
}
