using ContainerFlow.Container.Api.Contracts;
using ContainerFlow.Container.Api.Domain;
using ContainerFlow.Contracts.Enums;

namespace ContainerFlow.Container.Api.Services;

public interface IContainerService
{
    Task<IReadOnlyList<Domain.Container>> GetAllAsync(ContainerStatus? status, CancellationToken ct = default);
    Task<Domain.Container?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Domain.Container> RegisterAsync(RegisterContainerRequest request, CancellationToken ct = default);
    Task<Domain.Container> AllocateAsync(Guid id, AllocateContainerRequest request, Guid correlationId, CancellationToken ct = default);
    Task<Domain.Container> MoveAsync(Guid id, MoveContainerRequest request, Guid correlationId, CancellationToken ct = default);
    Task<UtilizationResponse> GetUtilizationAsync(CancellationToken ct = default);
}