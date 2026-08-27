using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Amazon;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.EC2;
using Amazon.EC2.Model;

namespace CspmEngine
{
    // ==========================================
    // 1. THE MULTI-CLOUD BLUEPRINT (INTERFACE)
    // ==========================================
    public interface ICloudScanner
    {
        string CloudProvider { get; }
        Task<List<ScanFinding>> RunScanAsync(int scanId, string webhookUrl);
    }

    // ==========================================
    // 2. THE AWS SCANNER PLUGIN (Bulletproof)
    // ==========================================
    public class AwsScanner : ICloudScanner
    {
        public string CloudProvider => "AWS";

        public async Task<List<ScanFinding>> RunScanAsync(int scanId, string webhookUrl)
        {
            var findings = new List<ScanFinding>();
            var iamClient = new AmazonIdentityManagementServiceClient();
            var ec2Client = new AmazonEC2Client(RegionEndpoint.EUNorth1); 

            Console.WriteLine($"[{CloudProvider}] Scanning Identity & Access Management...");
            var iamResponse = await iamClient.ListRolesAsync();
            
            // Safety check: Ensure roles exist
            if (iamResponse?.Roles != null)
            {
                foreach (var role in iamResponse.Roles)
                {
                    if (role.RoleName.StartsWith("AWSServiceRole")) continue;

                    var attachedPolicies = await iamClient.ListAttachedRolePoliciesAsync(new ListAttachedRolePoliciesRequest { RoleName = role.RoleName });
                    
                    if (attachedPolicies.AttachedPolicies?.Any(p => p.PolicyName == "AdministratorAccess") == true)
                    {
                        findings.Add(new ScanFinding { ScanId = scanId, ResourceIdentifier = role.RoleName, RuleCode = "IAM_NO_WILDCARD_ADMIN", Severity = "CRITICAL", Status = "FAIL", Evidence = "Attached policy 'AdministratorAccess' found." });
                    }
                }
            }

            Console.WriteLine($"[{CloudProvider}] Scanning EC2 Security Groups...");
            var sgResponse = await ec2Client.DescribeSecurityGroupsAsync();
            
            // Safety check: Ensure security groups exist
            if (sgResponse?.SecurityGroups != null)
            {
                foreach (var sg in sgResponse.SecurityGroups)
                {
                    // THE FIX: Only loop through IP rules if they actually exist!
                    if (sg.IpPermissions != null)
                    {
                        foreach (var ipPerm in sg.IpPermissions)
                        {
                            if (ipPerm.IpProtocol == "tcp" && ipPerm.FromPort <= 22 && ipPerm.ToPort >= 22 && ipPerm.Ipv4Ranges?.Any(ip => ip.CidrIp == "0.0.0.0/0") == true)
                            {
                                findings.Add(new ScanFinding { ScanId = scanId, ResourceIdentifier = sg.GroupName, RuleCode = "EC2_SSH_OPEN_TO_INTERNET", Severity = "CRITICAL", Status = "FAIL", Evidence = "Inbound SSH (Port 22) open to 0.0.0.0/0." });
                            }
                        }
                    }
                }
            }
            return findings;
        }
    }
    
    // ==========================================
    // 3. THE AZURE SCANNER PLUGIN (Mocked)
    // ==========================================
    public class AzureScanner : ICloudScanner
    {
        public string CloudProvider => "Azure";

        public async Task<List<ScanFinding>> RunScanAsync(int scanId, string webhookUrl)
        {
            Console.WriteLine($"[{CloudProvider}] Scanning Azure Resource Graph...");
            await Task.Delay(1000); // Simulating API call
            
            // Simulating a clean Azure environment for now
            return new List<ScanFinding>(); 
        }
    }

    // ==========================================
    // 4. THE GCP SCANNER PLUGIN (Mocked)
    // ==========================================
    public class GcpScanner : ICloudScanner
    {
        public string CloudProvider => "GCP";

