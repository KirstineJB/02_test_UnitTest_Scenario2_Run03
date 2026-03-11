using ConsoleApp.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Services
{
    public class TodoService
    {
        private readonly ITodoRepository _todoRepository;
        public TodoService(ITodoRepository todoRepository)
        {
            _todoRepository = todoRepository;
        }
        public async Task<bool> CompleteTodoAsync(Guid todoId)
        {
            var todo = await _todoRepository.GetByIdAsync(todoId);
            if (todo == null)
                throw new ArgumentException("todo er ikke fundet");

            if (!todo.IsCompleted)
            {
                todo.IsCompleted = true;
                todo.CompletedAt = DateTime.UtcNow;
                await _todoRepository.UpdateAsync(todo);
                return true;
            }
            return false;
        }
    }
}
