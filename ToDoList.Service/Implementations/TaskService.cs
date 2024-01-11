﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ToDoList.DAL.Interfaces;
using ToDoList.Domain.Entity;
using ToDoList.Domain.Enums;
using ToDoList.Domain.Extensions;
using ToDoList.Domain.Filters.Task;
using ToDoList.Domain.Response;
using ToDoList.Domain.ViewModels.Task;
using ToDoList.Service.Interfaces;

namespace ToDoList.Service.Implementations;

public class TaskService : ITaskService
{
    private readonly IBaseRepository<TaskEntity> _taskRepository;
    private ILogger<TaskService> _logger;

    public TaskService(IBaseRepository<TaskEntity> taskRepository, ILogger<TaskService> logger)
    {
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<IBaseResponse<IEnumerable<TaskViewModel>>> GetCompletedTasks()
    {
        try
        {
            var tasks = await _taskRepository.GetAll()
                .Where(x => x.IsDone == true)
                .Where(x => x.Created == DateTime.Today)
                .Select(x => new TaskViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                }).ToListAsync();
            return new BaseResponse<IEnumerable<TaskViewModel>>()
            {
                Data = tasks,
                StatusCode = StatusCode.OK
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"[TaskService.GetCompletedTasks]: {e.Message}");
            return new BaseResponse<IEnumerable<TaskViewModel>>()
            {
                StatusCode = StatusCode.InternalServerError
            };
        }
    }

    public async Task<IBaseResponse<TaskEntity>> Create(CreateTaskViewModel model)
    {
        try
        {
            model.Validate();
            _logger.LogInformation($"Запрос на создание задачи - {model.Name}");

            var task = await _taskRepository.GetAll()
                .Where(x => x.Created.Date == DateTime.Today)
                .FirstOrDefaultAsync(x => x.Name == model.Name);
            if (task != null)
            {
                return new BaseResponse<TaskEntity>()
                {
                    Description = "Задача с таким названием уже есть",
                    StatusCode = StatusCode.TaskAlreadyExists,
                };
            }

            task = new TaskEntity()
            {
                Name = model.Name,
                Description = model.Description,
                Priority = model.Priority,
                Created = DateTime.Now,
                IsDone = false
            };
            await _taskRepository.Create(task);
            
            _logger.LogInformation($"Задача создалась: {task.Name} {task.Created}");
            return new BaseResponse<TaskEntity>()
            {
                StatusCode = StatusCode.OK,
                Description = "Задача создалась"
            };
        }
        catch (Exception e)
        {
           _logger.LogError(e, $"[TaskService.Create]: {e.Message}");
           return new BaseResponse<TaskEntity>()
           {
               Description = $"{e.Message}",
               StatusCode = StatusCode.InternalServerError
           };
        }
    }

    public async Task<IBaseResponse<bool>> EndTask(long id)
    {
        try
        {
            var task = await _taskRepository.GetAll().FirstOrDefaultAsync(x => x.Id == id);
            if (task == null)
            {
                return new BaseResponse<bool>()
                {
                    Description = "Задача не найдена",
                    StatusCode = StatusCode.TaskNotFound,
                };
            }

            task.IsDone = true;
            await _taskRepository.Update(task);

            return new BaseResponse<bool>()
            {
                Description = "Задача завершена",
                StatusCode = StatusCode.OK,
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"[TaskService.EndTask]: {e.Message}");
            return new BaseResponse<bool>()
            {
                Description = $"{e.Message}",
                StatusCode = StatusCode.InternalServerError
            };
        }
    }

    public async Task<IBaseResponse<IEnumerable<TaskViewModel>>> GetTasks(TaskFilter filter)
    {
        try
        {
            var tasks = await _taskRepository.GetAll()
                .Where(x => !x.IsDone)
                .WhereIf(!string.IsNullOrWhiteSpace(filter.Name), x => x.Name == filter.Name)
                .WhereIf(filter.Priority.HasValue, x => x.Priority == filter.Priority)
                .Select(x => new TaskViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    IsDone = x.IsDone == true ? "Готова" : "Не готова",
                    Priority = x.Priority.GetDisplayName(),
                    Created = x.Created.ToLongDateString()
                })
                .ToListAsync();

            return new BaseResponse<IEnumerable<TaskViewModel>>()
            {
                Data = tasks,
                StatusCode = StatusCode.OK,
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"[TaskService.Create]: {e.Message}");
            return new BaseResponse<IEnumerable<TaskViewModel>>()
            {
                Description = $"{e.Message}",
                StatusCode = StatusCode.InternalServerError
            };
        }
    }
}