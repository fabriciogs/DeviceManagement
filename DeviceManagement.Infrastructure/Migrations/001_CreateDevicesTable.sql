CREATE TABLE Devices (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Brand NVARCHAR(100) NOT NULL,
    State INT NOT NULL, -- 1: Available, 2: InUse, 3: Inactive
    CreationTime DATETIME NOT NULL
);