namespace TaskManagementApp.Models
{
    public class Priority
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public string Color { get; set; }
        public IEnumerable<ToDoTask> Tasks { get; set; }    
    }
}
