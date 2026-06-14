# React Frontend Build Prompt for Restaurant API

Use the following prompt with ChatGPT, Codex, Claude, or another coding agent to generate the frontend.

---

## Prompt

You are a senior React and TypeScript engineer. Build a complete, polished, responsive restaurant management frontend for an existing ASP.NET Core Web API.

Create the frontend as a new application named `restaurant-frontend`. Use:

- React with TypeScript
- Vite
- React Router
- TanStack Query for server state, caching, invalidation, loading, and errors
- Axios for HTTP
- React Hook Form with Zod validation
- Tailwind CSS
- shadcn/ui or an equivalent accessible component library
- Lucide icons
- Sonner or an equivalent toast library
- Vitest and React Testing Library for focused tests

Do not create a fake backend. Integrate every supported workflow with the API described below. Keep API-specific logic in a typed service layer rather than calling Axios directly from pages.

### Product Experiences

The application has three experiences:

1. A public, mobile-first table ordering experience opened from a table-specific URL token.
2. A protected chef workspace for accepting and preparing orders.
3. A protected admin dashboard for managing menu items, categories, tables, users, chefs, and orders.

The visual design should feel like a modern premium restaurant: warm cream backgrounds, charcoal text, restrained terracotta or deep green accents, generous spacing, strong food photography, subtle shadows, and excellent mobile behavior. Avoid a generic admin-template appearance.

### API Configuration

Use this environment variable:

```env
VITE_API_BASE_URL=https://localhost:7197/api
```

The development API may also run at `http://localhost:5151/api`, but HTTPS is required for refresh-token cookies because the backend sets cookies with `Secure=true`.

Create `.env.example` and document local setup in `README.md`.

### API Data Models

```ts
type Category = {
  id: number;
  name: string;
  displayOrder: number;
};

type MenuItem = {
  id: number;
  name: string;
  description: string;
  price: number;
  categoryId: number;
  isAvailable: boolean;
  prepTimeMinutes: number;
  imageUrl: string | null;
};

type RestaurantTable = {
  id: number;
  tableNumber: number;
  isActive: boolean;
  token: string;
};

type OrderItem = {
  id: number;
  orderId: number;
  menuItemId: number;
  quantity: number;
  price: number;
};

type Order = {
  id: number;
  tableId: number;
  status: number | string;
  chefId: number | null;
  createdAt: string;
  totalAmount: number;
  orderItems: OrderItem[];
};

type User = {
  id: number;
  name: string;
  email: string;
  role: string;
  status: string;
  lastAssignedAt: string | null;
};
```

Use these enum mappings:

```ts
const ORDER_STATUS = {
  Pending: 0,
  Assigned: 1,
  Accepted: 2,
  Preparing: 3,
  Ready: 4,
  Completed: 5,
  Cancelled: 6,
} as const;

const USER_ROLE = {
  Admin: 0,
  Chef: 1,
  Customer: 2,
} as const;

const USER_STATUS = {
  Available: 0,
  Busy: 1,
  Offline: 2,
} as const;
```

The login API can return `role` as either a number or a string depending on backend serialization. Normalize both forms into `"Admin" | "Chef" | "Customer"`. Likewise, normalize order statuses received as either numeric or string values.

### Authentication

The access token lasts 15 minutes. Store it in memory where possible, with a small persisted session fallback so browser refresh does not immediately lose the current session. Never expose or attempt to read the HTTP-only refresh token.

Login:

```http
POST /Auth/login
Content-Type: application/json

{
  "email": "string",
  "password": "string"
}
```

Successful response:

```json
{
  "accessToken": "jwt",
  "token": "jwt",
  "role": 0,
  "userId": 1
}
```

The login response sets:

- HTTP-only `refreshToken` cookie
- readable `csrfToken` cookie

Configure Axios with `withCredentials: true`.

Refresh:

```http
POST /Auth/refresh-token
X-CSRF-TOKEN: <value of csrfToken cookie>
```

Create an Axios response interceptor that:

1. Handles a protected request returning `401`.
2. Runs only one refresh request at a time.
3. Reads the `csrfToken` cookie and sends it as `X-CSRF-TOKEN`.
4. Updates the access token from the refresh response.
5. Retries queued failed requests once.
6. Clears the session and redirects to `/login` if refresh fails.
7. Avoids refresh loops for login, refresh, and logout requests.

