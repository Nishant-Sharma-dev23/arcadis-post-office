namespace Postal.Api.Domain;

/// <summary>Thrown when an action breaks a business rule (returned as 409 Conflict).</summary>
public class DomainException(string message) : Exception(message);
