using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApp.Models
{
    public class ToDoTask
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public string ?Description { get; set; }
        
        public DateTime Deadline { get; set; } 

        public bool IsCompleted {  get; set; }
        [ForeignKey("priority")]
        [DisplayName("Priority")]
        public int  PriorityId {  get; set; }    
        public Priority ?priority { get; set; }  

        [ForeignKey("User")]
        public string ? UserId {  get; set; } 
        public IdentityUser? User { get; set; }  

    }
}
