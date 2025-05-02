namespace AlloVoisinClone.Domain.Common
{
    /// <summary>
    /// Marker interface for aggregate roots
    /// </summary>
    /// <remarks>
    /// In Domain-Driven Design, an aggregate root is the entry point to an aggregate.
    /// Repositories should only return aggregate roots, not their children.
    /// </remarks>
    public interface IAggregateRoot
    {
    }
}
