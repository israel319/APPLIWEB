using Microsoft.EntityFrameworkCore;
using AppPlusPlus.Application.Common;
using AppPlusPlus.Application.DTOs.Prestations;
using AppPlusPlus.Application.Interfaces;
using AppPlusPlus.Application.Services.Finance;
using AppPlusPlus.Application.Services.Prestations;
using AppPlusPlus.Domain.Common;
using AppPlusPlus.Domain.Entities.Prestations;
using AppPlusPlus.Domain.Entities.Vente;
using AppPlusPlus.Domain.Enums;
using AppPlusPlus.Infrastructure.Persistence;

namespace AppPlusPlus.Infrastructure.QueryServices.Prestations;

public class ServiceManagementQueryService : IServiceManagementService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IClotureService _clotureService;
    private readonly INumberToWordsConverter _numberToWords;

    public ServiceManagementQueryService(
        IDbContextFactory<AppDbContext> dbFactory,
        IClotureService clotureService,
        INumberToWordsConverter numberToWords)
    {
        _dbFactory = dbFactory;
        _clotureService = clotureService;
        _numberToWords = numberToWords;
    }

    public async Task<List<ServiceProjectRowDto>> GetProjectsAsync(string? search = null)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var q = ctx.ServiceProjects
            .AsNoTracking()
            .Include(p => p.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(p => p.Title.Contains(s)
                || (p.Customer != null && p.Customer.CustomerName != null && p.Customer.CustomerName.Contains(s)));
        }

        var projects = await q.OrderByDescending(p => p.DateSys).ToListAsync();
        var ids = projects.Select(p => p.Id).ToList();

        var hoursByProject = await ctx.ServiceTimesheets
            .AsNoTracking()
            .Where(t => ids.Contains(t.ProjectId))
            .GroupBy(t => t.ProjectId)
            .Select(g => new
            {
                ProjectId = g.Key,
                Approved = g.Where(x => x.Status == (int)ServiceTimesheetStatus.Approved).Sum(x => x.Hours),
                Billable = g.Where(x => x.Status == (int)ServiceTimesheetStatus.Approved && x.FactId == null).Sum(x => x.Hours)
            })
            .ToDictionaryAsync(x => x.ProjectId);

        var taskCounts = await ctx.ServiceTasks
            .AsNoTracking()
            .Where(t => ids.Contains(t.ProjectId))
            .GroupBy(t => t.ProjectId)
            .Select(g => new { ProjectId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProjectId, x => x.Count);

        return projects.Select(p =>
        {
            hoursByProject.TryGetValue(p.Id, out var h);
            taskCounts.TryGetValue(p.Id, out var tc);
            return new ServiceProjectRowDto
            {
                Id = p.Id,
                Title = p.Title,
                CustomerName = p.Customer?.CustomerName,
                Status = p.Status,
                BillingMode = p.BillingMode,
                HourlyRate = p.HourlyRate,
                FlatAmount = p.FlatAmount,
                FlatInvoiced = p.FlatInvoiced,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                ApprovedHours = h?.Approved ?? 0,
                BillableHours = h?.Billable ?? 0,
                TaskCount = tc,
                DateSys = p.DateSys,
                User = p.User
            };
        }).ToList();
    }

    public async Task<ServiceProjectDetailDto?> GetProjectAsync(int id)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var p = await ctx.ServiceProjects
            .AsNoTracking()
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return null;

        var hours = await ctx.ServiceTimesheets
            .AsNoTracking()
            .Where(t => t.ProjectId == id)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Approved = g.Where(x => x.Status == (int)ServiceTimesheetStatus.Approved).Sum(x => x.Hours),
                Billable = g.Where(x => x.Status == (int)ServiceTimesheetStatus.Approved && x.FactId == null).Sum(x => x.Hours)
            })
            .FirstOrDefaultAsync();

        return new ServiceProjectDetailDto
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            CustomerId = p.CustomerId,
            CustomerName = p.Customer?.CustomerName,
            Status = p.Status,
            BillingMode = p.BillingMode,
            HourlyRate = p.HourlyRate,
            FlatAmount = p.FlatAmount,
            FlatInvoiced = p.FlatInvoiced,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            LocalisationId = p.LocalisationId,
            MoneyId = p.MoneyId,
            ApprovedHours = hours?.Approved ?? 0,
            BillableHours = hours?.Billable ?? 0
        };
    }

    public async Task<ServiceResult<int>> SaveProjectAsync(ServiceProjectDetailDto dto, string login)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return ServiceResult.Fail<int>("Le titre du projet est obligatoire.");

        await using var ctx = await _dbFactory.CreateDbContextAsync();

        ServiceProject entity;
        if (dto.Id > 0)
        {
            entity = await ctx.ServiceProjects.FirstOrDefaultAsync(p => p.Id == dto.Id)
                     ?? new ServiceProject();
            if (entity.Id == 0)
                return ServiceResult.Fail<int>("Projet introuvable.");
        }
        else
        {
            entity = new ServiceProject
            {
                User = login,
                Cumputer = Environment.MachineName,
                DateSys = DateTime.Now
            };
            ctx.ServiceProjects.Add(entity);
        }

        entity.Title = dto.Title.Trim();
        entity.Description = dto.Description?.Trim();
        entity.CustomerId = dto.CustomerId;
        entity.Status = dto.Status;
        entity.BillingMode = dto.BillingMode;
        entity.HourlyRate = dto.HourlyRate;
        entity.FlatAmount = dto.FlatAmount;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.LocalisationId = dto.LocalisationId;
        entity.MoneyId = dto.MoneyId ?? CurrencyDefaults.MoneyIdCdf;

        await ctx.SaveChangesAsync();
        return ServiceResult.Ok(entity.Id, "Projet enregistré.");
    }

    public async Task<List<ServiceTimesheetRowDto>> GetTimesheetsAsync(int? projectId = null, string? search = null)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var q = ctx.ServiceTimesheets
            .AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.Task)
            .AsQueryable();

        if (projectId.HasValue)
            q = q.Where(t => t.ProjectId == projectId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(t =>
                t.UserLogin.Contains(s)
                || (t.Description != null && t.Description.Contains(s))
                || (t.Project != null && t.Project.Title.Contains(s)));
        }

        return await q
            .OrderByDescending(t => t.WorkDate)
            .Select(t => new ServiceTimesheetRowDto
            {
                Id = t.Id,
                ProjectId = t.ProjectId,
                ProjectTitle = t.Project != null ? t.Project.Title : "",
                TaskId = t.TaskId,
                TaskTitle = t.Task != null ? t.Task.Title : null,
                UserLogin = t.UserLogin,
                WorkDate = t.WorkDate,
                Hours = t.Hours,
                Description = t.Description,
                Status = t.Status,
                FactId = t.FactId
            })
            .ToListAsync();
    }

    public async Task<ServiceResult<int>> SaveTimesheetAsync(ServiceTimesheet timesheet, string login)
    {
        if (timesheet.Hours <= 0)
            return ServiceResult.Fail<int>("Les heures doivent être supérieures à zéro.");

        await using var ctx = await _dbFactory.CreateDbContextAsync();

        ServiceTimesheet entity;
        if (timesheet.Id > 0)
        {
            entity = await ctx.ServiceTimesheets.FirstOrDefaultAsync(t => t.Id == timesheet.Id)
                     ?? new ServiceTimesheet();
            if (entity.Id == 0)
                return ServiceResult.Fail<int>("Feuille de temps introuvable.");
            if (entity.Status == (int)ServiceTimesheetStatus.Billed)
                return ServiceResult.Fail<int>("Cette feuille de temps est déjà facturée.");
        }
        else
        {
            entity = new ServiceTimesheet
            {
                User = login,
                DateSys = DateTime.Now,
                Status = (int)ServiceTimesheetStatus.Draft
            };
            ctx.ServiceTimesheets.Add(entity);
        }

        if (entity.Status == (int)ServiceTimesheetStatus.Billed)
            return ServiceResult.Fail<int>("Modification impossible : déjà facturée.");

        var projectExists = await ctx.ServiceProjects.AnyAsync(p => p.Id == timesheet.ProjectId);
        if (!projectExists)
            return ServiceResult.Fail<int>("Projet introuvable.");

        entity.ProjectId = timesheet.ProjectId;
        entity.TaskId = timesheet.TaskId;
        entity.UserLogin = string.IsNullOrWhiteSpace(timesheet.UserLogin) ? login : timesheet.UserLogin.Trim();
        entity.WorkDate = timesheet.WorkDate;
        entity.Hours = Math.Round(timesheet.Hours, 3);
        entity.Description = timesheet.Description?.Trim();

        if (entity.Id == 0 || entity.Status == (int)ServiceTimesheetStatus.Draft)
            entity.Status = (int)ServiceTimesheetStatus.Draft;

        await ctx.SaveChangesAsync();
        return ServiceResult.Ok(entity.Id, "Feuille de temps enregistrée.");
    }

    public async Task<ServiceResult> ApproveTimesheetAsync(int timesheetId, string login)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var ts = await ctx.ServiceTimesheets.FindAsync(timesheetId);
        if (ts == null) return ServiceResult.Fail("Feuille de temps introuvable.");
        if (ts.Status == (int)ServiceTimesheetStatus.Billed)
            return ServiceResult.Fail("Déjà facturée.");
        if (ts.Status == (int)ServiceTimesheetStatus.Cancelled)
            return ServiceResult.Fail("Feuille de temps annulée.");

        ts.Status = (int)ServiceTimesheetStatus.Approved;
        await ctx.SaveChangesAsync();
        return ServiceResult.Ok("Heures validées.");
    }

    public async Task<ServiceResult> CancelTimesheetAsync(int timesheetId, string login)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var ts = await ctx.ServiceTimesheets.FindAsync(timesheetId);
        if (ts == null) return ServiceResult.Fail("Feuille de temps introuvable.");
        if (ts.Status == (int)ServiceTimesheetStatus.Billed)
            return ServiceResult.Fail("Impossible d'annuler : déjà facturée.");

        ts.Status = (int)ServiceTimesheetStatus.Cancelled;
        await ctx.SaveChangesAsync();
        return ServiceResult.Ok("Feuille de temps annulée.");
    }

    public async Task<List<ServiceTaskRowDto>> GetTasksAsync(int? projectId = null, string? search = null)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var q = ctx.ServiceTasks.AsNoTracking().Include(t => t.Project).AsQueryable();

        if (projectId.HasValue)
            q = q.Where(t => t.ProjectId == projectId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(t => t.Title.Contains(s) || (t.Project != null && t.Project.Title.Contains(s)));
        }

        var tasks = await q.OrderByDescending(t => t.DateSys).ToListAsync();
        var ids = tasks.Select(t => t.Id).ToList();
        var hours = await ctx.ServiceTimesheets
            .AsNoTracking()
            .Where(ts => ts.TaskId.HasValue && ids.Contains(ts.TaskId.Value))
            .GroupBy(ts => ts.TaskId!.Value)
            .Select(g => new { TaskId = g.Key, Total = g.Sum(x => x.Hours) })
            .ToDictionaryAsync(x => x.TaskId, x => x.Total);

        return tasks.Select(t => new ServiceTaskRowDto
        {
            Id = t.Id,
            ProjectId = t.ProjectId,
            ProjectTitle = t.Project?.Title ?? "",
            Title = t.Title,
            AssignedUser = t.AssignedUser,
            PlannedStart = t.PlannedStart,
            PlannedEnd = t.PlannedEnd,
            Status = t.Status,
            Priority = t.Priority,
            LoggedHours = hours.GetValueOrDefault(t.Id)
        }).ToList();
    }

    public async Task<ServiceResult<int>> SaveTaskAsync(ServiceTask task, string login)
    {
        if (string.IsNullOrWhiteSpace(task.Title))
            return ServiceResult.Fail<int>("Le titre de la tâche est obligatoire.");

        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ServiceTask entity;
        if (task.Id > 0)
        {
            entity = await ctx.ServiceTasks.FirstOrDefaultAsync(t => t.Id == task.Id) ?? new ServiceTask();
            if (entity.Id == 0) return ServiceResult.Fail<int>("Tâche introuvable.");
        }
        else
        {
            entity = new ServiceTask { User = login, DateSys = DateTime.Now };
            ctx.ServiceTasks.Add(entity);
        }

        entity.ProjectId = task.ProjectId;
        entity.Title = task.Title.Trim();
        entity.Description = task.Description?.Trim();
        entity.AssignedUser = task.AssignedUser?.Trim();
        entity.PlannedStart = task.PlannedStart;
        entity.PlannedEnd = task.PlannedEnd;
        entity.Status = task.Status;
        entity.Priority = task.Priority;

        await ctx.SaveChangesAsync();
        return ServiceResult.Ok(entity.Id, "Tâche enregistrée.");
    }

    public async Task<ServiceResult> DeleteTaskAsync(int taskId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var task = await ctx.ServiceTasks.FindAsync(taskId);
        if (task == null) return ServiceResult.Fail("Tâche introuvable.");

        var hasBilled = await ctx.ServiceTimesheets.AnyAsync(t =>
            t.TaskId == taskId && t.Status == (int)ServiceTimesheetStatus.Billed);
        if (hasBilled)
            return ServiceResult.Fail("Impossible de supprimer : heures déjà facturées sur cette tâche.");

        ctx.ServiceTasks.Remove(task);
        await ctx.SaveChangesAsync();
        return ServiceResult.Ok("Tâche supprimée.");
    }

    public async Task<List<ServiceDeliverableRowDto>> GetDeliverablesAsync(int? projectId = null, string? search = null)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var q = ctx.ServiceDeliverables.AsNoTracking().Include(d => d.Project).AsQueryable();

        if (projectId.HasValue)
            q = q.Where(d => d.ProjectId == projectId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(d => d.Title.Contains(s) || (d.Project != null && d.Project.Title.Contains(s)));
        }

        return await q
            .OrderBy(d => d.DueDate)
            .Select(d => new ServiceDeliverableRowDto
            {
                Id = d.Id,
                ProjectId = d.ProjectId,
                ProjectTitle = d.Project != null ? d.Project.Title : "",
                Title = d.Title,
                DueDate = d.DueDate,
                DeliveredDate = d.DeliveredDate,
                Status = d.Status
            })
            .ToListAsync();
    }

    public async Task<ServiceResult<int>> SaveDeliverableAsync(ServiceDeliverable deliverable, string login)
    {
        if (string.IsNullOrWhiteSpace(deliverable.Title))
            return ServiceResult.Fail<int>("Le titre du livrable est obligatoire.");

        await using var ctx = await _dbFactory.CreateDbContextAsync();
        ServiceDeliverable entity;
        if (deliverable.Id > 0)
        {
            entity = await ctx.ServiceDeliverables.FirstOrDefaultAsync(d => d.Id == deliverable.Id)
                     ?? new ServiceDeliverable();
            if (entity.Id == 0) return ServiceResult.Fail<int>("Livrable introuvable.");
        }
        else
        {
            entity = new ServiceDeliverable { User = login, DateSys = DateTime.Now };
            ctx.ServiceDeliverables.Add(entity);
        }

        entity.ProjectId = deliverable.ProjectId;
        entity.TaskId = deliverable.TaskId;
        entity.Title = deliverable.Title.Trim();
        entity.Description = deliverable.Description?.Trim();
        entity.DueDate = deliverable.DueDate;
        entity.DeliveredDate = deliverable.DeliveredDate;
        entity.Status = deliverable.Status;

        await ctx.SaveChangesAsync();
        return ServiceResult.Ok(entity.Id, "Livrable enregistré.");
    }

    public async Task<ServiceResult> DeleteDeliverableAsync(int deliverableId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var d = await ctx.ServiceDeliverables.FindAsync(deliverableId);
        if (d == null) return ServiceResult.Fail("Livrable introuvable.");
        ctx.ServiceDeliverables.Remove(d);
        await ctx.SaveChangesAsync();
        return ServiceResult.Ok("Livrable supprimé.");
    }

    public async Task<ServiceResult<int>> GenerateInvoiceFromProjectAsync(int projectId, string login)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var periode = await ctx.Periodes.FirstOrDefaultAsync(p => p.Activated == true);
        if (periode?.FromDate == null || periode.ToDate == null)
            return ServiceResult.Fail<int>("Aucune période active n'est définie.");

        var today = DateTime.Today;
        if (today < periode.FromDate.Value.Date || today > periode.ToDate.Value.Date)
            return ServiceResult.Fail<int>("La date du jour n'est pas dans la période active.");

        var todayOnly = DateOnly.FromDateTime(today);
        var userLocIds = await ctx.UserLocalisations
            .Where(ul => ul.UserId == login && ul.LocalisationId.HasValue)
            .Select(ul => ul.LocalisationId!.Value)
            .ToListAsync();
        var hasClosed = await ctx.Versements.AnyAsync(v =>
            v.UserLogin == login
            && v.DateCloture == todayOnly
            && userLocIds.Contains(v.LocalisationId)
            && v.StatutCloture == 0);
        if (hasClosed)
            return ServiceResult.Fail<int>("Clôture en attente d'approbation admin — facturation impossible.");

        using var transaction = await ctx.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            var project = await ctx.ServiceProjects
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null)
                return ServiceResult.Fail<int>("Projet introuvable.");

            var timesheets = await ctx.ServiceTimesheets
                .Include(t => t.Task)
                .Where(t => t.ProjectId == projectId
                    && t.Status == (int)ServiceTimesheetStatus.Approved
                    && t.FactId == null)
                .ToListAsync();

            var billingMode = (ServiceBillingMode)project.BillingMode;
            var includeFlat = (billingMode is ServiceBillingMode.Flat or ServiceBillingMode.Mixed)
                && !project.FlatInvoiced
                && project.FlatAmount is > 0;

            var includeHourly = billingMode is ServiceBillingMode.Hourly or ServiceBillingMode.Mixed
                && timesheets.Count > 0
                && project.HourlyRate is > 0;

            if (billingMode == ServiceBillingMode.Flat && !includeFlat)
                return ServiceResult.Fail<int>("Le forfait de ce projet a déjà été facturé.");

            if (!includeFlat && !includeHourly && timesheets.Count == 0)
                return ServiceResult.Fail<int>("Aucune heure validée ni forfait à facturer.");

            if (billingMode == ServiceBillingMode.Hourly && !includeHourly)
                return ServiceResult.Fail<int>("Définissez un tarif horaire ou validez des heures.");

            var taux = await ctx.TauxChanges.OrderByDescending(t => t.Id).FirstOrDefaultAsync();
            var tauxValue = taux?.TauxValue ?? 1m;
            var customer = project.CustomerId.HasValue
                ? await ctx.Customers.FindAsync(project.CustomerId.Value)
                : null;

            decimal total = 0;
            var lineDefs = new List<(string Label, double Qte, double Pu)>();

            if (includeFlat)
            {
                var flat = project.FlatAmount!.Value;
                total += flat;
                lineDefs.Add(($"Forfait — {project.Title}", 1, (double)flat));
            }

            if (includeHourly)
            {
                var rate = project.HourlyRate!.Value;
                var groups = timesheets
                    .GroupBy(t => t.TaskId)
                    .ToList();

                foreach (var g in groups)
                {
                    var hours = g.Sum(x => x.Hours);
                    if (hours <= 0) continue;
                    var taskTitle = g.First().Task?.Title;
                    var label = taskTitle != null
                        ? $"Prestation — {project.Title} — {taskTitle}"
                        : $"Prestation — {project.Title}";
                    var lineTotal = hours * rate;
                    total += lineTotal;
                    lineDefs.Add((label, (double)hours, (double)rate));
                }
            }

            if (lineDefs.Count == 0)
                return ServiceResult.Fail<int>("Aucune ligne à facturer.");

            var isUsd = project.MoneyId.HasValue
                && await ctx.Moneys.AsNoTracking().AnyAsync(m =>
                    m.IdMonais == project.MoneyId && m.DescriptionMonais == "USD");
            var totalRounded = CurrencyFormat.RoundInvoicePayable(total, isUsd);

            var facture = new Fact
            {
                DescriptionName = customer?.CustomerName ?? "Client Anonyme",
                DescriptionArticle = string.Join(" | ", lineDefs.Select(l => l.Label)),
                Adresse = customer?.Adress,
                Email = customer?.Email,
                Telephone = customer?.Contact,
                Type = 1,
                Date = DateOnly.FromDateTime(today),
                Total = total,
                TotalApresReduction = totalRounded,
                Reduction = 0,
                TotalReduit = 0,
                Taux = tauxValue,
                Status = 0,
                DateSys = DateTime.Now,
                User = login,
                Cumputer = Environment.MachineName,
                CustomerId = project.CustomerId,
                ServiceProjectId = projectId,
                MoneyId = project.MoneyId ?? CurrencyDefaults.MoneyIdCdf
            };

            ctx.Facts.Add(facture);
            await ctx.SaveChangesAsync();

            foreach (var (label, qte, pu) in lineDefs)
            {
                var detail = new FactDetail
                {
                    IdFact = facture.Id,
                    IdArticle = null,
                    DescriptionLine = label,
                    Qte = qte,
                    Pu = pu,
                    Status = 0,
                    Localisationid = project.LocalisationId
                };
                var moneyId = project.MoneyId ?? CurrencyDefaults.MoneyIdCdf;
                var amounts = CurrencyFormat.ComputeMonetarySnapshot(
                    (decimal)pu, (decimal)qte, moneyId, tauxValue, puStoredInCdf: false);
                CurrencyFormat.ApplySnapshot(detail, amounts);
                ctx.FactDetails.Add(detail);
            }

            foreach (var ts in timesheets)
            {
                ts.Status = (int)ServiceTimesheetStatus.Billed;
                ts.FactId = facture.Id;
            }

            if (includeFlat)
                project.FlatInvoiced = true;

            var remaining = await ctx.ServiceTimesheets.AnyAsync(t =>
                t.ProjectId == projectId
                && t.Status == (int)ServiceTimesheetStatus.Approved
                && t.FactId == null);
            if (!remaining && (project.FlatInvoiced || billingMode != ServiceBillingMode.Flat))
                project.Status = (int)ServiceProjectStatus.Invoiced;
            else if (project.Status == (int)ServiceProjectStatus.Draft)
                project.Status = (int)ServiceProjectStatus.Active;

            await ctx.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult.Ok(facture.Id, "Facture brouillon créée.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ServiceResult.Fail<int>($"Erreur lors de la génération : {ex.Message}");
        }
    }

    public async Task<List<ServiceInvoiceRowDto>> GetServiceInvoicesAsync(int? projectId = null, string? search = null)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var q = ctx.Facts
            .AsNoTracking()
            .Include(f => f.ServiceProject)
            .Where(f => f.ServiceProjectId != null);

        if (projectId.HasValue)
            q = q.Where(f => f.ServiceProjectId == projectId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(f =>
                f.DescriptionName.Contains(s)
                || (f.ServiceProject != null && f.ServiceProject.Title.Contains(s)));
        }

        return await q
            .OrderByDescending(f => f.Date)
            .ThenByDescending(f => f.Id)
            .Select(f => new ServiceInvoiceRowDto
            {
                Id = f.Id,
                ProjectId = f.ServiceProjectId!.Value,
                ProjectTitle = f.ServiceProject != null ? f.ServiceProject.Title : "",
                ClientName = f.DescriptionName,
                Date = f.Date,
                Total = f.TotalApresReduction ?? f.Total ?? 0,
                Status = f.Status,
                User = f.User
            })
            .ToListAsync();
    }

    public async Task<ServiceInvoiceDetailDto?> GetServiceInvoiceAsync(int factId)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var fact = await ctx.Facts
            .AsNoTracking()
            .Include(f => f.ServiceProject)
            .Include(f => f.Money)
            .Include(f => f.Details)
            .FirstOrDefaultAsync(f => f.Id == factId && f.ServiceProjectId != null);

        if (fact == null) return null;

        var isUsd = fact.Money?.DescriptionMonais == "USD";
        return new ServiceInvoiceDetailDto
        {
            Id = fact.Id,
            ProjectId = fact.ServiceProjectId!.Value,
            ProjectTitle = fact.ServiceProject?.Title ?? "",
            ClientName = fact.DescriptionName,
            Date = fact.Date,
            Total = fact.Total ?? 0,
            TotalApresReduction = fact.TotalApresReduction,
            Taux = fact.Taux,
            MoneyId = fact.MoneyId,
            Status = fact.Status,
            IsUsd = isUsd,
            MontantEnLettres = fact.MontantEnLettres,
            Lines = fact.Details.Select(d => new ServiceInvoiceLineDto
            {
                Label = d.DescriptionLine ?? d.IdArticle ?? "—",
                Qte = d.Qte ?? 0,
                Pu = d.Pu ?? 0,
                Montant = d.Montant,
                MoneyId = d.MoneyId,
                Taux = d.Taux,
                MontantApresConversion = d.MontantApresConversion
            }).ToList()
        };
    }

    public async Task<ServiceResult> ValidateServiceInvoiceAsync(int factId, string login)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var check = await CheckBillingPeriodAndClotureAsync(ctx, login);
        if (check != null) return check;

        var fact = await ctx.Facts
            .Include(f => f.Money)
            .FirstOrDefaultAsync(f => f.Id == factId && f.ServiceProjectId != null);

        if (fact == null)
            return ServiceResult.Fail("Facture prestation introuvable.");

        if (fact.Status != 0)
            return ServiceResult.Fail("Seules les factures brouillon peuvent être validées.");

        var payable = fact.TotalApresReduction ?? fact.Total ?? 0;
        var isUsd = fact.Money?.DescriptionMonais == "USD";
        var displayAmount = isUsd && fact.Taux > 0
            ? Math.Round(payable / fact.Taux, 2)
            : payable;

        fact.Status = 1;
        fact.MontantEnLettres = _numberToWords.Convert(
            displayAmount,
            isUsd ? "dollars" : "francs",
            isUsd ? "cents" : "centimes");

        await ctx.SaveChangesAsync();
        return ServiceResult.Ok("Facture prestation validée.");
    }

    public async Task<ServiceResult> DeleteServiceInvoiceDraftAsync(int factId, string login)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var fact = await ctx.Facts
            .Include(f => f.Details)
            .FirstOrDefaultAsync(f => f.Id == factId && f.ServiceProjectId != null);

        if (fact == null)
            return ServiceResult.Fail("Facture prestation introuvable.");

        if (fact.Status != 0)
            return ServiceResult.Fail("Seul un brouillon peut être supprimé.");

        var projectId = fact.ServiceProjectId!.Value;
        var hadFlatLine = fact.Details.Any(d =>
            d.DescriptionLine != null && d.DescriptionLine.StartsWith("Forfait", StringComparison.OrdinalIgnoreCase));

        var timesheets = await ctx.ServiceTimesheets
            .Where(t => t.FactId == factId)
            .ToListAsync();

        foreach (var ts in timesheets)
        {
            ts.Status = (int)ServiceTimesheetStatus.Approved;
            ts.FactId = null;
        }

        ctx.FactDetails.RemoveRange(fact.Details);
        ctx.Facts.Remove(fact);

        var project = await ctx.ServiceProjects.FindAsync(projectId);
        if (project != null)
        {
            if (hadFlatLine)
                project.FlatInvoiced = false;

            if (project.Status == (int)ServiceProjectStatus.Invoiced)
            {
                var stillBilled = await ctx.ServiceTimesheets.AnyAsync(t =>
                    t.ProjectId == projectId && t.Status == (int)ServiceTimesheetStatus.Billed);
                if (!stillBilled)
                    project.Status = (int)ServiceProjectStatus.Active;
            }
        }

        await ctx.SaveChangesAsync();
        return ServiceResult.Ok("Brouillon supprimé. Les heures sont à nouveau disponibles pour facturation.");
    }

    async Task<ServiceResult?> CheckBillingPeriodAndClotureAsync(AppDbContext ctx, string login)
    {
        var periode = await ctx.Periodes.FirstOrDefaultAsync(p => p.Activated == true);
        if (periode?.FromDate == null || periode.ToDate == null)
            return ServiceResult.Fail("Aucune période active n'est définie.");

        var today = DateTime.Today;
        if (today < periode.FromDate.Value.Date || today > periode.ToDate.Value.Date)
            return ServiceResult.Fail("La date du jour n'est pas dans la période active.");

        var userLocIds = await ctx.UserLocalisations
            .Where(ul => ul.UserId == login && ul.LocalisationId.HasValue)
            .Select(ul => ul.LocalisationId!.Value)
            .ToListAsync();

        if (await _clotureService.HasUserClosedTodayAsync(login, userLocIds))
            return ServiceResult.Fail("Journée clôturée — opération impossible.");

        return null;
    }
}
