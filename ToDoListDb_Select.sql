-- =============================================
-- ДЗ № 13. SQL-скрипты для методов IToDoRepository
-- =============================================

-- 1. GetAllByUserIdAsync — все задачи пользователя
SELECT "Id", "Name", "UserId", "ListId", "CreatedAt", "Deadline", "State", "StateChangedAt"
FROM "ToDoItems"
WHERE "UserId" = :userId;

-- 2. GetActiveByUserIdAsync — активные задачи пользователя
SELECT "Id", "Name", "UserId", "ListId", "CreatedAt", "Deadline", "State", "StateChangedAt"
FROM "ToDoItems"
WHERE "UserId" = :userId
  AND "State" = 0;

-- 3. GetAsync — задача по Id
SELECT "Id", "Name", "UserId", "ListId", "CreatedAt", "Deadline", "State", "StateChangedAt"
FROM "ToDoItems"
WHERE "Id" = :id;

-- 4. ExistsByNameAsync — проверка существования задачи с таким именем у пользователя
SELECT EXISTS (
    SELECT 1
    FROM "ToDoItems"
    WHERE "UserId" = :userId
      AND "Name" = :name
);

-- 5. CountActiveAsync — количество активных задач пользователя
SELECT COUNT(*)
FROM "ToDoItems"
WHERE "UserId" = :userId
  AND "State" = 0;

-- 6. GetByUserIdAndListAsync — задачи пользователя по списку
--    Если listId IS NULL, возвращаем задачи без списка
SELECT "Id", "Name", "UserId", "ListId", "CreatedAt", "Deadline", "State", "StateChangedAt"
FROM "ToDoItems"
WHERE "UserId" = :userId
  AND ("ListId" = :listId OR (:listId IS NULL AND "ListId" IS NULL));

-- 7. FindAsync — поиск задач по предикату (пример: поиск по префиксу имени)
SELECT "Id", "Name", "UserId", "ListId", "CreatedAt", "Deadline", "State", "StateChangedAt"
FROM "ToDoItems"
WHERE "UserId" = :userId
  AND "Name" LIKE :namePrefix || '%';
