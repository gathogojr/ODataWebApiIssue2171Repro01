using System.ComponentModel.DataAnnotations;

namespace ODataWebApiIssue2171Repro01.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
