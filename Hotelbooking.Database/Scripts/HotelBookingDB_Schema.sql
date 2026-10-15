-- ====================================================================================================
-- Database: HotelBookingDB
-- Description: Hotel Booking Management System Database Script (16 Tables)
-- Architecture: ASP.NET Core MVC (.NET 8) with Entity Framework Core (Database First)
-- ====================================================================================================

-- 1. Create Database if not exists
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'HotelBookingDB')
BEGIN
    CREATE DATABASE [HotelBookingDB];
END
GO

USE [HotelBookingDB];
GO

-- ====================================================================================================
-- MASTER TABLES (1 - 11)
-- ====================================================================================================

-- 1. TblCustomer
IF OBJECT_ID(N'dbo.TblCustomer', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TblCustomer (
        CustomerId INT IDENTITY(1,1) NOT NULL,
        CustomerCode VARCHAR(50) NOT NULL,
        Name VARCHAR(150) NOT NULL,
        Phone VARCHAR(50) NULL,
        Email VARCHAR(150) NULL,
        Address VARCHAR(300) NULL,
        NRC VARCHAR(100) NULL,
        Gender VARCHAR(20) NULL,
        DateOfBirth DATE NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_TblCustomer_CreatedDate DEFAULT GETDATE(),
        CONSTRAINT PK_TblCustomer PRIMARY KEY CLUSTERED (CustomerId ASC),
        CONSTRAINT UQ_TblCustomer_CustomerCode UNIQUE NONCLUSTERED (CustomerCode ASC)
    );

    CREATE NONCLUSTERED INDEX IX_TblCustomer_Phone ON dbo.TblCustomer (Phone ASC);
    CREATE NONCLUSTERED INDEX IX_TblCustomer_NRC ON dbo.TblCustomer (NRC ASC);
    CREATE NONCLUSTERED INDEX IX_TblCustomer_Name ON dbo.TblCustomer (Name ASC);
END
GO

-- 2. TblFloor
IF OBJECT_ID(N'dbo.TblFloor', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TblFloor (
        FloorId INT IDENTITY(1,1) NOT NULL,
        FloorName VARCHAR(100) NOT NULL,
        Description VARCHAR(300) NULL,
        Status BIT NOT NULL CONSTRAINT DF_TblFloor_Status DEFAULT 1,
        CONSTRAINT PK_TblFloor PRIMARY KEY CLUSTERED (FloorId ASC),
        CONSTRAINT UQ_TblFloor_FloorName UNIQUE NONCLUSTERED (FloorName ASC)
    );
END
GO

-- 3. TblRoomType
IF OBJECT_ID(N'dbo.TblRoomType', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TblRoomType (
        RoomTypeId INT IDENTITY(1,1) NOT NULL,
        RoomTypeName VARCHAR(100) NOT NULL,
        Description VARCHAR(300) NULL,
        MaxGuest INT NOT NULL,
        Status BIT NOT NULL CONSTRAINT DF_TblRoomType_Status DEFAULT 1,
        CONSTRAINT PK_TblRoomType PRIMARY KEY CLUSTERED (RoomTypeId ASC),
        CONSTRAINT UQ_TblRoomType_RoomTypeName UNIQUE NONCLUSTERED (RoomTypeName ASC)
    );
END
GO

-- 4. TblRoomStatus
IF OBJECT_ID(N'dbo.TblRoomStatus', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TblRoomStatus (
        StatusId INT IDENTITY(1,1) NOT NULL,
        StatusName VARCHAR(50) NOT NULL,
        Description VARCHAR(200) NULL,
        IsAvailable BIT NOT NULL CONSTRAINT DF_TblRoomStatus_IsAvailable DEFAULT 1,
        CONSTRAINT PK_TblRoomStatus PRIMARY KEY CLUSTERED (StatusId ASC),
        CONSTRAINT UQ_TblRoomStatus_StatusName UNIQUE NONCLUSTERED (StatusName ASC)
    );
END
GO

-- Seed initial master records for TblRoomStatus
IF NOT EXISTS (SELECT 1 FROM dbo.TblRoomStatus WHERE StatusName = 'Available')
    INSERT INTO dbo.TblRoomStatus (StatusName, Description, IsAvailable) VALUES ('Available', 'Room is ready for booking', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.TblRoomStatus WHERE StatusName = 'Reserved')
    INSERT INTO dbo.TblRoomStatus (StatusName, Description, IsAvailable) VALUES ('Reserved', 'Room has confirmed reservation', 0);

IF NOT EXISTS (SELECT 1 FROM dbo.TblRoomStatus WHERE StatusName = 'Occupied')
    INSERT INTO dbo.TblRoomStatus (StatusName, Description, IsAvailable) VALUES ('Occupied', 'Guest currently staying', 0);

IF NOT EXISTS (SELECT 1 FROM dbo.TblRoomStatus WHERE StatusName = 'Maintenance')
    INSERT INTO dbo.TblRoomStatus (StatusName, Description, IsAvailable) VALUES ('Maintenance', 'Room under repair or out of service', 0);

IF NOT EXISTS (SELECT 1 FROM dbo.TblRoomStatus WHERE StatusName = 'Cleaning')
    INSERT INTO dbo.TblRoomStatus (StatusName, Description, IsAvailable) VALUES ('Cleaning', 'Room under housekeeping/cleaning', 0);
GO

-- 5. TblRoom
IF OBJECT_ID(N'dbo.TblRoom', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TblRoom (
        RoomId INT IDENTITY(1,1) NOT NULL,
        RoomNo VARCHAR(50) NOT NULL,
        FloorId INT NOT NULL,
        RoomTypeId INT NOT NULL,
        StatusId INT NOT NULL,
        Description VARCHAR(300) NULL,
        CONSTRAINT PK_TblRoom PRIMARY KEY CLUSTERED (RoomId ASC),
        CONSTRAINT UQ_TblRoom_RoomNo UNIQUE NONCLUSTERED (RoomNo ASC),
        CONSTRAINT FK_TblRoom_TblFloor FOREIGN KEY (FloorId) REFERENCES dbo.TblFloor (FloorId),
        CONSTRAINT FK_TblRoom_TblRoomType FOREIGN KEY (RoomTypeId) REFERENCES dbo.TblRoomType (RoomTypeId),
        CONSTRAINT FK_TblRoom_TblRoomStatus FOREIGN KEY (StatusId) REFERENCES dbo.TblRoomStatus (StatusId)
    );

    CREATE NONCLUSTERED INDEX IX_TblRoom_FloorId ON dbo.TblRoom (FloorId ASC);
    CREATE NONCLUSTERED INDEX IX_TblRoom_RoomTypeId ON dbo.TblRoom (RoomTypeId ASC);
    CREATE NONCLUSTERED INDEX IX_TblRoom_StatusId ON dbo.TblRoom (StatusId ASC);
END
GO

-- 6. TblRoomPrice (Room Price Master)
IF OBJECT_ID(N'dbo.TblRoomPrice', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TblRoomPrice (
        RoomPriceId INT IDENTITY(1,1) NOT NULL,
        RoomTypeId INT NOT NULL,
        PriceName VARCHAR(100) NOT NULL,
        EffectiveDate DATE NOT NULL,
        Status BIT NOT NULL CONSTRAINT DF_TblRoomPrice_Status DEFAULT 1,
        CONSTRAINT PK_TblRoomPrice PRIMARY KEY CLUSTERED (RoomPriceId ASC),
        CONSTRAINT FK_TblRoomPrice_TblRoomType FOREIGN KEY (RoomTypeId) REFERENCES dbo.TblRoomType (RoomTypeId)
    );

    CREATE NONCLUSTERED INDEX IX_TblRoomPrice_RoomTypeId_EffectiveDate 
        ON dbo.TblRoomPrice (RoomTypeId ASC, EffectiveDate DESC);
END
GO

-- 7. TblRoomPriceDetail (Time-based pricing detail)
IF OBJECT_ID(N'dbo.TblRoomPriceDetail', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TblRoomPriceDetail (
        RoomPriceDetailId INT IDENTITY(1,1) NOT NULL,
        RoomPriceId INT NOT NULL,
        StartTime TIME(0) NOT NULL,
        EndTime TIME(0) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        Status BIT NOT NULL CONSTRAINT DF_TblRoomPriceDetail_Status DEFAULT 1,
        CONSTRAINT PK_TblRoomPriceDetail PRIMARY KEY CLUSTERED (RoomPriceDetailId ASC),
        CONSTRAINT FK_TblRoomPriceDetail_TblRoomPrice FOREIGN KEY (RoomPriceId) REFERENCES dbo.TblRoomPrice (RoomPriceId) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX IX_TblRoomPriceDetail_RoomPriceId 
        ON dbo.TblRoomPriceDetail (RoomPriceId ASC);
END
GO

-- 8. TblServiceCategory
IF OBJECT_ID(N'dbo.TblServiceCategory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TblServiceCategory (
        ServiceCategoryId INT IDENTITY(1,1) NOT NULL,
        CategoryName VARCHAR(100) NOT NULL,
        Description VARCHAR(300) NULL,
        Status BIT NOT NULL CONSTRAINT DF_TblServiceCategory_Status DEFAULT 1,
        CONSTRAINT PK_TblServiceCategory PRIMARY KEY CLUSTERED (ServiceCategoryId ASC),
        CONSTRAINT UQ_TblServiceCategory_CategoryName UNIQUE NONCLUSTERED (CategoryName ASC)
    );
END
GO

-- 9. TblService
IF OBJECT_ID(N'dbo.TblService', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TblService (
        ServiceId INT IDENTITY(1,1) NOT NULL,
        ServiceCategoryId INT NOT NULL,
        ServiceName VARCHAR(150) NOT NULL,
        Description VARCHAR(300) NULL,
        Price DECIMAL(18,2) NOT NULL,
        Status BIT NOT NULL CONSTRAINT DF_TblService_Status DEFAULT 1,
        CONSTRAINT PK_TblService PRIMARY KEY CLUSTERED (ServiceId ASC),
        CONSTRAINT FK_TblService_TblServiceCategory FOREIGN KEY (ServiceCategoryId) REFERENCES dbo.TblServiceCategory (ServiceCategoryId)
    );

    CREATE NONCLUSTERED INDEX IX_TblService_ServiceCategoryId ON dbo.TblService (ServiceCategoryId ASC);
END
GO

-- 10. TblExtraBedPrice
IF OBJECT_ID(N'dbo.TblExtraBedPrice', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TblExtraBedPrice (
        ExtraBedPriceId INT IDENTITY(1,1) NOT NULL,
        RoomTypeId INT NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        EffectiveDate DATE NOT NULL,
        Status BIT NOT NULL CONSTRAINT DF_TblExtraBedPrice_Status DEFAULT 1,
        CONSTRAINT PK_TblExtraBedPrice PRIMARY KEY CLUSTERED (ExtraBedPriceId ASC),
        CONSTRAINT FK_TblExtraBedPrice_TblRoomType FOREIGN KEY (RoomTypeId) REFERENCES dbo.TblRoomType (RoomTypeId)
    );

    CREATE NONCLUSTERED INDEX IX_TblExtraBedPrice_RoomTypeId_EffectiveDate 
        ON dbo.TblExtraBedPrice (RoomTypeId ASC, EffectiveDate DESC);
END
GO

-- 11. TblBanking
IF OBJECT_ID(N'dbo.TblBanking', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TblBanking (
        BankingId INT IDENTITY(1,1) NOT NULL,
        BankName VARCHAR(150) NOT NULL,
        AccountName VARCHAR(150) NOT NULL,
        AccountNumber VARCHAR(100) NOT NULL,
        BranchName VARCHAR(150) NULL,
        AccountType VARCHAR(50) NULL,
        Description VARCHAR(300) NULL,
        Status BIT NOT NULL CONSTRAINT DF_TblBanking_Status DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_TblBanking_CreatedDate DEFAULT GETDATE(),
        CONSTRAINT PK_TblBanking PRIMARY KEY CLUSTERED (BankingId ASC)
    );
END
GO

-- ====================================================================================================
-- TRANSACTION TABLES (12 - 16)
-- ====================================================================================================

-- 12. TblBooking
IF OBJECT_ID(N'dbo.TblBooking', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TblBooking (
        BookingId INT IDENTITY(1,1) NOT NULL,
        BookingNo VARCHAR(50) NOT NULL,
        CustomerId INT NOT NULL,
        BookingDate DATETIME2 NOT NULL CONSTRAINT DF_TblBooking_BookingDate DEFAULT GETDATE(),
        Status VARCHAR(50) NOT NULL,
        Remarks VARCHAR(500) NULL,
        CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_TblBooking_CreatedDate DEFAULT GETDATE(),
        CONSTRAINT PK_TblBooking PRIMARY KEY CLUSTERED (BookingId ASC),
        CONSTRAINT UQ_TblBooking_BookingNo UNIQUE NONCLUSTERED (BookingNo ASC),
        CONSTRAINT FK_TblBooking_TblCustomer FOREIGN KEY (CustomerId) REFERENCES dbo.TblCustomer (CustomerId)
    );

    CREATE NONCLUSTERED INDEX IX_TblBooking_CustomerId ON dbo.TblBooking (CustomerId ASC);
    CREATE NONCLUSTERED INDEX IX_TblBooking_BookingDate ON dbo.TblBooking (BookingDate DESC);
    CREATE NONCLUSTERED INDEX IX_TblBooking_Status ON dbo.TblBooking (Status ASC);
END
GO

-- 13. TblBookingDetail
IF OBJECT_ID(N'dbo.TblBookingDetail', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TblBookingDetail (
        BookingDetailId INT IDENTITY(1,1) NOT NULL,
        BookingId INT NOT NULL,
        RoomId INT NOT NULL,
        CheckInDateTime DATETIME2 NOT NULL,
        CheckOutDateTime DATETIME2 NOT NULL,
        RoomPrice DECIMAL(18,2) NOT NULL,
        ExtraBedQty INT NOT NULL CONSTRAINT DF_TblBookingDetail_ExtraBedQty DEFAULT 0,
        ExtraBedPrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_TblBookingDetail_ExtraBedPrice DEFAULT 0,
        TotalRoomPrice DECIMAL(18,2) NOT NULL,
        Remarks VARCHAR(500) NULL,
        CONSTRAINT PK_TblBookingDetail PRIMARY KEY CLUSTERED (BookingDetailId ASC),
        CONSTRAINT FK_TblBookingDetail_TblBooking FOREIGN KEY (BookingId) REFERENCES dbo.TblBooking (BookingId),
        CONSTRAINT FK_TblBookingDetail_TblRoom FOREIGN KEY (RoomId) REFERENCES dbo.TblRoom (RoomId)
    );

    CREATE NONCLUSTERED INDEX IX_TblBookingDetail_BookingId ON dbo.TblBookingDetail (BookingId ASC);
    CREATE NONCLUSTERED INDEX IX_TblBookingDetail_RoomId ON dbo.TblBookingDetail (RoomId ASC);
    CREATE NONCLUSTERED INDEX IX_TblBookingDetail_Dates ON dbo.TblBookingDetail (CheckInDateTime ASC, CheckOutDateTime ASC);
END
GO

-- 14. TblBookingService
IF OBJECT_ID(N'dbo.TblBookingService', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TblBookingService (
        BookingServiceId INT IDENTITY(1,1) NOT NULL,
        BookingId INT NOT NULL,
        BookingDetailId INT NULL,
        ServiceId INT NOT NULL,
        Qty DECIMAL(18,2) NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        TotalAmount DECIMAL(18,2) NOT NULL,
        ServiceDate DATETIME2 NOT NULL CONSTRAINT DF_TblBookingService_ServiceDate DEFAULT GETDATE(),
        CONSTRAINT PK_TblBookingService PRIMARY KEY CLUSTERED (BookingServiceId ASC),
        CONSTRAINT FK_TblBookingService_TblBooking FOREIGN KEY (BookingId) REFERENCES dbo.TblBooking (BookingId),
        CONSTRAINT FK_TblBookingService_TblBookingDetail FOREIGN KEY (BookingDetailId) REFERENCES dbo.TblBookingDetail (BookingDetailId),
        CONSTRAINT FK_TblBookingService_TblService FOREIGN KEY (ServiceId) REFERENCES dbo.TblService (ServiceId)
    );

    CREATE NONCLUSTERED INDEX IX_TblBookingService_BookingId ON dbo.TblBookingService (BookingId ASC);
    CREATE NONCLUSTERED INDEX IX_TblBookingService_BookingDetailId ON dbo.TblBookingService (BookingDetailId ASC);
    CREATE NONCLUSTERED INDEX IX_TblBookingService_ServiceId ON dbo.TblBookingService (ServiceId ASC);
END
GO

-- 15. TblRoomTransfer
IF OBJECT_ID(N'dbo.TblRoomTransfer', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TblRoomTransfer (
        RoomTransferId INT IDENTITY(1,1) NOT NULL,
        BookingDetailId INT NOT NULL,
        FromRoomId INT NOT NULL,
        ToRoomId INT NOT NULL,
        TransferDate DATETIME2 NOT NULL CONSTRAINT DF_TblRoomTransfer_TransferDate DEFAULT GETDATE(),
        Reason VARCHAR(300) NULL,
        Remarks VARCHAR(500) NULL,
        CONSTRAINT PK_TblRoomTransfer PRIMARY KEY CLUSTERED (RoomTransferId ASC),
        CONSTRAINT FK_TblRoomTransfer_TblBookingDetail FOREIGN KEY (BookingDetailId) REFERENCES dbo.TblBookingDetail (BookingDetailId),
        CONSTRAINT FK_TblRoomTransfer_FromRoom FOREIGN KEY (FromRoomId) REFERENCES dbo.TblRoom (RoomId),
        CONSTRAINT FK_TblRoomTransfer_ToRoom FOREIGN KEY (ToRoomId) REFERENCES dbo.TblRoom (RoomId)
    );

    CREATE NONCLUSTERED INDEX IX_TblRoomTransfer_BookingDetailId ON dbo.TblRoomTransfer (BookingDetailId ASC);
    CREATE NONCLUSTERED INDEX IX_TblRoomTransfer_FromRoomId ON dbo.TblRoomTransfer (FromRoomId ASC);
    CREATE NONCLUSTERED INDEX IX_TblRoomTransfer_ToRoomId ON dbo.TblRoomTransfer (ToRoomId ASC);
END
GO

-- 16. TblPayment
IF OBJECT_ID(N'dbo.TblPayment', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TblPayment (
        PaymentId INT IDENTITY(1,1) NOT NULL,
        BookingId INT NOT NULL,
        PaymentDate DATETIME2 NOT NULL CONSTRAINT DF_TblPayment_PaymentDate DEFAULT GETDATE(),
        Amount DECIMAL(18,2) NOT NULL,
        PaymentMethod VARCHAR(50) NOT NULL,
        PaymentStatus VARCHAR(50) NOT NULL,
        BankingId INT NULL,
        ReferenceNo VARCHAR(100) NULL,
        Remarks VARCHAR(500) NULL,
        CONSTRAINT PK_TblPayment PRIMARY KEY CLUSTERED (PaymentId ASC),
        CONSTRAINT FK_TblPayment_TblBooking FOREIGN KEY (BookingId) REFERENCES dbo.TblBooking (BookingId),
        CONSTRAINT FK_TblPayment_TblBanking FOREIGN KEY (BankingId) REFERENCES dbo.TblBanking (BankingId)
    );

    CREATE NONCLUSTERED INDEX IX_TblPayment_BookingId ON dbo.TblPayment (BookingId ASC);
    CREATE NONCLUSTERED INDEX IX_TblPayment_BankingId ON dbo.TblPayment (BankingId ASC);
    CREATE NONCLUSTERED INDEX IX_TblPayment_PaymentDate ON dbo.TblPayment (PaymentDate DESC);
END
GO
