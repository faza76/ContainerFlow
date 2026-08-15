namespace ContainerFlow.Container.Api.Domain;

/// <summary>Thrown when a container status transition (or allocation) is invalid.</summary>
public sealed class InvalidContainerTransitionException : InvalidOperationException
{
    public InvalidContainerTransitionException(string message) : base(message) { }
}