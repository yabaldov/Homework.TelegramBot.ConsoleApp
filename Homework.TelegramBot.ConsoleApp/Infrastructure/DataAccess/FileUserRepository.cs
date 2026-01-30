using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Homework.TelegramBot.ConsoleApp.Core.DataAccess;
using Homework.TelegramBot.ConsoleApp.Core.Entities;
using Homework.TelegramBot.ConsoleApp.Infrastructure.DataAccess.Dto;

namespace Homework.TelegramBot.ConsoleApp.Infrastructure.DataAccess
{
    public class FileUserRepository : IUserRepository
    {
        private readonly string _basePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public FileUserRepository(string basePath)
        {
            _basePath = basePath;
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

        public async Task AddAsync(ToDoUser user, CancellationToken ct)
        {
            var filePath = GetFilePath(user.UserId);
            var dto = ToDoUserDto.FromEntity(user);
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json, ct);
        }

        public async Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct)
        {
            var filePath = GetFilePath(userId);

            if (!File.Exists(filePath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(filePath, ct);
            var dto = JsonSerializer.Deserialize<ToDoUserDto>(json, _jsonOptions);

            return dto?.ToEntity();
        }

        public async Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct)
        {
            if (!Directory.Exists(_basePath))
            {
                return null;
            }

            var files = Directory.GetFiles(_basePath, "*.json");

            foreach (var filePath in files)
            {
                ct.ThrowIfCancellationRequested();

                var json = await File.ReadAllTextAsync(filePath, ct);
                var dto = JsonSerializer.Deserialize<ToDoUserDto>(json, _jsonOptions);

                if (dto != null && dto.TelegramUserId == telegramUserId)
                {
                    return dto.ToEntity();
                }
            }

            return null;
        }

        private string GetFilePath(Guid userId)
        {
            return Path.Combine(_basePath, $"{userId}.json");
        }
    }
}
