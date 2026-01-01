-- 宠物互助平台数据库初始化脚本
-- MySQL 8.0+
-- 执行前请确保数据库服务器正在运行

-- 创建数据库（如果不存在）
CREATE DATABASE IF NOT EXISTS petpal
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

-- 使用数据库
USE petpal;

-- 设置字符集
SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;

-- 创建用户表
CREATE TABLE IF NOT EXISTS Users (
    Id VARCHAR(255) PRIMARY KEY,
    Username VARCHAR(50) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Phone VARCHAR(20) NOT NULL UNIQUE,
    Email VARCHAR(100),
    Role INT NOT NULL DEFAULT 0,
    Status INT NOT NULL DEFAULT 0,
    ReputationScore INT NOT NULL DEFAULT 100,
    ReputationLevel NVARCHAR(50) NOT NULL DEFAULT '新手',
    IsRealNameCertified BOOLEAN NOT NULL DEFAULT FALSE,
    IsPetCertified BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastLoginAt DATETIME NULL,
    INDEX idx_username (Username),
    INDEX idx_phone (Phone),
    INDEX idx_email (Email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 创建宠物表
CREATE TABLE IF NOT EXISTS Pets (
    Id VARCHAR(255) PRIMARY KEY,
    UserId VARCHAR(255) NOT NULL,
    Name VARCHAR(50) NOT NULL,
    Type VARCHAR(50) NOT NULL,
    Breed VARCHAR(50),
    Age INT NOT NULL DEFAULT 0,
    IsVaccinated BOOLEAN NOT NULL DEFAULT FALSE,
    Description TEXT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_userid (UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 创建互助订单表
CREATE TABLE IF NOT EXISTS MutualOrders (
    Id VARCHAR(255) PRIMARY KEY,
    RequesterId VARCHAR(255) NOT NULL,
    HelperId VARCHAR(255) NULL,
    PetId VARCHAR(255) NOT NULL,
    HelpType INT NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NOT NULL,
    Longitude DOUBLE NOT NULL,
    Latitude DOUBLE NOT NULL,
    Remark TEXT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AcceptedAt DATETIME NULL,
    CompletedAt DATETIME NULL,
    FOREIGN KEY (RequesterId) REFERENCES Users(Id) ON DELETE RESTRICT,
    FOREIGN KEY (HelperId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (PetId) REFERENCES Pets(Id) ON DELETE CASCADE,
    INDEX idx_requester (RequesterId),
    INDEX idx_helper (HelperId),
    INDEX idx_status_created (Status, CreatedAt),
    INDEX idx_location (Longitude, Latitude)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 创建订单评价表
CREATE TABLE IF NOT EXISTS OrderEvaluations (
    Id VARCHAR(255) PRIMARY KEY,
    OrderId VARCHAR(255) NOT NULL,
    EvaluatorId VARCHAR(255) NOT NULL,
    EvaluatedUserId VARCHAR(255) NOT NULL,
    EvaluationType VARCHAR(50) NOT NULL,
    Score INT NOT NULL CHECK (Score >= 1 AND Score <= 5),
    Content TEXT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (OrderId) REFERENCES MutualOrders(Id) ON DELETE CASCADE,
    FOREIGN KEY (EvaluatorId) REFERENCES Users(Id) ON DELETE RESTRICT,
    FOREIGN KEY (EvaluatedUserId) REFERENCES Users(Id) ON DELETE RESTRICT,
    INDEX idx_order (OrderId),
    INDEX idx_evaluator (EvaluatorId),
    INDEX idx_evaluated (EvaluatedUserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 显示创建结果
SELECT 'Database initialization completed successfully!' as Status;
SHOW TABLES;
