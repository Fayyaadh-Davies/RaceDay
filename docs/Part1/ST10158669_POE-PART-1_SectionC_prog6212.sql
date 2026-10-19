/*
-- RACEDAY SYSTEM
-- SECTION C - MYSQL DATABASE SCRIPT
*/

CREATE DATABASE IF NOT EXISTS RaceDayDB;

USE RaceDayDB;

/*
-- 1. ORGANISATION
*/

CREATE TABLE Organisation (
    OrganisationID INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(150) NOT NULL,
    RegistrationNumber VARCHAR(50) NOT NULL UNIQUE,
    ContactEmail VARCHAR(150) NOT NULL,
    ContactNumber VARCHAR(30),
    Address VARCHAR(250)
);


/*
-- 2. USER
*/

CREATE TABLE User (
    UserID INT AUTO_INCREMENT PRIMARY KEY,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL,

    CONSTRAINT CK_User_Role
    CHECK (Role IN ('Admin', 'Organiser', 'Participant'))
);


/*
-- 3. PARTICIPANT
*/
CREATE TABLE Participant (
    ParticipantID INT AUTO_INCREMENT PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    DateOfBirth DATE,
    Gender VARCHAR(20),
    EmergencyContact VARCHAR(100),
    EmergencyContactNumber VARCHAR(30),

    CONSTRAINT FK_Participant_User
    FOREIGN KEY (UserID)
    REFERENCES User(UserID)
);


/*
-- 4. CATEGORY
*/

CREATE TABLE Category (
    CategoryID INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Type VARCHAR(20) NOT NULL,
    DistanceKM DECIMAL(6,2) NOT NULL,

    CONSTRAINT CK_Category_Type
    CHECK (Type IN ('Running', 'Walking', 'Cycling')),

    CONSTRAINT CK_Category_Distance
    CHECK (DistanceKM > 0)
);


/*
-- 5. EVENT
*/

CREATE TABLE Event (
    EventID INT AUTO_INCREMENT PRIMARY KEY,
    OrganisationID INT NOT NULL,
    Name VARCHAR(150) NOT NULL,
    Description VARCHAR(500),
    EventDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    Location VARCHAR(200) NOT NULL,
    Status VARCHAR(30) NOT NULL DEFAULT 'Upcoming',

    CONSTRAINT FK_Event_Organisation
    FOREIGN KEY (OrganisationID)
    REFERENCES Organisation(OrganisationID),

    CONSTRAINT CK_Event_Status
    CHECK (Status IN ('Upcoming', 'Open', 'Completed', 'Cancelled'))
);


/*
-- 6. EVENT CATEGORY
*/

CREATE TABLE EventCategory (
    EventCategoryID INT AUTO_INCREMENT PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL DEFAULT 0,
    MaximumParticipants INT,

    CONSTRAINT FK_EventCategory_Event
    FOREIGN KEY (EventID)
    REFERENCES Event(EventID)
    ON DELETE CASCADE,

    CONSTRAINT FK_EventCategory_Category
    FOREIGN KEY (CategoryID)
    REFERENCES Category(CategoryID),

    CONSTRAINT UQ_Event_Category
    UNIQUE (EventID, CategoryID)
);


/*
-- 7. ENROLMENT
*/

CREATE TABLE Enrolment (
    EnrolmentID INT AUTO_INCREMENT PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    EventCategoryID INT NOT NULL,
    EnrolmentDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(30) NOT NULL DEFAULT 'Confirmed',

    CONSTRAINT FK_Enrolment_Participant
    FOREIGN KEY (ParticipantID)
    REFERENCES Participant(ParticipantID),

    CONSTRAINT FK_Enrolment_Event
    FOREIGN KEY (EventID)
    REFERENCES Event(EventID),

    CONSTRAINT FK_Enrolment_EventCategory
    FOREIGN KEY (EventCategoryID)
    REFERENCES EventCategory(EventCategoryID),

    CONSTRAINT CK_Enrolment_Status
    CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled')),

    CONSTRAINT UQ_Participant_Event
    UNIQUE (ParticipantID, EventID)
);


/*
-- 8. RESULT
*/

CREATE TABLE Result (
    ResultID INT AUTO_INCREMENT PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,
    FinishTime TIME,
    Position INT,
    Pace VARCHAR(30),
    ResultStatus VARCHAR(30) NOT NULL DEFAULT 'Finished',

    CONSTRAINT FK_Result_Enrolment
    FOREIGN KEY (EnrolmentID)
    REFERENCES Enrolment(EnrolmentID),

    CONSTRAINT CK_Result_Position
    CHECK (Position IS NULL OR Position > 0)
);