Logout:

```http
POST /Auth/logout
Authorization: Bearer <access-token>
```

Current identity:

```http
GET /Auth/me
Authorization: Bearer <access-token>
```

Response:

```json
{
  "userId": "1",
  "email": "admin@example.com",
  "role": "Admin"
}
```

Forgot password:

```http
POST /Auth/forgot-password

{
  "email": "string",
  "password": "string",
  "confirmPassword": "string"
}
```

Important: the existing endpoint resets the password directly and does not send an email or verify a reset token. Label the UI accurately as a direct password reset for this project rather than pretending an email was sent.

Create protected-route and role-route components:

- Admin routes require `Admin`.
- Chef routes allow `Chef` and may also allow `Admin` where useful.
- Public ordering routes require no login.
- Redirect an authenticated user to the correct dashboard for their role.

### API Endpoints

#### Categories

- `GET /Category` - public, list categories
- `GET /Category/{id}` - public, get category
- `POST /Category` - Admin

```json
{ "name": "Starters", "displayOrder": 1 }
```

- `PUT /Category` - Admin; send the complete category:

```json
{ "id": 1, "name": "Starters", "displayOrder": 1 }
```

- `DELETE /Category/{id}` - Admin

Sort categories client-side by `displayOrder`.

#### Menu

- `GET /Menu` - public, all menu items, including unavailable items
- `GET /Menu/{id}` - public
- `GET /Menu/category/{categoryId}` - public, available items only
- `POST /Menu` - Admin
- `PUT /Menu/{id}` - Admin
- `DELETE /Menu/{id}` - Admin

Create/update body:

```json
{
  "name": "Margherita Pizza",
  "description": "Tomato, mozzarella, and basil",
  "price": 12.5,
  "categoryId": 2,
  "isAvailable": true,
  "prepTimeMinutes": 15,
  "imageUrl": "https://example.com/image.jpg"
}
```

There is no image-upload endpoint. Accept an image URL and show a graceful food-image placeholder for missing or broken images.

#### Tables

- `GET /Table` - Admin
- `POST /Table` - Admin

```json
{ "tableNumber": 12, "isActive": true }
```

The response contains the generated table token.

- `DELETE /Table/{id}` - Admin

There is currently no table update endpoint. Do not show an edit/activate toggle that calls a nonexistent API.

For each table, create a customer URL in this form:

```text
<frontend-origin>/table/<table-token>
```

Show the URL, copy action, and a QR code that points to it.

#### Orders

- `GET /Order` - Admin or Chef
- `GET /Order/{id}` - Admin or Chef
- `GET /Order/table/{tableId}` - Admin or Chef
- `POST /Order?token={tableToken}` - public

Order body:

```json
{
  "items": [
    { "menuItemId": 1, "quantity": 2 },
    { "menuItemId": 4, "quantity": 1 }
  ]
}
```

- `PUT /Order/{id}/status?status={status}` - Admin or Chef

The status query value can be a numeric enum value or enum name. Prefer the enum name for readability.

Order creation behavior to reflect in the UI:

- The server validates the table token and active state.
- Prices and totals are calculated by the server.
- If a table already has a `Pending` order, newly submitted items are merged into that order and matching quantities are increased.
- A successful response is plain text: `"Order placed successfully"`.
- Do not trust or send client-calculated prices.

Order responses contain item IDs and prices but do not contain menu item names. Join `orderItems[].menuItemId` to the cached `GET /Menu` result. Display a safe fallback such as `Menu item #12` if an item no longer exists.

#### Chef Workflow

All chef routes require role `Chef` or `Admin`.

- `GET /Chef/orders`
- `PUT /Chef/accept-order?orderId={id}`
- `PUT /Chef/reject-order?orderId={id}`
- `PUT /Chef/update-status?orderId={id}&status={status}`

Behavior:

- An available chef sees all pending orders.
- After accepting an order, the chef becomes busy and sees that assigned order.
- Rejecting returns the order to pending and makes the chef available.
- Completing the order makes the chef available again.

Use this intended status flow:

```text
Pending -> Accepted -> Preparing -> Ready -> Completed
```

