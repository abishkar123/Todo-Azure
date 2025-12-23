using Microsoft.AspNetCore.Mvc;
using TodoApp.Models;
using TodoApp.Repositories;

namespace TodoApp.Controllers
{
    public class TodoController : Controller
    {
        private readonly ITodoRepository _repo;

        public TodoController(ITodoRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index(string? status, string? priority, string? category)
        {
            var allItems = await _repo.GetAllAsync();
            
            // Build view model with stats
            var viewModel = new TodoViewModel
            {
                TotalCount = allItems.Count,
                CompletedCount = allItems.Count(x => x.IsDone),
                PendingCount = allItems.Count(x => !x.IsDone),
                OverdueCount = allItems.Count(x => x.IsOverdue),
                DueTodayCount = allItems.Count(x => x.IsDueToday),
                Categories = allItems.Where(x => !string.IsNullOrEmpty(x.Category))
                                    .Select(x => x.Category!)
                                    .Distinct()
                                    .OrderBy(x => x)
                                    .ToList(),
                FilterStatus = status,
                FilterPriority = priority,
                FilterCategory = category
            };

            // Apply filters
            var filtered = allItems.AsEnumerable();
            
            if (!string.IsNullOrEmpty(status))
            {
                filtered = status switch
                {
                    "done" => filtered.Where(x => x.IsDone),
                    "pending" => filtered.Where(x => !x.IsDone),
                    "overdue" => filtered.Where(x => x.IsOverdue),
                    "today" => filtered.Where(x => x.IsDueToday),
                    "important" => filtered.Where(x => x.IsImportant),
                    _ => filtered
                };
            }

            if (!string.IsNullOrEmpty(priority) && Enum.TryParse<Priority>(priority, out var p))
            {
                filtered = filtered.Where(x => x.Priority == p);
            }

            if (!string.IsNullOrEmpty(category))
            {
                filtered = filtered.Where(x => x.Category == category);
            }

            // Sort: Incomplete first, then by priority (high to low), then by due date
            viewModel.Todos = filtered
                .OrderBy(x => x.IsDone)
                .ThenByDescending(x => x.Priority)
                .ThenBy(x => x.DueDate ?? DateTime.MaxValue)
                .ThenByDescending(x => x.CreatedAt)
                .ToList();

            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View(new TodoItem { DueDate = DateTime.Today.AddDays(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TodoItem item)
        {
            if (!ModelState.IsValid)
                return View(item);

            item.CreatedAt = DateTime.UtcNow;
            await _repo.CreateAsync(item);
            TempData["Success"] = "Task created successfully!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(string id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, TodoItem item)
        {
            if (!ModelState.IsValid) return View(item);
            await _repo.UpdateAsync(id, item);
            TempData["Success"] = "Task updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            await _repo.DeleteAsync(id);
            TempData["Success"] = "Task deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleDone(string id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();
            
            item.IsDone = !item.IsDone;
            item.CompletedAt = item.IsDone ? DateTime.UtcNow : null;
            await _repo.UpdateAsync(id, item);
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleImportant(string id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();
            
            item.IsImportant = !item.IsImportant;
            await _repo.UpdateAsync(id, item);
            
            return RedirectToAction(nameof(Index));
        }
    }
}
