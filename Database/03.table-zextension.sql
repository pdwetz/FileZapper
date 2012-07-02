USE [zapper]
GO

/****** Object:  Table [dbo].[zextension]    Script Date: 07/02/2012 00:02:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[zextension](
	[Name] [varchar](50) NOT NULL,
	[IsIgnored] [bit] NOT NULL,
	[IsUnwanted] [bit] NOT NULL,
 CONSTRAINT [PK_zextension] PRIMARY KEY CLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[zextension] ADD  CONSTRAINT [DF_zextension_IsIgnored]  DEFAULT ((0)) FOR [IsIgnored]
GO

ALTER TABLE [dbo].[zextension] ADD  CONSTRAINT [DF_zextension_IsUnwanted]  DEFAULT ((0)) FOR [IsUnwanted]
GO


