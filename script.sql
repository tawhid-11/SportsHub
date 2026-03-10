USE [SportsHubDB]
GO

/****** Object:  Table [dbo].[Overs] ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Overs]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Overs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CricketMatchID] [int] NOT NULL,
	[BowlerId] [int] NOT NULL,
	[Innings] [int] NOT NULL,
	[OverNumber] [int] NULL,
 CONSTRAINT [PK_Overs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]
END
GO

/****** Object:  Table [dbo].[MatchBallByBall] ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MatchBallByBall]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[MatchBallByBall](
	[BallID] [int] IDENTITY(1,1) NOT NULL,
	[OverId] [int] NULL,
	[StrikerPlayerID] [int] NULL,
	[NonStrikerPlayerID] [int] NULL,
	[BowlerPlayerID] [int] NULL,
	[Run] [int] NULL,
	[IsWicket] [bit] NULL DEFAULT 0,
	[IsBye] [bit] NULL DEFAULT 0,
	[BallType] [nvarchar](50) NULL, -- 'Normal', 'Wide', 'NoBall'
	[WicketType] [nvarchar](50) NULL,
	[PlayerOutID] [int] NULL,
	[CreatedAt] [datetime2](7) NULL DEFAULT GETDATE(),
 CONSTRAINT [PK_MatchBallByBall] PRIMARY KEY CLUSTERED 
(
	[BallID] ASC
)
) ON [PRIMARY]
END
GO

-- If tables exist, ensure the columns added during development are present
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Overs') AND name = 'OverNumber')
    ALTER TABLE Overs ADD OverNumber INT NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MatchBallByBall') AND name = 'IsBye')
    ALTER TABLE MatchBallByBall ADD IsBye BIT DEFAULT 0;
