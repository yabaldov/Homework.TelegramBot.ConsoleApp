using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Homework.TelegramBot.ConsoleApp.Core.DataAccess;
using Homework.TelegramBot.ConsoleApp.Core.Entities;
using Homework.TelegramBot.ConsoleApp.Infrastructure.DataAccess.Dto;

namespace Homework.TelegramBot.ConsoleApp.Infrastructure.DataAccess
{
    public class FileToDoRepository : IToDoRepository
    {
        private readonly string _basePath;
        private readonly IUserRepository _userRepository;
        private readonly JsonSerializerOptions _jsonOptions;
        private Dictionary<Guid, Guid> _index; // ToDoItemId → UserId
        private IToDoListRepository? _toDoListRepository;

        public FileToDoRepository(string basePath, IUserRepository userRepository)
        {
            _basePath = basePath;
            _userRepository = userRepository;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            _index = new Dictionary<Guid, Guid>();

            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }

            LoadOrRebuildIndex();
        }

        public void SetToDoListRepository(IToDoListRepository toDoListRepository)
        {
            _toDoListRepository = toDoListRepository;
        }

        public async Task AddAsync(ToDoItem item, CancellationToken ct)
        {
            var userFolderPath = GetUserFolderPath(item.User.UserId);
            if (!Directory.Exists(userFolderPath))
            {
                Directory.CreateDirectory(userFolderPath);
            }

            var filePath = GetFilePath(item.User.UserId, item.Id);
            var dto = ToDoItemDto.FromEntity(item);
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json, ct);

            _index[item.Id] = item.User.UserId;
            await SaveIndexAsync(ct);
        }

        public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct)
        {
            if (!_index.TryGetValue(id, out var userId))
            {
                return null;
            }

            var filePath = GetFilePath(userId, id);
            if (!File.Exists(filePath))
            {
                return null;
            }

            return await ReadToDoItemAsync(filePath, ct);
        }

        public async Task UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            if (!_index.TryGetValue(item.Id, out var userId))
            {
                return;
            }

            var filePath = GetFilePath(userId, item.Id);
            var dto = ToDoItemDto.FromEntity(item);
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json, ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!_index.TryGetValue(id, out var userId))
            {
                return;
            }

            var filePath = GetFilePath(userId, id);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            _index.Remove(id);
            await SaveIndexAsync(ct);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return await GetItemsByUserIdAsync(userId, _ => true, ct);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return await GetItemsByUserIdAsync(userId, item => item.State == ToDoItemState.Active, ct);
        }

        public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            return await GetItemsByUserIdAsync(userId, predicate, ct);
        }

        public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            var items = await GetAllByUserIdAsync(userId, ct);
            return items.Any(item => item.Name == name);
        }

        public async Task<int> CountActiveAsync(Guid userId, CancellationToken ct)
        {
            var items = await GetActiveByUserIdAsync(userId, ct);
            return items.Count;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetByUserIdAndListAsync(Guid userId, Guid? listId, CancellationToken ct)
        {
            return await GetItemsByUserIdAsync(userId, item => item.List?.Id == listId, ct);
        }

        private async Task<List<ToDoItem>> GetItemsByUserIdAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            var result = new List<ToDoItem>();
            var userFolderPath = GetUserFolderPath(userId);

            if (!Directory.Exists(userFolderPath))
            {
                return result;
            }

            var files = Directory.GetFiles(userFolderPath, "*.json");

            foreach (var filePath in files)
            {
                ct.ThrowIfCancellationRequested();

                var item = await ReadToDoItemAsync(filePath, ct);
                if (item != null && predicate(item))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private async Task<ToDoItem?> ReadToDoItemAsync(string filePath, CancellationToken ct)
        {
            var json = await File.ReadAllTextAsync(filePath, ct);
            var dto = JsonSerializer.Deserialize<ToDoItemDto>(json, _jsonOptions);

            if (dto == null)
            {
                return null;
            }

            var user = await _userRepository.GetUserAsync(dto.UserId, ct);
            if (user == null)
            {
                return null;
            }

            ToDoList? list = null;
            if (dto.ListId.HasValue && _toDoListRepository != null)
            {
                list = await _toDoListRepository.GetAsync(dto.ListId.Value, ct);
            }

            return dto.ToEntity(user, list);
        }

        private string GetUserFolderPath(Guid userId)
        {
            return Path.Combine(_basePath, userId.ToString());
        }

        private string GetFilePath(Guid userId, Guid itemId)
        {
            return Path.Combine(_basePath, userId.ToString(), $"{itemId}.json");
        }

        private string GetIndexPath()
        {
            return Path.Combine(_basePath, "index.json");
        }

        private void LoadOrRebuildIndex()
        {
            var indexPath = GetIndexPath();

            if (File.Exists(indexPath))
            {
                var json = File.ReadAllText(indexPath);
                var indexData = JsonSerializer.Deserialize<IndexDto>(json, _jsonOptions);
                _index = indexData?.Items ?? new Dictionary<Guid, Guid>();
            }
            else
            {
                RebuildIndex();
                SaveIndexAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
        }

        private void RebuildIndex()
        {
            _index.Clear();

            if (!Directory.Exists(_basePath))
            {
                return;
            }

            var entries = Directory.GetDirectories(_basePath)
                .Where(folder => Guid.TryParse(Path.GetFileName(folder), out _))
                .SelectMany(folder =>
                {
                    var userId = Guid.Parse(Path.GetFileName(folder));
                    return Directory.GetFiles(folder, "*.json")
                        .Where(file => Guid.TryParse(Path.GetFileNameWithoutExtension(file), out _))
                        .Select(file => (itemId: Guid.Parse(Path.GetFileNameWithoutExtension(file)), userId));
                });

            foreach (var (itemId, userId) in entries)
            {
                _index[itemId] = userId;
            }
        }

        private async Task SaveIndexAsync(CancellationToken ct)
        {
            var indexPath = GetIndexPath();
            var indexData = new IndexDto { Items = _index };
            var json = JsonSerializer.Serialize(indexData, _jsonOptions);
            await File.WriteAllTextAsync(indexPath, json, ct);
        }

        private class IndexDto
        {
            public Dictionary<Guid, Guid> Items { get; set; } = new();
        }
    }
}
