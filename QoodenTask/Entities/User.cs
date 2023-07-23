using System.ComponentModel.DataAnnotations;

namespace QoodenTask.Models;

public class User
{
    public int Id { get; set; }
    [MinLength(4)]
    [MaxLength(8)]
    public string UserName { get; set; }
    [MinLength(4)]
    [MaxLength(8)]
    public string Password { get; set; }

    private string _role = "User";
    public string Role
    {
        get { return _role;}
        set
        {
            if (value == "Admin")
                _role = value;
        }
    }

    public bool IsActive { get; set; } = true;

    public List<Balance> Balances { get; set; }
    public List<Transaction> Transactions { get; set; }
}