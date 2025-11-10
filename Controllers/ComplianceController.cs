using ADPA.Models.DTOs.Compliance;
using ADPA.Models.Entities;
using ADPA.Services.Compliance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ADPA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ComplianceController : ControllerBase
    {
        private readonly IComplianceService _complianceService;

        public ComplianceController(IComplianceService complianceService)
        {
            _complianceService = complianceService;
        }

        private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";

        // Compliance Dashboard
        [HttpGet("dashboard")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator")]
        public async Task<ActionResult<ComplianceDashboardData>> GetDashboard()
        {
            var dashboardData = await _complianceService.GetComplianceDashboardAsync();
            var frameworkStatuses = await _complianceService.GetOverallComplianceStatusAsync();
            var complianceScores = await _complianceService.GetComplianceScoresAsync();

            var dashboard = new ComplianceDashboardData
            {
                Overview = new ComplianceOverview
                {
                    TotalPolicies = (int)(dashboardData.GetValueOrDefault("TotalPolicies", 0)),
                    ActiveViolations = (int)(dashboardData.GetValueOrDefault("ActiveViolations", 0)),
                    CriticalViolations = (int)(dashboardData.GetValueOrDefault("CriticalViolations", 0)),
                    PendingDataSubjectRequests = (int)(dashboardData.GetValueOrDefault("PendingDataSubjectRequests", 0)),
                    ComplianceScore = (int)(dashboardData.GetValueOrDefault("ComplianceScore", 0))
                },
                FrameworkStatuses = frameworkStatuses.Select(fs => new ComplianceFrameworkStatus
                {
                    Framework = fs.Key,
                    FrameworkName = fs.Key.ToString(),
                    Status = fs.Value,
                    Score = complianceScores.GetValueOrDefault(fs.Key, 0)
                }).ToList()
            };

            return Ok(dashboard);
        }

        // Policy Management
        [HttpGet("policies")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator,User")]
        public async Task<ActionResult<IEnumerable<CompliancePolicyDto>>> GetPolicies([FromQuery] ComplianceFramework? framework = null)
        {
            var policies = await _complianceService.GetPoliciesAsync(framework);
            
            var policyDtos = policies.Select(p => new CompliancePolicyDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Framework = p.Framework,
                PolicyType = p.PolicyType,
                IsActive = p.IsActive,
                IsMandatory = p.IsMandatory,
                EffectiveDate = p.EffectiveDate,
                ExpiryDate = p.ExpiryDate,
                Version = p.Version,
                ControlCount = p.Controls?.Count ?? 0,
                ViolationCount = p.Violations?.Count ?? 0
            });

            return Ok(policyDtos);
        }

        [HttpGet("policies/{id}")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator,User")]
        public async Task<ActionResult<CompliancePolicyDto>> GetPolicy(Guid id)
        {
            var policy = await _complianceService.GetPolicyAsync(id);
            if (policy == null)
                return NotFound();

            var policyDto = new CompliancePolicyDto
            {
                Id = policy.Id,
                Name = policy.Name,
                Description = policy.Description,
                Framework = policy.Framework,
                PolicyType = policy.PolicyType,
                PolicyContent = policy.PolicyContent,
                RequirementReference = policy.RequirementReference,
                IsActive = policy.IsActive,
                IsMandatory = policy.IsMandatory,
                EffectiveDate = policy.EffectiveDate,
                ExpiryDate = policy.ExpiryDate,
                Version = policy.Version,
                ControlCount = policy.Controls?.Count ?? 0,
                ViolationCount = policy.Violations?.Count ?? 0
            };

            return Ok(policyDto);
        }

        [HttpPost("policies")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator")]
        public async Task<ActionResult<CompliancePolicyDto>> CreatePolicy([FromBody] CreateCompliancePolicyRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var policy = new CompliancePolicy
            {
                Name = request.Name,
                Description = request.Description,
                Framework = request.Framework,
                PolicyType = request.PolicyType,
                PolicyContent = request.PolicyContent,
                RequirementReference = request.RequirementReference,
                IsMandatory = request.IsMandatory,
                EffectiveDate = request.EffectiveDate,
                ExpiryDate = request.ExpiryDate
            };

            var createdPolicy = await _complianceService.CreatePolicyAsync(policy, CurrentUserId);

            var policyDto = new CompliancePolicyDto
            {
                Id = createdPolicy.Id,
                Name = createdPolicy.Name,
                Description = createdPolicy.Description,
                Framework = createdPolicy.Framework,
                PolicyType = createdPolicy.PolicyType,
                IsActive = createdPolicy.IsActive,
                Version = createdPolicy.Version
            };

            return CreatedAtAction(nameof(GetPolicy), new { id = createdPolicy.Id }, policyDto);
        }

        [HttpPut("policies/{id}")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator")]
        public async Task<ActionResult<CompliancePolicyDto>> UpdatePolicy(Guid id, [FromBody] UpdateCompliancePolicyRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var policy = new CompliancePolicy
            {
                Name = request.Name,
                Description = request.Description,
                PolicyContent = request.PolicyContent,
                RequirementReference = request.RequirementReference,
                EffectiveDate = request.EffectiveDate,
                ExpiryDate = request.ExpiryDate
            };

            try
            {
                var updatedPolicy = await _complianceService.UpdatePolicyAsync(id, policy, CurrentUserId);

                var policyDto = new CompliancePolicyDto
                {
                    Id = updatedPolicy.Id,
                    Name = updatedPolicy.Name,
                    Description = updatedPolicy.Description,
                    Version = updatedPolicy.Version
                };

                return Ok(policyDto);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [HttpDelete("policies/{id}")]
        [Authorize(Roles = "ComplianceOfficer,Administrator")]
        public async Task<ActionResult> DeletePolicy(Guid id)
        {
            var result = await _complianceService.DeletePolicyAsync(id, CurrentUserId);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // Policy Controls
        [HttpGet("policies/{policyId}/controls")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator,User")]
        public async Task<ActionResult<IEnumerable<PolicyControlDto>>> GetPolicyControls(Guid policyId)
        {
            var controls = await _complianceService.GetPolicyControlsAsync(policyId);
            
            var controlDtos = controls.Select(c => new PolicyControlDto
            {
                Id = c.Id,
                PolicyId = c.PolicyId,
                ControlName = c.ControlName,
                Description = c.Description,
                IsAutomated = c.IsAutomated,
                CheckFrequencyDays = c.CheckFrequencyDays,
                Status = c.Status,
                LastChecked = c.LastChecked,
                NextCheckDue = c.NextCheckDue,
                ResponsibleRole = c.ResponsibleRole,
                Evidence = c.Evidence
            });

            return Ok(controlDtos);
        }

        [HttpPost("policies/{policyId}/controls")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator")]
        public async Task<ActionResult<PolicyControlDto>> CreatePolicyControl(Guid policyId, [FromBody] CreatePolicyControlRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var control = new PolicyControl
            {
                PolicyId = policyId,
                ControlName = request.ControlName,
                Description = request.Description,
                ImplementationGuidance = request.ImplementationGuidance,
                ValidationCriteria = request.ValidationCriteria,
                IsAutomated = request.IsAutomated,
                CheckFrequencyDays = request.CheckFrequencyDays,
                ResponsibleRole = request.ResponsibleRole
            };

            var createdControl = await _complianceService.CreateControlAsync(control, CurrentUserId);

            var controlDto = new PolicyControlDto
            {
                Id = createdControl.Id,
                PolicyId = createdControl.PolicyId,
                ControlName = createdControl.ControlName,
                Description = createdControl.Description,
                IsAutomated = createdControl.IsAutomated,
                Status = createdControl.Status
            };

            return Ok(controlDto);
        }

        [HttpPost("controls/{controlId}/check")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator")]
        public async Task<ActionResult<ComplianceStatus>> CheckControlCompliance(Guid controlId)
        {
            var status = await _complianceService.CheckControlComplianceAsync(controlId);
            return Ok(new { ControlId = controlId, Status = status, CheckedAt = DateTime.UtcNow });
        }

        [HttpPost("controls/check-automated")]
        [Authorize(Roles = "ComplianceOfficer,Administrator")]
        public async Task<ActionResult> RunAutomatedChecks()
        {
            var result = await _complianceService.RunAutomatedControlChecksAsync();
            return Ok(new { Success = result, ExecutedAt = DateTime.UtcNow });
        }

        // Violation Management
        [HttpGet("violations")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator,SecurityAnalyst")]
        public async Task<ActionResult<IEnumerable<ComplianceViolationDto>>> GetViolations([FromQuery] ViolationSearchRequest request)
        {
            var violations = await _complianceService.GetViolationsAsync(request.Framework, request.Status);
            
            var violationDtos = violations.Select(v => new ComplianceViolationDto
            {
                Id = v.Id,
                PolicyName = v.Policy?.Name,
                ControlName = v.Control?.ControlName,
                Title = v.Title,
                Description = v.Description,
                Severity = v.Severity,
                Framework = v.Framework,
                DetectedAt = v.DetectedAt,
                DetectedBy = v.DetectedBy,
                Status = v.Status,
                AffectedEntity = v.AffectedEntity,
                AffectedRecordCount = v.AffectedRecordCount,
                RequiresNotification = v.RequiresNotification,
                NotificationDeadline = v.NotificationDeadline,
                IsNotified = v.IsNotified,
                PotentialFine = v.PotentialFine,
                Currency = v.Currency,
                ActionCount = v.Actions?.Count ?? 0,
                DaysOpen = v.ResolvedAt.HasValue ? 0 : (DateTime.UtcNow - v.DetectedAt).Days
            });

            return Ok(violationDtos);
        }

        [HttpGet("violations/critical")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator,SecurityAnalyst")]
        public async Task<ActionResult<IEnumerable<ComplianceViolationDto>>> GetCriticalViolations()
        {
            var violations = await _complianceService.GetCriticalViolationsAsync();
            
            var violationDtos = violations.Select(v => new ComplianceViolationDto
            {
                Id = v.Id,
                Title = v.Title,
                Severity = v.Severity,
                Framework = v.Framework,
                DetectedAt = v.DetectedAt,
                Status = v.Status,
                AffectedRecordCount = v.AffectedRecordCount,
                RequiresNotification = v.RequiresNotification,
                NotificationDeadline = v.NotificationDeadline,
                DaysOpen = (DateTime.UtcNow - v.DetectedAt).Days
            });

            return Ok(violationDtos);
        }

        [HttpPost("violations")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator,SecurityAnalyst")]
        public async Task<ActionResult<ComplianceViolationDto>> CreateViolation([FromBody] CreateViolationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var violation = new ComplianceViolation
            {
                PolicyId = request.PolicyId,
                ControlId = request.ControlId,
                Title = request.Title,
                Description = request.Description,
                Severity = request.Severity,
                Framework = request.Framework,
                AffectedEntity = request.AffectedEntity,
                AffectedData = request.AffectedData,
                AffectedRecordCount = request.AffectedRecordCount,
                RiskAssessment = request.RiskAssessment,
                PotentialFine = request.PotentialFine,
                Currency = request.Currency
            };

            var createdViolation = await _complianceService.CreateViolationAsync(violation, CurrentUserId);

            var violationDto = new ComplianceViolationDto
            {
                Id = createdViolation.Id,
                Title = createdViolation.Title,
                Severity = createdViolation.Severity,
                Framework = createdViolation.Framework,
                DetectedAt = createdViolation.DetectedAt,
                Status = createdViolation.Status
            };

            return CreatedAtAction(nameof(GetViolations), violationDto);
        }

        [HttpPut("violations/{id}/resolve")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator")]
        public async Task<ActionResult> ResolveViolation(Guid id, [FromBody] ResolveViolationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _complianceService.ResolveViolationAsync(id, request.ResolutionNotes, CurrentUserId);
            if (!result)
                return NotFound();

            return Ok(new { ViolationId = id, ResolvedAt = DateTime.UtcNow, ResolvedBy = CurrentUserId });
        }

        [HttpPost("violations/detect")]
        [Authorize(Roles = "ComplianceOfficer,Administrator")]
        public async Task<ActionResult> DetectViolations()
        {
            var result = await _complianceService.DetectViolationsAsync();
            return Ok(new { Success = result, ExecutedAt = DateTime.UtcNow });
        }

        // Data Subject Rights
        [HttpGet("data-subject-requests")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator,PrivacyAnalyst")]
        public async Task<ActionResult<IEnumerable<DataSubjectRequestDto>>> GetDataSubjectRequests([FromQuery] DataSubjectRequestSearchRequest request)
        {
            var requests = await _complianceService.GetDataSubjectRequestsAsync(request.RequestType);
            
            var requestDtos = requests.Select(r => new DataSubjectRequestDto
            {
                Id = r.Id,
                RequestNumber = r.RequestNumber,
                RequestType = r.RequestType,
                SubjectName = r.SubjectName,
                SubjectEmail = r.SubjectEmail,
                RequestDescription = r.RequestDescription,
                RequestedAt = r.RequestedAt,
                ResponseDeadline = r.ResponseDeadline,
                Status = r.Status,
                AssignedTo = r.AssignedTo,
                CompletedAt = r.CompletedAt,
                IsVerified = r.IsVerified,
                IsComplexRequest = r.IsComplexRequest,
                ExtensionDays = r.ExtensionDays,
                DaysRemaining = r.ResponseDeadline.HasValue ? 
                    Math.Max(0, (r.ResponseDeadline.Value - DateTime.UtcNow).Days) : 0
            });

            return Ok(requestDtos);
        }

        [HttpPost("data-subject-requests")]
        public async Task<ActionResult<DataSubjectRequestDto>> CreateDataSubjectRequest([FromBody] CreateDataSubjectRequestRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dataSubjectRequest = new DataSubjectRequest
            {
                RequestType = request.RequestType,
                SubjectName = request.SubjectName,
                SubjectEmail = request.SubjectEmail,
                SubjectPhone = request.SubjectPhone,
                RequestDescription = request.RequestDescription,
                IdentityVerification = request.IdentityVerification,
                IsComplexRequest = request.IsComplexRequest,
                ProcessingLegalBasis = request.ProcessingLegalBasis
            };

            var createdRequest = await _complianceService.CreateDataSubjectRequestAsync(dataSubjectRequest);

            var requestDto = new DataSubjectRequestDto
            {
                Id = createdRequest.Id,
                RequestNumber = createdRequest.RequestNumber,
                RequestType = createdRequest.RequestType,
                SubjectName = createdRequest.SubjectName,
                SubjectEmail = createdRequest.SubjectEmail,
                Status = createdRequest.Status,
                RequestedAt = createdRequest.RequestedAt,
                ResponseDeadline = createdRequest.ResponseDeadline
            };

            return CreatedAtAction(nameof(GetDataSubjectRequests), requestDto);
        }

        [HttpPut("data-subject-requests/{id}/process")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator,PrivacyAnalyst")]
        public async Task<ActionResult<DataSubjectRequestDto>> ProcessDataSubjectRequest(Guid id, [FromBody] ProcessDataSubjectRequestRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var processedRequest = await _complianceService.ProcessDataSubjectRequestAsync(id, CurrentUserId);
                
                if (!string.IsNullOrEmpty(request.ResponseDetails))
                {
                    await _complianceService.CompleteDataSubjectRequestAsync(id, request.ResponseDetails, CurrentUserId);
                }

                var requestDto = new DataSubjectRequestDto
                {
                    Id = processedRequest.Id,
                    RequestNumber = processedRequest.RequestNumber,
                    Status = processedRequest.Status,
                    AssignedTo = processedRequest.AssignedTo
                };

                return Ok(requestDto);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [HttpPost("data-subject-requests/{id}/verify")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator,PrivacyAnalyst")]
        public async Task<ActionResult> VerifyDataSubjectIdentity(Guid id)
        {
            var result = await _complianceService.VerifyDataSubjectIdentityAsync(id, CurrentUserId);
            if (!result)
                return NotFound();

            return Ok(new { RequestId = id, VerifiedAt = DateTime.UtcNow, VerifiedBy = CurrentUserId });
        }

        [HttpGet("data-subject-requests/{id}/export")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator")]
        public async Task<ActionResult> ExportPersonalData(Guid id, [FromQuery] string subjectEmail)
        {
            if (string.IsNullOrEmpty(subjectEmail))
                return BadRequest("Subject email is required");

            var exportData = await _complianceService.ExportPersonalDataAsync(subjectEmail);
            return File(exportData, "application/json", $"personal-data-export-{DateTime.UtcNow:yyyyMMdd}.json");
        }

        // Data Retention Management
        [HttpGet("retention-policies")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator")]
        public async Task<ActionResult<IEnumerable<DataRetentionPolicyDto>>> GetRetentionPolicies()
        {
            var policies = await _complianceService.GetRetentionPoliciesAsync();
            
            var policyDtos = policies.Select(p => new DataRetentionPolicyDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                DataCategory = p.DataCategory,
                ApplicableFramework = p.ApplicableFramework,
                RetentionPeriodDays = p.RetentionPeriodDays,
                RetentionAction = p.RetentionAction,
                LegalBasis = p.LegalBasis,
                IsActive = p.IsActive,
                EffectiveDate = p.EffectiveDate,
                ResponsibleRole = p.ResponsibleRole
            });

            return Ok(policyDtos);
        }

        [HttpPost("retention-policies")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator")]
        public async Task<ActionResult<DataRetentionPolicyDto>> CreateRetentionPolicy([FromBody] CreateRetentionPolicyRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var policy = new DataRetentionPolicy
            {
                Name = request.Name,
                Description = request.Description,
                DataCategory = request.DataCategory,
                ApplicableFramework = request.ApplicableFramework,
                RetentionPeriodDays = request.RetentionPeriodDays,
                RetentionCriteria = request.RetentionCriteria,
                RetentionAction = request.RetentionAction,
                LegalBasis = request.LegalBasis,
                EffectiveDate = request.EffectiveDate,
                ResponsibleRole = request.ResponsibleRole
            };

            var createdPolicy = await _complianceService.CreateRetentionPolicyAsync(policy, CurrentUserId);

            var policyDto = new DataRetentionPolicyDto
            {
                Id = createdPolicy.Id,
                Name = createdPolicy.Name,
                DataCategory = createdPolicy.DataCategory,
                RetentionPeriodDays = createdPolicy.RetentionPeriodDays,
                RetentionAction = createdPolicy.RetentionAction
            };

            return CreatedAtAction(nameof(GetRetentionPolicies), policyDto);
        }

        [HttpGet("retention-records/expiring")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator")]
        public async Task<ActionResult<IEnumerable<DataRetentionRecordDto>>> GetExpiringData([FromQuery] int daysFromNow = 30)
        {
            var records = await _complianceService.GetExpiringDataAsync(daysFromNow);
            
            var recordDtos = records.Select(r => new DataRetentionRecordDto
            {
                Id = r.Id,
                PolicyName = r.Policy?.Name,
                DataIdentifier = r.DataIdentifier,
                DataLocation = r.DataLocation,
                DataCreated = r.DataCreated,
                RetentionExpiry = r.RetentionExpiry,
                ScheduledAction = r.ScheduledAction,
                Status = r.Status,
                ActionTaken = r.ActionTaken,
                ActionResult = r.ActionResult,
                DaysUntilExpiry = Math.Max(0, (r.RetentionExpiry - DateTime.UtcNow).Days)
            });

            return Ok(recordDtos);
        }

        [HttpPost("retention/process-schedule")]
        [Authorize(Roles = "ComplianceOfficer,Administrator")]
        public async Task<ActionResult> ProcessRetentionSchedule()
        {
            var result = await _complianceService.ProcessRetentionScheduleAsync();
            return Ok(new { Success = result, ProcessedAt = DateTime.UtcNow });
        }

        // Privacy Impact Assessments
        [HttpGet("privacy-impact-assessments")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator,PrivacyAnalyst")]
        public async Task<ActionResult<IEnumerable<PrivacyImpactAssessmentDto>>> GetPrivacyImpactAssessments([FromQuery] ComplianceStatus? status = null)
        {
            var assessments = await _complianceService.GetPIAsAsync(status);
            
            var assessmentDtos = assessments.Select(a => new PrivacyImpactAssessmentDto
            {
                Id = a.Id,
                Title = a.Title,
                ProjectName = a.ProjectName,
                ProjectDescription = a.ProjectDescription,
                DataController = a.DataController,
                AssessmentDate = a.AssessmentDate,
                ConductedBy = a.ConductedBy,
                Status = a.Status,
                InvolvesSpecialCategories = a.InvolvesSpecialCategories,
                InvolvesChildren = a.InvolvesChildren,
                InvolvesProfiling = a.InvolvesProfiling,
                RiskScore = a.RiskScore,
                ReviewDate = a.ReviewDate,
                RequiresMonitoring = a.RequiresMonitoring,
                StakeholderCount = a.Stakeholders?.Count ?? 0,
                RiskCount = a.Risks?.Count ?? 0
            });

            return Ok(assessmentDtos);
        }

        [HttpPost("privacy-impact-assessments")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator,PrivacyAnalyst")]
        public async Task<ActionResult<PrivacyImpactAssessmentDto>> CreatePrivacyImpactAssessment([FromBody] CreatePIARequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var pia = new PrivacyImpactAssessment
            {
                Title = request.Title,
                ProjectName = request.ProjectName,
                ProjectDescription = request.ProjectDescription,
                DataController = request.DataController,
                DataProcessor = request.DataProcessor,
                DataTypes = request.DataTypes,
                ProcessingPurpose = request.ProcessingPurpose,
                LegalBasis = request.LegalBasis,
                DataSources = request.DataSources,
                DataRecipients = request.DataRecipients,
                InvolvesSpecialCategories = request.InvolvesSpecialCategories,
                InvolvesChildren = request.InvolvesChildren,
                InvolvesProfiling = request.InvolvesProfiling,
                InvolvesAutomatedDecisions = request.InvolvesAutomatedDecisions,
                PrivacyRisks = request.PrivacyRisks,
                RiskMitigation = request.RiskMitigation
            };

            var createdPIA = await _complianceService.CreatePIAAsync(pia, CurrentUserId);

            var piaDto = new PrivacyImpactAssessmentDto
            {
                Id = createdPIA.Id,
                Title = createdPIA.Title,
                ProjectName = createdPIA.ProjectName,
                Status = createdPIA.Status,
                AssessmentDate = createdPIA.AssessmentDate,
                ConductedBy = createdPIA.ConductedBy
            };

            return CreatedAtAction(nameof(GetPrivacyImpactAssessments), piaDto);
        }

        // Compliance Assessments
        [HttpGet("assessments")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator")]
        public async Task<ActionResult<IEnumerable<ComplianceAssessmentDto>>> GetAssessments([FromQuery] ComplianceFramework? framework = null)
        {
            var assessments = await _complianceService.GetAssessmentsAsync(framework);
            
            var assessmentDtos = assessments.Select(a => new ComplianceAssessmentDto
            {
                Id = a.Id,
                AssessmentName = a.AssessmentName,
                Framework = a.Framework,
                PolicyName = a.Policy?.Name,
                AssessmentDate = a.AssessmentDate,
                AssessorName = a.AssessorName,
                AssessorOrganization = a.AssessorOrganization,
                OverallStatus = a.OverallStatus,
                ComplianceScore = a.ComplianceScore,
                Methodology = a.Methodology,
                Scope = a.Scope,
                NextAssessmentDue = a.NextAssessmentDue,
                CertificationStatus = a.CertificationStatus,
                CertificationExpiry = a.CertificationExpiry
            });

            return Ok(assessmentDtos);
        }

        [HttpPost("assessments")]
        [Authorize(Roles = "ComplianceOfficer,Administrator")]
        public async Task<ActionResult<ComplianceAssessmentDto>> CreateAssessment([FromBody] CreateAssessmentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var assessment = new ComplianceAssessment
            {
                AssessmentName = request.AssessmentName,
                Framework = request.Framework,
                PolicyId = request.PolicyId,
                AssessorOrganization = request.AssessorOrganization,
                Methodology = request.Methodology,
                Scope = request.Scope,
                NextAssessmentDue = request.NextAssessmentDue,
                OverallStatus = ComplianceStatus.UnderReview,
                ComplianceScore = 0
            };

            var createdAssessment = await _complianceService.CreateAssessmentAsync(assessment, CurrentUserId);

            var assessmentDto = new ComplianceAssessmentDto
            {
                Id = createdAssessment.Id,
                AssessmentName = createdAssessment.AssessmentName,
                Framework = createdAssessment.Framework,
                AssessmentDate = createdAssessment.AssessmentDate,
                AssessorName = createdAssessment.AssessorName,
                OverallStatus = createdAssessment.OverallStatus
            };

            return CreatedAtAction(nameof(GetAssessments), assessmentDto);
        }

        // Breach Management
        [HttpPost("breaches/report")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator,SecurityAnalyst")]
        public async Task<ActionResult> ReportBreach([FromBody] BreachReportRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _complianceService.ReportDataBreachAsync(
                request.Description, 
                request.Severity, 
                request.AffectedRecordCount, 
                CurrentUserId);

            return Ok(new { Success = result, ReportedAt = DateTime.UtcNow, ReportedBy = CurrentUserId });
        }

        [HttpGet("breaches/requiring-notification")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator")]
        public async Task<ActionResult<IEnumerable<ComplianceViolationDto>>> GetBreachesRequiringNotification()
        {
            var breaches = await _complianceService.GetBreachesRequiringNotificationAsync();
            
            var breachDtos = breaches.Select(b => new ComplianceViolationDto
            {
                Id = b.Id,
                Title = b.Title,
                Severity = b.Severity,
                Framework = b.Framework,
                DetectedAt = b.DetectedAt,
                NotificationDeadline = b.NotificationDeadline,
                AffectedRecordCount = b.AffectedRecordCount,
                RequiresNotification = b.RequiresNotification,
                IsNotified = b.IsNotified,
                DaysOpen = (DateTime.UtcNow - b.DetectedAt).Days
            });

            return Ok(breachDtos);
        }

        [HttpPost("breaches/{violationId}/notify")]
        [Authorize(Roles = "ComplianceOfficer,DataProtectionOfficer,Administrator")]
        public async Task<ActionResult> NotifyBreach(Guid violationId, [FromBody] BreachNotificationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = true;

            if (request.NotifyRegulatoryAuthorities)
            {
                result &= await _complianceService.NotifyRegulatoryAuthoritiesAsync(violationId);
            }

            if (request.NotifyDataSubjects)
            {
                result &= await _complianceService.NotifyDataSubjectsAsync(violationId);
            }

            return Ok(new { Success = result, NotifiedAt = DateTime.UtcNow });
        }

        // Configuration
        [HttpGet("configuration")]
        [Authorize(Roles = "ComplianceOfficer,Administrator")]
        public async Task<ActionResult<ComplianceConfigurationDto>> GetConfiguration()
        {
            var config = await _complianceService.GetConfigurationAsync();
            
            var configDto = new ComplianceConfigurationDto
            {
                EnableGDPRCompliance = config.EnableGDPRCompliance,
                EnableHIPAACompliance = config.EnableHIPAACompliance,
                EnableSOXCompliance = config.EnableSOXCompliance,
                EnablePCICompliance = config.EnablePCICompliance,
                EnableAutomaticViolationDetection = config.EnableAutomaticViolationDetection,
                EnableDataSubjectRights = config.EnableDataSubjectRights,
                EnableDataRetentionManagement = config.EnableDataRetentionManagement,
                EnablePrivacyImpactAssessments = config.EnablePrivacyImpactAssessments,
                DataSubjectRequestTimeoutDays = config.DataSubjectRequestTimeoutDays,
                BreachNotificationHours = config.BreachNotificationHours,
                ComplianceAssessmentFrequencyDays = config.ComplianceAssessmentFrequencyDays,
                DataRetentionCheckFrequencyDays = config.DataRetentionCheckFrequencyDays,
                DefaultDataController = config.DefaultDataController,
                DefaultDataProtectionOfficer = config.DefaultDataProtectionOfficer,
                ComplianceEmail = config.ComplianceEmail,
                OrganizationName = config.OrganizationName,
                OrganizationAddress = config.OrganizationAddress,
                FrameworkSettings = config.FrameworkSettings
            };

            return Ok(configDto);
        }

        [HttpPut("configuration")]
        [Authorize(Roles = "ComplianceOfficer,Administrator")]
        public async Task<ActionResult> UpdateConfiguration([FromBody] ComplianceConfigurationDto configDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var config = new ComplianceConfiguration
            {
                EnableGDPRCompliance = configDto.EnableGDPRCompliance,
                EnableHIPAACompliance = configDto.EnableHIPAACompliance,
                EnableSOXCompliance = configDto.EnableSOXCompliance,
                EnablePCICompliance = configDto.EnablePCICompliance,
                EnableAutomaticViolationDetection = configDto.EnableAutomaticViolationDetection,
                EnableDataSubjectRights = configDto.EnableDataSubjectRights,
                EnableDataRetentionManagement = configDto.EnableDataRetentionManagement,
                EnablePrivacyImpactAssessments = configDto.EnablePrivacyImpactAssessments,
                DataSubjectRequestTimeoutDays = configDto.DataSubjectRequestTimeoutDays,
                BreachNotificationHours = configDto.BreachNotificationHours,
                ComplianceAssessmentFrequencyDays = configDto.ComplianceAssessmentFrequencyDays,
                DataRetentionCheckFrequencyDays = configDto.DataRetentionCheckFrequencyDays,
                DefaultDataController = configDto.DefaultDataController,
                DefaultDataProtectionOfficer = configDto.DefaultDataProtectionOfficer,
                ComplianceEmail = configDto.ComplianceEmail,
                OrganizationName = configDto.OrganizationName,
                OrganizationAddress = configDto.OrganizationAddress,
                FrameworkSettings = configDto.FrameworkSettings ?? new Dictionary<string, string>()
            };

            var result = await _complianceService.UpdateConfigurationAsync(config, CurrentUserId);
            return Ok(new { Success = result, UpdatedAt = DateTime.UtcNow });
        }
    }
}