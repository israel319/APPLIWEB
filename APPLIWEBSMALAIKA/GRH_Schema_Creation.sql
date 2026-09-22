-- ============================================================================
-- SCRIPT DE CRÉATION - NOUVELLE BASE DE DONNÉES GRH (ISOLÉE)
-- ============================================================================
-- Projet: AppPlusPlus - Module GRH
-- Date: Juin 2026
-- Nom BDD: APW_GRH
-- Description: Création complète de la BDD GRH indépendante
-- Serveur: SQL Server 2019+
-- ============================================================================

-- ============================================================================
-- 1. CRÉATION DE LA BASE DE DONNÉES
-- ============================================================================

-- Vérifier si la BDD existe et la supprimer
IF EXISTS(SELECT * FROM sys.databases WHERE name = 'APW_GRH')
BEGIN
    ALTER DATABASE APW_GRH SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE APW_GRH;
END;

-- Créer la nouvelle BDD
CREATE DATABASE APW_GRH
    COLLATE French_CI_AS;

GO

-- Utiliser la nouvelle BDD
USE APW_GRH;

GO

-- ============================================================================
-- 2. CRÉATION DES TABLES - ORGANISATION
-- ============================================================================

-- Table: Company (Entreprises)
CREATE TABLE [dbo].[T_GRH_Companies] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [CompanyCode] VARCHAR(50) NOT NULL UNIQUE,
    [CompanyName] VARCHAR(100) NOT NULL,
    [LegalName] VARCHAR(150),
    [Description] NVARCHAR(MAX),
    [RegistrationNumber] VARCHAR(50),
    [TaxId] VARCHAR(50),
    [Address] VARCHAR(200),
    [City] VARCHAR(50),
    [ZipCode] VARCHAR(10),
    [Country] VARCHAR(50),
    [Phone] VARCHAR(20),
    [Email] VARCHAR(100),
    [Website] VARCHAR(200),
    [Logo] VARCHAR(500),
    [HeadquartersLocation] VARCHAR(100),
    [NumberOfEmployees] INT DEFAULT 0,
    [Currency] VARCHAR(3) DEFAULT 'TND',
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE()
);

-- Table: Department (Départements)
CREATE TABLE [dbo].[T_GRH_Departments] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [DepartmentCode] VARCHAR(50) NOT NULL UNIQUE,
    [DepartmentName] VARCHAR(100) NOT NULL,
    [CompanyId] INT NOT NULL,
    [Description] NVARCHAR(MAX),
    [HeadId] INT,
    [Location] VARCHAR(100),
    [Budget] DECIMAL(18, 2),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_Department_Company] FOREIGN KEY ([CompanyId]) 
        REFERENCES [T_GRH_Companies]([Id])
);

-- Table: Service (Services/Divisions)
CREATE TABLE [dbo].[T_GRH_Services] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [ServiceCode] VARCHAR(50) NOT NULL UNIQUE,
    [ServiceName] VARCHAR(100) NOT NULL,
    [DepartmentId] INT NOT NULL,
    [CompanyId] INT NOT NULL,
    [Description] NVARCHAR(MAX),
    [HeadId] INT,
    [Budget] DECIMAL(18, 2),
    [Location] VARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_Service_Department] FOREIGN KEY ([DepartmentId]) 
        REFERENCES [T_GRH_Departments]([Id]),
    CONSTRAINT [FK_Service_Company] FOREIGN KEY ([CompanyId]) 
        REFERENCES [T_GRH_Companies]([Id])
);

-- Table: JobPosition (Postes/Métiers)
CREATE TABLE [dbo].[T_GRH_JobPositions] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [PositionCode] VARCHAR(50) NOT NULL UNIQUE,
    [PositionName] VARCHAR(100) NOT NULL,
    [DepartmentId] INT NOT NULL,
    [CompanyId] INT NOT NULL,
    [Description] NVARCHAR(MAX),
    [SalaryRange_Min] DECIMAL(18, 2),
    [SalaryRange_Max] DECIMAL(18, 2),
    [Level] VARCHAR(50), -- Junior/Middle/Senior/Lead/Manager/Director
    [RequiredEducation] VARCHAR(100),
    [RequiredExperience] INT,
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_JobPosition_Department] FOREIGN KEY ([DepartmentId]) 
        REFERENCES [T_GRH_Departments]([Id]),
    CONSTRAINT [FK_JobPosition_Company] FOREIGN KEY ([CompanyId]) 
        REFERENCES [T_GRH_Companies]([Id])
);

-- Table: EmployeeCategory (Catégories avec avantages)
CREATE TABLE [dbo].[T_GRH_EmployeeCategories] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [CategoryCode] VARCHAR(50) NOT NULL UNIQUE,
    [CategoryName] VARCHAR(100) NOT NULL,
    [Description] NVARCHAR(MAX),
    [CategoryLevel] INT, -- 1-10
    [BaseSalaryBand] VARCHAR(50),
    [HealthInsuranceType] VARCHAR(50), -- Full/Partial/None
    [PensionContributionRate] DECIMAL(5, 2), -- %
    [VehicleAllowance] BIT DEFAULT 0,
    [TelephoneAllowance] BIT DEFAULT 0,
    [FoodAllowance] BIT DEFAULT 0,
    [FoodAllowanceAmount] DECIMAL(18, 2),
    [TransportAllowance] BIT DEFAULT 0,
    [TransportAllowanceAmount] DECIMAL(18, 2),
    [HousingAllowance] BIT DEFAULT 0,
    [HousingAllowanceAmount] DECIMAL(18, 2),
    [ChildAllowance] BIT DEFAULT 0,
    [ChildAllowanceAmount] DECIMAL(18, 2),
    [PerformanceBonusEligible] BIT DEFAULT 0,
    [LeaveEntitlementDays] INT DEFAULT 20,
    [AnnualHealthCheckup] BIT DEFAULT 0,
    [FamilyMemberInsurance] BIT DEFAULT 0,
    [TrainingBudgetPerYear] DECIMAL(18, 2),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE()
);

-- Table: EmployeeClassification (Classification contrats)
CREATE TABLE [dbo].[T_GRH_EmployeeClassifications] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [ClassificationCode] VARCHAR(50) NOT NULL UNIQUE,
    [ClassificationName] VARCHAR(100) NOT NULL,
    [Description] NVARCHAR(MAX),
    [ContractType] VARCHAR(50), -- Permanent/Temporary/Contract/Intern/Consultant
    [SocialSecurityEligible] BIT DEFAULT 1,
    [HealthInsuranceRequired] BIT DEFAULT 1,
    [BenefitsEligible] BIT DEFAULT 1,
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE()
);

GO

-- ============================================================================
-- 3. CRÉATION DES TABLES - EMPLOYÉS
-- ============================================================================

