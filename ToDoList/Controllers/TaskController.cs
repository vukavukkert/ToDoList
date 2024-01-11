using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ToDoList.Domain.Filters.Task;
using ToDoList.Domain.ViewModels.Task;
using ToDoList.Service.Interfaces;

namespace ToDoList.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetCompletedTasks()
        {
            var result = await _taskService.GetCompletedTasks();
            return Json(new { data = result.Data });
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(CreateTaskViewModel model)
        {
            var response = await _taskService.Create(model);
            if (response.StatusCode == Domain.Enums.StatusCode.OK)
            {
                return Ok(new { description = response.Description });
            }
            return BadRequest(new {description = response.Description});
        }

        [HttpPost]
        public async Task<IActionResult> EndTask(long id)
        {
            var response = await _taskService.EndTask(id);
            if (response.StatusCode == Domain.Enums.StatusCode.OK)
            {
                return Ok(new { description = response.Description });
            }
            return BadRequest(new {description = response.Description});
        }
        
        [HttpPost]
        public async  Task<IActionResult> TaskHandler(TaskFilter filter)
        {
            var response = await _taskService.GetTasks(filter);
            return Json(new { data = response.Data});
        }
    }
}
