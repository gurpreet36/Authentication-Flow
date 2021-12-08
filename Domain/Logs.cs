using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationAPI.Domain
{
    public class Logs
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id{get;set;}
        public string Email { get; set; }
        [Required]
        public DateTime TimeLog { get; set; }
    }
}