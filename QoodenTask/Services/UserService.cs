﻿using Microsoft.EntityFrameworkCore;
using QoodenTask.Data;
using QoodenTask.Models;
using QoodenTask.ServiceInterfaces;

namespace QoodenTask.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<bool> CheckIfExistById(int id, bool includeBlocked = false)
    {
        return _dbContext.Users.Any(u => u.Id == id && (includeBlocked || u.IsActive));
    }    
    
    public async Task<User?> GetById(int id, bool includeBlocked = false)
    {
        return await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == id && (includeBlocked || u.IsActive));
    }

    public async Task<IList<User>?> GetAll()
    {
        return await _dbContext.Users.ToListAsync();
    }

    public async Task<User?> Create(UserDto userDto, string role)
    {
        var newUser = new User
        {
            UserName = userDto.UserName,
            Password = userDto.Password,
            Role = role
        };
        await _dbContext.Users.AddAsync(newUser);
        await _dbContext.SaveChangesAsync();

        return newUser;
    }

    public async void SetRole(User user, string newRole)
    {
        user.Role = newRole;
        await Update(user);
    }
    public async void ChangePassword(User user, string newPassword)
    {
        user.Password = newPassword;
        await Update(user);
    }

    public async Task<User> Update(User user)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User?> Block(int userId)
    {
        return await ChangingActivity(userId,false);
    }
    
    public async Task<User?> Unblock(int userId)
    {
        return await ChangingActivity(userId,true);
    }
    
    private async Task<User?> ChangingActivity(int userId, bool isActive)
    {
        var user = await GetById(userId, true);
        if (user != null)
        {
            user.IsActive = isActive;
            await Update(user);
        }
        return user;
    }
}