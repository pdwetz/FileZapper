USE [zapper]
GO

/****** Object:  Table [dbo].[zfolder]    Script Date: 07/02/2012 00:03:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[zfolder](
	[FullPath] [varchar](260) NOT NULL,
	[IsEnabled] [bit] NOT NULL,
	[Priority] [int] NOT NULL,
 CONSTRAINT [PK_zfolder] PRIMARY KEY CLUSTERED 
(
	[FullPath] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[zfolder] ADD  CONSTRAINT [DF_zfolder_IsEnabled]  DEFAULT ((0)) FOR [IsEnabled]
GO

ALTER TABLE [dbo].[zfolder] ADD  CONSTRAINT [DF_zfolder_Priority]  DEFAULT ((0)) FOR [Priority]
GO


