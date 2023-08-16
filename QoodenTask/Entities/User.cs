﻿using System.ComponentModel.DataAnnotations;

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
    public string Role { get; set; }

    public bool IsActive { get; set; } = true;

    public List<Balance> Balances { get; set; }
    public List<Transaction> Transactions { get; set; }
}