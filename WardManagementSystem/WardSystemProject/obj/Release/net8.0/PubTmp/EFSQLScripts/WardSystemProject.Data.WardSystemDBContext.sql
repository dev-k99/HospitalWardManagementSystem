IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [Medications] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [Dosage] nvarchar(50) NOT NULL,
        [Schedule] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Medications] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [Staff] (
        [Id] int NOT NULL IDENTITY,
        [FirstName] nvarchar(50) NOT NULL,
        [LastName] nvarchar(50) NOT NULL,
        [Role] nvarchar(50) NOT NULL,
        [Email] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Staff] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [Wards] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Wards] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [Consumables] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [QuantityOnHand] int NOT NULL,
        [ReorderLevel] int NOT NULL,
        [Unit] nvarchar(20) NOT NULL,
        [WardId] int NOT NULL,
        [LastUpdated] datetime2 NULL,
        [LastStockTake] datetime2 NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Consumables] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Consumables_Wards_WardId] FOREIGN KEY ([WardId]) REFERENCES [Wards] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [Rooms] (
        [Id] int NOT NULL IDENTITY,
        [RoomNumber] nvarchar(50) NOT NULL,
        [WardId] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Rooms] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Rooms_Wards_WardId] FOREIGN KEY ([WardId]) REFERENCES [Wards] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [StockTakes] (
        [Id] int NOT NULL IDENTITY,
        [StockTakeDate] datetime2 NOT NULL,
        [StockManagerId] int NOT NULL,
        [WardId] int NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_StockTakes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StockTakes_Staff_StockManagerId] FOREIGN KEY ([StockManagerId]) REFERENCES [Staff] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StockTakes_Wards_WardId] FOREIGN KEY ([WardId]) REFERENCES [Wards] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [ConsumableOrders] (
        [Id] int NOT NULL IDENTITY,
        [ConsumableId] int NOT NULL,
        [StockManagerId] int NOT NULL,
        [Quantity] int NOT NULL,
        [OrderDate] datetime2 NOT NULL,
        [ReceivedDate] datetime2 NULL,
        [Status] nvarchar(50) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_ConsumableOrders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ConsumableOrders_Consumables_ConsumableId] FOREIGN KEY ([ConsumableId]) REFERENCES [Consumables] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ConsumableOrders_Staff_StockManagerId] FOREIGN KEY ([StockManagerId]) REFERENCES [Staff] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [Allergies] (
        [Id] int NOT NULL IDENTITY,
        [PatientId] int NOT NULL,
        [AllergyName] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [PatientFolderFolderId] int NULL,
        CONSTRAINT [PK_Allergies] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [Beds] (
        [Id] int NOT NULL IDENTITY,
        [BedNumber] nvarchar(20) NOT NULL,
        [RoomId] int NOT NULL,
        [PatientId] int NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Beds] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Beds_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [Patients] (
        [Id] int NOT NULL IDENTITY,
        [FirstName] nvarchar(50) NOT NULL,
        [LastName] nvarchar(50) NOT NULL,
        [DateOfBirth] datetime2 NOT NULL,
        [Gender] nvarchar(max) NOT NULL,
        [ContactNumber] nvarchar(max) NOT NULL,
        [EmergencyContact] nvarchar(max) NOT NULL,
        [EmergencyContactNumber] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [NextOfKin] nvarchar(max) NOT NULL,
        [NextOfKinContact] nvarchar(max) NOT NULL,
        [BloodType] nvarchar(max) NOT NULL,
        [ChronicMedications] nvarchar(max) NOT NULL,
        [MedicalHistory] nvarchar(max) NOT NULL,
        [Allergies] nvarchar(max) NOT NULL,
        [AdmissionReason] nvarchar(max) NOT NULL,
        [AdmissionDate] datetime2 NULL,
        [DischargeDate] datetime2 NULL,
        [DischargeSummary] nvarchar(max) NOT NULL,
        [AssignedDoctorId] int NULL,
        [WardId] int NULL,
        [BedId] int NULL,
        [PatientStatus] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Patients] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Patients_Beds_BedId] FOREIGN KEY ([BedId]) REFERENCES [Beds] ([Id]),
        CONSTRAINT [FK_Patients_Staff_AssignedDoctorId] FOREIGN KEY ([AssignedDoctorId]) REFERENCES [Staff] ([Id]),
        CONSTRAINT [FK_Patients_Wards_WardId] FOREIGN KEY ([WardId]) REFERENCES [Wards] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [PatientFolders] (
        [FolderId] int NOT NULL IDENTITY,
        [PatientId] int NOT NULL,
        [AdmissionDate] datetime2 NULL,
        [DischargeDate] datetime2 NULL,
        [DischargeSummary] nvarchar(max) NOT NULL,
        [AssignedDoctorId] int NULL,
        [WardId] int NULL,
        [BedId] int NULL,
        [PatientStatus] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_PatientFolders] PRIMARY KEY ([FolderId]),
        CONSTRAINT [FK_PatientFolders_Beds_BedId] FOREIGN KEY ([BedId]) REFERENCES [Beds] ([Id]),
        CONSTRAINT [FK_PatientFolders_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PatientFolders_Staff_AssignedDoctorId] FOREIGN KEY ([AssignedDoctorId]) REFERENCES [Staff] ([Id]),
        CONSTRAINT [FK_PatientFolders_Wards_WardId] FOREIGN KEY ([WardId]) REFERENCES [Wards] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [DoctorInstructions] (
        [Id] int NOT NULL IDENTITY,
        [PatientId] int NOT NULL,
        [DoctorId] int NOT NULL,
        [Details] nvarchar(1000) NOT NULL,
        [InstructionDate] datetime2 NOT NULL,
        [InstructionType] nvarchar(max) NOT NULL,
        [Priority] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [Instructions] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [PatientFolderFolderId] int NULL,
        CONSTRAINT [PK_DoctorInstructions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DoctorInstructions_PatientFolders_PatientFolderFolderId] FOREIGN KEY ([PatientFolderFolderId]) REFERENCES [PatientFolders] ([FolderId]),
        CONSTRAINT [FK_DoctorInstructions_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DoctorInstructions_Staff_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [Staff] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [DoctorVisits] (
        [Id] int NOT NULL IDENTITY,
        [PatientId] int NOT NULL,
        [DoctorId] int NOT NULL,
        [VisitDate] datetime2 NOT NULL,
        [VisitType] nvarchar(100) NOT NULL,
        [Notes] nvarchar(500) NOT NULL,
        [NextVisitDate] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [PatientFolderFolderId] int NULL,
        CONSTRAINT [PK_DoctorVisits] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DoctorVisits_PatientFolders_PatientFolderFolderId] FOREIGN KEY ([PatientFolderFolderId]) REFERENCES [PatientFolders] ([FolderId]),
        CONSTRAINT [FK_DoctorVisits_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DoctorVisits_Staff_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [Staff] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [MedicalConditions] (
        [Id] int NOT NULL IDENTITY,
        [PatientId] int NOT NULL,
        [ConditionName] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [PatientFolderFolderId] int NULL,
        CONSTRAINT [PK_MedicalConditions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MedicalConditions_PatientFolders_PatientFolderFolderId] FOREIGN KEY ([PatientFolderFolderId]) REFERENCES [PatientFolders] ([FolderId]),
        CONSTRAINT [FK_MedicalConditions_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [MedicationAdministrations] (
        [Id] int NOT NULL IDENTITY,
        [PatientId] int NOT NULL,
        [MedicationId] int NOT NULL,
        [AdministrationDate] datetime2 NOT NULL,
        [AdministeringStaffId] int NOT NULL,
        [Dosage] nvarchar(max) NOT NULL,
        [AdministrationMethod] nvarchar(max) NOT NULL,
        [Notes] nvarchar(max) NOT NULL,
        [AdministeredBy] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [PatientFolderFolderId] int NULL,
        CONSTRAINT [PK_MedicationAdministrations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MedicationAdministrations_Medications_MedicationId] FOREIGN KEY ([MedicationId]) REFERENCES [Medications] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_MedicationAdministrations_PatientFolders_PatientFolderFolderId] FOREIGN KEY ([PatientFolderFolderId]) REFERENCES [PatientFolders] ([FolderId]),
        CONSTRAINT [FK_MedicationAdministrations_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_MedicationAdministrations_Staff_AdministeringStaffId] FOREIGN KEY ([AdministeringStaffId]) REFERENCES [Staff] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [PatientMovements] (
        [Id] int NOT NULL IDENTITY,
        [PatientId] int NOT NULL,
        [FromWardId] int NOT NULL,
        [ToWardId] int NOT NULL,
        [MovementDate] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [PatientFolderFolderId] int NULL,
        CONSTRAINT [PK_PatientMovements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PatientMovements_PatientFolders_PatientFolderFolderId] FOREIGN KEY ([PatientFolderFolderId]) REFERENCES [PatientFolders] ([FolderId]),
        CONSTRAINT [FK_PatientMovements_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PatientMovements_Wards_FromWardId] FOREIGN KEY ([FromWardId]) REFERENCES [Wards] ([Id]),
        CONSTRAINT [FK_PatientMovements_Wards_ToWardId] FOREIGN KEY ([ToWardId]) REFERENCES [Wards] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [Prescriptions] (
        [Id] int NOT NULL IDENTITY,
        [PatientId] int NOT NULL,
        [DoctorId] int NOT NULL,
        [MedicationId] int NOT NULL,
        [DosageInstructions] nvarchar(100) NOT NULL,
        [Duration] nvarchar(50) NOT NULL,
        [Instructions] nvarchar(500) NOT NULL,
        [PrescriptionDate] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [PatientFolderFolderId] int NULL,
        CONSTRAINT [PK_Prescriptions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Prescriptions_Medications_MedicationId] FOREIGN KEY ([MedicationId]) REFERENCES [Medications] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Prescriptions_PatientFolders_PatientFolderFolderId] FOREIGN KEY ([PatientFolderFolderId]) REFERENCES [PatientFolders] ([FolderId]),
        CONSTRAINT [FK_Prescriptions_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Prescriptions_Staff_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [Staff] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [VitalSigns] (
        [Id] int NOT NULL IDENTITY,
        [PatientId] int NOT NULL,
        [Temperature] float NOT NULL,
        [Pulse] int NOT NULL,
        [RecordDate] datetime2 NOT NULL,
        [BloodPressure] nvarchar(max) NOT NULL,
        [HeartRate] int NULL,
        [RespiratoryRate] int NULL,
        [OxygenSaturation] int NULL,
        [Notes] nvarchar(max) NOT NULL,
        [RecordedBy] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [PatientFolderFolderId] int NULL,
        CONSTRAINT [PK_VitalSigns] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VitalSigns_PatientFolders_PatientFolderFolderId] FOREIGN KEY ([PatientFolderFolderId]) REFERENCES [PatientFolders] ([FolderId]),
        CONSTRAINT [FK_VitalSigns_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE TABLE [PrescriptionOrders] (
        [Id] int NOT NULL IDENTITY,
        [PrescriptionId] int NOT NULL,
        [ScriptManagerId] int NOT NULL,
        [OrderDate] datetime2 NOT NULL,
        [SentToPharmacy] datetime2 NULL,
        [ReceivedInWard] datetime2 NULL,
        [Status] nvarchar(50) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_PrescriptionOrders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PrescriptionOrders_Prescriptions_PrescriptionId] FOREIGN KEY ([PrescriptionId]) REFERENCES [Prescriptions] ([Id]),
        CONSTRAINT [FK_PrescriptionOrders_Staff_ScriptManagerId] FOREIGN KEY ([ScriptManagerId]) REFERENCES [Staff] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_Allergies_PatientFolderFolderId] ON [Allergies] ([PatientFolderFolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_Allergies_PatientId] ON [Allergies] ([PatientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_Beds_PatientId] ON [Beds] ([PatientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_Beds_RoomId] ON [Beds] ([RoomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_ConsumableOrders_ConsumableId] ON [ConsumableOrders] ([ConsumableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_ConsumableOrders_StockManagerId] ON [ConsumableOrders] ([StockManagerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_Consumables_WardId] ON [Consumables] ([WardId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_DoctorInstructions_DoctorId] ON [DoctorInstructions] ([DoctorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_DoctorInstructions_PatientFolderFolderId] ON [DoctorInstructions] ([PatientFolderFolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_DoctorInstructions_PatientId] ON [DoctorInstructions] ([PatientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_DoctorVisits_DoctorId] ON [DoctorVisits] ([DoctorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_DoctorVisits_PatientFolderFolderId] ON [DoctorVisits] ([PatientFolderFolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_DoctorVisits_PatientId] ON [DoctorVisits] ([PatientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_MedicalConditions_PatientFolderFolderId] ON [MedicalConditions] ([PatientFolderFolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_MedicalConditions_PatientId] ON [MedicalConditions] ([PatientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_MedicationAdministrations_AdministeringStaffId] ON [MedicationAdministrations] ([AdministeringStaffId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_MedicationAdministrations_MedicationId] ON [MedicationAdministrations] ([MedicationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_MedicationAdministrations_PatientFolderFolderId] ON [MedicationAdministrations] ([PatientFolderFolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_MedicationAdministrations_PatientId] ON [MedicationAdministrations] ([PatientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_PatientFolders_AssignedDoctorId] ON [PatientFolders] ([AssignedDoctorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_PatientFolders_BedId] ON [PatientFolders] ([BedId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_PatientFolders_PatientId] ON [PatientFolders] ([PatientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_PatientFolders_WardId] ON [PatientFolders] ([WardId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_PatientMovements_FromWardId] ON [PatientMovements] ([FromWardId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_PatientMovements_PatientFolderFolderId] ON [PatientMovements] ([PatientFolderFolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_PatientMovements_PatientId] ON [PatientMovements] ([PatientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_PatientMovements_ToWardId] ON [PatientMovements] ([ToWardId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_Patients_AssignedDoctorId] ON [Patients] ([AssignedDoctorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_Patients_BedId] ON [Patients] ([BedId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_Patients_WardId] ON [Patients] ([WardId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_PrescriptionOrders_PrescriptionId] ON [PrescriptionOrders] ([PrescriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_PrescriptionOrders_ScriptManagerId] ON [PrescriptionOrders] ([ScriptManagerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_Prescriptions_DoctorId] ON [Prescriptions] ([DoctorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_Prescriptions_MedicationId] ON [Prescriptions] ([MedicationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_Prescriptions_PatientFolderFolderId] ON [Prescriptions] ([PatientFolderFolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_Prescriptions_PatientId] ON [Prescriptions] ([PatientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_Rooms_WardId] ON [Rooms] ([WardId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_StockTakes_StockManagerId] ON [StockTakes] ([StockManagerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_StockTakes_WardId] ON [StockTakes] ([WardId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_VitalSigns_PatientFolderFolderId] ON [VitalSigns] ([PatientFolderFolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    CREATE INDEX [IX_VitalSigns_PatientId] ON [VitalSigns] ([PatientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    ALTER TABLE [Allergies] ADD CONSTRAINT [FK_Allergies_PatientFolders_PatientFolderFolderId] FOREIGN KEY ([PatientFolderFolderId]) REFERENCES [PatientFolders] ([FolderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    ALTER TABLE [Allergies] ADD CONSTRAINT [FK_Allergies_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    ALTER TABLE [Beds] ADD CONSTRAINT [FK_Beds_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027115502_initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251027115502_initial', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027120540_newTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251027120540_newTable', N'8.0.0');
END;
GO

COMMIT;
GO

