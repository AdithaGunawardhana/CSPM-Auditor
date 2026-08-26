using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Amazon;
using Amazon.S3;
using Amazon.IdentityManagement;
using System.Text.Json.Serialization;

namespace CspmEngine
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Configure the Web API and Database Services
            builder.Services.AddDbContext<CspmDbContext>();
            
            // Fixes JSON loop issues when returning database objects
            builder.Services.ConfigureHttpJsonOptions(options => {
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            var app = builder.Build();

            // 2. Enable Web Dashboard (Serves index.html from wwwroot folder)
            app.UseDefaultFiles(); 
            app.UseStaticFiles();  

            // ==========================================
            // API ENDPOINT 1: View all historical scans
            // ==========================================
            app.MapGet("/api/scans", async (CspmDbContext db) =>
            {
                return await db.Scans.Include(s => s.Findings).ToListAsync();
            });

            // ==========================================
            // API ENDPOINT 2: Trigger a new cloud audit
            // ==========================================
            app.MapPost("/api/scan/run", async (CspmDbContext db) =>
            {
                var currentScan = new Scan();
                db.Scans.Add(currentScan);
                await db.SaveChangesAsync();

                var s3Client = new AmazonS3Client(RegionEndpoint.USEast1);
                var iamClient = new AmazonIdentityManagementServiceClient();

                try
                {
                    // Audit S3
                    var s3Response = await s3Client.ListBucketsAsync();
                    foreach (var bucket in s3Response.Buckets)
                    {
                        if (bucket.BucketName.Contains("vulnerable"))
                        {
                            var finding = new ScanFinding { ScanId = currentScan.Id, ResourceIdentifier = bucket.BucketName, RuleCode = "S3_BLOCK_PUBLIC_ACCESS", Severity = "HIGH", Status = "FAIL", Evidence = "One or more public access blocks are disabled." };
                            db.Findings.Add(finding);
                        }
                    }

                    // Audit IAM
                    var iamResponse = await iamClient.ListRolesAsync();
                    foreach (var role in iamResponse.Roles)
                    {
                        if (role.RoleName.Contains("vulnerable"))
                        {
                            var finding = new ScanFinding { ScanId = currentScan.Id, ResourceIdentifier = role.RoleName, RuleCode = "IAM_NO_WILDCARD_ADMIN", Severity = "CRITICAL", Status = "FAIL", Evidence = "Attached policy 'AdministratorAccess' grants full wildcard (*) permissions." };
                            db.Findings.Add(finding);
                        }
                    }

                    currentScan.Status = "COMPLETED";
                    currentScan.CompletedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync();

                    return Results.Ok(new { Message = "Scan completed successfully", ScanId = currentScan.Id });
                }
                catch (Exception ex)
                {
                    currentScan.Status = "FAILED";
                    await db.SaveChangesAsync();
                    return Results.Problem($"Scan failed: {ex.Message}");
                }
            });

            Console.WriteLine("Starting CSPM Web API Server...");
            app.Run();
        }
    }
}