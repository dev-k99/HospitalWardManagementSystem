# Ward Management System

A comprehensive hospital ward management system built with ASP.NET Core MVC, Entity Framework Core, and Bootstrap 5.

## 🏥 System Overview

The Ward Management System is designed to manage hospital wards, patients, staff, medications, and medical records efficiently. It provides role-based access control for different types of healthcare professionals.

## Screenshots
![Welcom Page](<Screenshots/Screenshot (395).png>)
![Patient Folder](<Screenshots/Screenshot (398).png>)
![Delete View](<Screenshots/Screenshot (397).png>)
![Admission Form](<Screenshots/Screenshot (396).png>)
![Patient Details](<Screenshots/Screenshot (399).png>)
![Patient Care Dashboard](<Screenshots/Screenshot (400).png>)
![Admin Dashboard](<Screenshots/Screenshot (401).png>)
![Ward Management](<Screenshots/Screenshot (402).png>)
![Bed Management](<Screenshots/Screenshot (403).png>)
![Medication Management](<Screenshots/Screenshot (404).png>)


## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or full SQL Server)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/dev-k99/HospitalWardManagementSystem
   cd WardSystemProject
   ```

2. **Update Connection String**
   - Open `appsettings.json`
   - Update the `WardConn` connection string to point to your SQL Server instance

3. **Run Database Migrations**
   ```bash
   dotnet ef database update
   ```

### Connection String Configuration

The application uses a comprehensive SQL Server connection string with the following parameters:

#### **Server Configuration**
- **`Server`**: SQL Server instance name (e.g., `localhost\SQLEXPRESS` or `your-server\INSTANCE`)
- **`Database`**: Target database name (default: `WardManagementDb`)

#### **Authentication & Security**
- **`Trusted_Connection=True`**: Uses Windows Authentication (current user's credentials)
- **`TrustServerCertificate=True`**: Bypasses SSL certificate validation (development only)

#### **Performance & Reliability**
- **`MultipleActiveResultSets=true`**: Allows multiple active result sets per connection

#### **Example Connection Strings**


**⚠️ Security Note**: The current connection string uses `TrustServerCertificate=True` for development convenience. In production environments, use proper SSL certificates and consider SQL Authentication for better security.

4. **Run the Application**
   ```bash
   dotnet run
   ```

## 👤 User Authentication & Registration

### Default Administrator Account
The system comes with a pre-configured administrator account:
- **Username**: `admin`
- **Password**: `Admin123!`
- **Role**: Administrator
- **Email**: `admin@wardsystem.com`

### Available Roles for Registration
- **Doctor**: Access to patient management, visits, prescriptions, and instructions
- **Nurse**: Access to patient care, vital signs, and medication administration
- **Nursing Sister**: All nurse permissions plus supervisory duties
- **Ward Admin**: Access to patient admission, discharge, and ward management

## 🏗️ System Architecture

### Controllers
- **AdministrationController**: Manages wards, medications, allergies, and medical conditions
- **BedManagementController**: Manages bed assignments and availability
- **DoctorPatientController**: Manages doctor visits and prescriptions
- **PatientCareController**: Manages vital signs and medication administration
- **PatientManagementController**: Manages patient admission, discharge, and movement
- **RoomManagementController**: Manages room assignments
- **StaffManagementController**: Manages staff information


## 🔮 Future Enhancements

- **Dashboard**: Role-specific dashboards with key metrics
- **Reporting**: Comprehensive reporting and analytics
- **Notifications**: Real-time notifications for critical events
- **Mobile App**: Native mobile application
- **API**: RESTful API for third-party integrations
- **Audit Logging**: Comprehensive audit trail
- **Advanced Search**: Full-text search capabilities

## 📋 Current Project Status

### ✅ Completed Features
- **Core System**: Complete ward, patient, staff, and medication management
- **Authentication**: Full user registration and login system
- **Role-Based Access**: Proper authorization for all user types
- **Patient Management**: Complete patient lifecycle (admission → care → discharge)
- **Medical Records**: Vital signs, medication administration, doctor visits
- **Inventory Management**: Consumables and prescription processing
- **Modern UI**: Bootstrap 5 responsive design with Bootstrap Icons

### 🔧 Recent Fixes
- **Authentication Issues**: Resolved 404 errors in DoctorPatient routes
- **View Consistency**: Fixed naming mismatches between controllers and views
- **Bootstrap Compatibility**: Updated all views to Bootstrap 5 standards
- **Code Cleanup**: Removed debug code and duplicate files
- **Navigation**: Fixed broken links and action references

### 🚀 Ready for Use
The system is now fully functional with:
- Working user registration and authentication
- Proper role-based access control
- Clean, modern user interface
- Comprehensive patient management
- Secure data handling
- Responsive design for all devices

