-- =============================================
-- ДЗ № 16. Нотификации
-- Создание таблицы Notifications и индекса
-- =============================================

CREATE TABLE "Notifications" (
    "Id"          UUID          NOT NULL,
    "UserId"      UUID          NOT NULL,
    "Type"        VARCHAR(200)  NOT NULL,
    "Text"        TEXT          NOT NULL,
    "ScheduledAt" TIMESTAMP     NOT NULL,
    "IsNotified"  BOOLEAN       NOT NULL DEFAULT FALSE,
    "NotifiedAt"  TIMESTAMP     NULL,

    CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Notifications_ToDoUsers" FOREIGN KEY ("UserId")
        REFERENCES "ToDoUsers" ("UserId")
);

CREATE INDEX "IX_Notifications_UserId"
    ON "Notifications" ("UserId");
