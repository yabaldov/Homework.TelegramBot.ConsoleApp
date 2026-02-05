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
    public class FileToDoListRepository : IToDoListRepository
    {
        private readonly string _basePath;
        private readonly IUserRepository _userRepository;
        private readonly JsonSerializerOptions _jsonOptions;

        public FileToDoListRepository(string basePath, IUserRepository userRepository)
        {
            _basePath = basePath;
            _userRepository = userRepository;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task AddAsync(ToDoList list, CancellationToken ct)
        {
            var filePath = GetFilePath(list.Id);
            var dto = ToDoListDto.FromEntity(list);
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json, ct);
        }

        public async Task<ToDoList?> GetAsync(Guid id, CancellationToken ct)
        {
            var filePath = GetFilePath(id);

            if (!File.Exists(filePath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(filePath, ct);
            var dto = JsonSerializer.Deserialize<ToDoListDto>(json, _jsonOptions);

            if (dto == null)
            {
                return null;
            }

            var user = await _userRepository.GetUserAsync(dto.UserId, ct);
            if (user == null)
            {
                return null;
            }

            return dto.ToEntity(user);
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserIdAsync(Guid userId, CancellationToken ct)
        {
            var result = new List<ToDoList>();

            if (!Directory.Exists(_basePath))
            {
                return result;
            }

            var user = await _userRepository.GetUserAsync(userId, ct);
            if (user == null)
            {
                return result;
            }

            var files = Directory.GetFiles(_basePath, "*.json");

            foreach (var filePath in files)
            {
                ct.ThrowIfCancellationRequested();

                var json = await File.ReadAllTextAsync(filePath, ct);
                var dto = JsonSerializer.Deserialize<ToDoListDto>(json, _jsonOptions);

                if (dto != null && dto.UserId == userId)
                {
                    result.Add(dto.ToEntity(user));
                }
            }

            return result;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var filePath = GetFilePath(id);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            await Task.CompletedTask;
        }

        public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            var lists = await GetByUserIdAsync(userId, ct);
            return lists.Any(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private string GetFilePath(Guid listId)
        {
            return Path.Combine(_basePath, $"{listId}.json");
        }
    }
}
