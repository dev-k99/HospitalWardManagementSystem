# Hospital Ward Management System

A comprehensive hospital ward management system built with ASP.NET Core MVC, Entity Framework Core, and Bootstrap 5.

## 🏥 System Overview

The Ward Management System is designed to manage hospital wards, patients, staff, medications, and medical records efficiently. It provides role-based access control for different types of healthcare professionals.

## 👥 User Roles & Permissions

### Administrator
- **Manage Wards**: Create, edit, delete, and view ward information
- **Manage Rooms**: Create, edit, delete, and view room assignments
- **Manage Beds**: Create, edit, delete, and view bed assignments
- **Manage Staff**: Create, edit, delete, and view staff information
- **Manage Medications**: Create, edit, delete, and view medication records
- **Manage Allergies**: Create, edit, delete, and view patient allergy records
- **Manage Medical Conditions**: Create, edit, delete, and view patient medical condition records

### Ward Admin
- **Patient Management**: Admit, discharge, and manage patient information
- **Patient Movement**: Track patient movements between wards
- **Patient Records**: View comprehensive patient details including allergies and medical conditions

### Doctor
- **Patient Visits**: Create, edit, delete, and view doctor visits
- **Prescriptions**: Create, edit, delete, and view patient prescriptions
- **Patient Folder**: Access comprehensive patient medical history

### Nurse
- **Vital Signs**: Record and manage patient vital signs
- **Medication Administration**: Administer non-scheduled medications
- **Doctor Instructions**: View and record doctor instructions
- **Patient Care**: Provide direct patient care services

### Nursing Sister
- **Scheduled Medication**: Administer scheduled medications
- **Patient Care**: All nurse responsibilities plus additional supervisory duties

### Script Manager
- **Prescription Orders**: View, process, and manage prescription orders
- **Script Processing**: Forward scripts to pharmacy and receive medications

### Consumables Manager
- **Consumable Orders**: Create, edit, delete, and view consumable orders
- **Stock Management**: Check and update consumable stock levels
- **Inventory Control**: Manage hospital consumables and supplies

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or full SQL Server)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone [repository-url]
   cd WardSystemProject
   ```

2. **Update Connection String**
   - Open `appsettings.json`
   - Update the `WardConn` connection string to point to your SQL Server instance
   - See [Connection String Configuration](#connection-string-configuration) below for detailed options

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

#### **Timeout Settings**
- **`Connection Timeout=60`**: 60 seconds to establish initial database connection
- **`Command Timeout=60`**: 60 seconds for SQL commands to complete execution

#### **Connection Pooling**
- **`Pooling=true`**: Enables connection pooling for better performance
- **`Max Pool Size=200`**: Maximum number of connections in the pool
- **`Min Pool Size=5`**: Minimum connections kept open and ready for use

#### **Performance & Reliability**
- **`MultipleActiveResultSets=true`**: Allows multiple active result sets per connection

#### **Example Connection Strings**

**Local SQL Server Express:**
```json
"WardConn": "Server=localhost\\SQLEXPRESS;Database=WardManagementDb;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=60;Command Timeout=60;Max Pool Size=200;Min Pool Size=5;Pooling=true;MultipleActiveResultSets=true;"
```

**Remote SQL Server:**
```json
"WardConn": "Server=your-server.com,1433;Database=WardManagementDb;User Id=your-username;Password=your-password;TrustServerCertificate=True;Connection Timeout=60;Command Timeout=60;Max Pool Size=200;Min Pool Size=5;Pooling=true;MultipleActiveResultSets=true;"
```

**Azure SQL Database:**
```json
"WardConn": "Server=your-server.database.windows.net,1433;Database=WardManagementDb;User Id=your-username;Password=your-password;TrustServerCertificate=False;Encrypt=True;Connection Timeout=60;Command Timeout=60;Max Pool Size=200;Min Pool Size=5;Pooling=true;MultipleActiveResultSets=true;"
```

**⚠️ Security Note**: The current connection string uses `TrustServerCertificate=True` for development convenience. In production environments, use proper SSL certificates and consider SQL Authentication for better security.

4. **Run the Application**
   ```bash
   dotnet run
   ```

5. **Access the Application**
   - Navigate to `https://localhost:5001` or `http://localhost:5000`

## 👤 User Authentication & Registration

### Default Administrator Account
The system comes with a pre-configured administrator account:
- **Username**: `admin`
- **Password**: `Admin123!`
- **Role**: Administrator
- **Email**: `admin@wardsystem.com`

### User Registration
New users can register themselves through the registration system:
1. Navigate to `/Accounts/Register`
2. Fill out the registration form with personal details
3. Choose your role from available options
4. System automatically creates corresponding Staff records
5. Users are automatically signed in after successful registration

### Available Roles for Registration
- **Doctor**: Access to patient management, visits, prescriptions, and instructions
- **Nurse**: Access to patient care, vital signs, and medication administration
- **Nursing Sister**: All nurse permissions plus supervisory duties
- **Ward Admin**: Access to patient admission, discharge, and ward management
- **Script Manager**: Access to prescription processing and medication management
- **Consumables Manager**: Access to inventory and supply management

### Login System
- **Flexible Authentication**: Login with username or email
- **Remember Me**: Option to stay logged in
- **Role-Based Redirection**: Automatic redirection based on user role
- **Secure Password Storage**: Passwords are properly hashed using ASP.NET Core Identity

