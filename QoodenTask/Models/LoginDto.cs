using System.ComponentModel.DataAnnotations;

namespace QoodenTask.Models;

public class LoginDto
{
    public int UserId { get; set; }
    //[Range(4,8)]
    [MinLength(4)]
    [MaxLength(8)]
    public string Password { get; set; }
}