-- Table: Employee (Données employés)
CREATE TABLE [dbo].[T_GRH_Employees] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [EmployeeCode] VARCHAR(50) NOT NULL UNIQUE,
    [FirstName] VARCHAR(50) NOT NULL,
    [LastName] VARCHAR(50) NOT NULL,
    [Email] VARCHAR(100) UNIQUE,
    [Phone] VARCHAR(20),
    [BirthDate] DATETIME,
    [BirthPlace] VARCHAR(100),
    [Gender] VARCHAR(10), -- M/F
    [MaritalStatus] VARCHAR(50), -- Single/Married/Divorced/Widowed
    [Nationality] VARCHAR(50),
    [IdNumber] VARCHAR(50),
    [IdType] VARCHAR(50), -- CIN/Passport/Other
    [JoiningDate] DATETIME NOT NULL,
    [CompanyId] INT NOT NULL,
    [DepartmentId] INT NOT NULL,
    [ServiceId] INT,
    [JobPositionId] INT NOT NULL,
    [CategoryId] INT,
    [ClassificationId] INT,
    [ReportingTo] INT, -- FK to Employee (Manager)
    [EmployeeStatus] VARCHAR(50), -- Active/Inactive/OnLeave/Suspended/Terminated
    [StatusChangedDate] DATETIME,
    [EmploymentType] VARCHAR(50), -- FullTime/PartTime/Contract/Intern
    [Address] VARCHAR(200),
    [City] VARCHAR(50),
    [ZipCode] VARCHAR(10),
    [EmergencyContact] VARCHAR(100),
    [EmergencyPhone] VARCHAR(20),
    [BankAccountNumber] VARCHAR(50),
    [BankName] VARCHAR(100),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [CreatedBy] INT,
    [UpdatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedBy] INT,
    CONSTRAINT [FK_Employee_Company] FOREIGN KEY ([CompanyId]) 
        REFERENCES [T_GRH_Companies]([Id]),
    CONSTRAINT [FK_Employee_Department] FOREIGN KEY ([DepartmentId]) 
        REFERENCES [T_GRH_Departments]([Id]),
    CONSTRAINT [FK_Employee_Service] FOREIGN KEY ([ServiceId]) 
        REFERENCES [T_GRH_Services]([Id]),
    CONSTRAINT [FK_Employee_JobPosition] FOREIGN KEY ([JobPositionId]) 
        REFERENCES [T_GRH_JobPositions]([Id]),
    CONSTRAINT [FK_Employee_Category] FOREIGN KEY ([CategoryId]) 
        REFERENCES [T_GRH_EmployeeCategories]([Id]),
    CONSTRAINT [FK_Employee_Classification] FOREIGN KEY ([ClassificationId]) 
        REFERENCES [T_GRH_EmployeeClassifications]([Id]),
    CONSTRAINT [FK_Employee_Manager] FOREIGN KEY ([ReportingTo]) 
        REFERENCES [T_GRH_Employees]([Id])
);

-- Table: EmployeeContract (Contrats de travail)
CREATE TABLE [dbo].[T_GRH_EmployeeContracts] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [EmployeeId] INT NOT NULL,
    [ContractType] VARCHAR(50), -- CDI/CDD/Stage/Apprenticeship
    [ContractNumber] VARCHAR(50) UNIQUE,
    [StartDate] DATETIME NOT NULL,
    [EndDate] DATETIME,
    [SalaryAmount] DECIMAL(18, 2) NOT NULL,
    [Position] VARCHAR(100),
    [Department] VARCHAR(100),
    [WorkingHoursPerWeek] DECIMAL(5, 2) DEFAULT 40,
    [ContractStatus] VARCHAR(50), -- Active/Expired/Terminated
    [ContractPdf] VARCHAR(500),
    [TerminationReason] VARCHAR(200),
    [TerminationDate] DATETIME,
    [Notes] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [CreatedBy] INT,
    CONSTRAINT [FK_EmployeeContract_Employee] FOREIGN KEY ([EmployeeId]) 
        REFERENCES [T_GRH_Employees]([Id]) ON DELETE CASCADE
);

-- Table: EmployeeEducation (Formation/Études)
CREATE TABLE [dbo].[T_GRH_EmployeeEducations] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [EmployeeId] INT NOT NULL,
    [EducationType] VARCHAR(50), -- HighSchool/Bachelor/Master/Doctorate/Certificate
    [InstitutionName] VARCHAR(100),
    [FieldOfStudy] VARCHAR(100),
    [StartDate] DATETIME,
    [EndDate] DATETIME,
    [Grade] VARCHAR(10),
    [Certificate] VARCHAR(500),
    [Description] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_EmployeeEducation_Employee] FOREIGN KEY ([EmployeeId]) 
        REFERENCES [T_GRH_Employees]([Id]) ON DELETE CASCADE
);

-- Table: EmployeeSkill (Compétences)
CREATE TABLE [dbo].[T_GRH_EmployeeSkills] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [EmployeeId] INT NOT NULL,
    [SkillName] VARCHAR(50),
    [ProficiencyLevel] VARCHAR(50), -- Beginner/Intermediate/Advanced/Expert
    [YearsOfExperience] INT,
    [Verified] BIT DEFAULT 0,
    [VerifiedBy] INT,
    [VerifiedDate] DATETIME,
    [Description] NVARCHAR(MAX),
    CONSTRAINT [FK_EmployeeSkill_Employee] FOREIGN KEY ([EmployeeId]) 
        REFERENCES [T_GRH_Employees]([Id]) ON DELETE CASCADE
);

GO

-- ============================================================================
-- 4. CRÉATION DES TABLES - CONGÉS & ABSENCES
-- ============================================================================

-- Table: LeaveType (Types de congés)
CREATE TABLE [dbo].[T_GRH_LeaveTypes] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [LeaveCode] VARCHAR(50) NOT NULL UNIQUE,
    [LeaveTypeName] VARCHAR(100) NOT NULL,
    [Description] NVARCHAR(MAX),
    [DaysAllowedPerYear] INT NOT NULL,
    [IsPaid] BIT DEFAULT 1,
    [RequiresApproval] BIT DEFAULT 1,
    [MaxConsecutiveDays] INT,
    [CarryoverDays] INT,
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE()
);

