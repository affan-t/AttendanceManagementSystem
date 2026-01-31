# Attendance Management System

> A robust, full-stack web application for managing university/college academic attendance, course allocations, and student records. Built with **ASP.NET Core (MVC)** and **SQL Server**.

![Project Status](https://img.shields.io/badge/status-active-success.svg)
![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Tech](https://img.shields.io/badge/tech-ASP.NET%20Core%20%7C%20EF%20Core%20%7C%20MSSQL-blueviolet)

---

##  Overview

The **Attendance Management System** automates the traditional manual attendance process for universities and colleges.  
It enables administrators to manage academic structures, teachers to digitally mark and analyze attendance, and students to track their attendance progress in real time.

The system implements **role-based access control** (Admin, Teacher, Student), visual reports, and automated low-attendance alerts.

---

##  Key Features

###  Admin Module
- **User Management:** Manage users, roles, and permissions using ASP.NET Core Identity.
- **Academic Structure:** Create, update, and manage Batches, Semesters, Sections, and Courses.
- **Profiles:** Maintain detailed Student and Teacher profiles.
- **Course Allocation:** Assign courses to teachers for specific batches and sections.

###  Teacher Module
- **Dashboard:** View today’s classes and low-attendance alerts.
- **Mark Attendance:** Simple and intuitive interface to mark students Present/Absent.
- **Attendance History:** View and edit past attendance records.
- **Reports:**
  - Graphical visualization of attendance trends.
  - **Export to CSV** for offline usage.
- **Smart Alerts:** Automatic highlighting of students with attendance below **75%**.

###  Student Module
- **Dashboard:** View enrolled courses.
- **Track Progress:** Monitor attendance percentage per subject in real time.

---

##  Tech Stack

- **Framework:** ASP.NET Core MVC (.NET 8.0)
- **Database:** Microsoft SQL Server
- **ORM:** Entity Framework Core (Code-First / Database-First)
- **Authentication:** ASP.NET Core Identity
- **Frontend:** Razor Views, Bootstrap 5, JavaScript
- **Icons:** Bootstrap Icons
- **Hosting:** SmarterASP.net (IIS)

---

##  Getting Started

Follow the instructions below to set up the project locally.

###  Prerequisites
- Visual Studio 2022
- .NET 8.0 SDK
- SQL Server (LocalDB or Express)

---

##  Installation

###  Clone the Repository
```bash
git clone https://github.com/affan-t/AttendanceManagementSystem.git
```

###  Configure Database

Open **appsettings.json** and update the connection string if required:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AttendanceDB;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

###  Run Migrations (or SQL Script)

Open **Package Manager Console** in Visual Studio and run:

```powershell
Update-Database
```

###  Run the Application
Press **F5** in Visual Studio to launch the project.

---

##  Default Login Credentials

The system is pre-seeded with demo accounts:

| Role    | Email                 | Password     |
|--------|-----------------------|--------------|
| Admin  | admin@uet.edu.pk      | Password123! |
| Teacher| teacher@uet.edu.pk    | Password123! |
| Student| student@uet.edu.pk    | Password123! |

>  Passwords are securely hashed using ASP.NET Core Identity.

---

##  Database Schema

Core tables included in the system:

- **AspNetUsers / AspNetRoles** (Identity)
- **Teachers / Students** (Profiles linked to users)
- **Courses / Batches / Sections / Semesters**
- **CourseAllocations** (Teacher ↔ Course ↔ Batch)
- **Enrollments** (Student ↔ Course Allocation)
- **Attendances** (Daily attendance records)
- **TimetableEntries** (Class schedule configuration)

---

##  Deployment (SmarterASP)

1. Publish the project to a local folder using Visual Studio.
2. In `web.config`, ensure:
   ```xml
   <environmentVariables>
     <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
   </environmentVariables>
   ```
3. If using precompiled views, ensure the following is set in `.csproj`:
   ```xml
   <CopyRazorGenerateFilesToPublishDirectory>true</CopyRazorGenerateFilesToPublishDirectory>
   ```
4. Upload files via FTP or File Manager.
5. Import the SQL script into the hosting database.

---

##  Contributing

Contributions are welcome! Follow these steps:

1. Fork the repository
2. Create your feature branch  
   ```bash
   git checkout -b feature/AmazingFeature
   ```
3. Commit your changes  
   ```bash
   git commit -m "Add some AmazingFeature"
   ```
4. Push to the branch  
   ```bash
   git push origin feature/AmazingFeature
   ```
5. Open a Pull Request

---

##  License

Distributed under the **MIT License**. See `LICENSE` for more information.

---

##  Developed By

**Affan Tahir**  
Cybersecurity & Computer Science Enthusiast  
