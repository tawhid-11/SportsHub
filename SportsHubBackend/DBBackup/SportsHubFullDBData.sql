
USE [SportsHubDB]
GO
/****** Object:  Table [dbo].[CricketMatch]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CricketMatch](
	[CricketMatchID] [int] IDENTITY(1,1) NOT NULL,
	[TeamScheduleID] [int] NOT NULL,
	[TossWinnerTeamID] [int] NULL,
	[TossChoice] [nvarchar](10) NULL,
	[Overs] [int] NULL,
	[Umpire] [nvarchar](100) NULL,
	[Venue] [nvarchar](200) NULL,
	[StrikerPlayerID] [int] NULL,
	[NonStrikerPlayerID] [int] NULL,
	[BowlerPlayerID] [int] NULL,
	[CurrentInnings] [int] NULL,
	[MatchStatus] [nvarchar](50) NULL,
	[WinnerTeamID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[CricketMatchID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MatchBallByBall]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MatchBallByBall](
	[BallID] [int] IDENTITY(1,1) NOT NULL,
	[OverId] [int] NULL,
	[StrikerPlayerID] [int] NULL,
	[NonStrikerPlayerID] [int] NULL,
	[BowlerPlayerID] [int] NULL,
	[Run] [int] NULL,
	[IsWicket] [bit] NULL,
	[BallType] [nvarchar](50) NULL,
	[WicketType] [nvarchar](50) NULL,
	[PlayerOutID] [int] NULL,
	[CreatedAt] [datetime2](7) NULL,
	[IsBye] [bit] NULL,
 CONSTRAINT [PK_MatchBallByBall] PRIMARY KEY CLUSTERED 
(
	[BallID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Overs]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Overs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CricketMatchID] [int] NOT NULL,
	[BowlerId] [int] NOT NULL,
	[Innings] [int] NOT NULL,
	[OverNumber] [int] NULL,
 CONSTRAINT [PK_Overs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentHistory]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentHistory](
	[PaymentHistoryID] [int] IDENTITY(1,1) NOT NULL,
	[TournamentTeamMappingID] [int] NOT NULL,
	[TournamentId] [int] NOT NULL,
	[TeamId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[PaymentMethod] [nvarchar](50) NULL,
	[PaymentStatus] [nvarchar](50) NOT NULL,
	[TransactionId] [nvarchar](255) NULL,
	[PaymentDate] [datetime] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[bkashPaymentId] [nvarchar](255) NULL,
	[bkashTransactionId] [nvarchar](255) NULL,
	[UpdatedDate] [datetime] NULL,
	[FailureReason] [nvarchar](500) NULL,
 CONSTRAINT [PK_PaymentHistory] PRIMARY KEY CLUSTERED 
(
	[PaymentHistoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PlayerRole]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PlayerRole](
	[PlayerRoleID] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] [nvarchar](200) NULL,
	[Description] [nvarchar](200) NULL,
	[IsActive] [bit] NULL,
	[CreatedAt] [datetime] NULL,
 CONSTRAINT [PK_PlayerRole] PRIMARY KEY CLUSTERED 
(
	[PlayerRoleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Players]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Players](
	[PlayerID] [int] IDENTITY(1,1) NOT NULL,
	[TeamsID] [int] NULL,
	[PlayerRoleID] [int] NULL,
	[PlayerImage] [nvarchar](200) NULL,
	[FullName] [nvarchar](200) NULL,
	[Nationality] [nvarchar](200) NULL,
	[DateOfBirth] [date] NULL,
	[BirthPlace] [nvarchar](200) NULL,
	[NickName] [nvarchar](200) NULL,
	[BattingStyle] [nvarchar](200) NULL,
	[BowlingStyle] [nvarchar](200) NULL,
	[IsActive] [nvarchar](200) NULL,
 CONSTRAINT [PK_Player] PRIMARY KEY CLUSTERED 
(
	[PlayerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TeamPayment]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TeamPayment](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Phone] [nvarchar](10) NULL,
	[OTP] [int] NULL,
	[Amount] [float] NULL,
	[userId] [int] NULL,
 CONSTRAINT [PK_TeamPayment] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Teams]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Teams](
	[TeamsID] [int] IDENTITY(1,1) NOT NULL,
	[TeamName] [nvarchar](200) NOT NULL,
	[UserId] [int] NULL,
	[ShortName] [nvarchar](200) NULL,
	[TeamLogo] [nvarchar](200) NULL,
	[TeamOwnerName] [nvarchar](200) NULL,
	[TeamOwnerEmail] [nvarchar](200) NULL,
	[TeamOwnerPhoneNumber] [nvarchar](200) NULL,
	[CoachName] [nvarchar](200) NULL,
	[FoundedYear] [int] NULL,
	[TotalPlayers] [int] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_Teams] PRIMARY KEY CLUSTERED 
(
	[TeamsID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TeamSchedule]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TeamSchedule](
	[TeamScheduleID] [int] IDENTITY(1,1) NOT NULL,
	[TeamAID] [int] NULL,
	[TeamBID] [int] NULL,
	[MatchDate] [datetime2](7) NULL,
	[TournamentID] [int] NULL,
	[Phase] [nvarchar](50) NULL,
 CONSTRAINT [PK_TeamSchedule] PRIMARY KEY CLUSTERED 
(
	[TeamScheduleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TournamentPointTable]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TournamentPointTable](
	[PointTableID] [int] IDENTITY(1,1) NOT NULL,
	[TournamentID] [int] NULL,
	[TeamsID] [int] NULL,
	[Played] [int] NULL,
	[Won] [int] NULL,
	[Lost] [int] NULL,
	[Draw] [int] NULL,
	[NR] [int] NULL,
	[Points] [int] NULL,
	[NRR] [float] NULL,
	[TotalRunsScored] [int] NULL,
	[TotalBallsFaced] [int] NULL,
	[TotalRunsConceded] [int] NULL,
	[TotalBallsBowled] [int] NULL,
 CONSTRAINT [PK_TournamentPointTable] PRIMARY KEY CLUSTERED 
(
	[PointTableID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tournaments]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tournaments](
	[TournamentID] [int] IDENTITY(1,1) NOT NULL,
	[TournamentName] [nvarchar](200) NULL,
	[Prize] [nvarchar](200) NULL,
	[StartDate] [datetime2](7) NULL,
	[EndDate] [datetime2](7) NULL,
	[Location] [nvarchar](200) NULL,
	[TournamentTypeID] [int] NULL,
	[RegistrationDeadline] [datetime2](7) NULL,
	[TotalPlayer] [int] NULL,
	[MatchPlayer] [int] NULL,
	[ExtraPlayer] [int] NULL,
	[Status] [nvarchar](200) NULL,
	[RegistrationFee] [int] NULL,
	[FieldFee] [int] NULL,
	[MaxTeams] [int] NULL,
	[ContactNumber] [nvarchar](200) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[CreatedBy] [int] NULL,
	[UpdatedBy] [int] NULL,
	[IsActive] [bit] NULL,
	[CurrentPhase] [nvarchar](50) NULL,
 CONSTRAINT [PK_Tournaments] PRIMARY KEY CLUSTERED 
(
	[TournamentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TournamentTeamMapping]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TournamentTeamMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TournamentId] [int] NULL,
	[TeamId] [int] NULL,
	[PaymentStatus] [nvarchar](50) NULL,
	[PaymentDate] [datetime] NULL,
	[bkashPaymentId] [nvarchar](50) NULL,
	[bkashTransactionId] [nvarchar](255) NULL,
	[CreatedDate] [datetime] NULL,
	[GroupId] [int] NULL,
 CONSTRAINT [PK_TournamentTeamMapping] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TournamentType]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TournamentType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NULL,
 CONSTRAINT [PK_TournamentType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserInfo]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NULL,
	[Email] [nvarchar](100) NULL,
	[Phone] [nvarchar](100) NULL,
	[UserType] [nvarchar](100) NULL,
	[Password] [nvarchar](100) NULL,
 CONSTRAINT [PK_UserInfo] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[CricketMatch] ON 
GO
INSERT [dbo].[CricketMatch] ([CricketMatchID], [TeamScheduleID], [TossWinnerTeamID], [TossChoice], [Overs], [Umpire], [Venue], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [CurrentInnings], [MatchStatus], [WinnerTeamID]) VALUES (2002, 2003, 6008, N'Bat', 3, N'Tamzid', N'Mirpur', 7, 8, 2, 2, N'Finished', 6009)
GO
SET IDENTITY_INSERT [dbo].[CricketMatch] OFF
GO
SET IDENTITY_INSERT [dbo].[MatchBallByBall] ON 
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1002, 1002, 1, 4, 5, 1, 0, N'Wide', NULL, NULL, CAST(N'2026-01-25T11:52:53.8700000' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1003, 1002, 1, 4, 5, 6, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:53:08.2100000' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1004, 1002, 1, 4, 5, 0, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:53:20.1866667' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1005, 1002, 1, 4, 5, 0, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:53:20.3400000' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1006, 1002, 1, 4, 5, 0, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:53:20.4600000' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1007, 1002, 1, 4, 5, 0, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:53:20.6700000' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1008, 1002, 1, 4, 5, 1, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:53:24.5433333' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1009, 1003, 1, 4, 10, 3, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:53:49.4233333' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1010, 1003, 4, 1, 10, 2, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:53:51.8000000' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1011, 1003, 4, 1, 10, 1, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:53:58.4800000' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1012, 1003, 1, 4, 10, 1, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:53:59.4300000' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1013, 1003, 4, 1, 10, 0, 1, N'Normal', N'Bowled', 4, CAST(N'2026-01-25T11:54:16.3066667' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1014, 1003, 3, 1, 10, 1, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:54:21.8333333' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1015, 1004, 3, 1, 7, 1, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:55:04.4966667' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1016, 1004, 1, 3, 7, 2, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:55:05.8800000' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1017, 1004, 1, 3, 7, 4, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:55:08.1866667' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1018, 1004, 1, 3, 7, 4, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:55:08.9566667' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1019, 1004, 1, 3, 7, 4, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:55:14.1233333' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1020, 1004, 1, 3, 7, 2, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:55:16.3466667' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1021, 1005, 8, 7, 4, 0, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:56:20.9166667' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1022, 1005, 8, 7, 4, 6, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:57:12.0366667' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1023, 1005, 8, 7, 4, 6, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:57:14.0933333' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1024, 1005, 8, 7, 4, 6, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:57:15.0000000' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1025, 1005, 8, 7, 4, 6, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:57:15.6700000' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1026, 1005, 8, 7, 4, 6, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:57:18.9500000' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1027, 1006, 7, 8, 2, 2, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:57:31.1533333' AS DateTime2), 0)
GO
INSERT [dbo].[MatchBallByBall] ([BallID], [OverId], [StrikerPlayerID], [NonStrikerPlayerID], [BowlerPlayerID], [Run], [IsWicket], [BallType], [WicketType], [PlayerOutID], [CreatedAt], [IsBye]) VALUES (1028, 1006, 7, 8, 2, 3, 0, N'Normal', NULL, NULL, CAST(N'2026-01-25T11:57:36.0000000' AS DateTime2), 0)
GO
SET IDENTITY_INSERT [dbo].[MatchBallByBall] OFF
GO
SET IDENTITY_INSERT [dbo].[Overs] ON 
GO
INSERT [dbo].[Overs] ([Id], [CricketMatchID], [BowlerId], [Innings], [OverNumber]) VALUES (1002, 2002, 5, 1, 1)
GO
INSERT [dbo].[Overs] ([Id], [CricketMatchID], [BowlerId], [Innings], [OverNumber]) VALUES (1003, 2002, 10, 1, 2)
GO
INSERT [dbo].[Overs] ([Id], [CricketMatchID], [BowlerId], [Innings], [OverNumber]) VALUES (1004, 2002, 7, 1, 3)
GO
INSERT [dbo].[Overs] ([Id], [CricketMatchID], [BowlerId], [Innings], [OverNumber]) VALUES (1005, 2002, 4, 2, 1)
GO
INSERT [dbo].[Overs] ([Id], [CricketMatchID], [BowlerId], [Innings], [OverNumber]) VALUES (1006, 2002, 2, 2, 2)
GO
SET IDENTITY_INSERT [dbo].[Overs] OFF
GO
SET IDENTITY_INSERT [dbo].[PlayerRole] ON 
GO
INSERT [dbo].[PlayerRole] ([PlayerRoleID], [RoleName], [Description], [IsActive], [CreatedAt]) VALUES (6, N'Batting Allrounder', N'GoodBater', 1, CAST(N'2026-01-22T14:05:23.367' AS DateTime))
GO
INSERT [dbo].[PlayerRole] ([PlayerRoleID], [RoleName], [Description], [IsActive], [CreatedAt]) VALUES (7, N'Batsman', N'Excellent', 1, CAST(N'2026-01-22T14:05:47.260' AS DateTime))
GO
INSERT [dbo].[PlayerRole] ([PlayerRoleID], [RoleName], [Description], [IsActive], [CreatedAt]) VALUES (8, N'Bowler', N'Death Bowler', 1, CAST(N'2026-01-22T14:06:00.897' AS DateTime))
GO
INSERT [dbo].[PlayerRole] ([PlayerRoleID], [RoleName], [Description], [IsActive], [CreatedAt]) VALUES (9, N'Bowling Allrounder', N'Can do power hitting', 1, CAST(N'2026-01-22T14:06:16.567' AS DateTime))
GO
INSERT [dbo].[PlayerRole] ([PlayerRoleID], [RoleName], [Description], [IsActive], [CreatedAt]) VALUES (10, N'All-rounder', N'Exxceptional', 1, CAST(N'2026-01-22T14:06:39.187' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[PlayerRole] OFF
GO
SET IDENTITY_INSERT [dbo].[Players] ON 
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (1, 6008, 7, N'/images/3d79f93f-d5c9-4401-8a7f-949cfa4488c3.jpeg', N'Abir', N'Bangladeshi', CAST(N'2000-01-22' AS Date), NULL, N'abir', N'Right Handed Batter', N'Off Spinner', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (2, 6008, 6, N'/images/bedfef2b-23f7-4b02-984a-f999f5beae63.jpeg', N'Hridoy', N'Bangladeshi', CAST(N'2000-11-11' AS Date), NULL, N'hr', N'Left Handed Batter', N'Right-Arm FastBowler', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (3, 6008, 8, N'/images/fea06860-66da-4a56-b3af-a84bc8edf428.jpeg', N'Rakin', N'Bangladeshi', CAST(N'2000-01-24' AS Date), NULL, N'rk', N'Left Handed Batter', N'Left-Arm Medium Bowler', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (4, 6008, 8, N'/images/7460e2b8-f066-40e6-8ff3-f3b686f75709.jpeg', N'Mohammad', N'Bangladeshi', CAST(N'2000-01-25' AS Date), NULL, N'md', N'Left Handed Batter', N'Right-Arm Medium Bowler', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (5, 6009, 7, N'/images/56208b52-f285-42f9-b398-c054ce46ddff.jpeg', N'Sahinur', N'Bangladeshi', CAST(N'2000-01-25' AS Date), NULL, N'Sahin', N'Left Handed Batter', N'Left-Arm Orthodox', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (6, 6009, 7, N'/images/15f66ebb-612b-4282-b23c-5a6271515da6.jpeg', N'Munna', N'Bangladeshi', CAST(N'2000-01-17' AS Date), NULL, N'Munna', N'Left Handed Batter', N'Right-Arm Medium Bowler', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (7, 6009, 9, N'/images/40fd3af5-477e-46da-80fe-4e9cbe1439bc.jpeg', N'Tuhin', N'Bangladeshi', CAST(N'2000-01-25' AS Date), NULL, N'Tuhin', N'Left Handed Batter', N'Left-Arm Fast Bowler', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (8, 6009, 8, N'/images/33c9bab7-5a46-4bc0-b238-4d9c41dc8513.jpeg', N'Tawhid Islam', N'Bangladeshi', CAST(N'2000-01-25' AS Date), NULL, N'tawshak', N'Left Handed Batter', N'Right-Arm Medium Bowler', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (9, 6009, 8, N'/images/893874cf-09fb-468e-808b-daaf8d060443.jpeg', N'Jittu', N'Bangladeshi', CAST(N'2000-01-19' AS Date), NULL, N'ji', N'Left Handed Batter', N'Right-Arm Medium Bowler', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (10, 6009, 7, N'/images/9e2b7dc4-1a40-43f6-b4b9-f783fd3d184f.jpeg', N'Nabin', N'Bangladeshi', CAST(N'2000-01-25' AS Date), NULL, N'nab', N'Left Handed Batter', N'Right-Arm FastBowler', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (11, 6009, 8, N'/images/12cf1369-0662-4721-a9b3-7dc250b90cc3.jpeg', N'Sakib', N'Bangladeshi', CAST(N'2001-01-20' AS Date), NULL, N'sak', N'Left Handed Batter', N'Leg Spinner', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (12, 6009, 7, N'/images/242c366a-033b-46fb-9ee9-9a3bcecc5789.jpeg', N'Rakibul', N'Bangladeshi', CAST(N'2000-01-12' AS Date), NULL, N'rak', N'Left Handed Batter', N'Right-Arm Medium Bowler', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (13, 6008, 7, N'/images/df054543-5970-4e25-8a88-b5918ee83606.jpeg', N'Rakibul', N'Bangladeshi', CAST(N'2026-01-18' AS Date), NULL, N'rak', N'Left Handed Batter', N'Right-Arm Medium Bowler', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (14, 6008, 8, N'/images/16bc396f-56a3-4b94-b504-2ad2076b0ebc.jpeg', N'Rabiul', N'Bangladeshi', CAST(N'2000-01-12' AS Date), NULL, N'rabi', N'Left Handed Batter', N'Off Spinner', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (15, 6008, 9, N'/images/23ea8f2d-1ddd-41f7-933f-c6a0d968b416.jpeg', N'Sohanur', N'Bangladeshi', CAST(N'2026-01-19' AS Date), NULL, N'sohan', N'Left Handed Batter', N'Right-Arm Medium Bowler', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (16, 6008, 7, N'/images/b5f6cae1-31a3-4651-b098-9a455f73d22c.jpeg', N'Taimur', N'Bangladeshi', CAST(N'2001-01-26' AS Date), NULL, N'taim', N'Left Handed Batter', N'Off Spinner', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (17, 6008, 8, N'/images/7249cc71-1375-4283-a8bc-42e6aa28229b.jpeg', N'sabbirul', N'Bangladeshi', CAST(N'2000-01-25' AS Date), NULL, N'sabbir', N'Left Handed Batter', N'Right-Arm Medium Bowler', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (18, 6008, 9, N'/images/f567b1de-78b3-4a1a-aa64-9f3b57597dd6.jpeg', N'Ratul', N'Bangladeshi', CAST(N'2000-01-25' AS Date), NULL, N'rat', N'Right Handed Batter', N'Leg Spinner', NULL)
GO
INSERT [dbo].[Players] ([PlayerID], [TeamsID], [PlayerRoleID], [PlayerImage], [FullName], [Nationality], [DateOfBirth], [BirthPlace], [NickName], [BattingStyle], [BowlingStyle], [IsActive]) VALUES (19, 6008, 9, N'/images/37a6f1c7-1cf9-4598-a876-f1f41a46c936.jpeg', N'Ikramul', N'Bangladeshi', CAST(N'2000-01-25' AS Date), NULL, N'ikram', N'Left Handed Batter', N'Right-Arm FastBowler', NULL)
GO
SET IDENTITY_INSERT [dbo].[Players] OFF
GO
SET IDENTITY_INSERT [dbo].[Teams] ON 
GO
INSERT [dbo].[Teams] ([TeamsID], [TeamName], [UserId], [ShortName], [TeamLogo], [TeamOwnerName], [TeamOwnerEmail], [TeamOwnerPhoneNumber], [CoachName], [FoundedYear], [TotalPlayers], [IsActive]) VALUES (6006, N'Dhaka Dynamytes', 7009, N'DD', N'/images/b9d69b0b-f8cd-48cb-91a6-8d3d89ca333b.jpeg', N'Tamzid', N'tam@gmail.com', N'01676786442', N'Ridoy', 1998, 11, 1)
GO
INSERT [dbo].[Teams] ([TeamsID], [TeamName], [UserId], [ShortName], [TeamLogo], [TeamOwnerName], [TeamOwnerEmail], [TeamOwnerPhoneNumber], [CoachName], [FoundedYear], [TotalPlayers], [IsActive]) VALUES (6007, N'Rajshahi', 7010, N'RR', N'/images/f148484e-6f5b-46ab-9cee-90be14181381.jpeg', N'Sakil', N'sakil@gmail.com', N'01676767662', N'Tamim', 2020, 11, 1)
GO
INSERT [dbo].[Teams] ([TeamsID], [TeamName], [UserId], [ShortName], [TeamLogo], [TeamOwnerName], [TeamOwnerEmail], [TeamOwnerPhoneNumber], [CoachName], [FoundedYear], [TotalPlayers], [IsActive]) VALUES (6008, N'Team Alumni', 7011, N'TA', N'/images/46f40fc9-4e0a-4fed-a688-78def4b443af.jpeg', N'Pranto', N'pranto@gmail.com', N'01787878787', N'Zunaid', 2010, 11, 1)
GO
INSERT [dbo].[Teams] ([TeamsID], [TeamName], [UserId], [ShortName], [TeamLogo], [TeamOwnerName], [TeamOwnerEmail], [TeamOwnerPhoneNumber], [CoachName], [FoundedYear], [TotalPlayers], [IsActive]) VALUES (6009, N'Path Finders', 7012, N'PF', N'/images/5b291b00-a65c-4f55-8e96-6e7ba1b9e6f8.jpeg', N'Dr. Abijit Saha', N'abijit@gmail.com', N'01767858576', N'Nich Pothas', 2006, 11, 1)
GO
INSERT [dbo].[Teams] ([TeamsID], [TeamName], [UserId], [ShortName], [TeamLogo], [TeamOwnerName], [TeamOwnerEmail], [TeamOwnerPhoneNumber], [CoachName], [FoundedYear], [TotalPlayers], [IsActive]) VALUES (7006, N'Team-X', 8008, N'X', N'/images/0484bbff-b104-4778-9e94-49ddc094b565.jpeg', N'Tawhid', N'tauhid@gmail.com', N'01676749110', N'Phil', 2012, 13, 1)
GO
SET IDENTITY_INSERT [dbo].[Teams] OFF
GO
SET IDENTITY_INSERT [dbo].[TeamSchedule] ON 
GO
INSERT [dbo].[TeamSchedule] ([TeamScheduleID], [TeamAID], [TeamBID], [MatchDate], [TournamentID], [Phase]) VALUES (2002, 6006, 6007, CAST(N'2026-01-23T23:46:34.1200000' AS DateTime2), 3004, N'Semi-Final')
GO
INSERT [dbo].[TeamSchedule] ([TeamScheduleID], [TeamAID], [TeamBID], [MatchDate], [TournamentID], [Phase]) VALUES (2003, 6008, 6009, CAST(N'2026-01-24T13:25:18.7533333' AS DateTime2), 3007, N'Semi-Final')
GO
SET IDENTITY_INSERT [dbo].[TeamSchedule] OFF
GO
SET IDENTITY_INSERT [dbo].[TournamentPointTable] ON 
GO
INSERT [dbo].[TournamentPointTable] ([PointTableID], [TournamentID], [TeamsID], [Played], [Won], [Lost], [Draw], [NR], [Points], [NRR], [TotalRunsScored], [TotalBallsFaced], [TotalRunsConceded], [TotalBallsBowled]) VALUES (3004, 3007, 6008, 1, 0, 1, 0, 0, 0, -15.25, 33, 18, 35, 8)
GO
INSERT [dbo].[TournamentPointTable] ([PointTableID], [TournamentID], [TeamsID], [Played], [Won], [Lost], [Draw], [NR], [Points], [NRR], [TotalRunsScored], [TotalBallsFaced], [TotalRunsConceded], [TotalBallsBowled]) VALUES (3005, 3007, 6009, 1, 1, 0, 0, 0, 2, 15.25, 35, 8, 33, 18)
GO
SET IDENTITY_INSERT [dbo].[TournamentPointTable] OFF
GO
SET IDENTITY_INSERT [dbo].[Tournaments] ON 
GO
INSERT [dbo].[Tournaments] ([TournamentID], [TournamentName], [Prize], [StartDate], [EndDate], [Location], [TournamentTypeID], [RegistrationDeadline], [TotalPlayer], [MatchPlayer], [ExtraPlayer], [Status], [RegistrationFee], [FieldFee], [MaxTeams], [ContactNumber], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive], [CurrentPhase]) VALUES (3004, N'Hight Voltage tournament', N'100Taka', CAST(N'2026-01-23T00:00:00.0000000' AS DateTime2), CAST(N'2026-01-23T00:00:00.0000000' AS DateTime2), N'Konda bazar Para', 1013, CAST(N'2026-01-23T00:00:00.0000000' AS DateTime2), 13, 11, 2, N'Active', 1, 1, 2, N'1610595016', NULL, NULL, NULL, NULL, NULL, N'Semi-Final')
GO
INSERT [dbo].[Tournaments] ([TournamentID], [TournamentName], [Prize], [StartDate], [EndDate], [Location], [TournamentTypeID], [RegistrationDeadline], [TotalPlayer], [MatchPlayer], [ExtraPlayer], [Status], [RegistrationFee], [FieldFee], [MaxTeams], [ContactNumber], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive], [CurrentPhase]) VALUES (3007, N'Iubat CSE Cricket Tournament', N'Golden Trophy', CAST(N'2026-01-24T00:00:00.0000000' AS DateTime2), CAST(N'2026-01-25T00:00:00.0000000' AS DateTime2), N'IUBAT Field', 1013, CAST(N'2026-01-24T00:00:00.0000000' AS DateTime2), 11, 11, 0, N'Active', 1, 1, 2, N'178877327', NULL, NULL, NULL, NULL, NULL, N'Final')
GO
INSERT [dbo].[Tournaments] ([TournamentID], [TournamentName], [Prize], [StartDate], [EndDate], [Location], [TournamentTypeID], [RegistrationDeadline], [TotalPlayer], [MatchPlayer], [ExtraPlayer], [Status], [RegistrationFee], [FieldFee], [MaxTeams], [ContactNumber], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive], [CurrentPhase]) VALUES (3008, N'Day night Cricket tournament', N'1 Lakh Taka', CAST(N'2026-01-26T00:00:00.0000000' AS DateTime2), CAST(N'2026-01-30T00:00:00.0000000' AS DateTime2), N'Savar', 1013, CAST(N'2026-01-25T00:00:00.0000000' AS DateTime2), 12, 11, 1, N'Finished', 1, 1, 4, N'1610595016', NULL, NULL, NULL, NULL, NULL, N'Final')
GO
INSERT [dbo].[Tournaments] ([TournamentID], [TournamentName], [Prize], [StartDate], [EndDate], [Location], [TournamentTypeID], [RegistrationDeadline], [TotalPlayer], [MatchPlayer], [ExtraPlayer], [Status], [RegistrationFee], [FieldFee], [MaxTeams], [ContactNumber], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive], [CurrentPhase]) VALUES (4004, N'Cse Cricket Tournament', N'Trophy', CAST(N'2026-01-25T00:00:00.0000000' AS DateTime2), CAST(N'2026-01-31T00:00:00.0000000' AS DateTime2), N'Iubat Field', 1013, CAST(N'2026-01-25T00:00:00.0000000' AS DateTime2), 12, 11, 1, N'Active', 1, 1, 2, N'1678485851', NULL, NULL, NULL, NULL, NULL, N'Semi-Final')
GO
SET IDENTITY_INSERT [dbo].[Tournaments] OFF
GO
SET IDENTITY_INSERT [dbo].[TournamentTeamMapping] ON 
GO
INSERT [dbo].[TournamentTeamMapping] ([Id], [TournamentId], [TeamId], [PaymentStatus], [PaymentDate], [bkashPaymentId], [bkashTransactionId], [CreatedDate], [GroupId]) VALUES (4009, 3004, 6006, N'Pending', NULL, N'TR0011t0KErYJ1769189713588', NULL, CAST(N'2026-01-23T23:35:14.030' AS DateTime), NULL)
GO
INSERT [dbo].[TournamentTeamMapping] ([Id], [TournamentId], [TeamId], [PaymentStatus], [PaymentDate], [bkashPaymentId], [bkashTransactionId], [CreatedDate], [GroupId]) VALUES (4010, 3004, 6007, N'Paid', NULL, N'TR00112LP4sON1769190199122', NULL, CAST(N'2026-01-23T23:43:19.550' AS DateTime), NULL)
GO
INSERT [dbo].[TournamentTeamMapping] ([Id], [TournamentId], [TeamId], [PaymentStatus], [PaymentDate], [bkashPaymentId], [bkashTransactionId], [CreatedDate], [GroupId]) VALUES (4011, 3007, 6008, N'Paid', NULL, N'TR00119jK27VS1769238542239', NULL, CAST(N'2026-01-24T13:09:02.430' AS DateTime), NULL)
GO
INSERT [dbo].[TournamentTeamMapping] ([Id], [TournamentId], [TeamId], [PaymentStatus], [PaymentDate], [bkashPaymentId], [bkashTransactionId], [CreatedDate], [GroupId]) VALUES (4012, 3007, 6009, N'Paid', NULL, N'TR0011nFNVj6D1769238760437', NULL, CAST(N'2026-01-24T13:12:40.637' AS DateTime), NULL)
GO
INSERT [dbo].[TournamentTeamMapping] ([Id], [TournamentId], [TeamId], [PaymentStatus], [PaymentDate], [bkashPaymentId], [bkashTransactionId], [CreatedDate], [GroupId]) VALUES (5010, 4004, 7006, N'Pending', NULL, N'TR0011ibP5s901769243987260', NULL, CAST(N'2026-01-24T14:39:47.387' AS DateTime), NULL)
GO
SET IDENTITY_INSERT [dbo].[TournamentTeamMapping] OFF
GO
SET IDENTITY_INSERT [dbo].[TournamentType] ON 
GO
INSERT [dbo].[TournamentType] ([Id], [Name]) VALUES (1013, N'KnockOut')
GO
SET IDENTITY_INSERT [dbo].[TournamentType] OFF
GO
SET IDENTITY_INSERT [dbo].[UserInfo] ON 
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (1, N'Tawhid', N'tawhid@gmail.com', N'01788773237', N'admin', N'1234')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7009, N'Tamzid', N'tam@gmail.com', N'01676786442', N'TeamOwner', N'tam@gmail.com')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7010, N'Sakil', N'sakil@gmail.com', N'01676767662', N'TeamOwner', N'sakil@gmail.com')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7011, N'Pranto', N'pranto@gmail.com', N'01787878787', N'TeamOwner', N'pranto@gmail.com')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7012, N'Dr. Abijit Saha', N'abijit@gmail.com', N'01767858576', N'TeamOwner', N'abijit@gmail.com')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7013, N'Abir', N'0178438637@player.sportshub.local', N'0178438637', N'Player', N'0178438637')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7014, N'Hridoy', N'01786767666@player.sportshub.local', N'01786767666', N'Player', N'01786767666')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7015, N'Rakin', N'01786875666@player.sportshub.local', N'01786875666', N'Player', N'01786875666')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7016, N'Mohammad', N'01776786864@player.sportshub.local', N'01776786864', N'Player', N'01776786864')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7017, N'Sahinur', N'01786857652@player.sportshub.local', N'01786857652', N'Player', N'01786857652')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7018, N'Munna', N'016767868776@player.sportshub.local', N'016767868776', N'Player', N'016767868776')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7019, N'Tuhin', N'017867867876@player.sportshub.local', N'017867867876', N'Player', N'017867867876')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7020, N'Tawhid Islam', N'01786876567@player.sportshub.local', N'01786876567', N'Player', N'01786876567')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7021, N'Jittu', N'01878976677@player.sportshub.local', N'01878976677', N'Player', N'01878976677')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7022, N'Nabin', N'01687867867@player.sportshub.local', N'01687867867', N'Player', N'01687867867')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7023, N'Sakib', N'015785875865@player.sportshub.local', N'015785875865', N'Player', N'015785875865')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7024, N'Rakibul', N'01687686886@player.sportshub.local', N'01687686886', N'Player', N'01687686886')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7025, N'Rakibul', N'01787887878@player.sportshub.local', N'01787887878', N'Player', N'01787887878')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7026, N'Rabiul', N'01686686878@player.sportshub.local', N'01686686878', N'Player', N'01686686878')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7027, N'Sohanur', N'01787888678@player.sportshub.local', N'01787888678', N'Player', N'01787888678')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7028, N'Taimur', N'01767678676@player.sportshub.local', N'01767678676', N'Player', N'01767678676')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7029, N'sabbirul', N'017878979878@player.sportshub.local', N'017878979878', N'Player', N'017878979878')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7030, N'Ratul', N'01767678666@player.sportshub.local', N'01767678666', N'Player', N'01767678666')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7031, N'Ikramul', N'01787897977@player.sportshub.local', N'01787897977', N'Player', N'01787897977')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (7032, N'Arif Afsar', N'01787877766@player.sportshub.local', N'01787877766', N'Player', N'01787877766')
GO
INSERT [dbo].[UserInfo] ([ID], [Name], [Email], [Phone], [UserType], [Password]) VALUES (8008, N'Tawhid', N'tauhid@gmail.com', N'01676749110', N'TeamOwner', N'tauhid@gmail.com')
GO
SET IDENTITY_INSERT [dbo].[UserInfo] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_PaymentHistory_bkashPaymentId]    Script Date: 02/21/2026 02:30:50 PM ******/
CREATE NONCLUSTERED INDEX [IX_PaymentHistory_bkashPaymentId] ON [dbo].[PaymentHistory]
(
	[bkashPaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_PaymentHistory_PaymentStatus]    Script Date: 02/21/2026 02:30:50 PM ******/
CREATE NONCLUSTERED INDEX [IX_PaymentHistory_PaymentStatus] ON [dbo].[PaymentHistory]
(
	[PaymentStatus] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CricketMatch] ADD  DEFAULT ((1)) FOR [CurrentInnings]
GO
ALTER TABLE [dbo].[CricketMatch] ADD  DEFAULT ('Started') FOR [MatchStatus]
GO
ALTER TABLE [dbo].[MatchBallByBall] ADD  DEFAULT ((0)) FOR [IsBye]
GO
ALTER TABLE [dbo].[PaymentHistory] ADD  DEFAULT ('Demo') FOR [PaymentMethod]
GO
ALTER TABLE [dbo].[PaymentHistory] ADD  DEFAULT ('Completed') FOR [PaymentStatus]
GO
ALTER TABLE [dbo].[PaymentHistory] ADD  DEFAULT (getdate()) FOR [PaymentDate]
GO
ALTER TABLE [dbo].[PaymentHistory] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[TournamentPointTable] ADD  DEFAULT ((0)) FOR [TotalRunsScored]
GO
ALTER TABLE [dbo].[TournamentPointTable] ADD  DEFAULT ((0)) FOR [TotalBallsFaced]
GO
ALTER TABLE [dbo].[TournamentPointTable] ADD  DEFAULT ((0)) FOR [TotalRunsConceded]
GO
ALTER TABLE [dbo].[TournamentPointTable] ADD  DEFAULT ((0)) FOR [TotalBallsBowled]
GO
ALTER TABLE [dbo].[TournamentTeamMapping] ADD  CONSTRAINT [DF__Tournamen__Creat__2739D489]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[PaymentHistory]  WITH CHECK ADD  CONSTRAINT [FK_PaymentHistory_Teams] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Teams] ([TeamsID])
GO
ALTER TABLE [dbo].[PaymentHistory] CHECK CONSTRAINT [FK_PaymentHistory_Teams]
GO
ALTER TABLE [dbo].[PaymentHistory]  WITH CHECK ADD  CONSTRAINT [FK_PaymentHistory_Tournaments] FOREIGN KEY([TournamentId])
REFERENCES [dbo].[Tournaments] ([TournamentID])
GO
ALTER TABLE [dbo].[PaymentHistory] CHECK CONSTRAINT [FK_PaymentHistory_Tournaments]
GO
ALTER TABLE [dbo].[PaymentHistory]  WITH CHECK ADD  CONSTRAINT [FK_PaymentHistory_TournamentTeamMapping] FOREIGN KEY([TournamentTeamMappingID])
REFERENCES [dbo].[TournamentTeamMapping] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PaymentHistory] CHECK CONSTRAINT [FK_PaymentHistory_TournamentTeamMapping]
GO
/****** Object:  StoredProcedure [dbo].[SP_CheckTournamentReadyForSchedule]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- Stored Procedure to check which tournaments are ready for scheduling/next phase
CREATE   PROCEDURE [dbo].[SP_CheckTournamentReadyForSchedule]
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Tournaments with Status 'Ready' and no phase yet (Initial scheduling)
    -- 2. Tournaments with 'Active' status where all matches in current phase are 'Finished'
    -- Note: A match is considered 'Finished' only if a record exists in CricketMatch with MatchStatus = 'Finished'
    SELECT t.TournamentID
    FROM Tournaments t
    WHERE 
        (t.Status = 'Upcoming' AND t.CurrentPhase IS NULL)
        OR
        (t.Status = 'Active' AND t.CurrentPhase IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM TeamSchedule ts 
            LEFT JOIN CricketMatch cm ON ts.TeamScheduleID = cm.TeamScheduleID
            WHERE ts.TournamentID = t.TournamentID 
            AND ts.Phase = t.CurrentPhase 
            AND (cm.MatchStatus IS NULL OR cm.MatchStatus != 'Finished')
        ))
END


GO
/****** Object:  StoredProcedure [dbo].[SP_CricketMatch]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SP_CricketMatch]
    @Flag INT,                       -- 1=Insert, 2=Update, 3=Delete, 4=Select, 5=Select By ID
    @CricketMatchID INT = NULL,      -- For update/delete/select by id
    @TeamScheduleID INT = NULL,
    @TossWinnerTeamID INT = NULL,
    @TossChoice NVARCHAR(10) = NULL, -- 'Bat' or 'Ball'
    @Overs INT = NULL,
    @Umpire NVARCHAR(100) = NULL,
    @Venue NVARCHAR(100) = NULL,
    @StrikerPlayerID INT=NULL,
    @NonStrikerPlayerID INT=NULL,
    @BowlerPlayerID INT=NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Insert
    IF @Flag = 1
    BEGIN
        INSERT INTO CricketMatch (TeamScheduleID, TossWinnerTeamID, TossChoice, Overs, Umpire, Venue)
        VALUES (@TeamScheduleID, @TossWinnerTeamID, @TossChoice, @Overs, @Umpire, @Venue);

        SELECT SCOPE_IDENTITY() AS NewCricketMatchID;
    END

    -- Update
    ELSE IF @Flag = 2
    BEGIN
        UPDATE CricketMatch
        SET 
            TeamScheduleID = ISNULL(@TeamScheduleID, TeamScheduleID),
            TossWinnerTeamID = ISNULL(@TossWinnerTeamID, TossWinnerTeamID),
            TossChoice = ISNULL(@TossChoice, TossChoice),
            Overs = ISNULL(@Overs, Overs),
            Umpire = ISNULL(@Umpire, Umpire),
            Venue = ISNULL(@Venue, Venue)
        WHERE CricketMatchID = @CricketMatchID;

        SELECT 'Updated Successfully' AS Message;
    END

    -- Delete
    ELSE IF @Flag = 3
    BEGIN
        DELETE FROM CricketMatch
        WHERE CricketMatchID = @CricketMatchID;

        SELECT 'Deleted Successfully' AS Message;
    END

    -- Select All
    ELSE IF @Flag = 4
    BEGIN
        SELECT 
            CricketMatchID,
            TeamScheduleID,
            TossWinnerTeamID,
            TossChoice,
            Overs,
            Umpire,
            Venue
        FROM CricketMatch
        ORDER BY CricketMatchID DESC;
    END

    -- Select By ID
    ELSE IF @Flag = 5
    BEGIN
        SELECT 
            CricketMatchID,
            TeamScheduleID,
            TossWinnerTeamID,
            TossChoice,
            Overs,
            Umpire,
            Venue
        FROM CricketMatch
        WHERE CricketMatchID = @CricketMatchID;
    END
    ELSE IF @Flag = 6
    BEGIN
     SELECT c.* , 
     ps.FullName as StrikerName,
     ps.PlayerImage as StrikerImage,
     pno.FullName as NonStrikerName,
     pno.PlayerImage as NonStrikerImage,
     pbl.FullName as BowlerName,
     pno.PlayerImage as BowlerImage
     
     FROM CricketMatch c
     LEFT JOIN Players ps on ps.PlayerID = c.StrikerPlayerID
     left join Players pno on pno.PlayerID = c.NonStrikerPlayerID
     left join Players pbl on pbl.PlayerID = c.BowlerPlayerID
     WHERE TeamScheduleID = @TeamScheduleID
    END
    --SelectBy PlayerID
    If @Flag=6
    Begin
    Update CricketMatch SET
            StrikerPlayerID =ISNULL( @StrikerPlayerID,StrikerPlayerID),
            NonStrikerPlayerID=ISNULL(@NonStrikerPlayerID, NonStrikerPlayerID),
            BowlerPlayerID = ISNULL(@BowlerPlayerID, BowlerPlayerID)
                 
        WHERE CricketMatchID = @CricketMatchID;

        SELECT 'Updated Successfully' AS Message;
END

ELSE IF(@Flag =7)
BEGIN
    SELECT 
    tmA.TeamName  AS TeamAName,
    tmA.TeamLogo AS TeamALogo,
    tmB.TeamName AS TeamBName,
    tmB.TeamLogo AS TeamBLogo,

    CAST(COUNT(bl.BallID) / 6 AS VARCHAR(10)) 
        + '.' + 
    CAST(COUNT(bl.BallID) % 6 AS VARCHAR(10)) AS Overs,

    SUM(ISNULL(bl.Run, 0)) AS TotalRun,
    CASE 
        WHEN cm.TossWinnerTeamID = tmA.TeamsID THEN tmA.TeamName
        ELSE tmB.TeamName
    END AS TossWinnerTeam,
        cm.CricketMatchID

FROM CricketMatch cm
LEFT JOIN TeamSchedule ts ON cm.TeamScheduleID = ts.TeamScheduleID
LEFT JOIN Teams tmA ON tmA.TeamsID = ts.TeamAID 
LEFT JOIN Teams tmB ON tmB.TeamsID = ts.TeamBID
LEFT JOIN Overs ov ON ov.CricketMatchID = cm.CricketMatchID
LEFT JOIN MatchBallByBall bl ON bl.OverId = ov.Id
WHERE cm.CricketMatchID =@CricketMatchID
GROUP BY 
    tmA.TeamName,
    tmA.TeamLogo,
    tmB.TeamName,
    tmB.TeamLogo,
    cm.TossWinnerTeamID,
    tmA.TeamsID,
    tmB.TeamsID,
      cm.CricketMatchID

END


ELSE IF(@Flag =8)
BEGIN
    SELECT 
    tmA.TeamName  AS TeamAName,
    tmA.TeamLogo AS TeamALogo,
    tmB.TeamName AS TeamBName,
    tmB.TeamLogo AS TeamBLogo,

    CAST(COUNT(bl.BallID) / 6 AS VARCHAR(10)) 
        + '.' + 
    CAST(COUNT(bl.BallID) % 6 AS VARCHAR(10)) AS Overs,

    SUM(ISNULL(bl.Run, 0)) AS TotalRun,
    CASE 
        WHEN cm.TossWinnerTeamID = tmA.TeamsID THEN tmA.TeamName
        ELSE tmB.TeamName
    END AS TossWinnerTeam,
    cm.CricketMatchID

FROM CricketMatch cm
LEFT JOIN TeamSchedule ts ON cm.TeamScheduleID = ts.TeamScheduleID
LEFT JOIN Teams tmA ON tmA.TeamsID = ts.TeamAID 
LEFT JOIN Teams tmB ON tmB.TeamsID = ts.TeamBID
LEFT JOIN Overs ov ON ov.CricketMatchID = cm.CricketMatchID
LEFT JOIN MatchBallByBall bl ON bl.OverId = ov.Id
GROUP BY 
    tmA.TeamName,
    tmA.TeamLogo,
    tmB.TeamName,
    tmB.TeamLogo,
    cm.TossWinnerTeamID,
    tmA.TeamsID,
    tmB.TeamsID,
      cm.CricketMatchID

END
END

GO
/****** Object:  StoredProcedure [dbo].[SP_GenerateTeamSchedule]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- Main Procedure to Generate Schedule
CREATE   PROCEDURE [dbo].[SP_GenerateTeamSchedule]
    @TournamentID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TypeID INT, @TypeName NVARCHAR(100), @MaxTeams INT, @CurrentPhase NVARCHAR(50);
    
    SELECT 
        @TypeID = t.TournamentTypeID, 
        @TypeName = tt.Name, 
        @MaxTeams = t.MaxTeams,
        @CurrentPhase = t.CurrentPhase
    FROM Tournaments t
    JOIN TournamentType tt ON t.TournamentTypeID = tt.Id
    WHERE t.TournamentID = @TournamentID;

    -- Handle different types
    
    -- 1. Round Robin
    IF @TypeName LIKE '%Round Robin%'
    BEGIN
        IF @CurrentPhase IS NULL
        BEGIN
            -- Generate all pairs (Simple version: everyone plays everyone once)
            INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
            SELECT @TournamentID, t1.TeamId, t2.TeamId, GETDATE(), 'Round Robin'
            FROM TournamentTeamMapping t1
            JOIN TournamentTeamMapping t2 ON t1.TournamentId = t2.TournamentId AND t1.TeamId < t2.TeamId
            WHERE t1.TournamentId = @TournamentID;

            UPDATE Tournaments SET CurrentPhase = 'Round Robin', Status = 'Active' WHERE TournamentID = @TournamentID;
        END
    END
    
    -- 2. Knockout
    ELSE IF @TypeName LIKE '%Knockout%'
    BEGIN
        IF @CurrentPhase IS NULL
        BEGIN
            -- Initial pairings (Quarter-Finals for 8, Semi-Finals for 4)
            DECLARE @PhaseName NVARCHAR(50) = 'Semi-Final';
            IF @MaxTeams > 4 SET @PhaseName = 'Quarter-Final';
            
            -- Simple logic: pair team 1 with 2, 3 with 4, etc.
            -- Using common table expression to get ranked list of teams
            ;WITH TeamsRanked AS (
                SELECT TeamId, ROW_NUMBER() OVER (ORDER BY TeamId) as r
                FROM TournamentTeamMapping WHERE TournamentId = @TournamentID
            )
            INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
            SELECT @TournamentID, t1.TeamId, t2.TeamId, GETDATE(), @PhaseName
            FROM TeamsRanked t1
            JOIN TeamsRanked t2 ON t1.r + 1 = t2.r
            WHERE t1.r % 2 = 1;

            UPDATE Tournaments SET CurrentPhase = @PhaseName, Status = 'Active' WHERE TournamentID = @TournamentID;
        END
        ELSE IF @CurrentPhase = 'Quarter-Final'
        BEGIN
            -- Generate Semi-Final from Quarter-Final winners
            -- (Implementation of winner tracking needed in TeamSchedule)
            -- For now, placeholder to move to next phase
             UPDATE Tournaments SET CurrentPhase = 'Semi-Final' WHERE TournamentID = @TournamentID;
        END
        ELSE IF @CurrentPhase = 'Semi-Final'
        BEGIN
             UPDATE Tournaments SET CurrentPhase = 'Final' WHERE TournamentID = @TournamentID;
        END
        ELSE IF @CurrentPhase = 'Final'
        BEGIN
             UPDATE Tournaments SET Status = 'Finished' WHERE TournamentID = @TournamentID;
        END
    END
    
    -- 3. Group Stage + Knockout
    ELSE IF @TypeName LIKE '%Group%'
    BEGIN
        IF @CurrentPhase IS NULL
        BEGIN
            -- 1. Assign Teams to Groups (Randomly or balanced)
            UPDATE TournamentTeamMapping
            SET GroupId = (r % 2) + 1 -- Assign to Group 1 or 2 for simple 2-group setup
            FROM (SELECT ID, ROW_NUMBER() OVER (ORDER BY ID) as r FROM TournamentTeamMapping WHERE TournamentId = @TournamentID) t
            WHERE TournamentTeamMapping.ID = t.ID;

            -- 2. Generate Group Stage Matches
            INSERT INTO TeamSchedule (TournamentID, TeamAID, TeamBID, MatchDate, Phase)
            SELECT @TournamentID, t1.TeamId, t2.TeamId, GETDATE(), 'Group Stage'
            FROM TournamentTeamMapping t1
            JOIN TournamentTeamMapping t2 ON t1.TournamentId = t2.TournamentId AND t1.TeamId < t2.TeamId
            WHERE t1.TournamentId = @TournamentID AND t1.GroupId = t2.GroupId;

            UPDATE Tournaments SET CurrentPhase = 'Group Stage', Status = 'Active' WHERE TournamentID = @TournamentID;
        END
        ELSE IF @CurrentPhase = 'Group Stage'
        BEGIN
            -- After group stage, move to Knockout (Semi-Final)
             UPDATE Tournaments SET CurrentPhase = 'Semi-Final' WHERE TournamentID = @TournamentID;
             -- Logic to pick top 2 from Group 1 and Top 2 from Group 2 would go here
        END
    END
END
GO
/****** Object:  StoredProcedure [dbo].[SP_Over]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_Over]
    @Flag INT,                        -- 1=Insert, 2=Update, 3=Delete, 4=Select, 5=Select By ID
    @Id int=0,
    @CricketMatchID	int =0,
    @BowlerId INT =0,
    @Innings INT=0
AS
BEGIN
    SET NOCOUNT ON;
   

    -- Insert
    IF @Flag = 1
    BEGIN
        INSERT INTO OVERS (CricketMatchID,BowlerId,Innings)
        VALUES (@CricketMatchID,@BowlerId,@Innings);

        SELECT SCOPE_IDENTITY() AS NewOverId;
    END

  


END

GO
/****** Object:  StoredProcedure [dbo].[SP_PerBall]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_PerBall]
    @Flag INT,                        -- 1=Insert, 2=Update, 3=Delete, 4=Select, 5=Select By ID
    @BallID INT = 0,
    @OverId INT = 0,
    @StrikerPlayerID INT = 0,
    @Run INT = 0,
    @IsWicket BIT = 0,
    @BallType NVARCHAR(50) = NULL,
    @Boundary INT = 0,
    @CricketMatchID INT = 0,
    @BowlerId INT = 0,
    @Innings INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================
    -- INSERT NEW BALL
    -- =========================
    IF @Flag = 1
    BEGIN
        INSERT INTO MatchBallByBall
            (OverId, StrikerPlayerID, Run, IsWicket, BallType, Boundry, CreatedAt)
        VALUES
            (@OverId, @StrikerPlayerID, @Run, @IsWicket, @BallType, @Boundary, GETDATE());

        -- Return inserted BallID
        SELECT SCOPE_IDENTITY() AS NewBallID;
    END

    -- =========================
    -- UPDATE BALL
    -- =========================
    IF @Flag = 2 AND @BallID > 0
    BEGIN
        UPDATE MatchBallByBall
        SET
            OverId = ISNULL(@OverId, OverId),
            StrikerPlayerID = ISNULL(@StrikerPlayerID, StrikerPlayerID),
            Run = ISNULL(@Run, Run),
            IsWicket = ISNULL(@IsWicket, IsWicket),
            BallType = ISNULL(@BallType, BallType),
            Boundry = ISNULL(@Boundary, Boundry)
        WHERE BallID = @BallID;

        SELECT @BallID AS UpdatedBallID;
    END

    -- =========================
    -- DELETE BALL
    -- =========================
    IF @Flag = 3 AND @BallID > 0
    BEGIN
        DELETE FROM MatchBallByBall WHERE BallID = @BallID;
        SELECT @BallID AS DeletedBallID;
    END

    -- =========================
    -- SELECT ALL BALLS FOR AN OVER
    -- =========================
    IF @Flag = 4
    BEGIN
        SELECT *
        FROM MatchBallByBall
        WHERE OverId = @OverId;
    END

    -- =========================
    -- SELECT BALL BY ID
    -- =========================
    IF @Flag = 5 AND @BallID > 0
    BEGIN
        SELECT *
        FROM MatchBallByBall
        WHERE BallID = @BallID;
    END
END
GO
/****** Object:  StoredProcedure [dbo].[SP_PlayerRole]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_PlayerRole]
(
    @Flag INT,
    @PlayerRoleID INT = NULL,
    @RoleName NVARCHAR(200) = NULL,
    @Description NVARCHAR(500) = NULL,
    @IsActive BIT = NULL,
    @CreatedAt DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Flag = 1
    BEGIN
        SELECT * FROM PlayerRole;
    END
    ELSE IF @Flag = 2 -- Insert
    BEGIN
        INSERT INTO PlayerRole(RoleName, Description, IsActive, CreatedAt)
        VALUES(@RoleName, @Description, @IsActive, @CreatedAt);

        SELECT SCOPE_IDENTITY() AS PlayerRoleID;
    END
    ELSE IF @Flag = 3 -- Update
    BEGIN
        UPDATE PlayerRole
        SET RoleName=@RoleName, Description=@Description, IsActive=@IsActive, CreatedAt=@CreatedAt
        WHERE PlayerRoleID=@PlayerRoleID;

        SELECT @PlayerRoleID AS PlayerRoleID;
    END
    ELSE IF @Flag = 4 -- Delete
    BEGIN
        DELETE FROM PlayerRole WHERE PlayerRoleID=@PlayerRoleID;
        SELECT @PlayerRoleID AS PlayerRoleID;
    END
    ELSE IF @Flag = 5 -- Get by ID
    BEGIN
        SELECT * FROM PlayerRole WHERE PlayerRoleID=@PlayerRoleID;
    END
END
GO
/****** Object:  StoredProcedure [dbo].[SP_Players]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_Players]
(
    @Flag INT,
    @PlayerID INT = NULL,
    @TeamsID INT = NULL,
    @PlayerRoleID INT = NULL,
    @PlayerImage NVARCHAR(200) = NULL,
    @FullName NVARCHAR(200) = NULL,
    @Nationality NVARCHAR(200) = NULL,
    @DateOfBirth DATE = NULL,
    @NickName NVARCHAR(200) = NULL,
    @BattingStyle NVARCHAR(200) = NULL,
    @BowlingStyle NVARCHAR(200) = NULL,
    @IsActive BIT = NULL,
    @UserId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Flag = 1 -- Get all Players
    BEGIN
        SELECT * FROM Players;
    END
    ELSE IF @Flag = 2 -- Insert Player
    BEGIN
        INSERT INTO Players(TeamsID, PlayerRoleID, PlayerImage, FullName, Nationality, DateOfBirth, NickName,
                            BattingStyle, BowlingStyle, IsActive)
        VALUES(@TeamsID, @PlayerRoleID, @PlayerImage, @FullName, @Nationality, @DateOfBirth, @NickName,
               @BattingStyle, @BowlingStyle, @IsActive);

        SELECT SCOPE_IDENTITY() AS PlayerID;
    END
    ELSE IF @Flag = 3 -- Update Player
    BEGIN
        UPDATE Players
        SET TeamsID=@TeamsID, PlayerRoleID=@PlayerRoleID, PlayerImage=ISNULL(@PlayerImage,PlayerImage), FullName=@FullName,
            Nationality=@Nationality, DateOfBirth=@DateOfBirth, NickName=@NickName,
            BattingStyle=@BattingStyle, BowlingStyle=@BowlingStyle, IsActive=@IsActive
        WHERE PlayerID=@PlayerID;

        SELECT @PlayerID AS PlayerID;
    END
    ELSE IF @Flag = 5 -- Delete Player
    BEGIN
        DELETE FROM Players WHERE PlayerID=@PlayerID;
        SELECT @PlayerID AS PlayerID;
    END
    ELSE IF @Flag = 6 -- Get Player by ID
    BEGIN
        SELECT * FROM Players WHERE PlayerID=@PlayerID;
    END
    ELSE IF @Flag = 7 -- Get Player by Team Owner ID
    BEGIN
        SELECT p.*,t.TeamName, pr.RoleName, pr.Description
        FROM Players p
        Left JOIN Teams t ON t.TeamsID = p.TeamsID
         Left JOIN PlayerRole pr ON pr.PlayerRoleID = p.PlayerRoleID
        WHERE t.UserId=@UserId;
    END
END
GO
/****** Object:  StoredProcedure [dbo].[SP_TeamPayment]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

                        CREATE PROCEDURE [dbo].[SP_TeamPayment]
                            @Flag INT,
                            @ID INT = NULL,
                            @Phone NVARCHAR(50) = NULL,
                            @OTP NVARCHAR(10) = NULL,
                            @Amount DECIMAL(10,2) = NULL,
                            @userId INT = NULL
                        AS
                        BEGIN
                            SET NOCOUNT ON;

                            IF @Flag = 1
                            BEGIN
                                SELECT 
                                    ttm.ID,
                                    t.TeamName,
                                    t.TeamOwnerName,
                                    t.TeamOwnerPhoneNumber AS Phone,
                                    tour.TournamentName,
                                    tour.RegistrationFee AS Amount,
                                    ttm.bkashPaymentId,
                                    ttm.bkashTransactionId,
                                    ttm.PaymentStatus,
                                    ttm.PaymentDate,
                                    ttm.CreatedDate,
                                    u.Name AS UserName,
                                    u.Email AS UserEmail
                                FROM TournamentTeamMapping ttm
                                INNER JOIN Teams t ON ttm.TeamId = t.TeamsID
                                INNER JOIN Tournaments tour ON ttm.TournamentId = tour.TournamentID
                                LEFT JOIN Users u ON t.UserId = u.ID
                                WHERE ttm.bkashPaymentId IS NOT NULL
                                ORDER BY ttm.CreatedDate DESC;
                            END
                            ELSE IF @Flag = 3
                            BEGIN
                                UPDATE TournamentTeamMapping
                                SET PaymentStatus = 'Completed',
                                    PaymentDate = GETDATE()
                                WHERE bkashPaymentId = @Phone;
                            END
                        END
                    
GO
/****** Object:  StoredProcedure [dbo].[SP_Teams]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_Teams]
(
    @Flag INT,
    @TeamsID INT = NULL,
    @TeamName NVARCHAR(200) = NULL,
    @UserId INT = NULL,
    @ShortName NVARCHAR(200) = NULL,
    @TeamLogo NVARCHAR(200) = NULL,
    @TeamOwnerName NVARCHAR(200) = NULL,
    @TeamOwnerEmail NVARCHAR(200) = NULL,
    @TeamOwnerPhoneNumber NVARCHAR(200) = NULL,
    @CoachName NVARCHAR(200) = NULL,
    @FoundedYear INT = NULL,
    @TotalPlayers INT = NULL,
    @IsActive BIT = NULL,
    @TournamentId INT = NULL,
    @TeamId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Flag = 1 -- Get all Teams
    BEGIN
        SELECT * FROM Teams;
    END
    ELSE IF @Flag = 2 -- Insert Team
    BEGIN
        INSERT INTO Teams(TeamName, UserId, ShortName, TeamLogo, TeamOwnerName, TeamOwnerEmail, TeamOwnerPhoneNumber,
                          CoachName, FoundedYear, TotalPlayers, IsActive)
        VALUES(@TeamName, @UserId, @ShortName, @TeamLogo, @TeamOwnerName, @TeamOwnerEmail, @TeamOwnerPhoneNumber,
               @CoachName, @FoundedYear, @TotalPlayers, @IsActive);

        SELECT SCOPE_IDENTITY() AS TeamsID;
    END
    ELSE IF @Flag = 3 -- Update Team
    BEGIN
        UPDATE Teams
        SET TeamName=@TeamName, UserId=@UserId, ShortName=@ShortName, TeamLogo=@TeamLogo,
            TeamOwnerName=@TeamOwnerName, TeamOwnerEmail=@TeamOwnerEmail, TeamOwnerPhoneNumber=@TeamOwnerPhoneNumber,
            CoachName=@CoachName, FoundedYear=@FoundedYear, TotalPlayers=@TotalPlayers, IsActive=@IsActive
        WHERE TeamsID=@TeamsID;

        SELECT @TeamsID AS TeamsID;
    END
    ELSE IF @Flag = 4 -- Delete Team
    BEGIN
        DELETE FROM Teams WHERE TeamsID=@TeamsID;
        SELECT @TeamsID AS TeamsID;
    END
    ELSE IF @Flag = 6 -- Get Team by ID
    BEGIN
        SELECT * FROM Teams WHERE TeamsID=@TeamsID;
    END
    ELSE IF @Flag = 7 -- Get Team by UserID
    BEGIN
        SELECT * FROM Teams WHERE UserId=@UserId;
    END
    ELSE IF @Flag = 8 -- Get Teams by TournamentID
    BEGIN
        SELECT t.*
        FROM Teams t
        INNER JOIN TournamentTeamMapping m ON m.TeamId = t.TeamsID
        WHERE m.TournamentId = @TournamentId;
    END
    ELSE IF @Flag = 9 -- Get Player by TeamID
    BEGIN
        SELECT p.*, pr.RoleName
        FROM Players p
        left join PlayerRole pr ON pr.PlayerRoleID=p.PlayerRoleID
        WHERE p.TeamsID=@TeamsID;
        
    END
END
GO
/****** Object:  StoredProcedure [dbo].[SP_TeamSchedule]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_TeamSchedule]
(
    @Flag INT,

    @TeamScheduleID INT = NULL,
    @TeamAID INT = NULL,
    @TeamBID INT = NULL,
    @MatchDate DATETIME2(7) = NULL,
    @TournamentID INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    /* =========================
       @Flag = 1 → GET ALL
       ========================= */
   IF (@Flag = 1)
    BEGIN
        SELECT
            ts.TeamScheduleID,
            ts.TeamAID,
            ta.TeamName AS TeamAName,
            ts.TeamBID,
            tb.TeamName AS TeamBName,
            ts.MatchDate,
            ts.TournamentID,
            tr.TournamentName,
            ta.TeamLogo as TeamALogo,
            tb.TeamLogo as TeamBLogo
        FROM TeamSchedule ts
        LEFT JOIN Teams ta ON ts.TeamAID = ta.TeamsID
        LEFT JOIN Teams tb ON ts.TeamBID = tb.TeamsID
        LEFT JOIN Tournaments tr on tr.TournamentID = ts.TournamentID
        WHERE ts.TournamentID=@TournamentID
        ORDER BY ts.MatchDate;
    END

    /* =========================
       @Flag = 2 → POST (INSERT)
       ========================= */
    ELSE IF (@Flag = 2)
    BEGIN
        INSERT INTO TeamSchedule
        (
            TeamAID,
            TeamBID,
            MatchDate,
            TournamentID
        )
        VALUES
        (
            @TeamAID,
            @TeamBID,
            @MatchDate,
            @TournamentID
        );

        SELECT SCOPE_IDENTITY() AS TeamScheduleID;
    END

    /* =========================
       @Flag = 3 → UPDATE
       ========================= */
    ELSE IF (@Flag = 3)
    BEGIN
        UPDATE TeamSchedule
        SET
            TeamAID = @TeamAID,
            TeamBID = @TeamBID,
            MatchDate = @MatchDate,
            TournamentID = @TournamentID
        WHERE TeamScheduleID = @TeamScheduleID;

        SELECT @TeamScheduleID AS TeamScheduleID;
    END

    /* =========================
       @Flag = 4 → DELETE
       ========================= */
    ELSE IF (@Flag = 4)
    BEGIN
        DELETE FROM TeamSchedule
        WHERE TeamScheduleID = @TeamScheduleID;

        SELECT @TeamScheduleID AS TeamScheduleID;
    END
    ELSE IF (@Flag = 5)
    BEGIN
        SELECT
            ts.TeamScheduleID,
            ts.TeamAID,
            ta.TeamName AS TeamAName,
            ts.TeamBID,
            tb.TeamName AS TeamBName,
            ts.MatchDate,
            ts.TournamentID
        FROM TeamSchedule ts
        LEFT JOIN Teams ta ON ts.TeamAID = ta.TeamsID
        LEFT JOIN Teams tb ON ts.TeamBID = tb.TeamsID
        WHERE ts.TeamScheduleID = @TeamScheduleID;
    END

    IF(@Flag = 6)
    BEGIN
        SELECT 
             p.FullName AS PlayerName,
   
            CASE 
                WHEN p.TeamsID = ts.TeamAID THEN 'TeamA'
                ELSE 'TeamB'
            END AS TeamSide,
            pr.RoleName,
            p.PlayerImage
        FROM Players p
        INNER JOIN TeamSchedule ts ON p.TeamsID IN (ts.TeamAID, ts.TeamBID)
        LEFT JOIN PlayerRole pr on pr.PlayerRoleID = p.PlayerRoleID
        WHERE ts.TeamScheduleID = @TeamScheduleID
        ORDER BY TeamSide, p.FullName;

    END
    IF(@Flag = 7)
    BEGIN
        SELECT 
            ts.TeamScheduleID,
            ts.TeamAID,
            ts.TeamBID,
            ts.MatchDate,
            ts.TournamentID,
            t.TournamentName,
            ta.TeamName AS TeamAName,
            tb.TeamName AS TeamBName
        FROM TeamSchedule ts
        INNER JOIN Tournaments t ON ts.TournamentID = t.TournamentID
        INNER JOIN Teams ta ON ts.TeamAID = ta.TeamsID
        INNER JOIN Teams tb ON ts.TeamBID = tb.TeamsID
        WHERE CAST(ts.MatchDate AS DATE) = CAST('2026-01-24' AS DATE)
        ORDER BY ts.MatchDate;

    END

