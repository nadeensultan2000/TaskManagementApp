using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagementApp.Data;
using TaskManagementApp.Data.Migrations;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext context;
        public TaskController(ApplicationDbContext context)
        {
            this.context = context;
        }
        [HttpGet]
        public IActionResult Index(string filter="")
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            List<ToDoTask> tasks;
                switch (filter)
                {
                    case "completed":
                    tasks = context.Tasks.Include(p => p.priority).Where(u => u.UserId == claim.Value).Where(c => c.IsCompleted == true).ToList();
                    break;
                    case "notCompleted":
                    tasks = context.Tasks.Include(p => p.priority).Where(u => u.UserId == claim.Value).Where(c => c.IsCompleted == false).ToList();
                    break;
                    case "AllTasks":
                    tasks = context.Tasks.Include(p => p.priority).Where(u => u.UserId == claim.Value).ToList();
                    break; 

                    default :
                    tasks = context.Tasks.Include(p => p.priority).Where(u => u.UserId == claim.Value).ToList();
                    break;
                }
           return View(tasks);}

        public void CreateSelectedList(int selectedid=0)
        {
            IEnumerable<Priority> priorities = context.Priorities.ToList();
            SelectList items = new SelectList(priorities, "Id", "Name",selectedid);
            ViewBag.priorityList = items;
        }
        [HttpGet]
        public IActionResult Create()
        {
            CreateSelectedList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ToDoTask task)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim=claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            task.UserId = claim.Value;
           if (task.Deadline <= DateTime.Now)
      {
            ModelState.AddModelError("Deadline", "Deadline can't be before now");
     }
   
            if (ModelState.IsValid)
            {
                context.Tasks.Add(task);
                await context.SaveChangesAsync();
                return Json(new { success = true, message = "Task saved successfully.", redirectUrl = Url.Action("Index","Task") });
            }
            return Json(new { success = false, errors = ModelState.ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()) });
        }
        public IActionResult FilterTasks(int filterid)
        {


            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Update(int? id)
        {
            ToDoTask task = context.Tasks.Find(id);
            if (task == null) { return NotFound(); }
            else
            {
                CreateSelectedList();
                return View(task);
            }
        }
        [HttpPost,ActionName("Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ToDoTask task)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            task.UserId = claim.Value;
          if (task.Deadline <= DateTime.Now)
          {
              ModelState.AddModelError("Deadline", "Deadline can't be before now");
          }
            if (ModelState.IsValid)
            {
                context.Tasks.Update(task);
                await context.SaveChangesAsync();
                return Json(new { success = true, message = "Task updated successfully.", redirectUrl = Url.Action("Index", "Task") });

            }

            return Json(new { success = false, errors = ModelState.ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()) });

        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id) {
            var task = await context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            // Perform the delete operation
            context.Tasks.Remove(task);
            await context.SaveChangesAsync();

            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> CompleteTask(int id) {

            ToDoTask task =await context.Tasks.FindAsync(id);
            if (task == null)
            {
                return Json(new { success = false, message = "Task not found." });
            }
            task.IsCompleted = true;
            await context.SaveChangesAsync();
            return Json(new { success = true, message = "Task marked as completed." });
        }
        [HttpPost]
        public IActionResult RemoveCompletedTasks()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var complete_tasks =  context.Tasks.Where(u => u.UserId == claim.Value).Where(c => c.IsCompleted == true).ToList();
            if (complete_tasks.Count() == 0) {
                return Json(new { success = false, message = "There are no completed tasks to remove." });
            }
            else
            {
                context.Tasks.RemoveRange(complete_tasks);
               context.SaveChanges();
                return Json(new { success = true, message = $"{complete_tasks.Count()} completed tasks have been removed." });
            }
        }
        [HttpPost]
        public IActionResult RemoveTimeOutTasks()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var timeout_tasks = context.Tasks.Where(c => c.UserId == claim.Value).Where(c =>c.IsCompleted == false).Where(c => c.Deadline <= DateTime.Now).ToList();
            if (timeout_tasks.Count() == 0)
            {
                return Json(new { success = false, message = "There are no Time Out tasks to remove." });
            }
            else
            {
                context.Tasks.RemoveRange(timeout_tasks);
               context.SaveChanges();
                return Json(new { success = true, message = $"{timeout_tasks.Count()} Time Out tasks have been removed." });
            }
        }

    }
}
