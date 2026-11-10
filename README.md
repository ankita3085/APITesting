# TeachingAPI

A small ASP.NET Core (.NET 10) Web API for teaching C#: CRUD on products, in-memory storage, JWT auth.

Run: `dotnet run` (listens on http://localhost:5000). Try requests from `TeachingAPI.http`.

Suggested reading order: `Program.cs` -> `Models/` -> `Dtos/` -> `Services/` -> `Controllers/`.

| Endpoint | Auth |
|---|---|
| POST /api/auth/register, /api/auth/login | public |
| GET /api/products, /api/products/{id} | public |
| POST, PUT /api/products | any logged-in user |
| DELETE /api/products/{id} | Admin only (seeded: admin / Admin123!) |
