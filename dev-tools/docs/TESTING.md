# Testing Guide

This document describes the testing strategy and how to add tests to the Recipes PWA project.

## Testing Stack

### Backend (.NET)
- **xUnit** - Testing framework
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing with WebApplicationFactory
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database for isolated tests
- **FluentAssertions** (optional) - More readable assertions

### Frontend - Main App (Nuxt 3)
- **Vitest** - Fast unit testing framework (built into Nuxt)
- **@vue/test-utils** - Vue component testing utilities
- **Playwright** (optional) - E2E testing

### Frontend - Admin Panel (React)
- **Vitest** - Testing framework (faster than Jest)
- **@testing-library/react** - React component testing
- **MSW** (Mock Service Worker) - API mocking for tests

## Backend Testing Setup

### 1. Create Test Project

```bash
cd backend
dotnet new xunit -n RecipesApi.Tests
cd RecipesApi.Tests
dotnet add reference ../RecipesApi/RecipesApi.csproj
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

### 2. Make Program Class Public

In `Program.cs`, add at the end:

```csharp
// Make Program class accessible for integration tests
public partial class Program { }
```

### 3. Create Integration Test Base Class

```csharp
public class IntegrationTestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;

    public IntegrationTestBase()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureServices(services =>
                {
                    // Remove real database
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    // Add in-memory database
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                    });
                });
            });

        Client = Factory.CreateClient();
    }

    protected AppDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
    }
}
```

### 4. Write Tests

```csharp
public class AuthEndpointsTests : IntegrationTestBase
{
    [Fact]
    public async Task Register_WithValidData_ReturnsToken()
    {
        // Arrange
        var registerData = new
        {
            email = "test@example.com",
            password = "password123",
            username = "testuser"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseBody);
        Assert.True(jsonDoc.RootElement.TryGetProperty("token", out _));
    }
}
```

### 5. Run Tests

```bash
dotnet test
dotnet test --verbosity normal  # Verbose output
dotnet test --filter "FullyQualifiedName~AuthEndpoints"  # Run specific tests
```

## Frontend Testing (Nuxt 3)

### 1. Setup Vitest (Already Included)

Vitest is automatically configured in Nuxt 3. Create `vitest.config.ts`:

```typescript
import { defineVitestConfig } from '@nuxt/test-utils/config'

export default defineVitestConfig({
  test: {
    environment: 'nuxt',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html']
    }
  }
})
```

### 2. Install Testing Dependencies

```bash
cd frontend/app
npm install -D @nuxt/test-utils @vue/test-utils vitest happy-dom
```

### 3. Write Component Tests

```typescript
// tests/components/RecipeCard.test.ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import RecipeCard from '~/components/RecipeCard.vue'

describe('RecipeCard', () => {
  it('renders recipe title', () => {
    const wrapper = mount(RecipeCard, {
      props: {
        recipe: {
          id: '1',
          title: 'Test Recipe',
          description: 'Test description',
          price: 10
        }
      }
    })

    expect(wrapper.text()).toContain('Test Recipe')
  })
})
```

### 4. Write Composable Tests

```typescript
// tests/composables/useAuth.test.ts
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { useAuth } from '~/composables/useAuth'

describe('useAuth', () => {
  beforeEach(() => {
    // Reset state before each test
    vi.clearAllMocks()
  })

  it('initializes with null user', () => {
    const { user } = useAuth()
    expect(user.value).toBeNull()
  })

  it('stores user after login', async () => {
    const { login, user } = useAuth()

    // Mock successful login
    await login('test@example.com', 'password')

    expect(user.value).not.toBeNull()
    expect(user.value?.email).toBe('test@example.com')
  })
})
```

### 5. Run Tests

```bash
npm run test        # Run all tests
npm run test:watch  # Watch mode
npm run test:coverage  # Generate coverage report
```

## Frontend Testing (React Admin)

### 1. Setup Vitest

```bash
cd frontend/admin
npm install -D vitest @testing-library/react @testing-library/jest-dom jsdom
```

Update `vite.config.ts`:

```typescript
export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'jsdom',
    setupFiles: './tests/setup.ts',
    globals: true
  }
})
```

### 2. Create Setup File

```typescript
// tests/setup.ts
import '@testing-library/jest-dom'
```

### 3. Write Component Tests

```typescript
// tests/components/RecipeTable.test.tsx
import { render, screen } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import RecipeTable from '@/components/RecipeTable'

