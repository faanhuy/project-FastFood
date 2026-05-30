using FluentAssertions;
using Moq;
using Xunit;
using SmartShop.Application.Features.Addresses.Commands.UpdateAddress;
using SmartShop.Application.Interfaces;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Tests.Addresses;

public class UpdateAddressCommandHandlerTests
{
    private readonly Mock<IUserAddressRepository> _addressRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ILocalizationService> _localization = new();
    private readonly Mock<ICurrentLanguageService> _langService = new();

    public UpdateAddressCommandHandlerTests()
    {
        _localization.Setup(l => l.GetMessage(It.IsAny<string>(), It.IsAny<string>(), null)).Returns("ok");
        _langService.Setup(s => s.Language).Returns("vi");
    }

    private UpdateAddressCommandHandler CreateHandler() =>
        new(_addressRepo.Object, _uow.Object, _localization.Object, _langService.Object);

    private static UpdateAddressCommand ValidCommand(Guid addressId, Guid userId) =>
        new(addressId, userId, "Nhà riêng mới", "Nguyen Van B", "0909999888",
            "456 Đường Nguyễn Huệ");

    [Fact]
    public async Task UpdateAddress_ValidRequest_ReturnsUpdatedDto()
    {
        var userId = Guid.NewGuid();
        var address = UserAddress.Create(userId, "Nhà riêng cũ", "Nguyen Van A",
            "0901234567", "123 Đường Lê Lợi");
        var addressId = address.Id;

        _addressRepo.Setup(r => r.GetByIdAsync(addressId, default)).ReturnsAsync(address);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var command = ValidCommand(addressId, userId);
        var result = await CreateHandler().Handle(command, default);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Label.Should().Be("Nhà riêng mới");
        result.Data.RecipientName.Should().Be("Nguyen Van B");
        result.Data.Phone.Should().Be("0909999888");
        result.Data.Street.Should().Be("456 Đường Nguyễn Huệ");
    }

    [Fact]
    public async Task UpdateAddress_AddressNotFound_ThrowsNotFoundException()
    {
        var addressId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _addressRepo.Setup(r => r.GetByIdAsync(addressId, default)).ReturnsAsync((UserAddress?)null);

        var act = () => CreateHandler().Handle(ValidCommand(addressId, userId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAddress_WrongOwner_ThrowsUnauthorizedException()
    {
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var address = UserAddress.Create(ownerId, "Nhà riêng", "Nguyen Van A",
            "0901234567", "123 Đường Lê Lợi");
        var addressId = address.Id;

        _addressRepo.Setup(r => r.GetByIdAsync(addressId, default)).ReturnsAsync(address);

        var command = ValidCommand(addressId, requesterId);
        var act = () => CreateHandler().Handle(command, default);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task UpdateAddress_WithProvinceIdAndWardId_SavesThem()
    {
        var userId = Guid.NewGuid();
        var address = UserAddress.Create(userId, "Nhà riêng", "Nguyen Van A",
            "0901234567", "123 Đường Lê Lợi");
        var addressId = address.Id;

        _addressRepo.Setup(r => r.GetByIdAsync(addressId, default)).ReturnsAsync(address);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var command = new UpdateAddressCommand(
            addressId, userId, "Nhà riêng mới", "Nguyen Van B", "0909999888",
            "456 Đường Nguyễn Huệ", ProvinceId: 1, WardId: 1001);

        var result = await CreateHandler().Handle(command, default);

        result.Data.Should().NotBeNull();
        result.Data!.ProvinceId.Should().Be(1);
        result.Data.WardId.Should().Be(1001);
    }

    [Fact]
    public async Task UpdateAddress_CallsRepositoryUpdate_Once()
    {
        var userId = Guid.NewGuid();
        var address = UserAddress.Create(userId, "Nhà riêng", "Nguyen Van A",
            "0901234567", "123 Đường Lê Lợi");
        var addressId = address.Id;

        _addressRepo.Setup(r => r.GetByIdAsync(addressId, default)).ReturnsAsync(address);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await CreateHandler().Handle(ValidCommand(addressId, userId), default);

        _addressRepo.Verify(r => r.Update(It.IsAny<UserAddress>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