Also display `Assigned` and `Cancelled` correctly if returned by the API.

#### Users

- `GET /User` - Admin
- `GET /User/chefs` - Admin
- `POST /User` - Admin
- `POST /User/register` - public
- `DELETE /User/{id}` - Admin

Create-user body:

```json
{
  "name": "string",
  "email": "string",
  "phoneNumber": "string or null",
  "password": "string",
  "confirmPassword": "string",
  "address": "string or null",
  "role": 1
}
```

The public registration endpoint is described by the backend as chef registration, but it accepts the supplied numeric role. For safety, the public registration UI must always send `role: 1` and must not expose a role selector.

The admin create-user form may select Admin, Chef, or Customer and send `0`, `1`, or `2`.

### Required Routes and Pages

Build these routes:

```text
/                         public restaurant landing page
/login                    staff login
/register-chef            chef registration
/forgot-password          direct password reset
/table/:token             customer menu and ordering
/order-success            order confirmation

/admin                    admin overview
/admin/menu               menu management
/admin/categories         category management
/admin/tables             table and QR management
/admin/orders             all-orders board
/admin/users              user management
/admin/chefs              chef status overview

/chef                     chef order workspace
/chef/orders/:id          chef order details

*                         branded not-found page
```

### Public Restaurant Experience

Create a high-quality landing page with:

- Hero section and primary call to action
- Featured available menu items loaded from the API
- Category preview
- Restaurant story/service section
- Staff login link kept visually secondary
- Responsive navigation and footer

Do not imply that customers can place an order without a valid table token. The generic landing page may browse the menu, but ordering is enabled only on `/table/:token`.

### Table Ordering Experience

Make `/table/:token` mobile-first and suitable for QR-code usage:

- Show restaurant header and a compact table-session indicator based on the token.
- Fetch categories and menu items.
- Display only available items to customers.
- Category tabs or horizontal chips.
- Search by item name or description.
- Menu cards with image, price, prep time, description, quantity controls, and add-to-cart.
- Sticky cart button on mobile.
- Cart drawer/page with quantity editing, removal, subtotal, item count, and estimated preparation information.
- Persist separate carts by table token in local storage.
- On submit, call `POST /Order?token=...`.
- Disable duplicate submits and show useful errors for invalid/inactive table tokens.
- Clear the cart only after a successful order.
- Show a confirmation page and explain that additional items submitted while the order is still pending may be merged by the server.

The API has no public endpoint for resolving a token to a table number and no public order-status endpoint. Do not fabricate table details or live customer tracking. Keep confirmation messaging honest.

### Chef Workspace

Design for fast use on a kitchen tablet:

- Large, glanceable order cards.
- Poll `GET /Chef/orders` every 10 seconds while the page is visible.
- Group or label orders by status and sort oldest first.
- Show order ID, table ID, age, total, item names, quantities, and notes that no customization data exists.
- Available-chef view: pending orders with Accept and Reject actions.
- Busy-chef view: focused accepted order with Preparing, Ready, and Completed actions.
- Confirmation for destructive or terminal actions.
- Optimistic UI only where rollback behavior is reliable; otherwise use mutation loading states and query invalidation.
- Add accessible status colors with text labels, not color alone.

### Admin Dashboard

Admin overview:

- Summary cards calculated from fetched data: total menu items, available items, active tables, pending/active orders, chefs by status.
- Recent orders.
- Quick actions.
- Do not invent analytics or revenue history endpoints. Any displayed totals must be clearly derived from the currently fetched orders.

Menu management:

- Search, category filter, availability filter.
- Responsive table/card layout.
- Create, edit, and delete dialogs.
- Validated price, category, prep time, image URL, and availability.
- Image previews and fallbacks.

Category management:

- List sorted by display order.
- Create, edit, and delete.
- Prevent obviously invalid empty names and negative display order values in the UI.

Table management:

- List table number, active state, token, customer URL, and QR code.
- Create and delete tables.
- Copy token and customer URL.
- Print-friendly QR card.
- Do not offer editing because the backend has no update route.

Orders:

- Board or table with status filters, table filter, chef filter, search by order ID, and created-date sorting.
- Join menu item details client-side.
- Order details drawer/dialog.
- Admin status update action.
- Refresh button and optional 10-second polling.

