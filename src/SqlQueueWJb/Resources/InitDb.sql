IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'SqlQueueWJb')
BEGIN
    CREATE DATABASE [SqlQueueWJb];
END
GO

USE [SqlQueueWJb];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WJbHistory](
	[JobId] [bigint] NOT NULL,
	[JobPriority] [tinyint] NOT NULL,
	[Created] [datetime] NOT NULL,
	[JobStatus] [tinyint] NOT NULL,
	[ActionCode] [nvarchar](64) NOT NULL,
	[JobMore] [nvarchar](max) NULL,
	[Started] [datetime] NULL,
	[Finished] [datetime] NULL,
 CONSTRAINT [PK_WJbHistory] PRIMARY KEY CLUSTERED 
(
	[JobId] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WJbQueue](
	[JobId] [bigint] IDENTITY(1000,1) NOT NULL,
	[JobPriority] [tinyint] NOT NULL,
	[Created] [datetime] NOT NULL,
	[JobStatus] [tinyint] NOT NULL,
	[ActionCode] [nvarchar](64) NOT NULL,
	[JobMore] [nvarchar](max) NULL,
	[Started] [datetime] NULL,
	[Finished] [datetime] NULL,
 CONSTRAINT [PK_WJbQueue] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[WJbQueue] ADD  CONSTRAINT [DF_WJbQueue_JobPriority]  DEFAULT ((2)) FOR [JobPriority]
GO
ALTER TABLE [dbo].[WJbQueue] ADD  CONSTRAINT [DF_WJbQueue_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[WJbQueue] ADD  CONSTRAINT [DF_WJbQueue_JobStatus]  DEFAULT ((0)) FOR [JobStatus]
GO
