namespace Elzik.Breef.Api.Tests.Functional;

/// <summary>
/// xUnit collection definition for Docker-based tests.
/// Ensures Docker tests run serially to prevent container conflicts.
/// </summary>
[CollectionDefinition("Docker Tests", DisableParallelization = true)]
public class DockerTestsCollection
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
