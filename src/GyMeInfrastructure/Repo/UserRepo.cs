﻿using GymAppCore.IRepo;
using GymAppCore.Models.Entities;
using GymAppCore.Models.Results;
using GymAppInfrastructure.Options;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;

namespace GymAppInfrastructure.Repo;

internal class UserRepo : IUserRepo
{
    private readonly GyMePostgresContext _gyMePostgresContext;
    public UserRepo(GyMePostgresContext gyMePostgresContext)
    {
        _gyMePostgresContext = gyMePostgresContext;
    }
    
    public async Task<User?> Get(Guid id)
        => await _gyMePostgresContext.Users.Include(x => x.ExtendedUser).FirstOrDefaultAsync(x => x.Id == id);
    
    public async Task<User?> GetOnlyValid(Guid id)
        => await _gyMePostgresContext.Users.Include(x => x.ExtendedUser).FirstOrDefaultAsync(x => x.Id == id && x.Valid);

    public async Task<User?> Get(string userName)
        => await _gyMePostgresContext.Users.Include(x => x.ExtendedUser).FirstOrDefaultAsync(x => x.UserName == userName);

    public async Task<List<User>> FindUsers(string key, int page, int size)
        => await _gyMePostgresContext.Users.Where(x => (x.FirstName + x.LastName).ToLower().Contains(key.ToLower().Trim()) || x.UserName.Contains(key) && x.Valid)
            .Skip(page*size)
            .Take(size)
            .Include(x => x.ExtendedUser)
            .ToListAsync();

    public async Task<List<User>> GetFriends(Guid id, int page, int size)
        => await _gyMePostgresContext.UserFriends.Where(x => x.UserId == id && x.FriendStatus == FriendStatus.Friend).Select(x => x.Friend)
            .Skip(page*size)
            .Take(size)
            .ToListAsync();

    public async Task<UserFriend?> GetFriend(Guid user, Guid friend)
        => await _gyMePostgresContext.UserFriends.FirstOrDefaultAsync(x => x.UserId == user);

    public async Task<IEnumerable<CommonFriendsResult>> GetCommonFriendsSortedByCount(Guid userId, int page, int size = 50)
        => await _gyMePostgresContext.UserFriends
            .Where(x => x.UserId == userId)
            .SelectMany(x => x.Friend.Friends)
            .Where(x => x.FriendId != userId && x.FriendStatus == FriendStatus.Friend)
            .Include(x => x.Friend.Friends)
            .ThenInclude(x => x.Friend.ExtendedUser)
            .GroupBy(x => x.Friend)
            .OrderBy(x => x.Count())
            .Skip(page*size)
            .Take(size)
            .Select(group => new CommonFriendsResult
            {
                User = group.Key,
                CommonFriendsCount = group.Count()
            })
            .ToListAsync();

    public async Task<bool> AddFriend(IEnumerable<UserFriend> userFriend)
    {
        await _gyMePostgresContext.UserFriends.AddRangeAsync(userFriend);
        return await UtilsRepo.SaveDatabaseChanges(_gyMePostgresContext);
    }

    public async Task<bool> RemoveFriend(UserFriend userFriend)
    {
        _gyMePostgresContext.UserFriends.Remove(userFriend);
        return await UtilsRepo.SaveDatabaseChanges(_gyMePostgresContext);
    }
    
    public async Task<bool> RemoveFriend(IEnumerable<UserFriend> userFriend)
    {
        _gyMePostgresContext.UserFriends.RemoveRange(userFriend);
        return await UtilsRepo.SaveDatabaseChanges(_gyMePostgresContext);
    }

    public async Task<bool> Update(User user)
    {
        _gyMePostgresContext.Users.Update(user);
        return await UtilsRepo.SaveDatabaseChanges(_gyMePostgresContext);
    }

    public async Task<bool> RemoveUser(User user)
    {
        _gyMePostgresContext.Users.Remove(user);
        return await UtilsRepo.SaveDatabaseChanges(_gyMePostgresContext);
    }
}