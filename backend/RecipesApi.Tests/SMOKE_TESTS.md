# Smoke Tests - Manual Testing Guide

This guide provides step-by-step instructions for manual smoke testing of critical functionality.
Run these tests before deploying to production to ensure core features work correctly.

## Prerequisites

1. Start the backend API: `cd backend/RecipesApi && dotnet run`
2. Start the frontend: `cd frontend/app && npm run dev`
3. Open browser to http://recipes.local

## Test Suite 1: Authentication

### Test 1.1: User Registration
**Expected**: User can create account successfully

1. Navigate to http://recipes.local/register
2. Fill in form:
   - Email: test@example.com
   - Username: testuser
   - Password: Test123!
3. Click "Register"

**✓ Pass Criteria**:
- Redirected to dashboard
- See "Balance: 100 points" (starting balance)
- Username displayed in header

### Test 1.2: User Login
**Expected**: Registered user can login

1. Navigate to http://recipes.local/login
2. Enter credentials from Test 1.1
3. Click "Login"

**✓ Pass Criteria**:
- Redirected to dashboard
- User balance visible
- Can access protected routes

### Test 1.3: Invalid Login
**Expected**: Wrong password rejected

1. Navigate to http://recipes.local/login
2. Enter wrong password
3. Click "Login"

**✓ Pass Criteria**:
- Error message displayed
- Not logged in
- Stays on login page

## Test Suite 2: Recipes

### Test 2.1: View Recipe List
**Expected**: Can browse available recipes

1. Login as user
2. Navigate to /recipes
3. Scroll through list

**✓ Pass Criteria**:
- Recipes displayed in grid
- Each recipe shows: title, description, price, views
- Pagination works

### Test 2.2: View Recipe Details
**Expected**: Can view single recipe

1. From recipe list, click any recipe
2. View details page

**✓ Pass Criteria**:
- Full recipe info displayed
- View count increments
- "Buy" button visible if not owned

### Test 2.3: Purchase Recipe
**Expected**: Can buy recipe with points

1. Find recipe with price ≤ 100 (your balance)
2. Click "Buy Recipe"
3. Confirm purchase

**✓ Pass Criteria**:
- Points deducted from balance
- Recipe added to "My Recipes"
- Transaction visible in history
- Author receives points

## Test Suite 3: Points System

### Test 3.1: Top Up Points
**Expected**: Can add points to balance

1. Go to Dashboard
2. Enter amount (e.g., 50)
3. Click "Add Points"

**✓ Pass Criteria**:
- Balance increases
- Transaction recorded
- Type: "TopUp" in history

### Test 3.2: View Transaction History
**Expected**: All transactions visible

1. On Dashboard, scroll to "Recent Transactions"
2. Review list

**✓ Pass Criteria**:
- All transactions listed
- Shows: type, amount, date
- Icons match transaction type (💰 TopUp, 🛒 Purchase, 💸 Sale)

## Test Suite 4: Subscriptions

### Test 4.1: View Subscription Plans
**Expected**: Can see available subscriptions

1. Navigate to /subscriptions
2. View plans

**✓ Pass Criteria**:
- Plans displayed with details
- Price, duration, benefits shown
- "Subscribe" button visible

### Test 4.2: Purchase Subscription
**Expected**: Can buy subscription

1. Select a plan
2. Click "Subscribe"
3. Confirm purchase

**✓ Pass Criteria**:
- Points deducted
- Subscription shown as "Active"
- Can access subscription-only recipes
- Expiry date displayed

### Test 4.3: Access Subscription Recipe
**Expected**: Subscribed users can view premium recipes

1. With active subscription, go to /recipes
2. Find recipe marked "Requires Subscription"
3. Click to view

**✓ Pass Criteria**:
- Recipe details visible
- No purchase required
- Non-subscribers see "Subscribe to view"

## Test Suite 5: Trading

### Test 5.1: Create Trade Offer
**Expected**: Can offer recipe trade

1. Go to /my-recipes
2. Click "Trade" on owned recipe
3. Search for user by username
4. Select recipe you want
5. Send offer

**✓ Pass Criteria**:
- Offer sent successfully
- Visible in "Outgoing Trades"
- Target user sees in "Incoming Trades"

### Test 5.2: Accept Trade
**Expected**: Recipient can accept trade

1. Login as recipient user
2. Go to /trades
3. View incoming offer
4. Click "Accept"

**✓ Pass Criteria**:
- Recipes exchanged
- Both users see new recipes in "My Recipes"
- Trade status = "Completed"

### Test 5.3: Decline Trade
**Expected**: Can reject unwanted trades

1. View incoming trade
2. Click "Decline"

