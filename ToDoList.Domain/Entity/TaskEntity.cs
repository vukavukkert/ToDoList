
using ToDoList.Domain.Enums;

namespace ToDoList.Domain.Entity
{
    public class TaskEntity
    {
        public bool IsDone { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Priority Priority { get; set; }
        public DateTime Created { get; set; }
    }
}
