-- CapfortuneBE MySQL schema (Railway)
-- Run this against the Railway MySQL database before first deploy.

CREATE TABLE IF NOT EXISTS Customer_Enquiries (
    Id             INT NOT NULL AUTO_INCREMENT,
    CustomerName   VARCHAR(200) NOT NULL,
    CustomerNumber VARCHAR(20)  NOT NULL,
    CustomerEmail  VARCHAR(255) NOT NULL,
    Description    VARCHAR(1000) NULL,
    Status         VARCHAR(50)  NOT NULL,
    Source         VARCHAR(100) NULL,
    CreatedDate    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate   DATETIME NULL,
    PRIMARY KEY (Id),
    INDEX IX_Customer_Enquiries_Source (Source),
    INDEX IX_Customer_Enquiries_CreatedDate (CreatedDate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Run once. This MySQL version doesn't support ADD COLUMN IF NOT EXISTS,
-- so re-running after the columns exist will error — that's expected.
ALTER TABLE Customer_Enquiries
    ADD COLUMN ReplySubject VARCHAR(255) NULL,
    ADD COLUMN ReplyMessage TEXT NULL;

CREATE TABLE IF NOT EXISTS AdminUsers (
    UserId       INT NOT NULL AUTO_INCREMENT,
    UserName     VARCHAR(150) NOT NULL,
    UserMail     VARCHAR(255) NOT NULL,
    MobileNumber VARCHAR(20)  NOT NULL,
    Password     VARCHAR(255) NOT NULL,
    Role         VARCHAR(50)  NOT NULL,
    IsActive     BOOLEAN NOT NULL DEFAULT TRUE,
    PRIMARY KEY (UserId),
    UNIQUE KEY UQ_AdminUsers_UserMail (UserMail)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ErrorLogs (
    Id              INT NOT NULL AUTO_INCREMENT,
    Layer           VARCHAR(100) NOT NULL,
    MethodName      VARCHAR(150) NOT NULL,
    Message         VARCHAR(1000) NOT NULL,
    StackTrace      TEXT NULL,
    RequestPayload  TEXT NULL,
    InnerException  VARCHAR(1000) NULL,
    CreatedDate     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
