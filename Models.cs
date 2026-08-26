using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CspmEngine
{
    // Database context
    public class CspmDbContext : DbContext
    {
        public DbSet<Scan> Scans { get; set; }
        public DbSet<CloudResource> Resources { get; set; }
        public DbSet<ScanFinding> Findings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=cspm_audit.db");
    }

    public class Scan
    {
        public int Id { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; } = "IN_PROGRESS";
        public List<ScanFinding> Findings { get; set; } = new();
    }

    public class CloudResource
    {
        public int Id { get; set; }
        public string ResourceIdentifier { get; set; } // Bucket name or Role ARN
        public string ResourceType { get; set; }       // AWS::S3::Bucket or AWS::IAM::Role
        public string Region { get; set; }
        public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;
    }

    public class ScanFinding
    {
        public int Id { get; set; }
        public int ScanId { get; set; }
        public Scan Scan { get; set; }
        public string ResourceIdentifier { get; set; }
        public string RuleCode { get; set; }
        public string Severity { get; set; }           // CRITICAL, HIGH, MEDIUM, LOW
        public string Status { get; set; }             // PASS, FAIL
        public string Evidence { get; set; }
    }
}