-- Table: LeaveRequest (Demandes de congés)
CREATE TABLE [dbo].[T_GRH_LeaveRequests] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [EmployeeId] INT NOT NULL,
    [LeaveTypeId] INT NOT NULL,
    [StartDate] DATETIME NOT NULL,
    [EndDate] DATETIME NOT NULL,
    [NumberOfDays] DECIMAL(5, 2) NOT NULL,
    [Reason] NVARCHAR(MAX),
    [LeaveStatus] VARCHAR(50), -- Draft/Pending/Approved/Rejected/Cancelled
    [SubmittedAt] DATETIME,
    [ApprovedAt] DATETIME,
    [ApprovedBy] INT,
    [RejectionReason] NVARCHAR(MAX),
    [Notes] NVARCHAR(MAX),
    [Attachment] VARCHAR(500),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [CreatedBy] INT,
    CONSTRAINT [FK_LeaveRequest_Employee] FOREIGN KEY ([EmployeeId]) 
        REFERENCES [T_GRH_Employees]([Id]),
    CONSTRAINT [FK_LeaveRequest_LeaveType] FOREIGN KEY ([LeaveTypeId]) 
        REFERENCES [T_GRH_LeaveTypes]([Id])
);

-- Table: LeaveSaldo (Soldes de congés)
CREATE TABLE [dbo].[T_GRH_LeaveSaldos] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [EmployeeId] INT NOT NULL,
    [LeaveTypeId] INT NOT NULL,
    [Year] INT NOT NULL,
    [AllowedDays] DECIMAL(5, 2),
    [UsedDays] DECIMAL(5, 2) DEFAULT 0,
    [ApprovedDays] DECIMAL(5, 2) DEFAULT 0,
    [PendingDays] DECIMAL(5, 2) DEFAULT 0,
    [CarriedOverDays] DECIMAL(5, 2) DEFAULT 0,
    [LastUpdated] DATETIME DEFAULT GETDATE(),
    [UpdatedBy] INT,
    CONSTRAINT [FK_LeaveSaldo_Employee] FOREIGN KEY ([EmployeeId]) 
        REFERENCES [T_GRH_Employees]([Id]),
    CONSTRAINT [FK_LeaveSaldo_LeaveType] FOREIGN KEY ([LeaveTypeId]) 
        REFERENCES [T_GRH_LeaveTypes]([Id]),
    CONSTRAINT [UQ_LeaveSaldo] UNIQUE ([EmployeeId], [LeaveTypeId], [Year])
);

-- Table: AbsenceType (Types d'absences)
CREATE TABLE [dbo].[T_GRH_AbsenceTypes] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [AbsenceCode] VARCHAR(50) NOT NULL UNIQUE,
    [AbsenceTypeName] VARCHAR(100),
    [Description] NVARCHAR(MAX),
    [IsPaid] BIT DEFAULT 0,
    [IsActive] BIT DEFAULT 1
);

-- Table: Absence (Absences)
CREATE TABLE [dbo].[T_GRH_Absences] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [EmployeeId] INT NOT NULL,
    [AbsenceTypeId] INT NOT NULL,
    [AbsenceDate] DATETIME NOT NULL,
    [Duration] VARCHAR(50), -- FullDay/HalfDay/Hours
    [Hours] INT,
    [Reason] NVARCHAR(MAX),
    [AbsenceStatus] VARCHAR(50), -- Reported/Justified/Pending/Unjustified
    [Justification] NVARCHAR(MAX),
    [Attachment] VARCHAR(500),
    [ReportedAt] DATETIME,
    [ReportedBy] INT,
    [ReproachState] VARCHAR(50), -- None/Warned/UnderInvestigation
    [Notes] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [CreatedBy] INT,
    CONSTRAINT [FK_Absence_Employee] FOREIGN KEY ([EmployeeId]) 
        REFERENCES [T_GRH_Employees]([Id]),
    CONSTRAINT [FK_Absence_AbsenceType] FOREIGN KEY ([AbsenceTypeId]) 
        REFERENCES [T_GRH_AbsenceTypes]([Id])
);

GO

-- ============================================================================
-- 5. CRÉATION DES TABLES - PAIE & SALAIRES
-- ============================================================================

-- Table: Salary (Salaires courants)
CREATE TABLE [dbo].[T_GRH_Salaries] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [EmployeeId] INT NOT NULL UNIQUE,
    [ContractId] INT,
    [BaseSalary] DECIMAL(18, 2) NOT NULL,
    [Currency] VARCHAR(3) DEFAULT 'TND',
    [EffectiveDate] DATETIME,
    [EndDate] DATETIME,
    [SalaryBand] VARCHAR(50),
    [PaymentFrequency] VARCHAR(50), -- Monthly/Bi-weekly/Weekly
    [PaymentMethod] VARCHAR(50), -- BankTransfer/Check/Cash
    [BankAccountNumber] VARCHAR(50),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_Salary_Employee] FOREIGN KEY ([EmployeeId]) 
        REFERENCES [T_GRH_Employees]([Id])
);

-- Table: SalaryComponent (Éléments de paie)
CREATE TABLE [dbo].[T_GRH_SalaryComponents] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [ComponentCode] VARCHAR(50) NOT NULL UNIQUE,
    [ComponentName] VARCHAR(100) NOT NULL,
    [ComponentType] VARCHAR(50), -- Earning/Deduction/Tax/Insurance
    [Description] NVARCHAR(MAX),
    [Percentage] DECIMAL(5, 2),
    [FixedAmount] DECIMAL(18, 2),
    [IsDefault] BIT DEFAULT 0,
    [CalculationMethod] VARCHAR(50), -- Fixed/Percentage/Manual
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE()
);

-- Table: Deduction (Déductions)
CREATE TABLE [dbo].[T_GRH_Deductions] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [EmployeeId] INT NOT NULL,
    [DeductionTypeId] INT,
    [Amount] DECIMAL(18, 2) NOT NULL,
    [StartDate] DATETIME,
    [EndDate] DATETIME,
    [Reason] VARCHAR(200),
    [ApprovedBy] INT,
    [ApprovedAt] DATETIME,
    [Notes] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [CreatedBy] INT,
    CONSTRAINT [FK_Deduction_Employee] FOREIGN KEY ([EmployeeId]) 
        REFERENCES [T_GRH_Employees]([Id]),
    CONSTRAINT [FK_Deduction_SalaryComponent] FOREIGN KEY ([DeductionTypeId]) 
        REFERENCES [T_GRH_SalaryComponents]([Id])
);

-- Table: PayrollRun (Paie mensuelle)
CREATE TABLE [dbo].[T_GRH_PayrollRuns] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [PayrollCode] VARCHAR(50) NOT NULL UNIQUE,
    [PeriodStartDate] DATETIME NOT NULL,
    [PeriodEndDate] DATETIME NOT NULL,
    [PaymentDate] DATETIME,
    [PayrollStatus] VARCHAR(50), -- Draft/Prepared/Approved/Paid/Cancelled
    [NumberOfEmployees] INT DEFAULT 0,
    [TotalGrossSalary] DECIMAL(18, 2) DEFAULT 0,
    [TotalDeductions] DECIMAL(18, 2) DEFAULT 0,
    [TotalNetSalary] DECIMAL(18, 2) DEFAULT 0,
    [TotalTaxes] DECIMAL(18, 2) DEFAULT 0,
    [TotalInsurance] DECIMAL(18, 2) DEFAULT 0,
    [PreparedAt] DATETIME,
    [PreparedBy] INT,
    [ApprovedAt] DATETIME,
    [ApprovedBy] INT,
    [PaidAt] DATETIME,
    [Notes] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [CreatedBy] INT
);

