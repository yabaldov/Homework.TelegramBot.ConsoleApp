-- =============================================
-- ДЗ № 13. Модель базы данных
-- Создание таблиц, внешних ключей и индексов
-- =============================================

-- Таблица пользователей
CREATE TABLE "ToDoUsers" (
    "UserId"            UUID        NOT NULL,
    "TelegramUserId"    BIGINT      NOT NULL,
    "TelegramUserName"  VARCHAR(200) NOT NULL,
    "RegisteredAt"      TIMESTAMP   NOT NULL,

    CONSTRAINT "PK_ToDoUsers" PRIMARY KEY ("UserId")
);

-- Таблица списков задач
CREATE TABLE "ToDoLists" (
    "Id"        UUID            NOT NULL,
    "Name"      VARCHAR(10)     NOT NULL,
    "UserId"    UUID            NOT NULL,
    "CreatedAt" TIMESTAMP       NOT NULL,

    CONSTRAINT "PK_ToDoLists" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ToDoLists_ToDoUsers" FOREIGN KEY ("UserId")
        REFERENCES "ToDoUsers" ("UserId")
);

-- Таблица задач
CREATE TABLE "ToDoItems" (
    "Id"             UUID          NOT NULL,
    "Name"           VARCHAR(100)  NOT NULL,
    "UserId"         UUID          NOT NULL,
    "ListId"         UUID          NULL,
    "CreatedAt"      TIMESTAMP     NOT NULL,
    "Deadline"       TIMESTAMP     NOT NULL,
    "State"          INT           NOT NULL DEFAULT 0,
    "StateChangedAt" TIMESTAMP     NULL,

    CONSTRAINT "PK_ToDoItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ToDoItems_ToDoUsers" FOREIGN KEY ("UserId")
        REFERENCES "ToDoUsers" ("UserId"),
    CONSTRAINT "FK_ToDoItems_ToDoLists" FOREIGN KEY ("ListId")
        REFERENCES "ToDoLists" ("Id")
);

-- =============================================
-- Индексы
-- =============================================

-- Уникальный индекс для поиска пользователя по Telegram ID
CREATE UNIQUE INDEX "UX_ToDoUsers_TelegramUserId"
    ON "ToDoUsers" ("TelegramUserId");

-- Индексы для внешних ключей
CREATE INDEX "IX_ToDoLists_UserId"
    ON "ToDoLists" ("UserId");

CREATE INDEX "IX_ToDoItems_UserId"
    ON "ToDoItems" ("UserId");

CREATE INDEX "IX_ToDoItems_ListId"
    ON "ToDoItems" ("ListId");
