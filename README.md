# DQMS-Backend

DQMS-Backend is a backend application that manages companies' document and quality management processes. The system provides a secure API with JWT-based authentication and is fully compatible with the DQMS frontend application.

---

## Features

- Role-based access: Super admin, manager, employee.
- Document management: Add, revision, update, delete, and list documents.
- Document type management: SOP, Policy, Procedure, etc.
- Quality processes and document approval mechanism.
- Secure JWT-based authentication.

---

## Technologies

- .NET Core
- MySQL
- ADO.NET (Dapper or Entity Framework not used)
- JWT Authentication

---

## Installation

1. Clone the project:
```bash
git clone https://github.com/kullanici/dqms-backend.git
cd dqms-backend
