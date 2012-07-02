USE [zapper]
GO

/****** Object:  Table [dbo].[zfile_deleted]    Script Date: 07/02/2012 00:03:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[zfile_deleted](
	[FullPath] [varchar](260) NOT NULL,
	[Deleted] [datetime2](7) NOT NULL,
	[SessionId] [uniqueidentifier] NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[Extension] [varchar](50) NOT NULL,
	[Directory] [varchar](255) NOT NULL,
	[Size] [bigint] NOT NULL,
	[FileModified] [smalldatetime] NULL,
	[ContentHash] [varchar](50) NULL,
	[Added] [datetime2](7) NULL,
	[Modified] [datetime2](7) NULL,
 CONSTRAINT [PK_zfile_deleted] PRIMARY KEY NONCLUSTERED 
(
	[FullPath] ASC,
	[Deleted] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