Users:

- List safe `UserResponseDto` fields only.
- Role and status badges.
- Create and delete users.
- Never expect password, phone number, or address in list responses because the API does not return them.

Chefs:

- Use `GET /User/chefs`.
- Show availability status, current assignment indication inferred from orders, and last assigned time.
- There are no endpoints for manually changing chef availability, so do not provide fake status controls.

### Architecture

Use a maintainable feature-oriented structure similar to:

```text
src/
  app/
  api/
  components/
  features/
    auth/
    menu/
    categories/
    cart/
    orders/
    tables/
    users/
    chef/
  layouts/
  pages/
  routes/
  types/
  utils/
```

Implement:

- Central Axios client
- Typed endpoint services
- Query key factory
- Auth provider/store
- Route guards
- Role and status normalization utilities
- Currency, date, and elapsed-time formatting
- API error parser that handles plain text and JSON errors
- Reusable loading skeletons, empty states, error states, confirmation dialogs, badges, data tables, and form fields
- Error boundary
- Accessible keyboard navigation and visible focus states
- Responsive layouts from 320px mobile through desktop
- Lazy-loaded protected route groups where sensible

Use `Intl.NumberFormat` for currency and make the currency easy to change from one configuration constant. Default to `INR` unless project configuration specifies another currency.

### Validation and UX

- Validate required fields and sensible numeric limits.
- Prevent zero or negative cart quantities.
- Disable unavailable menu items.
- Show server error messages when useful.
- Handle `401`, `403`, `404`, network errors, and invalid table tokens distinctly.
- Use confirmation dialogs before deletes.
- Invalidate or update TanStack Query caches after mutations.
- Show polished skeletons rather than blank screens.
- Ensure every form has labels and useful validation messages.
- Do not use browser `alert()` or `confirm()`.

### Tests

Add focused tests for:

- Role normalization from numeric and string values
- Order-status normalization
- Auth refresh queue behavior
- Protected and role-based routing
- Cart calculations and per-token persistence
- Order payload construction
- Menu item joining for order display

### Deliverables

Produce a runnable frontend with:

- Complete source code
- `.env.example`
- `README.md` with installation, HTTPS API setup, and run instructions
- Clean TypeScript with no avoidable `any`
- Responsive, production-quality UI
- Working API integration for every endpoint listed above
- No mocked production data and no calls to nonexistent endpoints

Before finishing:

1. Run formatting.
2. Run TypeScript checks.
3. Run tests.
4. Run the production build.
5. Fix all errors.
6. Summarize the implemented routes, API integration, and any backend limitations that remain.

### Backend Prerequisites and Known Limitations

Clearly document these backend items in the frontend README:

1. The API currently has no CORS configuration. The backend must allow the frontend origin, credentials, required methods, and the `Authorization`, `Content-Type`, and `X-CSRF-TOKEN` headers.
2. Credentialed refresh cookies require HTTPS during local development because they use `Secure=true` and `SameSite=None`.
3. The table repository delete method has an inverted null check, so table deletion will not work until the backend is fixed.
4. Order responses do not include menu item names, category data, table numbers, or chef names. The frontend must join available data client-side.
5. There is no public order tracking endpoint.
6. There is no public endpoint to resolve a table token into table metadata.
7. There is no image upload endpoint.
8. There is no category relationship included on menu item responses.
9. There are no pagination, search, reporting, payment, reservation, customer profile, or order-customization endpoints.
10. Several mutation endpoints return plain text or empty responses, so the client must not assume every success response is JSON.

Do not silently work around these limitations with fake behavior. Build graceful UI states and state the limitation where it affects the user experience.

---

## Project Analysis Summary

The backend is a .NET 10 solution using ASP.NET Core controllers, Entity Framework Core, SQL Server, JWT access tokens, rotating refresh-token cookies, and role authorization.

Implemented backend domains:

- Authentication and direct password reset
- Users and chef registration
- Categories
- Menu items
- Restaurant tables with generated tokens
- Public token-based ordering
- Chef order acceptance and status workflow
- Admin order, user, chef, menu, category, and table access

The most important integration detail is that anonymous customers order through `POST /api/Order?token=<table-token>`, while staff workflows use JWT bearer authentication.
