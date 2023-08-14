using Microsoft.EntityFrameworkCore;
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
        return id == await _dbContext.Users.Where(u => u.Id == id && (includeBlocked || u.IsActive)).Select(u => u.Id).SingleOrDefaultAsync();
    }    
    
    public async Task<User?> GetById(int id, bool includeBlocked = false)
    {
        return await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == id && (includeBlocked || u.IsActive));
    }

    public async Task<List<User>?> GetAll()
    {
        return await _dbContext.Users.ToListAsync();
    }

    public async Task<User?> Create(UserDto userDto)
    {
        var newUser = new User
        {
            UserName = userDto.UserName,
            Password = userDto.Password
        };
        await _dbContext.Users.AddAsync(newUser);
        await _dbContext.SaveChangesAsync();

        return newUser;
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
    
    public async Task<User?> ChangingActivity(int userId, bool isActive)
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