/*
-- 9. ROUTE
*/

CREATE TABLE Route (
    RouteID INT AUTO_INCREMENT PRIMARY KEY,
    EventID INT NOT NULL UNIQUE,
    Name VARCHAR(150) NOT NULL,
    DistanceKM DECIMAL(6,2) NOT NULL,
    ElevationM INT,
    MapURL VARCHAR(500),

    CONSTRAINT FK_Route_Event
    FOREIGN KEY (EventID)
    REFERENCES Event(EventID)
    ON DELETE CASCADE,

    CONSTRAINT CK_Route_Distance
    CHECK (DistanceKM > 0)
);


/*
-- 10. ENVIRONMENT
*/

CREATE TABLE Environment (
    EnvironmentID INT AUTO_INCREMENT PRIMARY KEY,
    EventID INT NOT NULL UNIQUE,
    TemperatureC DECIMAL(5,2),
    HumidityPercent DECIMAL(5,2),
    WindSpeedKMH DECIMAL(6,2),
    SurfaceCondition VARCHAR(100),

    CONSTRAINT FK_Environment_Event
    FOREIGN KEY (EventID)
    REFERENCES Event(EventID)
    ON DELETE CASCADE
);


/*
-- 11. WEATHER
*/

CREATE TABLE Weather (
    WeatherID INT AUTO_INCREMENT PRIMARY KEY,
    EventID INT NOT NULL,
    RecordedAt DATETIME NOT NULL,
    TemperatureC DECIMAL(5,2),
    WeatherCondition VARCHAR(100),
    WindSpeedKMH DECIMAL(6,2),
    RainProbability DECIMAL(5,2),

    CONSTRAINT FK_Weather_Event
    FOREIGN KEY (EventID)
    REFERENCES Event(EventID)
    ON DELETE CASCADE,

    CONSTRAINT CK_Weather_Rain
    CHECK (RainProbability BETWEEN 0 AND 100)
);


/*
-- SAMPLE DATA
-- 4 ORGANISATIONS
*/

INSERT INTO Organisation
(Name, RegistrationNumber, ContactEmail, ContactNumber, Address)
VALUES
('Johannesburg Road Runners',
 'ORG001',
 'info@jrr.co.za',
 '0115551001',
 'Johannesburg, Gauteng'),

('Durban Coastal Athletics',
 'ORG002',
 'events@dca.co.za',
 '0315551002',
 'Durban, KwaZulu-Natal'),

('Cape Active Events',
 'ORG003',
 'info@capeactive.co.za',
 '0215551003',
 'Cape Town, Western Cape'),

('Gauteng Cycling Events',
 'ORG004',
 'events@gce.co.za',
 '0115551004',
 'Pretoria, Gauteng');


/*
-- SAMPLE USERS
-- 7 PARTICIPANTS + ORGANISERS + ADMIN
*/

INSERT INTO User
(FirstName, LastName, Email, PasswordHash, Role)
VALUES
('Admin', 'RaceDay', 'admin@raceday.co.za', 'HASH_ADMIN', 'Admin'),

('Thabo', 'Mokoena', 'thabo@example.com', 'HASH001', 'Participant'),
('Ayesha', 'Davids', 'ayesha@example.com', 'HASH002', 'Participant'),
('Liam', 'Naidoo', 'liam@example.com', 'HASH003', 'Participant'),
('Mia', 'Jacobs', 'mia@example.com', 'HASH004', 'Participant'),
('Daniel', 'Peters', 'daniel@example.com', 'HASH005', 'Participant'),
('Zanele', 'Dlamini', 'zanele@example.com', 'HASH006', 'Participant'),
('Ryan', 'Smith', 'ryan@example.com', 'HASH007', 'Participant'),

('John', 'Radebe', 'john@jrr.co.za', 'HASH_ORG1', 'Organiser'),
('Fatima', 'Khan', 'fatima@dca.co.za', 'HASH_ORG2', 'Organiser'),
('Jason', 'Williams', 'jason@capeactive.co.za', 'HASH_ORG3', 'Organiser'),
('Peter', 'Molefe', 'peter@gce.co.za', 'HASH_ORG4', 'Organiser');


/*
-- 7 PARTICIPANTS
*/

INSERT INTO Participant
(UserID, DateOfBirth, Gender, EmergencyContact, EmergencyContactNumber)
VALUES
(2, '1998-05-12', 'Male', 'Sipho Mokoena', '0821111111'),
(3, '1999-08-21', 'Female', 'Sara Davids', '0822222222'),
(4, '1997-03-17', 'Male', 'Nadia Naidoo', '0823333333'),
(5, '2000-11-05', 'Female', 'Paul Jacobs', '0824444444'),
(6, '1996-07-09', 'Male', 'Linda Peters', '0825555555'),
(7, '1998-12-15', 'Female', 'Thandi Dlamini', '0826666666'),
(8, '1995-02-28', 'Male', 'Mark Smith', '0827777777');