-- Table: PayrollDetail (Détails de paie)
CREATE TABLE [dbo].[T_GRH_PayrollDetails] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [PayrollRunId] INT NOT NULL,
    [EmployeeId] INT NOT NULL,
    [BaseSalary] DECIMAL(18, 2),
    [Gross] DECIMAL(18, 2),
    [NetSalary] DECIMAL(18, 2),
    [TaxAmount] DECIMAL(18, 2),
    [InsuranceAmount] DECIMAL(18, 2),
    [OtherDeductions] DECIMAL(18, 2),
    [DetailJson] NVARCHAR(MAX), -- JSON breakdown
    [PaymentStatus] VARCHAR(50), -- Pending/Paid/Cancelled
    [PaymentDate] DATETIME,
    [PaymentMethod] VARCHAR(50),
    [Reference] VARCHAR(100),
    [Notes] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [CreatedBy] INT,
    CONSTRAINT [FK_PayrollDetail_PayrollRun] FOREIGN KEY ([PayrollRunId]) 
        REFERENCES [T_GRH_PayrollRuns]([Id]),
    CONSTRAINT [FK_PayrollDetail_Employee] FOREIGN KEY ([EmployeeId]) 
        REFERENCES [T_GRH_Employees]([Id])
);

-- Table: PaymentHistory (Historique versements)
CREATE TABLE [dbo].[T_GRH_PaymentHistories] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [PayrollDetailId] INT NOT NULL,
    [EmployeeId] INT NOT NULL,
    [PaymentDate] DATETIME NOT NULL,
    [Amount] DECIMAL(18, 2) NOT NULL,
    [PaymentMethod] VARCHAR(50), -- BankTransfer/Check/Cash
    [Reference] VARCHAR(100),
    [BankName] VARCHAR(100),
    [Status] VARCHAR(50), -- Pending/Confirmed/Failed/Reversed
    [Notes] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [CreatedBy] INT,
    CONSTRAINT [FK_PaymentHistory_PayrollDetail] FOREIGN KEY ([PayrollDetailId]) 
        REFERENCES [T_GRH_PayrollDetails]([Id]),
    CONSTRAINT [FK_PaymentHistory_Employee] FOREIGN KEY ([EmployeeId]) 
        REFERENCES [T_GRH_Employees]([Id])
);

GO

-- ============================================================================
-- 6. CRÉATION DES TABLES - RECRUTEMENT
-- ============================================================================

-- Table: JobOpening (Offres d'emploi)
CREATE TABLE [dbo].[T_GRH_JobOpenings] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [JobCode] VARCHAR(50) NOT NULL UNIQUE,
    [JobTitle] VARCHAR(100) NOT NULL,
    [DepartmentId] INT,
    [Description] NVARCHAR(MAX),
    [Responsibilities] NVARCHAR(MAX),
    [Requirements] NVARCHAR(MAX),
    [SalaryMin] DECIMAL(18, 2),
    [SalaryMax] DECIMAL(18, 2),
    [OpeningDate] DATETIME,
    [ClosingDate] DATETIME,
    [Status] VARCHAR(50), -- Open/Closed/OnHold/Cancelled
    [NumberOfPositions] INT,
    [EmploymentType] VARCHAR(50), -- FullTime/PartTime/Contract
    [ExperienceLevel] VARCHAR(100),
    [CreatedBy] INT,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_JobOpening_Department] FOREIGN KEY ([DepartmentId]) 
        REFERENCES [T_GRH_Departments]([Id])
);

-- Table: Candidate (Candidats)
CREATE TABLE [dbo].[T_GRH_Candidates] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [CandidateCode] VARCHAR(50) NOT NULL UNIQUE,
    [FirstName] VARCHAR(50) NOT NULL,
    [LastName] VARCHAR(50) NOT NULL,
    [Email] VARCHAR(100) NOT NULL,
    [Phone] VARCHAR(20),
    [BirthDate] DATETIME,
    [Address] VARCHAR(200),
    [City] VARCHAR(50),
    [Nationality] VARCHAR(50),
    [CurrentEmployer] VARCHAR(100),
    [CurrentPosition] VARCHAR(100),
    [YearsOfExperience] INT,
    [Education] NVARCHAR(MAX),
    [Skills] NVARCHAR(MAX),
    [ResumeFile] VARCHAR(500),
    [CoverLetter] NVARCHAR(MAX),
    [SourceOfLead] VARCHAR(50), -- LinkedIn/Website/Agency/Referral/Other
    [ReferrerName] VARCHAR(100),
    [Rating] DECIMAL(3, 1),
    [Notes] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [CreatedBy] INT
);

-- Table: CandidateApplication (Candidatures)
CREATE TABLE [dbo].[T_GRH_CandidateApplications] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [CandidateId] INT NOT NULL,
    [JobOpeningId] INT NOT NULL,
    [ApplicationDate] DATETIME NOT NULL,
    [ApplicationStatus] VARCHAR(50), -- Applied/UnderReview/Shortlisted/Interviewed/OfferExtended/Hired/Rejected
    [ScreeningNotes] NVARCHAR(MAX),
    [ScreenedBy] INT,
    [ScreenedAt] DATETIME,
    [RejectionReason] NVARCHAR(MAX),
    [Notes] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [CreatedBy] INT,
    CONSTRAINT [FK_CandidateApplication_Candidate] FOREIGN KEY ([CandidateId]) 
        REFERENCES [T_GRH_Candidates]([Id]),
    CONSTRAINT [FK_CandidateApplication_JobOpening] FOREIGN KEY ([JobOpeningId]) 
        REFERENCES [T_GRH_JobOpenings]([Id])
);

