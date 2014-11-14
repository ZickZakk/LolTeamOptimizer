
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 11/14/2014 13:10:47
-- Generated from EDMX file: C:\Users\gjenschmischek\Desktop\LolTeamOptimzer\LolTeamOptimzer\DataModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_ChampionIsStrongAgainst]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[IsStrongAgainstSet] DROP CONSTRAINT [FK_ChampionIsStrongAgainst];
GO
IF OBJECT_ID(N'[dbo].[FK_ChampionIsStrongAgainst1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Champions] DROP CONSTRAINT [FK_ChampionIsStrongAgainst1];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Champions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Champions];
GO
IF OBJECT_ID(N'[dbo].[IsStrongAgainstSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[IsStrongAgainstSet];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Champions'
CREATE TABLE [dbo].[Champions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'IsStrongAgainstSet'
CREATE TABLE [dbo].[IsStrongAgainstSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Rating] int  NOT NULL,
    [Champion_Id] int  NOT NULL,
    [OtherChampion_Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Champions'
ALTER TABLE [dbo].[Champions]
ADD CONSTRAINT [PK_Champions]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'IsStrongAgainstSet'
ALTER TABLE [dbo].[IsStrongAgainstSet]
ADD CONSTRAINT [PK_IsStrongAgainstSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Champion_Id] in table 'IsStrongAgainstSet'
ALTER TABLE [dbo].[IsStrongAgainstSet]
ADD CONSTRAINT [FK_ChampionIsStrongAgainst]
    FOREIGN KEY ([Champion_Id])
    REFERENCES [dbo].[Champions]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ChampionIsStrongAgainst'
CREATE INDEX [IX_FK_ChampionIsStrongAgainst]
ON [dbo].[IsStrongAgainstSet]
    ([Champion_Id]);
GO

-- Creating foreign key on [OtherChampion_Id] in table 'IsStrongAgainstSet'
ALTER TABLE [dbo].[IsStrongAgainstSet]
ADD CONSTRAINT [FK_OtherChampionIsStrongAgainst]
    FOREIGN KEY ([OtherChampion_Id])
    REFERENCES [dbo].[Champions]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_OtherChampionIsStrongAgainst'
CREATE INDEX [IX_FK_OtherChampionIsStrongAgainst]
ON [dbo].[IsStrongAgainstSet]
    ([OtherChampion_Id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------