describe('RecipeTable', () => {
  it('renders recipe data', () => {
    const recipes = [
      { id: '1', title: 'Test Recipe', price: 10 }
    ]

    render(<RecipeTable recipes={recipes} />)

    expect(screen.getByText('Test Recipe')).toBeInTheDocument()
  })
})
```

### 4. Mock API Calls with MSW

```typescript
// tests/mocks/handlers.ts
import { http, HttpResponse } from 'msw'

export const handlers = [
  http.get('/api/recipes', () => {
    return HttpResponse.json({
      recipes: [
        { id: '1', title: 'Test Recipe', price: 10 }
      ]
    })
  })
]
```

### 5. Run Tests

```bash
npm run test
npm run test:ui  # Visual test interface
```

## Test Coverage Goals

Target coverage levels:
- **Backend Critical Paths**: 80%+ (auth, payments, trades)
- **Backend Business Logic**: 70%+
- **Frontend Components**: 60%+
- **Frontend Utils/Composables**: 80%+

## CI/CD Integration

Add to `.github/workflows/ci.yml`:

```yaml
test-backend:
  runs-on: ubuntu-latest
  steps:
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'
    - name: Run tests
      run: |
        cd backend/RecipesApi.Tests
        dotnet test --verbosity normal --collect:"XPlat Code Coverage"

test-frontend:
  runs-on: ubuntu-latest
  steps:
    - uses: actions/checkout@v4
    - name: Setup Node
      uses: actions/setup-node@v4
      with:
        node-version: '20'
    - name: Run tests
      run: |
        cd frontend/app
        npm ci
        npm run test
```

## Best Practices

### Backend
1. **Use InMemory database for tests** - Fast and isolated
2. **Test endpoints, not implementation** - Integration tests over unit tests
3. **Use descriptive test names** - `Register_WithInvalidEmail_ReturnsBadRequest`
4. **Arrange-Act-Assert pattern** - Clear test structure
5. **Clean state between tests** - Each test should be independent

### Frontend
1. **Test user behavior, not implementation** - Focus on what users see/do
2. **Mock external dependencies** - Use MSW for API calls
3. **Avoid testing library internals** - Don't test Vue/React implementation details
4. **Use data-testid sparingly** - Prefer accessible queries (role, label)
5. **Test loading and error states** - Not just happy path

## Common Patterns

### Testing Protected Endpoints

```csharp
[Fact]
public async Task GetProfile_WithValidToken_ReturnsUser()
{
    // Arrange - Register and login
    var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", new
    {
        email = "test@example.com",
        password = "password123",
        username = "testuser"
    });

    var loginResult = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();

    // Add token to requests
    Client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Token);

    // Act
    var response = await Client.GetAsync("/api/users/me");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

### Testing Vue Composables with API Calls

```typescript
import { vi } from 'vitest'

const mockFetch = vi.fn()
global.$fetch = mockFetch

describe('useRecipes', () => {
  it('fetches recipes from API', async () => {
    mockFetch.mockResolvedValueOnce({
      recipes: [{ id: '1', title: 'Test' }]
    })

    const { recipes, fetchRecipes } = useRecipes()
    await fetchRecipes()

    expect(mockFetch).toHaveBeenCalledWith('/api/recipes')
    expect(recipes.value).toHaveLength(1)
  })
})
```

## Resources

- [xUnit Documentation](https://xunit.net/)
- [WebApplicationFactory Guide](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Vitest Documentation](https://vitest.dev/)
- [Vue Testing Handbook](https://lmiller1990.github.io/vue-testing-handbook/)
- [Testing Library Best Practices](https://testing-library.com/docs/react-testing-library/intro)

## Future Improvements

- [ ] Add E2E tests with Playwright
- [ ] Setup visual regression testing (Percy/Chromatic)
- [ ] Add performance testing for critical endpoints
- [ ] Implement mutation testing (Stryker.NET)
- [ ] Add contract testing for API (Pact)
- [ ] Setup test data builders for complex objects
