namespace Bud.Server.Domain.Abstractions;

public sealed class DomainInvariantException(string message) : Exception(message);
