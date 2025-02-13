﻿using EmailHubApp.Database;
using EmailHubApp.Repository.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailHubApp.Repository.Base
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly EmailHubDB _db;

        public BaseRepository()
        {
            _db = new EmailHubDB();
        }
        public virtual async Task<IEnumerable<T>> GetAll()
        {
            return await _db.Set<T>().ToListAsync();
        }
        public virtual async Task<T> GetById(int? id)
        {
            return await _db.Set<T>().FindAsync(id);
        }
        public virtual async Task<bool> Create(T entity)
        {
            _db.Set<T>().Add(entity);
            return await _db.SaveChangesAsync() > 0;
        }
        public virtual async Task<bool> Remove(T entity)
        {
            _db.Set<T>().Remove(entity);
            return await _db.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> Update(T entity)
        {
            _db.Entry(entity).State = EntityState.Modified;
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
