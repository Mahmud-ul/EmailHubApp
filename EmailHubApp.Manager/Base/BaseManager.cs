﻿using EmailHubApp.Manager.Contract;
using EmailHubApp.Repository.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailHubApp.Manager.Base
{
    public abstract class BaseManager<T> : IBaseManager<T> where T : class
    {
        private readonly IBaseRepository<T> _iBaseRepository;

        public BaseManager(IBaseRepository<T> iBaseRepository)
        {
            _iBaseRepository = iBaseRepository;
        }

        public virtual async Task<IEnumerable<T>> GetAll()
        {
            return await _iBaseRepository.GetAll();
        }

        public virtual async Task<bool> Create(T entity)
        {
            return await _iBaseRepository.Create(entity);
        }

        public virtual async Task<T> GetById(int? id)
        {
            return await _iBaseRepository.GetById(id);
        }

        public virtual async Task<bool> Remove(T entity)
        {
            return await _iBaseRepository.Remove(entity);
        }

        public virtual async Task<bool> Update(T entity)
        {
            return await _iBaseRepository.Update(entity);
        }
    }
}
