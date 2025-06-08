# AccountServiceProvider

AccountServiceProvider is a microservice responsible for handling user registration, authentication, within the Ventixe Event Management System.

## Features

- User registration and identity creation using ASP.NET Core Identity
- JWT authentication
- Email verification via external VerificationServiceProvider
- Password reset functionality
- Azure Key Vault integration for managing secrets securely

## Default Roles & Authorization

All users registering through this service are automatically assigned the role `User`. 
This is an intentional decision as part of the **Minimum Viable Product (MVP)** design.

### Current authorization strategy:

- Any registered user can:
  - Create and manage their own events
  - Book events created by others
  - Manage (update or delete) their own bookings and events

This reflects the intended core functionality of a basic event system prototype.

## Configuration

All sensitive values are loaded via [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/general/overview).

Secrets expected in Key Vault:

| Key                                  | Description                            |
|-------------------------------------|----------------------------------------|
| `ConnectionStrings--SqlConnection`  | SQL Server connection string           |
| `Jwt--Issuer`                       | Expected issuer for JWT validation     |
| `Jwt--Audience`                     | Expected audience for JWT validation   |
| `Jwt--Secret`                       | Symmetric key for signing tokens       |
| `VerificationServiceUrl`           | URL to the verification microservice   |

## Technologies

- ASP.NET Core 9
- Entity Framework Core
- Azure Key Vault
- Microsoft Identity
- JWT Bearer Authentication