-- Table: Interview (Entretiens d'embauche)
CREATE TABLE [dbo].[T_GRH_Interviews] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [CandidateId] INT NOT NULL,
    [JobOpeningId] INT NOT NULL,
    [InterviewRound] INT,
    [InterviewDate] DATETIME,
    [InterviewTime] VARCHAR(10),
    [InterviewType] VARCHAR(50), -- Phone/Video/InPerson/Panel
    [InterviewerIds] NVARCHAR(MAX),
    [Location] VARCHAR(100),
    [Feedback] NVARCHAR(MAX),
    [Rating] DECIMAL(3, 1),
    [Result] VARCHAR(50), -- Pass/Fail/Pending
    [NextSteps] NVARCHAR(MAX),
    [ScheduledAt] DATETIME,
    [ScheduledBy] INT,
    [FeedbackGivenAt] DATETIME,
    [Notes] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [CreatedBy] INT,
    CONSTRAINT [FK_Interview_Candidate] FOREIGN KEY ([CandidateId]) 
        REFERENCES [T_GRH_Candidates]([Id]),
    CONSTRAINT [FK_Interview_JobOpening] FOREIGN KEY ([JobOpeningId]) 
        REFERENCES [T_GRH_JobOpenings]([Id])
);

GO

-- ============================================================================
-- 7. CRÉATION DES TABLES - ÉVALUATIONS
-- ============================================================================

-- Table: PerformanceEvaluation (Évaluations de performance)
CREATE TABLE [dbo].[T_GRH_PerformanceEvaluations] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [EvaluationCode] VARCHAR(50) NOT NULL UNIQUE,
    [EmployeeId] INT NOT NULL,
    [EvaluationPeriod] VARCHAR(50), -- Q1/Q2/Q3/Q4/Annual
    [Year] INT,
    [EvaluatorId] INT NOT NULL,
    [EvaluationDate] DATETIME,
    [Status] VARCHAR(50), -- Draft/Submitted/Approved/Completed
    [OverallScore] DECIMAL(3, 1),
    [Comments] NVARCHAR(MAX),
    [Strengths] NVARCHAR(MAX),
    [AreasForImprovement] NVARCHAR(MAX),
    [DevelopmentPlan] NVARCHAR(MAX),
    [SalaryRecommendation] NVARCHAR(MAX),
    [PromotionRecommendation] BIT DEFAULT 0,
    [ApprovedAt] DATETIME,
    [ApprovedBy] INT,
    [Notes] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [CreatedBy] INT,
    CONSTRAINT [FK_PerformanceEvaluation_Employee] FOREIGN KEY ([EmployeeId]) 
        REFERENCES [T_GRH_Employees]([Id])
);

-- Table: EvaluationCriteria (Critères d'évaluation)
CREATE TABLE [dbo].[T_GRH_EvaluationCriteria] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [PerformanceEvaluationId] INT NOT NULL,
    [CriteriaName] VARCHAR(100) NOT NULL,
    [Weight] DECIMAL(5, 2),
    [Score] DECIMAL(3, 1),
    [Comments] NVARCHAR(MAX),
    CONSTRAINT [FK_EvaluationCriteria_PerformanceEvaluation] FOREIGN KEY ([PerformanceEvaluationId]) 
        REFERENCES [T_GRH_PerformanceEvaluations]([Id]) ON DELETE CASCADE
);

-- Table: EmployeeReview (Avis/Revues des collègues)
CREATE TABLE [dbo].[T_GRH_EmployeeReviews] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [PerformanceEvaluationId] INT NOT NULL,
    [ReviewerId] INT NOT NULL,
    [ReviewType] VARCHAR(50), -- PeerReview/360Feedback/ManagerReview
    [Feedback] NVARCHAR(MAX),
    [Rating] DECIMAL(3, 1),
    [Anonymous] BIT DEFAULT 0,
    [SubmittedAt] DATETIME,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [CreatedBy] INT,
    CONSTRAINT [FK_EmployeeReview_PerformanceEvaluation] FOREIGN KEY ([PerformanceEvaluationId]) 
        REFERENCES [T_GRH_PerformanceEvaluations]([Id]) ON DELETE CASCADE
);

GO

-- ============================================================================
-- 8. CRÉATION DES TABLES - FORMATIONS
-- ============================================================================

-- Table: TrainingProgram (Programmes de formation)
CREATE TABLE [dbo].[T_GRH_TrainingPrograms] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [ProgramCode] VARCHAR(50) NOT NULL UNIQUE,
    [ProgramName] VARCHAR(100) NOT NULL,
    [Description] NVARCHAR(MAX),
    [Objective] NVARCHAR(MAX),
    [TrainingType] VARCHAR(50), -- Technical/Soft-Skills/Leadership/Compliance/Other
    [Duration] INT,
    [TrainingProvider] VARCHAR(100),
    [Cost] DECIMAL(18, 2),
    [Currency] VARCHAR(3),
    [TargetAudience] VARCHAR(100),
    [Status] VARCHAR(50), -- Active/Archived/Cancelled
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE()
);

-- Table: TrainingSession (Sessions de formation)
CREATE TABLE [dbo].[T_GRH_TrainingSessions] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [TrainingProgramId] INT NOT NULL,
    [SessionCode] VARCHAR(50) NOT NULL UNIQUE,
    [StartDate] DATETIME NOT NULL,
    [EndDate] DATETIME NOT NULL,
    [Location] VARCHAR(100),
    [Trainer] VARCHAR(100),
    [MaxParticipants] INT,
    [Status] VARCHAR(50), -- Planned/InProgress/Completed/Cancelled
    [Materials] VARCHAR(500),
    [CertificateTemplate] VARCHAR(500),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_TrainingSession_TrainingProgram] FOREIGN KEY ([TrainingProgramId]) 
        REFERENCES [T_GRH_TrainingPrograms]([Id])
);

-- Table: EmployeeTraining (Participations aux formations)
CREATE TABLE [dbo].[T_GRH_EmployeeTrainings] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [EmployeeId] INT NOT NULL,
    [TrainingSessionId] INT NOT NULL,
    [EnrollmentDate] DATETIME,
    [CompletionDate] DATETIME,
    [Status] VARCHAR(50), -- Enrolled/InProgress/Completed/Cancelled/NoShow
    [Score] DECIMAL(5, 2),
    [Feedback] NVARCHAR(MAX),
    [CertificateIssued] BIT DEFAULT 0,
    [CertificateUrl] VARCHAR(500),
    [CertificateNumber] VARCHAR(50),
    [Notes] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [CreatedBy] INT,
    CONSTRAINT [FK_EmployeeTraining_Employee] FOREIGN KEY ([EmployeeId]) 
        REFERENCES [T_GRH_Employees]([Id]),
    CONSTRAINT [FK_EmployeeTraining_TrainingSession] FOREIGN KEY ([TrainingSessionId]) 
        REFERENCES [T_GRH_TrainingSessions]([Id])
);

-- Table: TrainingBudget (Budget de formation)
CREATE TABLE [dbo].[T_GRH_TrainingBudgets] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [DepartmentId] INT,
    [Year] INT NOT NULL,
    [AllocatedBudget] DECIMAL(18, 2) NOT NULL,
    [UsedBudget] DECIMAL(18, 2) DEFAULT 0,
    [ApprovedAt] DATETIME,
    [ApprovedBy] INT,
    [Notes] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_TrainingBudget_Department] FOREIGN KEY ([DepartmentId]) 
        REFERENCES [T_GRH_Departments]([Id])
);