        public async Task<List<ScanFinding>> RunScanAsync(int scanId, string webhookUrl)
        {
            Console.WriteLine($"[{CloudProvider}] Scanning GCP Asset Inventory...");
            await Task.Delay(1000); // Simulating API call
            return new List<ScanFinding>(); 
        }
    }

    // ==========================================
    // 5. THE CORE ENGINE
    // ==========================================
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<CspmDbContext>();
            builder.Services.ConfigureHttpJsonOptions(options => {
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            // Dependency Injection: Register our cloud scanners!
            builder.Services.AddTransient<ICloudScanner, AwsScanner>();
            builder.Services.AddTransient<ICloudScanner, AzureScanner>();
            builder.Services.AddTransient<ICloudScanner, GcpScanner>();

            var app = builder.Build();
            app.UseDefaultFiles(); 
            app.UseStaticFiles();  

            app.MapGet("/api/scans", async (CspmDbContext db) =>
            {
                return await db.Scans.Include(s => s.Findings).ToListAsync();
            });

            // The beautifully refactored Multi-Cloud API Endpoint
            app.MapPost("/api/scan/run", async (IEnumerable<ICloudScanner> scanners, CspmDbContext db) =>
            {
                var currentScan = new Scan();
                db.Scans.Add(currentScan);
                await db.SaveChangesAsync();

                string webhookUrl = "https://webhook.site/22ec14ed-fa58-4f7e-815a-95ad72a6e6fb"; 
                Console.WriteLine("\n[MULTI-CLOUD AUDIT] Starting global scan...");

                // The engine loops through AWS, Azure, and GCP automatically!
                foreach (var scanner in scanners)
                {
                    var newFindings = await scanner.RunScanAsync(currentScan.Id, webhookUrl);
                    
                    foreach(var finding in newFindings)
                    {
                        db.Findings.Add(finding);
                        await SendWebhookAlertAsync(finding, webhookUrl, scanner.CloudProvider);
                    }
                }

                currentScan.Status = "COMPLETED";
                currentScan.CompletedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
                return Results.Ok(new { Message = "Multi-Cloud Scan completed", ScanId = currentScan.Id });
            });

            app.MapPost("/api/remediate", async (RemediateRequest request) =>
            {
                // Remediation logic remains the same
                try
                {
                    if (request.RuleCode == "IAM_NO_WILDCARD_ADMIN")
                    {
                        var iamClient = new AmazonIdentityManagementServiceClient();
                        await iamClient.DetachRolePolicyAsync(new DetachRolePolicyRequest { RoleName = request.ResourceIdentifier, PolicyArn = "arn:aws:iam::aws:policy/AdministratorAccess" });
                        return Results.Ok();
                    }
                    else if (request.RuleCode == "EC2_SSH_OPEN_TO_INTERNET")
                    {
                        var ec2Client = new AmazonEC2Client(RegionEndpoint.EUNorth1);
                        await ec2Client.RevokeSecurityGroupIngressAsync(new RevokeSecurityGroupIngressRequest {
                            GroupName = request.ResourceIdentifier,
                            IpPermissions = new List<IpPermission> { new IpPermission { IpProtocol = "tcp", FromPort = 22, ToPort = 22, Ipv4Ranges = new List<IpRange> { new IpRange { CidrIp = "0.0.0.0/0" } } } }
                        });
                        return Results.Ok();
                    }
                    return Results.BadRequest();
                }
                catch { return Results.Problem(); }
            });

            Console.WriteLine("Starting Multi-Cloud CSPM Web API Server...");
            app.Run();
        }

        static async Task SendWebhookAlertAsync(ScanFinding finding, string webhookUrl, string provider)
        {
            using var client = new HttpClient();
            var payload = new { text = $"🚨 *{provider} SECURITY ALERT* 🚨\n*Resource:* `{finding.ResourceIdentifier}`\n*Rule Failed:* `{finding.RuleCode}`\n*Evidence:* {finding.Evidence}" };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            try { await client.PostAsync(webhookUrl, content); } catch { }
        }
    }

    public record RemediateRequest(string ResourceIdentifier, string RuleCode);
}