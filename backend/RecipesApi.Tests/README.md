# API Testing Guide

This directory contains automated tests for the Recipes PWA API.

## Testing Approach

We use **manual REST API testing** instead of complex .NET integration test setup because:
- ✅ Simpler to write and maintain
- ✅ No database provider conflicts
- ✅ Tests actual HTTP endpoints (closest to real usage)
- ✅ Works with any REST client (Postman, VS Code REST Client, curl)
- ✅ Easy to run in CI/CD pipelines

## Available Test Files

### 1. **api-tests.http** - REST Client Tests
Interactive API tests for VS Code REST Client extension.

**Setup:**
1. Install [REST Client extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) in VS Code
2. Open `api-tests.http`
3. Click "Send Request" above each `###` separator

**Features:**
- Variables with timestamps for unique test data
- Response chaining (extract values from responses)
- Covers all user-facing functionality
- 40+ test scenarios

**Test Suites:**
- Authentication (register, login, errors)
- Recipes (list, create, view, search)
- Points System (top-up, purchase, transactions)
- Subscriptions (list, purchase, access)
- Trading (offer, accept, decline)
- Error Handling (auth errors, insufficient balance)
- Rate Limiting

### 2. **SMOKE_TESTS.md** - Manual Testing Guide
Step-by-step manual testing instructions with pass/fail criteria.

Use this for:
- Pre-production deployment validation
- Testing UI + API together
- Cross-browser testing
- PWA functionality
- Performance testing

**Quick Smoke Test (5 minutes):**
1. Register + Login
2. Buy a recipe
3. Top up points
4. Create trade offer
5. Access admin panel

### 3. **test-api.sh** / **run-api-tests.sh** - Automated Scripts
Bash scripts for automated testing (note: currently need updates for cookie-based auth).

## Running Tests

### Option 1: VS Code REST Client (Recommended)

```bash
# 1. Start API
cd backend/RecipesApi
dotnet run

# 2. Open api-tests.http in VS Code
# 3. Click "Send Request" for each test
```

### Option 2: Postman

Import `api-tests.http` or create manual requests:

```
POST http://localhost:5010/api/auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "password123",
  "username": "testuser"
}
```

### Option 3: curl (Command Line)

```bash
# Register
curl -X POST http://localhost:5010/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"pass123","username":"testuser"}'

# Login (saves cookie to file)
curl -c cookies.txt -X POST http://localhost:5010/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"pass123"}'

# Get profile (uses cookie)
curl -b cookies.txt http://localhost:5010/api/users/me
```

## Authentication Notes

**Important:** The API uses **httpOnly cookies** for JWT tokens (security best practice).

This means:
- Token is NOT in response body
- Token is in `Set-Cookie` header
- You must handle cookies in requests:
  - REST Client: Automatic cookie handling
  - Postman: Enable "Automatically follow redirects" and cookie jar
  - curl: Use `-c cookies.txt` (save) and `-b cookies.txt` (send)

## Test Data

Tests use timestamp-based data to avoid conflicts:
- Email: `test_{timestamp}@example.com`
- Username: `testuser_{timestamp}`
- Recipe titles: `Test Recipe {timestamp}`

This ensures:
- Tests can run multiple times
- No database cleanup needed
- Parallel test execution possible

## Expected Results

### Successful Test Run
```
✓ PASS: User registered
✓ PASS: Login successful
✓ PASS: Got user profile
✓ PASS: Retrieved recipe list
✓ PASS: Recipe created
✓ PASS: Points topped up
✓ PASS: Transactions retrieved

ALL TESTS PASSED (7/7)
```

### Common Issues

**"API is not accessible"**
- Ensure backend is running: `dotnet run` in `backend/RecipesApi`
- Check port 5010 is not in use: `lsof -ti:5010`

**"401 Unauthorized"**
- Cookie not sent with request
- Token expired (re-login)
- Check Authorization header format

**"400 Bad Request"**
- Invalid JSON format
- Missing required fields
- Invalid email format

## CI/CD Integration

### GitHub Actions Example

```yaml
name: API Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Start API
        run: |
          cd backend/RecipesApi
          dotnet run &
          sleep 10  # Wait for API to start

      - name: Run Tests
        run: |
          cd backend/RecipesApi.Tests
          bash test-api.sh
```

## Future Improvements

- [ ] Add Newman/Postman collection for automated CI testing
- [ ] Add more comprehensive error scenario tests
- [ ] Add performance/load testing
- [ ] Add security testing (SQL injection, XSS)
- [ ] Add contract testing
- [ ] Add E2E tests with Playwright

## Related Documentation

- [TESTING.md](../../dev-tools/docs/TESTING.md) - Overall testing strategy
- [SMOKE_TESTS.md](./SMOKE_TESTS.md) - Manual testing guide
- [API Documentation](http://localhost:5010/swagger) - Swagger UI (when API running)

## Contributing

When adding new API endpoints:

1. Add tests to `api-tests.http`
2. Add manual test steps to `SMOKE_TESTS.md`
3. Update this README if needed
4. Run full test suite before committing

## Support

For issues or questions:
- Check [TESTING.md](../../dev-tools/docs/TESTING.md)
- Review Swagger docs at http://localhost:5010/swagger
- Check application logs in console