GO

-- ============================================================================
-- 9. CRÉATION DES TABLES - DOCUMENTS
-- ============================================================================

-- Table: DocumentType (Types de documents)
CREATE TABLE [dbo].[T_GRH_DocumentTypes] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [DocumentCode] VARCHAR(50) NOT NULL UNIQUE,
    [DocumentName] VARCHAR(100) NOT NULL,
    [Description] NVARCHAR(MAX),
    [IsRequired] BIT DEFAULT 0,
    [Periodicity] VARCHAR(50), -- OneTime/Annual/Biennial/OnDemand
    [ReminderDays] INT,
    [MaxFileSize] BIGINT,
    [AllowedFormats] VARCHAR(100),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE()
);

-- Table: EmployeeDocument (Documents RH)
CREATE TABLE [dbo].[T_GRH_EmployeeDocuments] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [EmployeeId] INT NOT NULL,
    [DocumentTypeId] INT NOT NULL,
    [DocumentName] VARCHAR(100) NOT NULL,
    [DocumentFile] VARCHAR(500),
    [FileSize] BIGINT,
    [MimeType] VARCHAR(50),
    [UploadDate] DATETIME,
    [ExpiryDate] DATETIME,
    [IsRequired] BIT DEFAULT 0,
    [Status] VARCHAR(50), -- Valid/Expired/Pending/Rejected
    [Notes] NVARCHAR(MAX),
    [UploadedBy] INT,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [UpdatedAt] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_EmployeeDocument_Employee] FOREIGN KEY ([EmployeeId]) 
        REFERENCES [T_GRH_Employees]([Id]),
    CONSTRAINT [FK_EmployeeDocument_DocumentType] FOREIGN KEY ([DocumentTypeId]) 
        REFERENCES [T_GRH_DocumentTypes]([Id])
);

-- Table: EmployeeAttestation (Attestations et certificats)
CREATE TABLE [dbo].[T_GRH_EmployeeAttestations] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [AttestationCode] VARCHAR(50) NOT NULL UNIQUE,
    [EmployeeId] INT NOT NULL,
    [AttestationType] VARCHAR(50), -- Salary/Service/Unemployment/Resignation/Other
    [IssuingDate] DATETIME,
    [Purpose] NVARCHAR(MAX),
    [ValidityPeriod] INT,
    [ExpiryDate] DATETIME,
    [Template] VARCHAR(500),
    [GeneratedFile] VARCHAR(500),
    [SignedBy] INT,
    [SigningDate] DATETIME,
    [Notes] NVARCHAR(MAX),
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    [CreatedBy] INT,
    CONSTRAINT [FK_EmployeeAttestation_Employee] FOREIGN KEY ([EmployeeId]) 
        REFERENCES [T_GRH_Employees]([Id])
);

GO

-- ============================================================================
-- 10. CRÉATION DES INDICES
-- ============================================================================

-- Indices pour recherches rapides
CREATE INDEX [IX_Employee_DepartmentId] ON [T_GRH_Employees]([DepartmentId]);
CREATE INDEX [IX_Employee_JobPositionId] ON [T_GRH_Employees]([JobPositionId]);
CREATE INDEX [IX_Employee_EmployeeStatus] ON [T_GRH_Employees]([EmployeeStatus]);
CREATE INDEX [IX_Employee_CompanyId] ON [T_GRH_Employees]([CompanyId]);
CREATE INDEX [IX_Employee_EmployeeCode] ON [T_GRH_Employees]([EmployeeCode]);

CREATE INDEX [IX_LeaveRequest_EmployeeId] ON [T_GRH_LeaveRequests]([EmployeeId]);
CREATE INDEX [IX_LeaveRequest_Status] ON [T_GRH_LeaveRequests]([LeaveStatus]);
CREATE INDEX [IX_LeaveRequest_Dates] ON [T_GRH_LeaveRequests]([StartDate], [EndDate]);

CREATE INDEX [IX_PayrollRun_PeriodDates] ON [T_GRH_PayrollRuns]([PeriodStartDate], [PeriodEndDate]);
CREATE INDEX [IX_PayrollRun_Status] ON [T_GRH_PayrollRuns]([PayrollStatus]);

CREATE INDEX [IX_PayrollDetail_PayrollRunId] ON [T_GRH_PayrollDetails]([PayrollRunId]);
CREATE INDEX [IX_PayrollDetail_EmployeeId] ON [T_GRH_PayrollDetails]([EmployeeId]);

CREATE INDEX [IX_Absence_EmployeeId] ON [T_GRH_Absences]([EmployeeId]);
CREATE INDEX [IX_Absence_AbsenceDate] ON [T_GRH_Absences]([AbsenceDate]);

CREATE INDEX [IX_CandidateApplication_JobOpeningId] ON [T_GRH_CandidateApplications]([JobOpeningId]);
CREATE INDEX [IX_CandidateApplication_Status] ON [T_GRH_CandidateApplications]([ApplicationStatus]);

CREATE INDEX [IX_Department_CompanyId] ON [T_GRH_Departments]([CompanyId]);
CREATE INDEX [IX_Service_DepartmentId] ON [T_GRH_Services]([DepartmentId]);
CREATE INDEX [IX_JobPosition_DepartmentId] ON [T_GRH_JobPositions]([DepartmentId]);

CREATE INDEX [IX_Interview_CandidateId] ON [T_GRH_Interviews]([CandidateId]);
CREATE INDEX [IX_Interview_InterviewDate] ON [T_GRH_Interviews]([InterviewDate]);

CREATE INDEX [IX_EmployeeDocument_EmployeeId] ON [T_GRH_EmployeeDocuments]([EmployeeId]);
CREATE INDEX [IX_EmployeeDocument_ExpiryDate] ON [T_GRH_EmployeeDocuments]([ExpiryDate]);

GO

-- ============================================================================
-- 11. INSERTION DES DONNÉES DE RÉFÉRENCE
-- ============================================================================

-- Insérer les types de congés
INSERT INTO [T_GRH_LeaveTypes] ([LeaveCode], [LeaveTypeName], [DaysAllowedPerYear], [IsPaid], [RequiresApproval], [IsActive])
VALUES 
    ('ANNUAL', 'Congés payés', 20, 1, 1, 1),
    ('SICK', 'Congés maladie', 15, 1, 0, 1),
    ('MATERNITY', 'Congé maternité', 90, 1, 0, 1),
    ('PATERNITY', 'Congé paternité', 7, 1, 0, 1),
    ('UNPAID', 'Congé sans solde', 30, 0, 1, 1),
    ('TRAINING', 'Congé formation', 5, 1, 1, 1);