/*
-- CATEGORIES
*/

INSERT INTO Category
(Name, Type, DistanceKM)
VALUES
('5KM Fun Run', 'Running', 5.00),
('10KM Road Run', 'Running', 10.00),
('21KM Half Marathon', 'Running', 21.10),
('5KM Walk', 'Walking', 5.00),
('10KM Walk', 'Walking', 10.00),
('20KM Cycle', 'Cycling', 20.00),
('50KM Cycle', 'Cycling', 50.00);


/*
-- 4 EVENTS
*/

INSERT INTO Event
(OrganisationID, Name, Description, EventDate, StartTime, Location, Status)
VALUES
(1,
 'Johannesburg City Run',
 'Annual Johannesburg road running event',
 '2026-10-10',
 '06:30:00',
 'Johannesburg',
 'Open'),

(2,
 'Durban Coastal Walk',
 'Scenic coastal walking event',
 '2026-10-18',
 '07:00:00',
 'Durban',
 'Open'),

(3,
 'Cape Active Half Marathon',
 'Half marathon through Cape Town',
 '2026-11-01',
 '06:00:00',
 'Cape Town',
 'Upcoming'),

(4,
 'Gauteng Cycle Challenge',
 'Road cycling challenge',
 '2026-11-15',
 '05:30:00',
 'Pretoria',
 'Upcoming');


/*
-- EVENT CATEGORIES
*/

INSERT INTO EventCategory
(EventID, CategoryID, EntryFee, MaximumParticipants)
VALUES
(1, 1, 100.00, 1000),
(1, 2, 180.00, 1000),

(2, 4, 80.00, 500),
(2, 5, 120.00, 500),

(3, 2, 180.00, 1500),
(3, 3, 300.00, 1500),

(4, 6, 200.00, 800),
(4, 7, 350.00, 800);


/*
-- ROUTES
*/

INSERT INTO Route
(EventID, Name, DistanceKM, ElevationM, MapURL)
VALUES
(1,
 'Johannesburg City Route',
 10.00,
 180,
 'https://example.com/routes/johannesburg'),

(2,
 'Durban Coastal Route',
 10.00,
 65,
 'https://example.com/routes/durban'),

(3,
 'Cape Town Half Marathon Route',
 21.10,
 220,
 'https://example.com/routes/capetown'),

(4,
 'Gauteng Cycling Route',
 50.00,
 540,
 'https://example.com/routes/gauteng');


/*
-- ENVIRONMENT
*/

INSERT INTO Environment
(EventID, TemperatureC, HumidityPercent, WindSpeedKMH, SurfaceCondition)
VALUES
(1, 18.00, 55.00, 12.00, 'Dry asphalt'),
(2, 22.00, 70.00, 18.00, 'Dry coastal road'),
(3, 16.00, 60.00, 10.00, 'Dry asphalt'),
(4, 15.00, 50.00, 15.00, 'Dry road');


/*
-- WEATHER
*/

INSERT INTO Weather
(EventID, RecordedAt, TemperatureC, WeatherCondition,
 WindSpeedKMH, RainProbability)
VALUES
(1, '2026-10-10 05:30:00', 17.50, 'Partly Cloudy', 11.00, 10.00),

(2, '2026-10-18 06:00:00', 21.00, 'Sunny', 16.00, 5.00),

(3, '2026-11-01 05:00:00', 15.00, 'Clear', 9.00, 5.00),

(4, '2026-11-15 04:30:00', 14.00, 'Clear', 13.00, 8.00);


/*
-- SAMPLE ENROLMENTS
*/

INSERT INTO Enrolment
(ParticipantID, EventID, EventCategoryID, Status)
VALUES
(1, 1, 1, 'Confirmed'),
(2, 1, 2, 'Confirmed'),
(3, 1, 1, 'Confirmed'),
(4, 2, 3, 'Confirmed'),
(5, 2, 4, 'Confirmed'),
(6, 3, 6, 'Confirmed'),
(7, 4, 8, 'Confirmed');


/*
-- SAMPLE RESULTS
*/

INSERT INTO Result
(EnrolmentID, FinishTime, Position, Pace, ResultStatus)
VALUES
(1, '00:27:45', 15, '5:33 min/km', 'Finished'),
(2, '00:52:20', 21, '5:14 min/km', 'Finished'),
(3, '00:31:10', 38, '6:14 min/km', 'Finished');

