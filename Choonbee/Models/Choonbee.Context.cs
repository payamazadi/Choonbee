﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Choonbee.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ChoonbeeEdmx : DbContext
    {
        public ChoonbeeEdmx()
            : base("name=ChoonbeeEdmx")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<School> Schools { get; set; }
        public DbSet<RankGroup> RankGroups { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<AgeGroup> AgeGroups { get; set; }
        public DbSet<DivisionType> DivisionTypes { get; set; }
        public DbSet<DivisionStatus> DivisionStatuses { get; set; }
        public DbSet<DivisionFormScore> DivisionFormScores { get; set; }
        public DbSet<DivisionFormTieHistory> DivisionFormTieHistories { get; set; }
        public DbSet<DivisionWinner> DivisionWinners { get; set; }
        public DbSet<TournamentTeam> TournamentTeams { get; set; }
        public DbSet<DivisionDefault> DivisionDefaults { get; set; }
        public DbSet<Division> Divisions { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<DivisionParticipant> DivisionParticipants { get; set; }
    }
}
