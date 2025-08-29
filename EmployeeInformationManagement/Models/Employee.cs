using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeInformationManagement.Models;

public class Employee
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EmployeeID { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string LastName { get; set; }

    [Required]
    [StringLength(100)]
    public string Email { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? Salary { get; set; }
}