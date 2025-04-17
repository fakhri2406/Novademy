# Novademy

Novademy is an innovative online education platform specifically designed for Azerbaijani students preparing for their exams. Our mission is to make quality education accessible and affordable for all students in Azerbaijan.

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server
- Docker (optional)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/fakhri2406/Novademy.git
cd Novademy
```

2. Install backend dependencies:
```bash
dotnet restore
```

3. Configure the database:
```bash
dotnet ef database update
```

4. Run the application:
```bash
dotnet run --project Novademy.API
```

## 🏗️ Project Structure

```
Novademy/
├── Novademy.API/          # Presentation layer (RESTful APIs)
├── Novademy.Application/  # Application layer
├── Novademy.Contracts/    # Data transfer objects
└── Novademy.UnitTests/    # Unit tests
```

## 🔧 Tech Stack

- **Backend**
  - .NET 8.0
  - ASP.NET Core Web API
  - Entity Framework Core
  - Dapper
  - JWT Authentication
  - Swagger/OpenAPI

- **Database**
  - MS Azure SQL Server

## 📚 Documentation

- [API Documentation](https://github.com/user-attachments/files/19801481/api.docx)
