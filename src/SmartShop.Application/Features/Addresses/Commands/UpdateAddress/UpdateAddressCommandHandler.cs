using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Addresses.Commands.UpdateAddress;

public class UpdateAddressCommandHandler(
    IUserAddressRepository addressRepository,
    IUnitOfWork unitOfWork,
    ILocalizationService localization,
    ICurrentLanguageService languageService) : IRequestHandler<UpdateAddressCommand, ApiResponse<AddressDto>>
{
    public async Task<ApiResponse<AddressDto>> Handle(UpdateAddressCommand command, CancellationToken cancellationToken)
    {
        var address = await addressRepository.GetByIdAsync(command.AddressId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserAddress), command.AddressId);

        if (address.UserId != command.UserId)
            throw new UnauthorizedException("error.address_update_unauthorized", null);

        address.Update(
            command.Label,
            command.RecipientName,
            command.Phone,
            command.Street,
            command.ProvinceId,
            command.WardId);

        addressRepository.Update(address);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<AddressDto>.Ok(AddressDto.From(address),
            localization.GetMessage("success.address_updated", languageService.Language));
    }
}