## 🏗️ System Architecture

### Controllers
- **AdministrationController**: Manages wards, medications, allergies, and medical conditions
- **BedManagementController**: Manages bed assignments and availability
- **ConsumableScriptController**: Manages prescriptions and consumables
- **DoctorPatientController**: Manages doctor visits and prescriptions
- **PatientCareController**: Manages vital signs and medication administration
- **PatientManagementController**: Manages patient admission, discharge, and movement
- **RoomManagementController**: Manages room assignments
- **StaffManagementController**: Manages staff information

### Models
- **Patient**: Core patient information and relationships
- **Ward**: Ward information and room relationships
- **Room**: Room information and bed relationships
- **Bed**: Bed assignments and patient relationships
- **Staff**: Staff information and roles
- **Medication**: Medication information and prescriptions
- **Allergy**: Patient allergy records
- **MedicalCondition**: Patient medical condition records
- **VitalSign**: Patient vital sign recordings
- **MedicationAdministration**: Medication administration records
- **DoctorVisit**: Doctor visit records
- **Prescription**: Patient prescription records
- **DoctorInstruction**: Doctor instruction records
- **PatientMovement**: Patient ward movement tracking
- **Consumable**: Hospital consumable items
- **ConsumableOrder**: Consumable order management
- **PrescriptionOrder**: Prescription order management
- **CheckStock**: Manages Avaliabe Stock

### Views
All views are built with Bootstrap 5 and include:
- Responsive design for all screen sizes
- Bootstrap Icons for visual consistency
- Form validation (client and server-side)
- Role-based navigation
- Search functionality
- Soft delete operations
- Comprehensive CRUD operations

## 🔧 Key Features

### Role-Based Access Control
- Each user role has specific permissions
- Navigation menus adapt to user role
- Action buttons are role-specific

### Patient Management
- Complete patient lifecycle management
- Ward and bed assignment tracking
- Patient movement history
- Allergy and medical condition tracking

### Medical Records
- Comprehensive vital sign tracking
- Medication administration records
- Doctor visit and instruction management
- Prescription management

### Inventory Management
- Consumable stock tracking
- Order management for supplies
- Stock level monitoring

### User Interface
- Modern Bootstrap 5 design
- Responsive layout
- Intuitive navigation
- Search and filter capabilities
- Real-time status indicators

## 🛡️ Security Features

- **Authentication**: ASP.NET Core Identity
- **Authorization**: Role-based access control
- **CSRF Protection**: Anti-forgery tokens
- **Input Validation**: Client and server-side validation
- **Soft Delete**: Data preservation through logical deletion

## 📊 Database Schema

The system uses Entity Framework Core with SQL Server and includes:
- Soft delete functionality for all entities
- Proper foreign key relationships
- Indexed fields for performance
- Audit trails through timestamps

## 🚨 Error Handling

- Comprehensive exception handling
- User-friendly error messages
- Logging for debugging
- Graceful degradation

## 🔄 Data Flow

1. **Patient Admission**: Ward Admin admits patient → assigns to ward/bed
2. **Patient Care**: Nurses record vitals → Doctors provide instructions
3. **Medication**: Doctors prescribe → Script Managers process → Nurses administer
4. **Consumables**: Consumables Manager orders → tracks stock → updates inventory
5. **Patient Movement**: Ward Admins track patient transfers between wards
6. **Patient Discharge**: Ward Admin discharges patient → releases bed assignment

## 🎯 Best Practices Implemented

- **SOLID Principles**: Clean architecture and separation of concerns
- **DRY Principle**: Reusable components and utilities
- **Security First**: Input validation and authorization
- **Performance**: Optimized queries and caching
- **Maintainability**: Clean code and documentation
- **Scalability**: Modular design for easy expansion

## 🧹 Recent Improvements & Cleanup

### Authentication System Overhaul
- **Complete User Registration**: Self-service user registration with role selection
- **Enhanced Login**: Username/email flexibility with remember me option
- **Automatic Staff Record Creation**: Seamless integration between Identity and business logic
- **Role-Based Access Control**: Proper authorization throughout the system

### Code Cleanup & Modernization
- **Bootstrap 5 Migration**: Updated all views from Bootstrap 4 to Bootstrap 5
- **Removed Debug Code**: Cleaned up console logging and debug information
- **Eliminated Duplicate Files**: Removed redundant Create.cshtml and CreateDoctorVisit.cshtml
- **Fixed Naming Inconsistencies**: Aligned view names with controller actions
- **Updated CSS Classes**: Replaced deprecated classes (font-weight-bold → fw-bold, ml-* → ms-*, etc.)

### File Structure Optimization
- **Removed Empty Views**: Deleted ManageAllergies.cshtml (0 bytes)
- **Consolidated Authentication**: Single Accounts directory for all auth-related views
- **Eliminated Dead Code**: Removed unused debug panels and console statements
- **Streamlined Navigation**: Fixed broken links and action references

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

## 📞 Support

For technical support or questions about the Ward Management System, please contact:
- **Email**: ighsaans@mandela.ac.za
- **Project**: ONT3010 - Project III

## 📄 License

This project is developed for educational purposes as part of the ONT3010 course at Nelson Mandela University.

---

**Note**: This system is designed for educational demonstration and should be thoroughly tested before use in a production healthcare environment. 
