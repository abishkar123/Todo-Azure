using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models
{
    public enum Priority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Urgent = 3
    }

    public class TodoItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public bool IsDone { get; set; }

        public bool IsImportant { get; set; }

        public Priority Priority { get; set; } = Priority.Medium;

        [Display(Name = "Category")]
        public string? Category { get; set; }

        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Completed")]
        public DateTime? CompletedAt { get; set; }

        // Computed properties for UI
        [BsonIgnore]
        public bool IsOverdue => !IsDone && DueDate.HasValue && DueDate.Value.Date < DateTime.Today;

        [BsonIgnore]
        public bool IsDueToday => !IsDone && DueDate.HasValue && DueDate.Value.Date == DateTime.Today;

        [BsonIgnore]
        public bool IsDueSoon => !IsDone && DueDate.HasValue && DueDate.Value.Date > DateTime.Today && DueDate.Value.Date <= DateTime.Today.AddDays(3);
    }

    public class TodoViewModel
    {
        public List<TodoItem> Todos { get; set; } = new();
        public int TotalCount { get; set; }
        public int CompletedCount { get; set; }
        public int PendingCount { get; set; }
        public int OverdueCount { get; set; }
        public int DueTodayCount { get; set; }
        public string? FilterStatus { get; set; }
        public string? FilterPriority { get; set; }
        public string? FilterCategory { get; set; }
        public List<string> Categories { get; set; } = new();
    }
}
