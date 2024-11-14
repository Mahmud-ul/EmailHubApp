using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailHubApp.Manager.Contract
{
    public interface IBaseManager<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(int? Id);
        Task<bool> Create(T entity);
        Task<bool> Update(T entity);
        Task<bool> Remove(T entity);
    }
}
