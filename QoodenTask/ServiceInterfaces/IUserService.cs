﻿using QoodenTask.Models;

namespace QoodenTask.ServiceInterfaces;

public interface IUserService
{
    public Task<bool> CheckIfExistById(int id, bool includeBlocked = false);
    public Task<User?> GetById(int id, bool includeBlocked = false);
    public Task<List<User>?> GetAll();
    public Task<User?> Create(UserDto userDto);
    public void ChangePassword(User user, string newPassword);
    public Task<User> Update(User user);
    public Task<User?> Block(int userId);
    public Task<User?> Unblock(int userId);
}