﻿using GyMeCore.IRepo;
using GyMeCore.Models.Entities;
using GyMeCore.Models.Results;
using GyMeInfrastructure.Options;
using Microsoft.EntityFrameworkCore;

namespace GyMeInfrastructure.Repo;

public class CommentReactionRepo : ICommentReactionRepo
{
    private readonly GyMePostgresContext _gyMePostgresContext;

    public CommentReactionRepo(GyMePostgresContext gyMePostgresContext)
    {
        _gyMePostgresContext = gyMePostgresContext;
    }

    public IQueryable<CommentReaction> GetAll(Guid commentId)
        => _gyMePostgresContext.CommentReactions
            .Include(x => x.User)
            .Where(x => x.CommentId == commentId);
    
    public async Task<IEnumerable<ReactionsCountResult>> GetSpecificCommentReactionsCount(Guid commentId)
        => await _gyMePostgresContext.CommentReactions
            .Where(x => x.CommentId == commentId)
            .GroupBy(x => x.ReactionType)
            .Select(x => new ReactionsCountResult()
            {
                ReactionType = x.Key,
                Emoji = x.First().Emoji,
                Count = x.Count()
            }).ToListAsync();
    
    public async Task<int> GetCommentReactionsCount(Guid commentId)
        => await _gyMePostgresContext.CommentReactions
            .CountAsync(x => x.CommentId == commentId);

    public async Task<Dictionary<Guid, int>> GetCommentReactionsCount(IEnumerable<Guid> commentsId)
        => await _gyMePostgresContext.Comments
            .Where(x => commentsId.Contains(x.Id))
            .Include(x => x.CommentReactions)
            .ToDictionaryAsync(x => x.Id, x => x.CommentReactions.Count);
}