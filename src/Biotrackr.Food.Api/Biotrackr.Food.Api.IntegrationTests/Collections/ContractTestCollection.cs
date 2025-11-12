using Biotrackr.Food.Api.IntegrationTests.Fixtures;
using Xunit;

namespace Biotrackr.Food.Api.IntegrationTests.Collections;

/// <summary>
/// Collection definition for contract/smoke tests
/// Ensures tests share the same ContractTestFixture instance
/// Per decision-record 2025-10-28-integration-test-project-structure.md
/// </summary>
[CollectionDefinition(nameof(ContractTestCollection))]
public class ContractTestCollection : ICollectionFixture<ContractTestFixture>
{
}
