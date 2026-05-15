using MediatR;
using SmartShop.Application.Common.Models;

namespace SmartShop.Application.Features.Combos.Commands.DeleteCombo;

public record DeleteComboCommand(Guid Id) : IRequest<ApiResponse<object?>>;