END
GO
/****** Object:  StoredProcedure [dbo].[SP_TournamentPointTable]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[SP_TournamentPointTable]
    @Flag INT,
    @TournamentID INT = NULL,
    @TeamsID INT = NULL,
    @WinnerTeamID INT = NULL,
    @IsDraw BIT = 0,
    @RunsScored INT = 0,
    @BallsFaced INT = 0,
    @RunsConceded INT = 0,
    @BallsBowled INT = 0
AS
BEGIN
    IF @Flag = 1 -- Get Points Table by TournamentID (Showing all teams in tournament)
    BEGIN
        SELECT
            ISNULL(pt.PointTableID, 0) as PointTableID,
            @TournamentID as TournamentID,
            tm.TeamId as TeamsID,
            ISNULL(pt.Played, 0) as Played,
            ISNULL(pt.Won, 0) as Won,
            ISNULL(pt.Lost, 0) as Lost,
            ISNULL(pt.Draw, 0) as Draw,
            ISNULL(pt.NR, 0) as NR,
            ISNULL(pt.Points, 0) as Points,
            ISNULL(pt.NRR, 0) as NRR,
            t.TeamName,
            t.TeamLogo
        FROM TournamentTeamMapping tm
        JOIN Teams t ON tm.TeamId = t.TeamsID
        LEFT JOIN TournamentPointTable pt ON pt.TeamsID = tm.TeamId AND pt.TournamentID = tm.TournamentId
        WHERE tm.TournamentId = @TournamentID
        ORDER BY ISNULL(pt.Points, 0) DESC, ISNULL(pt.NRR, 0) DESC, t.TeamName ASC;
    END

    ELSE IF @Flag = 2 -- Update/Initialize Points for teams in a match
    BEGIN
        -- Initialize if not exists
        IF NOT EXISTS (SELECT 1 FROM TournamentPointTable WHERE TournamentID = @TournamentID AND TeamsID = @TeamsID)
        BEGIN
            INSERT INTO TournamentPointTable (TournamentID, TeamsID, Played, Won, Lost, Draw, NR, Points, NRR, TotalRunsScored, TotalBallsFaced, TotalRunsConceded, TotalBallsBowled)
            VALUES (@TournamentID, @TeamsID, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        END

        -- Update cumulative stats
        UPDATE TournamentPointTable
        SET 
            TotalRunsScored = TotalRunsScored + @RunsScored,
            TotalBallsFaced = TotalBallsFaced + @BallsFaced,
            TotalRunsConceded = TotalRunsConceded + @RunsConceded,
            TotalBallsBowled = TotalBallsBowled + @BallsBowled
        WHERE TournamentID = @TournamentID AND TeamsID = @TeamsID;

        -- Update match result stats
        IF @WinnerTeamID IS NOT NULL AND @WinnerTeamID > 0
        BEGIN
            IF @WinnerTeamID = @TeamsID
            BEGIN
                UPDATE TournamentPointTable
                SET Played = Played + 1, Won = Won + 1, Points = Points + 2
                WHERE TournamentID = @TournamentID AND TeamsID = @TeamsID;
            END
            ELSE
            BEGIN
                UPDATE TournamentPointTable
                SET Played = Played + 1, Lost = Lost + 1
                WHERE TournamentID = @TournamentID AND TeamsID = @TeamsID;
            END
        END
        ELSE IF @IsDraw = 1
        BEGIN
            UPDATE TournamentPointTable
            SET Played = Played + 1, Draw = Draw + 1, Points = Points + 1
            WHERE TournamentID = @TournamentID AND TeamsID = @TeamsID;
        END

        -- Calculate NRR
        DECLARE @RunsFor DECIMAL(18, 6), @OversFor DECIMAL(18, 6), @RunsAgainst DECIMAL(18, 6), @OversAgainst DECIMAL(18, 6);
        DECLARE @NewNRR DECIMAL(18, 3);

        SELECT 
            @RunsFor = CAST(TotalRunsScored AS DECIMAL(18, 6)),
            @OversFor = CAST(TotalBallsFaced AS DECIMAL(18, 6)) / 6.0,
            @RunsAgainst = CAST(TotalRunsConceded AS DECIMAL(18, 6)),
            @OversAgainst = CAST(TotalBallsBowled AS DECIMAL(18, 6)) / 6.0
        FROM TournamentPointTable
        WHERE TournamentID = @TournamentID AND TeamsID = @TeamsID;

        -- NRR = (Runs Scored / Overs Faced) - (Runs Conceded / Overs Bowled)
        IF @OversFor > 0 AND @OversAgainst > 0
        BEGIN
            SET @NewNRR = (@RunsFor / @OversFor) - (@RunsAgainst / @OversAgainst);
            
            UPDATE TournamentPointTable
            SET NRR = @NewNRR
            WHERE TournamentID = @TournamentID AND TeamsID = @TeamsID;
        END
    END
END
GO
/****** Object:  StoredProcedure [dbo].[SP_Tournaments]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_Tournaments]
(
    @Flag INT,
    @TournamentID INT = NULL,
    @TournamentName NVARCHAR(200) = NULL,
    @Prize nvarchar(200)=NULL,
    @Location NVARCHAR(200) = NULL,
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @TournamentTypeID INT = NULL,
    @RegistrationDeadline DATETIME = NULL,
    @RegistrationFee INT = NULL,
    @FieldFee INT = NULL,
    @MaxTeams INT = NULL,
    @TotalPlayer INT = NULL,
    @MatchPlayer INT = NULL,
    @ExtraPlayer INT = NULL,
    @Status NVARCHAR(200) = NULL,
    @ContactNumber NVARCHAR(200) = NULL,
    @CreatedBy INT = NULL,
    @CreatedAt DATETIME = NULL,
    @UpdatedBy INT = NULL,
    @UpdatedAt DATETIME = NULL,
    @IsActive BIT = NULL,
    @userId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Flag = 1
    BEGIN
        SELECT 
        t.TournamentID,
        t.TournamentName,
        ty.Name as TournamentTypeName,
        t.StartDate,
        t.EndDate, t.Location, t.RegistrationDeadLine, t.Status, t.TotalPlayer, t.MatchPlayer, t.ExtraPlayer, t.RegistrationFee, t.FieldFee, t.MaxTeams, t.ContactNumber
        FROM Tournaments t
        left join TournamentType ty on ty.Id = t.TournamentTypeID;
    END
    ELSE IF @Flag = 2 -- Insert
    BEGIN
        INSERT INTO Tournaments(TournamentName,Prize, Location, StartDate, EndDate, TournamentTypeID, RegistrationDeadline,
                                RegistrationFee, FieldFee, MaxTeams, TotalPlayer, MatchPlayer, ExtraPlayer, Status,
                                ContactNumber, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, IsActive)
        VALUES(@TournamentName,@Prize, @Location, @StartDate, @EndDate, @TournamentTypeID, @RegistrationDeadline,
               @RegistrationFee, @FieldFee, @MaxTeams, @TotalPlayer, @MatchPlayer, @ExtraPlayer, @Status,
               @ContactNumber, @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt, @IsActive);

        SELECT SCOPE_IDENTITY() AS TournamentID;
    END
    ELSE IF @Flag = 3 -- Update
    BEGIN
        UPDATE Tournaments
        SET TournamentName=@TournamentName, Prize=ISNULL(@Prize,Prize), Location=@Location, StartDate=@StartDate, EndDate=@EndDate,
            TournamentTypeID=@TournamentTypeID, RegistrationDeadline=@RegistrationDeadline,
            RegistrationFee=@RegistrationFee, FieldFee=@FieldFee, MaxTeams=@MaxTeams, TotalPlayer=@TotalPlayer,
            MatchPlayer=@MatchPlayer, ExtraPlayer=@ExtraPlayer, Status=@Status, ContactNumber=@ContactNumber,
            CreatedBy=@CreatedBy, CreatedAt=@CreatedAt, UpdatedBy=@UpdatedBy, UpdatedAt=@UpdatedAt, IsActive=@IsActive
        WHERE TournamentID=@TournamentID;

        SELECT @TournamentID AS TournamentID;
    END
    ELSE IF @Flag = 4 -- Delete
    BEGIN
        DELETE FROM Tournaments WHERE TournamentID=@TournamentID;
        SELECT @TournamentID AS TournamentID;
    END
    ELSE IF @Flag = 6 -- Get by ID
    BEGIN
        SELECT * FROM Tournaments WHERE TournamentID=@TournamentID;
    END
    ELSE IF @Flag=7
    Begin
    SELECT  t.TournamentID, 
    t.TournamentName,
    t.Prize,
    t.Location,
    t.StartDate, 
    t.EndDate, 
    t.TournamentTypeID,
    t.RegistrationDeadLine, 
    t.RegistrationFee, 
    t.FieldFee, 
    t.MaxTeams, 
    t.TotalPlayer,
    t.MatchPlayer, 
    t.ExtraPlayer,
    t.Status,
    t.ContactNumber,
    te.UserId
            

        FROM Tournaments t
        left join TournamentTeamMapping tm on tm.TournamentId = t.TournamentID
        left join Teams te on tm.TeamId = te.TeamsID
        WHERE te.UserId =@userId
    END

    ELSE IF @Flag = 8 -- Get Unregistered Tournament by UserID
    BEGIN
        SELECT * 
        FROM Tournaments t
        WHERE t.TournamentID NOT IN (
            SELECT TournamentID 
            FROM TournamentTeamMapping m
            INNER JOIN Teams te ON te.TeamsID = m.TeamID
            WHERE te.UserId = @userId
        );
    END
    ELSE  IF @Flag = 9
    BEGIN
        SELECT 
        t.TournamentID,
        t.TournamentName,
        ty.Name as TournamentTypeName,
        t.StartDate,
        t.EndDate, t.Location, t.RegistrationDeadLine, t.Status, t.TotalPlayer, t.MatchPlayer, t.ExtraPlayer, t.RegistrationFee, t.FieldFee, t.MaxTeams, t.ContactNumber
        FROM Tournaments t
        left join TournamentType ty on ty.Id = t.TournamentTypeID
        WHERE t.Status ='Upcoming'
    END
END
GO
/****** Object:  StoredProcedure [dbo].[SP_TournamentsType]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_TournamentsType]
    @Flag INT,
    @Id INT = NULL,
    @Name NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- ================= Get All =================
    IF @Flag = 1
    BEGIN
        SELECT Id, Name
        FROM TournamentType;
    END

    -- ================= Insert =================
    ELSE IF @Flag = 2
    BEGIN
        -- Check if name already exists
        IF EXISTS (SELECT 1 FROM TournamentType WHERE Name = @Name)
        BEGIN
            SELECT 0 AS Success, 'TournamentType already exists' AS Message;
            RETURN;
        END

        INSERT INTO TournamentType (Name)
        VALUES (@Name);

        SELECT 1 AS Success, 'TournamentType added successfully' AS Message,
               Id, Name
        FROM TournamentType
        WHERE Id = SCOPE_IDENTITY();
    END

    -- ================= Update =================
    ELSE IF @Flag = 3
    BEGIN
        -- Check if Id exists
        IF NOT EXISTS (SELECT 1 FROM TournamentType WHERE Id = @Id)
        BEGIN
            SELECT 0 AS Success, 'TournamentType not found' AS Message;
            RETURN;
        END

        UPDATE TournamentType
        SET Name = @Name
        WHERE Id = @Id;

        SELECT 1 AS Success, 'TournamentType updated successfully' AS Message,
               Id, Name
        FROM TournamentType
        WHERE Id = @Id;
    END

    -- ================= Delete =================
    ELSE IF @Flag = 4
    BEGIN
        -- Check if Id exists
        IF NOT EXISTS (SELECT 1 FROM TournamentType WHERE Id = @Id)
        BEGIN
            SELECT 0 AS Success, 'TournamentType not found' AS Message;
            RETURN;
        END

        DELETE FROM TournamentType
        WHERE Id = @Id;

        SELECT 1 AS Success, 'TournamentType deleted successfully' AS Message;
    END

    -- ================= Get By Id =================
    ELSE IF @Flag = 5
    BEGIN
        SELECT Id, Name
        FROM TournamentType
        WHERE Id = @Id;
    END
END
GO
/****** Object:  StoredProcedure [dbo].[SP_TournamentTeamMapping]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_TournamentTeamMapping]
    @Flag INT,
    @TournamentId INT = NULL,
    @TeamId INT = NULL,
    @bkashPaymentId nvarchar(50) = NULL,
    @UserId int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- ================= INSERT TEAM INTO TOURNAMENT =================
    IF @Flag = 1
    BEGIN
        -- Check duplicate entry
        IF EXISTS (
            SELECT 1 
            FROM TournamentTeamMapping
            WHERE TournamentId = @TournamentId
              AND TeamId = @TeamId
        )
        BEGIN
            SELECT 
                0 AS Success,
                'Team already registered in this tournament' AS Message;
            RETURN;
        END

        INSERT INTO TournamentTeamMapping (TournamentId, TeamId, PaymentStatus, bkashPaymentId)
        VALUES (@TournamentId, @TeamId, 'Pending', @bkashPaymentId);

        SELECT 
            1 AS Success,
            'Team successfully registered in tournament' AS Message,
            Id,
            TournamentId,
            TeamId
           
        FROM TournamentTeamMapping
        WHERE Id = SCOPE_IDENTITY();
    END
    If @Flag = 2 
    Begin 
     select * from Tournaments where TournamentID = @TournamentId
    end
    If @Flag = 3
    Begin 
     update TournamentTeamMapping set PaymentStatus = 'Paid' where bkashPaymentId = @bkashPaymentId
    end

END


--select * from TournamentTeamMapping
GO
/****** Object:  StoredProcedure [dbo].[SP_UserInfo]    Script Date: 02/21/2026 02:30:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_UserInfo]
    @Flag INT,
    @Name NVARCHAR(200) = NULL,
    @Email NVARCHAR(200) = NULL,
    @Phone NVARCHAR(200) = NULL,
    @UserType NVARCHAR(200) = NULL,
    @Password NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- ================= REGISTER =================
    IF @Flag = 1
    BEGIN
        -- Check if email already exists
        IF EXISTS (SELECT 1 FROM UserInfo WHERE Email = @Email)
        BEGIN
            SELECT 
                0 AS Success,
                'Email already exists' AS Message;
            RETURN;
        END

        -- Insert new user
        INSERT INTO UserInfo (Name, Email, Phone, UserType, Password)
        VALUES (@Name, @Email, @Phone, @UserType, @Password);

        -- Return inserted user info
        SELECT 
            1 AS Success,
            'Registration successful' AS Message,
            Id as UserId, Name, Email, Phone, UserType
        FROM UserInfo
        WHERE Id = SCOPE_IDENTITY();
    END

    -- ================= LOGIN =================
    ELSE IF @Flag = 2
    BEGIN
        -- Check user credentials
        IF EXISTS (SELECT 1 FROM UserInfo WHERE Email = @Email AND Password = @Password)
        BEGIN
            SELECT 
                1 AS Success,
                'Login successful' AS Message,
                ID, Name, Email, Phone, UserType
            FROM UserInfo
            WHERE Email = @Email AND Password = @Password;
        END
        ELSE
        BEGIN
            SELECT 
                0 AS Success,
                'Invalid email or password' AS Message;
        END
    END
    ELSE IF @Flag = 3
BEGIN
    SELECT 
        Id as UserID,
        Name,
        Email,
        Phone,
        UserType
    FROM UserInfo
END
END

GO
USE [master]
GO
ALTER DATABASE [SportsHubDB] SET  READ_WRITE 
GO
