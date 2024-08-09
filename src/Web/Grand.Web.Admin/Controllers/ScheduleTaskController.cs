﻿using AutoMapper;
using Grand.Business.Common.Services.Directory;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Business.Core.Interfaces.Common.Stores;
using Grand.Business.Core.Interfaces.System.ScheduleTasks;
using Grand.Business.Core.Utilities.Common.Security;
using Grand.Domain.Tasks;
using Grand.Infrastructure.Models;
using Grand.Web.Admin.Extensions.Mapping;
using Grand.Web.Admin.Models.Tasks;
using Grand.Web.Common.DataSource;
using Grand.Web.Common.Filters;
using Grand.Web.Common.Security.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Grand.Web.Common.Extensions;

namespace Grand.Web.Admin.Controllers;

[PermissionAuthorize(PermissionSystemName.ScheduleTasks)]
public class ScheduleTaskController : BaseAdminController
{
    #region Constructors

    public ScheduleTaskController(
        IScheduleTaskService scheduleTaskService,
        ITranslationService translationService,
        IDateTimeService dateTimeService,
        IStoreService storeService,
        IMapper mapper)
    {
        _scheduleTaskService = scheduleTaskService;
        _translationService = translationService;
        _dateTimeService = dateTimeService;
        _storeService = storeService;
        _mapper = mapper;
    }

    #endregion

    #region Fields

    private readonly IScheduleTaskService _scheduleTaskService;
    private readonly ITranslationService _translationService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IStoreService _storeService;
    private readonly IMapper _mapper;

    #endregion

    #region Utility

    [NonAction]
    protected virtual ScheduleTaskModel PrepareScheduleTaskModel(ScheduleTask task)
    {
        var model = _mapper.Map<ScheduleTaskModel>(task);
        model.LastStartUtc = task.LastStartUtc.ConvertToUserTime(_dateTimeService);
        model.LastSuccessUtc = task.LastSuccessUtc.ConvertToUserTime(_dateTimeService);
        model.LastEndUtc = task.LastNonSuccessEndUtc.ConvertToUserTime(_dateTimeService);

        return model;
    }

    [NonAction]
    protected virtual async Task<ScheduleTaskModel> PrepareStores(ScheduleTaskModel model)
    {
        model.AvailableStores.Add(new SelectListItem
            { Text = _translationService.GetResource("Admin.Common.Select"), Value = "" });
        foreach (var s in await _storeService.GetAllStores())
            model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id });

        return model;
    }

    #endregion

    #region Methods

    public IActionResult Index()
    {
        return RedirectToAction("List");
    }

    public IActionResult List()
    {
        return View();
    }

    [PermissionAuthorizeAction(PermissionActionName.List)]
    [HttpPost]
    public async Task<IActionResult> List(DataSourceRequest command)
    {
        var models = (await _scheduleTaskService.GetAllTasks())
            .Select(PrepareScheduleTaskModel)
            .ToList();
        var gridModel = new DataSourceResult {
            Data = models,
            Total = models.Count
        };
        return Json(gridModel);
    }

    [PermissionAuthorizeAction(PermissionActionName.Preview)]
    [HttpGet]
    public async Task<IActionResult> EditScheduler(string id)
    {
        var task = await _scheduleTaskService.GetTaskById(id);

        var model = _mapper.Map<ScheduleTaskModel>(task);
        model.LastStartUtc = task.LastStartUtc.ConvertToUserTime(_dateTimeService);
        model.LastSuccessUtc = task.LastSuccessUtc.ConvertToUserTime(_dateTimeService);
        model.LastEndUtc = task.LastNonSuccessEndUtc.ConvertToUserTime(_dateTimeService);

        model = await PrepareStores(model);
        return View(model);
    }

    [PermissionAuthorizeAction(PermissionActionName.Edit)]
    [HttpPost]
    [HttpPost]
    [ArgumentNameFilter(KeyName = "save-continue", Argument = "continueEditing")]
    public async Task<IActionResult> EditScheduler(ScheduleTaskModel model, bool continueEditing)
    {
        var scheduleTask = await _scheduleTaskService.GetTaskById(model.Id);
        if (ModelState.IsValid)
        {
            scheduleTask = _mapper.Map(model, scheduleTask);
            await _scheduleTaskService.UpdateTask(scheduleTask);
            Success(_translationService.GetResource("Admin.System.ScheduleTasks.Updated"));
            if (continueEditing) return await EditScheduler(model.Id);
            return RedirectToAction("List");
        }

        model.ScheduleTaskName = scheduleTask.ScheduleTaskName;
        model.Type = scheduleTask.Type;
        model = await PrepareStores(model);
        Error(ModelState);

        return View(model);
    }

    [PermissionAuthorizeAction(PermissionActionName.Edit)]
    public async Task<IActionResult> RunNow(string id)
    {
        try
        {
            var scheduleTask = await _scheduleTaskService.GetTaskById(id);
            if (scheduleTask == null) throw new Exception("Schedule task cannot be loaded");
            var typeofTask = Type.GetType(scheduleTask.Type);
            var task = HttpContext.RequestServices.GetServices<IScheduleTask>()
                .FirstOrDefault(x => x.GetType() == typeofTask);
            if (task != null)
            {
                scheduleTask.LastStartUtc = DateTime.UtcNow;
                try
                {
                    await task.Execute();
                    scheduleTask.LastSuccessUtc = DateTime.UtcNow;
                    scheduleTask.LastNonSuccessEndUtc = null;
                    Success(_translationService.GetResource("Admin.System.ScheduleTasks.RunNow.Done"));
                }
                catch (Exception exc)
                {
                    scheduleTask.LastNonSuccessEndUtc = DateTime.UtcNow;
                    Success(_translationService.GetResource("Admin.System.ScheduleTasks.RunNow.Done"));

                    Error($"Error while running the {scheduleTask.ScheduleTaskName} schedule task {exc.Message}");
                }

                await _scheduleTaskService.UpdateTask(scheduleTask);
            }
            else
            {
                Error($"Task {typeofTask?.Name} has not been registered");
            }
        }
        catch (Exception exc)
        {
            Error(exc);
        }

        return RedirectToAction("List");
    }

    #endregion
}