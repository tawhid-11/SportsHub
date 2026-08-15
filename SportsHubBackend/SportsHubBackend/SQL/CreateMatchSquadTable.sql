USE [SportsHubDB]
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MatchSquad]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[MatchSquad](
	[MatchSquadID] [int] IDENTITY(1,1) NOT NULL,
	[CricketMatchID] [int] NOT NULL,
	[TeamID] [int] NOT NULL,
	[PlayerID] [int] NOT NULL,
	[IsPlaying] [bit] NOT NULL DEFAULT 1,
	[IsCaptain] [bit] NOT NULL DEFAULT 0,
	[IsWicketKeeper] [bit] NOT NULL DEFAULT 0,
 CONSTRAINT [PK_MatchSquad] PRIMARY KEY CLUSTERED 
(
	[MatchSquadID] ASC
)
) ON [PRIMARY]

ALTER TABLE [dbo].[MatchSquad]  WITH CHECK ADD  CONSTRAINT [FK_MatchSquad_CricketMatch] FOREIGN KEY([CricketMatchID])
REFERENCES [dbo].[CricketMatch] ([CricketMatchID])
ON DELETE CASCADE

ALTER TABLE [dbo].[MatchSquad]  WITH CHECK ADD  CONSTRAINT [FK_MatchSquad_Players] FOREIGN KEY([PlayerID])
REFERENCES [dbo].[Players] ([PlayerID])

END
GO
