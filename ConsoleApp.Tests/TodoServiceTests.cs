using ConsoleApp.Models;
using ConsoleApp.Repositories;
using ConsoleApp.Services;
using Moq;

namespace ConsoleApp.Tests;

public class TodoServiceTests
{
    private readonly Mock<ITodoRepository> _repoMock;
    private readonly TodoService _sut;

    public TodoServiceTests()
    {
        _repoMock = new Mock<ITodoRepository>();
        _sut = new TodoService(_repoMock.Object);
    }

    // Path 1 - AC3: Todo does not exist

    [Fact]
    public async Task CompleteTodoAsync_WhenTodoDoesNotExist_ThrowsArgumentException()
    {
        var todoId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(todoId)).ReturnsAsync((TodoItem?)null);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CompleteTodoAsync(todoId));
    }

    [Fact]
    public async Task CompleteTodoAsync_WhenTodoDoesNotExist_UpdateAsyncIsNeverCalled()
    {
        var todoId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(todoId)).ReturnsAsync((TodoItem?)null);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CompleteTodoAsync(todoId));

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Never);
    }

    [Fact]
    public async Task CompleteTodoAsync_WithEmptyGuid_WhenTodoDoesNotExist_ThrowsArgumentException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(Guid.Empty)).ReturnsAsync((TodoItem?)null);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CompleteTodoAsync(Guid.Empty));
    }

    // Path 2 - AC1: Todo exists and is not yet completed

    [Fact]
    public async Task CompleteTodoAsync_WhenTodoIsNotCompleted_ReturnsTrue()
    {
        var todoId = Guid.NewGuid();
        var todo = new TodoItem { Id = todoId, IsCompleted = false };
        _repoMock.Setup(r => r.GetByIdAsync(todoId)).ReturnsAsync(todo);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<TodoItem>())).Returns(Task.CompletedTask);

        var result = await _sut.CompleteTodoAsync(todoId);

        Assert.True(result);
    }

    [Fact]
    public async Task CompleteTodoAsync_WhenTodoIsNotCompleted_SetsIsCompletedToTrue()
    {
        var todoId = Guid.NewGuid();
        var todo = new TodoItem { Id = todoId, IsCompleted = false };
        _repoMock.Setup(r => r.GetByIdAsync(todoId)).ReturnsAsync(todo);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<TodoItem>())).Returns(Task.CompletedTask);

        await _sut.CompleteTodoAsync(todoId);

        Assert.True(todo.IsCompleted);
    }

    [Fact]
    public async Task CompleteTodoAsync_WhenTodoIsNotCompleted_SetsCompletedAtToApproximatelyUtcNow()
    {
        var todoId = Guid.NewGuid();
        var todo = new TodoItem { Id = todoId, IsCompleted = false };
        _repoMock.Setup(r => r.GetByIdAsync(todoId)).ReturnsAsync(todo);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<TodoItem>())).Returns(Task.CompletedTask);

        var before = DateTime.UtcNow;
        await _sut.CompleteTodoAsync(todoId);
        var after = DateTime.UtcNow;

        Assert.NotNull(todo.CompletedAt);
        Assert.InRange(todo.CompletedAt.Value, before, after);
    }

    [Fact]
    public async Task CompleteTodoAsync_WhenTodoIsNotCompleted_CallsUpdateAsyncOnce()
    {
        var todoId = Guid.NewGuid();
        var todo = new TodoItem { Id = todoId, IsCompleted = false };
        _repoMock.Setup(r => r.GetByIdAsync(todoId)).ReturnsAsync(todo);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<TodoItem>())).Returns(Task.CompletedTask);

        await _sut.CompleteTodoAsync(todoId);

        _repoMock.Verify(r => r.UpdateAsync(todo), Times.Once);
    }

    // Path 3 - AC2: Todo exists and is already completed

    [Fact]
    public async Task CompleteTodoAsync_WhenTodoIsAlreadyCompleted_ReturnsFalse()
    {
        var todoId = Guid.NewGuid();
        var todo = new TodoItem { Id = todoId, IsCompleted = true, CompletedAt = DateTime.UtcNow.AddMinutes(-5) };
        _repoMock.Setup(r => r.GetByIdAsync(todoId)).ReturnsAsync(todo);

        var result = await _sut.CompleteTodoAsync(todoId);

        Assert.False(result);
    }

    [Fact]
    public async Task CompleteTodoAsync_WhenTodoIsAlreadyCompleted_UpdateAsyncIsNeverCalled()
    {
        var todoId = Guid.NewGuid();
        var todo = new TodoItem { Id = todoId, IsCompleted = true, CompletedAt = DateTime.UtcNow.AddMinutes(-5) };
        _repoMock.Setup(r => r.GetByIdAsync(todoId)).ReturnsAsync(todo);

        await _sut.CompleteTodoAsync(todoId);

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Never);
    }
}