-- Insérer les types d'absences
INSERT INTO [T_GRH_AbsenceTypes] ([AbsenceCode], [AbsenceTypeName], [IsPaid], [IsActive])
VALUES 
    ('LATE', 'Retard', 0, 1),
    ('UNJUSTIFIED', 'Absence non justifiée', 0, 1),
    ('JUSTIFIED', 'Absence justifiée', 1, 1),
    ('MEDICAL', 'Absence médicale', 1, 1);

-- Insérer les catégories d'employés
INSERT INTO [T_GRH_EmployeeCategories] 
([CategoryCode], [CategoryName], [CategoryLevel], [HealthInsuranceType], [VehicleAllowance], [TelephoneAllowance], 
 [HousingAllowance], [HousingAllowanceAmount], [TransportAllowance], [TransportAllowanceAmount], 
 [PerformanceBonusEligible], [LeaveEntitlementDays], [FamilyMemberInsurance], [TrainingBudgetPerYear], [IsActive])
VALUES
    ('CADRE_SUP', 'Cadre Supérieur', 9, 'Full', 1, 1, 1, 600, 1, 200, 1, 20, 1, 3000, 1),
    ('CADRE_MOY', 'Cadre Moyen', 7, 'Full', 0, 1, 0, NULL, 1, 150, 1, 20, 1, 1500, 1),
    ('TECHNICIEN', 'Technicien/Spécialiste', 6, 'Partial', 0, 1, 0, NULL, 1, 100, 1, 20, 0, 800, 1),
    ('EMPLOYE', 'Employé Standard', 3, 'Partial', 0, 0, 0, NULL, 1, 80, 0, 20, 0, 300, 1),
    ('STAGIAIRE', 'Stagiaire/Apprenti', 1, 'None', 0, 0, 0, NULL, 0, NULL, 0, 10, 0, 0, 1);

-- Insérer les classifications d'employés
INSERT INTO [T_GRH_EmployeeClassifications] 
([ClassificationCode], [ClassificationName], [ContractType], [SocialSecurityEligible], [HealthInsuranceRequired], [BenefitsEligible], [IsActive])
VALUES
    ('CDI', 'CDI (Permanent)', 'Permanent', 1, 1, 1, 1),
    ('CDD', 'CDD (Temporaire)', 'Temporary', 1, 1, 0, 1),
    ('PRESTATAIRE', 'Prestataire/Contract', 'Contract', 0, 0, 0, 1),
    ('STAGIAIRE', 'Stagiaire', 'Intern', 0, 0, 0, 1),
    ('CONSULTANT', 'Consultant', 'Consultant', 0, 0, 0, 1);

-- Insérer les éléments de paie standards
INSERT INTO [T_GRH_SalaryComponents] 
([ComponentCode], [ComponentName], [ComponentType], [CalculationMethod], [IsDefault], [IsActive])
VALUES
    ('BASE', 'Salaire de base', 'Earning', 'Fixed', 1, 1),
    ('BONUS', 'Prime performance', 'Earning', 'Percentage', 0, 1),
    ('OVERTIME', 'Heures supplémentaires', 'Earning', 'Manual', 0, 1),
    ('SENIORITY', 'Prime ancienneté', 'Earning', 'Percentage', 0, 1),
    ('CNSS', 'Cotisation CNSS', 'Deduction', 'Percentage', 1, 1),
    ('INCOME_TAX', 'Impôt sur le revenu', 'Tax', 'Manual', 1, 1),
    ('HEALTH_INS', 'Assurance maladie', 'Insurance', 'Fixed', 1, 1),
    ('ADVANCE', 'Avance sur salaire', 'Deduction', 'Manual', 0, 1);

-- Insérer les types de documents
INSERT INTO [T_GRH_DocumentTypes] 
([DocumentCode], [DocumentName], [IsRequired], [Periodicity], [ReminderDays], [IsActive])
VALUES
    ('CONTRACT', 'Contrat de travail', 1, 'OneTime', 30, 1),
    ('MEDICAL_CERT', 'Certificat médical', 1, 'Annual', 30, 1),
    ('ID_COPY', 'Copie CIN/Passeport', 1, 'Biennial', 60, 1),
    ('ADDRESS_PROOF', 'Justificatif de domicile', 0, 'Annual', 30, 1),
    ('QUALIFICATION', 'Diplômes/Certifications', 0, 'OneTime', 0, 1);

GO

-- ============================================================================
-- 12. CRÉATION DES PROCÉDURES STOCKÉES
-- ============================================================================

-- Procédure: Calculer la paie mensuelle
CREATE PROCEDURE [sp_CalculatePayroll]
    @PayrollRunId INT,
    @PeriodStartDate DATETIME,
    @PeriodEndDate DATETIME,
    @CalculatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @EmployeeId INT;
        DECLARE @BaseSalary DECIMAL(18,2);
        DECLARE @Gross DECIMAL(18,2);
        DECLARE @TotalDeductions DECIMAL(18,2);
        DECLARE @TaxAmount DECIMAL(18,2);
        DECLARE @NetSalary DECIMAL(18,2);
        
        -- Cursor pour chaque employé actif
        DECLARE emp_cursor CURSOR FOR
            SELECT e.Id, s.BaseSalary
            FROM T_GRH_Employees e
            INNER JOIN T_GRH_Salaries s ON e.Id = s.EmployeeId
            WHERE e.EmployeeStatus = 'Active';
        
        OPEN emp_cursor;
        FETCH NEXT FROM emp_cursor INTO @EmployeeId, @BaseSalary;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @Gross = @BaseSalary;
            SET @TaxAmount = @Gross * 0.1; -- 10% impôt
            SET @TotalDeductions = @Gross * 0.15; -- 15% total déductions
            SET @NetSalary = @Gross - @TotalDeductions;
            
            INSERT INTO T_GRH_PayrollDetails 
                (PayrollRunId, EmployeeId, BaseSalary, Gross, TaxAmount, OtherDeductions, NetSalary, PaymentStatus, CreatedBy, CreatedAt)
            VALUES 
                (@PayrollRunId, @EmployeeId, @BaseSalary, @Gross, @TaxAmount, @TotalDeductions - @TaxAmount, @NetSalary, 'Pending', @CalculatedBy, GETDATE());
            
            UPDATE T_GRH_PayrollRuns 
            SET TotalGrossSalary = TotalGrossSalary + @Gross,
                TotalDeductions = TotalDeductions + @TotalDeductions,
                TotalNetSalary = TotalNetSalary + @NetSalary,
                TotalTaxes = TotalTaxes + @TaxAmount,
                NumberOfEmployees = NumberOfEmployees + 1
            WHERE Id = @PayrollRunId;
            
            FETCH NEXT FROM emp_cursor INTO @EmployeeId, @BaseSalary;
        END;
        
        CLOSE emp_cursor;
        DEALLOCATE emp_cursor;
        
        UPDATE T_GRH_PayrollRuns 
        SET PayrollStatus = 'Prepared'
        WHERE Id = @PayrollRunId;
        
        COMMIT TRANSACTION;
        
        SELECT 'SUCCESS' AS Result, 'Payroll calculated successfully' AS Message;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 'ERROR' AS Result, ERROR_MESSAGE() AS Message;
    END CATCH;
