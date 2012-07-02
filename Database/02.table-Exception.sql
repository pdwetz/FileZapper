USE [zapper]
GO

/****** Object:  Table [dbo].[Exception]    Script Date: 07/01/2012 23:59:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Exception](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AddDate] [datetime2](7) NOT NULL,
	[ExceptionMessage] [varchar](500) NOT NULL,
	[ExceptionType] [varchar](255) NOT NULL,
	[AssemblyName] [varchar](255) NOT NULL,
	[Thread] [varchar](255) NOT NULL,
	[InnerException] [varchar](max) NULL,
	[StackTrace] [varchar](max) NULL,
	[ExceptionNotes] [varchar](max) NULL,
 CONSTRAINT [PK_Log] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


