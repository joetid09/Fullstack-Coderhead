﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tabloid_Fullstack.Data;
using Tabloid_Fullstack.Models;
using Tabloid_Fullstack.Models.ViewModels;

namespace Tabloid_Fullstack.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _context;

        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PostSummary> Get()
        {
            return _context.Post
                .Include(p => p.Category)
                .Where(p => p.IsApproved)
                .Where(p => p.PublishDateTime <= DateTime.Now)
                .OrderByDescending(p => p.PublishDateTime)
                .Select(p => new PostSummary()
                {
                    Id = p.Id,
                    ImageLocation = p.ImageLocation,
                    Title = p.Title,
                    AuthorId = p.UserProfileId,
                    AuthorName = p.UserProfile.DisplayName,
                    AbbreviatedText = p.Content.Substring(0, 200),
                    PublishDateTime = p.PublishDateTime,
                    Category = p.Category,
                    Content = p.Content
                })
                .ToList();
        }

        public Post GetById(int id)
        {
            return _context.Post
                .Include(p => p.UserProfile)
                .Include(p => p.Category)
                .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
                .Where(p => p.Id == id)
                .FirstOrDefault();
        }

        public List<Post> GetByUserId(int id)
        {
            return _context.Post
               .Include(p => p.UserProfile)
               .Include(p => p.Category)
               .Include(p => p.PostTags)
               .ThenInclude(pt => pt.Tag)
               .Where(p => p.UserProfileId == id)
               .ToList();
        }

        public List<ReactionCount> GetReactionCounts(int postId)
        {
            return _context.Reaction
                .Select(r => new ReactionCount()
                {
                    Reaction = r,
                    Count = r.PostReactions.Count(pr => pr.PostId == postId)
                })
                .ToList();
        }

        public List<PostReaction> GetPostReactionsByPost(int postId)
        {
            return _context.PostReaction
                .Where(pr => pr.PostId == postId)
                .ToList();
        }

        public void Add(Post post)
        {
            _context.Add(post);
            _context.SaveChanges();
        }

        public void AddReaction(PostReaction postReaction)
        {
            _context.Add(postReaction);
            _context.SaveChanges();
        }

        public void Update(Post post)
        {
            _context.Entry(post).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var post = GetById(id);
            var reactions = _context.PostReaction.Where(pr => pr.PostId == post.Id).ToList();
            foreach (var r in reactions)
            {
                _context.PostReaction.Remove(r);
            }
            _context.Post.Remove(post);
            _context.SaveChanges();
        }
    }
}