END;

GO

-- Procédure: Mettre à jour les soldes de congés
CREATE PROCEDURE [sp_UpdateLeaveSaldo]
    @EmployeeId INT,
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @LeaveTypeId INT;
    DECLARE @AllowedDays INT;
    DECLARE @UsedDays DECIMAL(5,1);
    DECLARE @ApprovedDays DECIMAL(5,1);
    
    DECLARE leave_cursor CURSOR FOR
        SELECT Id, DaysAllowedPerYear FROM T_GRH_LeaveTypes WHERE IsActive = 1;
    
    OPEN leave_cursor;
    FETCH NEXT FROM leave_cursor INTO @LeaveTypeId, @AllowedDays;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @UsedDays = ISNULL((
            SELECT SUM(NumberOfDays)
            FROM T_GRH_LeaveRequests
            WHERE EmployeeId = @EmployeeId 
                AND LeaveTypeId = @LeaveTypeId
                AND YEAR(StartDate) = @Year
                AND LeaveStatus IN ('Approved')
        ), 0);
        
        SET @ApprovedDays = ISNULL((
            SELECT SUM(NumberOfDays)
            FROM T_GRH_LeaveRequests
            WHERE EmployeeId = @EmployeeId 
                AND LeaveTypeId = @LeaveTypeId
                AND YEAR(StartDate) = @Year
                AND LeaveStatus = 'Pending'
        ), 0);
        
        IF EXISTS (SELECT 1 FROM T_GRH_LeaveSaldos 
                  WHERE EmployeeId = @EmployeeId AND LeaveTypeId = @LeaveTypeId AND Year = @Year)
        BEGIN
            UPDATE T_GRH_LeaveSaldos
            SET UsedDays = @UsedDays,
                ApprovedDays = @ApprovedDays,
                LastUpdated = GETDATE()
            WHERE EmployeeId = @EmployeeId AND LeaveTypeId = @LeaveTypeId AND Year = @Year;
        END
        ELSE
        BEGIN
            INSERT INTO T_GRH_LeaveSaldos 
                (EmployeeId, LeaveTypeId, Year, AllowedDays, UsedDays, ApprovedDays, PendingDays, LastUpdated)
            VALUES 
                (@EmployeeId, @LeaveTypeId, @Year, @AllowedDays, @UsedDays, @ApprovedDays, 0, GETDATE());
        END;
        
        FETCH NEXT FROM leave_cursor INTO @LeaveTypeId, @AllowedDays;
    END;
    
    CLOSE leave_cursor;
    DEALLOCATE leave_cursor;
END;

GO

-- Procédure: Réinitialiser les congés annuels
CREATE PROCEDURE [sp_GenerateAnnualLeaveReset]
    @CurrentYear INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @AnnualLeaveTypeId INT = (SELECT Id FROM T_GRH_LeaveTypes WHERE LeaveCode = 'ANNUAL');
        DECLARE @CarryoverDays INT = (SELECT CarryoverDays FROM T_GRH_LeaveTypes WHERE Id = @AnnualLeaveTypeId);
        
        IF @CarryoverDays IS NULL SET @CarryoverDays = 5;
        
        INSERT INTO T_GRH_LeaveSaldos (EmployeeId, LeaveTypeId, Year, AllowedDays, UsedDays, ApprovedDays, PendingDays, CarriedOverDays, LastUpdated)
        SELECT 
            ls.EmployeeId,
            ls.LeaveTypeId,
            @CurrentYear,
            lt.DaysAllowedPerYear,
            0,
            0,
            0,
            CASE WHEN (ls.AllowedDays - ls.UsedDays - ls.ApprovedDays) > @CarryoverDays 
                 THEN @CarryoverDays 
                 ELSE (ls.AllowedDays - ls.UsedDays - ls.ApprovedDays)
            END,
            GETDATE()
        FROM T_GRH_LeaveSaldos ls
        INNER JOIN T_GRH_LeaveTypes lt ON ls.LeaveTypeId = lt.Id
        INNER JOIN T_GRH_Employees e ON ls.EmployeeId = e.Id
        WHERE ls.Year = @CurrentYear - 1
            AND lt.Id = @AnnualLeaveTypeId
            AND e.EmployeeStatus = 'Active'
            AND NOT EXISTS (SELECT 1 FROM T_GRH_LeaveSaldos WHERE EmployeeId = ls.EmployeeId AND Year = @CurrentYear AND LeaveTypeId = @AnnualLeaveTypeId);
        
        COMMIT TRANSACTION;
        SELECT 'SUCCESS' AS Result, 'Annual leave reset completed' AS Message;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 'ERROR' AS Result, ERROR_MESSAGE() AS Message;
    END CATCH;
END;

GO

-- ============================================================================
-- 13. AFFICHAGE DU RÉSUMÉ
-- ============================================================================

PRINT '====================================================================';
PRINT 'BASE DE DONNÉES GRH CRÉÉE AVEC SUCCÈS!';
PRINT '====================================================================';
PRINT 'Base de données: APW_GRH';
PRINT 'Tables créées: 30+';
PRINT 'Indices créés: 20+';
PRINT 'Procédures stockées: 3';
PRINT 'Données de référence insérées: 4 tables';
PRINT '====================================================================';
PRINT '';
PRINT 'Tables principales:';
PRINT '  Organization: Company, Department, Service, JobPosition, EmployeeCategory, EmployeeClassification';
PRINT '  Employees: Employee, EmployeeContract, EmployeeEducation, EmployeeSkill';
PRINT '  Leave & Absence: LeaveType, LeaveRequest, LeaveSaldo, Absence, AbsenceType';
PRINT '  Payroll: Salary, SalaryComponent, Deduction, PayrollRun, PayrollDetail, PaymentHistory';
PRINT '  Recruitment: JobOpening, Candidate, CandidateApplication, Interview';
PRINT '  Evaluation: PerformanceEvaluation, EvaluationCriteria, EmployeeReview';
PRINT '  Training: TrainingProgram, TrainingSession, EmployeeTraining, TrainingBudget';
PRINT '  Documents: DocumentType, EmployeeDocument, EmployeeAttestation';
PRINT '';
PRINT '====================================================================';

GO