**✓ Pass Criteria**:
- Trade status = "Declined"
- Recipes not exchanged
- Offer removed from incoming list

## Test Suite 6: Admin Panel

### Test 6.1: Admin Login
**Expected**: Admin can access admin panel

1. Login as admin user
2. Click "Admin Panel" button in header
3. Navigate to http://admin.recipes.local

**✓ Pass Criteria**:
- Dashboard loads
- Statistics displayed
- Navigation menu visible

### Test 6.2: View Analytics
**Expected**: Can see system statistics

1. On admin dashboard, view stats

**✓ Pass Criteria**:
- Total users, recipes, revenue shown
- Recent transactions listed
- Top recipes displayed

### Test 6.3: Manage Recipes
**Expected**: Can edit/delete recipes

1. Navigate to "Recipes" tab
2. Click "Edit" on a recipe
3. Modify title
4. Save

**✓ Pass Criteria**:
- Recipe updated
- Changes visible on frontend
- Edit logged (if auditing enabled)

### Test 6.4: Manage Subscriptions
**Expected**: Can create/edit subscription plans

1. Navigate to "Subscriptions"
2. Click "Create Subscription"
3. Fill in details (name, price, duration)
4. Add recipes to plan
5. Save

**✓ Pass Criteria**:
- Subscription created
- Visible on frontend /subscriptions page
- Users can purchase it

## Performance Tests

### Test P.1: Page Load Time
**Expected**: Pages load quickly

1. Use browser DevTools Network tab
2. Load various pages
3. Measure load time

**✓ Pass Criteria**:
- Dashboard: < 2s
- Recipe list: < 3s
- Recipe details: < 1.5s

### Test P.2: API Response Time
**Expected**: API responds quickly

1. Open DevTools Network tab
2. Filter for "api" requests
3. Perform various actions

**✓ Pass Criteria**:
- GET /api/recipes: < 500ms
- POST /api/auth/login: < 300ms
- GET /api/users/me: < 200ms

## Security Tests

### Test S.1: Protected Routes
**Expected**: Unauthenticated users can't access protected pages

1. Logout
2. Try to access http://recipes.local/dashboard

**✓ Pass Criteria**:
- Redirected to /login
- Cannot view protected content

### Test S.2: Authorization
**Expected**: Users can only edit their own recipes

1. Login as regular user
2. Try to edit another user's recipe (direct API call or URL manipulation)

**✓ Pass Criteria**:
- Request denied (403 Forbidden)
- Recipe not modified

### Test S.3: Admin Protection
**Expected**: Regular users can't access admin panel

1. Login as non-admin user
2. Try to access http://admin.recipes.local

**✓ Pass Criteria**:
- Access denied or redirected
- Admin endpoints return 403

## Cross-Browser Tests

Test on:
- ✓ Chrome (latest)
- ✓ Firefox (latest)
- ✓ Safari (macOS)
- ✓ Mobile Safari (iOS)
- ✓ Chrome Mobile (Android)

## PWA Tests

### Test PWA.1: Install App
**Expected**: Can install as PWA

1. Open in Chrome
2. Click install prompt or "Install" in address bar

**✓ Pass Criteria**:
- App installs
- Opens in standalone window
- App icon visible on home screen/desktop

### Test PWA.2: Offline Access
**Expected**: Basic functionality works offline

1. Install PWA
2. Load app
3. Disconnect network
4. Navigate to cached pages

**✓ Pass Criteria**:
- Cached pages load
- Offline message shown for API calls
- Reconnects when network restored

## Regression Tests

Run after any major changes:

1. All authentication tests
2. Recipe purchase flow
3. Trade creation and acceptance
4. Subscription purchase
5. Points top-up

## Test Results Template

```
Date: YYYY-MM-DD
Tester: [Name]
Environment: [Development/Staging/Production]
Branch/Commit: [Git info]

| Test ID | Status | Notes |
|---------|--------|-------|
| 1.1     | ✓ PASS |       |
| 1.2     | ✓ PASS |       |
| 1.3     | ✗ FAIL | Bug #123 |
| ...     | ...    | ...   |

Critical Issues Found:
- [List any blocking issues]

Minor Issues:
- [List non-critical issues]

Overall Status: [PASS/FAIL/PASS WITH ISSUES]
```

## Automated Testing Notes

For automated versions of these tests, see:
- `/backend/RecipesApi.Tests` - API integration tests
- `/frontend/app/tests` - Nuxt component tests
- `/docs/TESTING.md` - Full testing strategy

## Quick Smoke Test (5 minutes)

If time is limited, run these critical tests:

1. Register new user → Login
2. Buy a recipe
3. Top up points
4. Create trade offer
5. Access admin panel (if admin)

If all 5 pass, core functionality is working.
