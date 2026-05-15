using MediatR;
using SmartShop.Application.Common.Models;

namespace SmartShop.Application.Features.Combos.Queries.GetComboById;

public record GetComboByIdQuery(Guid Id) : IRequest<ApiResponse<ComboDto>>;
