DROP TABLE IF EXISTS PasswordUsers;
CREATE TABLE PasswordUsers (
	Id	    INTEGER NOT NULL,
	Name	TEXT NOT NULL,
	Mail	TEXT NOT NULL,
	Info	TEXT,
	Icon	BLOB,
	PRIMARY KEY(Id AUTOINCREMENT)
);

DROP TABLE IF EXISTS UserLocations;
CREATE TABLE UserLocations (
	Id	INTEGER NOT NULL,
	Area	TEXT NOT NULL,
	Info	TEXT,
	User	INTEGER NOT NULL,
	Name	TEXT,
	Pass	BLOB NOT NULL,
	PRIMARY KEY(Id AUTOINCREMENT),
	FOREIGN KEY(User) REFERENCES PasswordUsers(Id) ON DELETE CASCADE
);

DROP TABLE IF EXISTS UserPasswords;
CREATE TABLE UserPasswords (
    Id		INTEGER NOT NULL,
	User	INTEGER NOT NULL,
	Hash	INTEGER NOT NULL,
	Pass	TEXT,
	PRIMARY KEY(Id AUTOINCREMENT),
	FOREIGN KEY(User) REFERENCES PasswordUsers(Id) ON DELETE CASCADE
);

