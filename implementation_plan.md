# Implementation Plan

## [Overview]
Создание полнофункционального веб-приложения для учета и классификации денежных средств с аналитической панелью.

Проект MoneyTree уже имеет прочную основу с реализованной аутентификацией пользователей, системой категорий, созданием транзакций и базовой аналитикой. Однако для полного соответствия требованиям необходимо добавить функциональность редактирования и удаления транзакций, управление категориями, улучшенную фильтрацию и дополнительные возможности аналитики. Текущая архитектура построена на CQRS с использованием MediatR, что обеспечивает хорошую масштабируемость и поддерживаемость кода.

## [Types]
Добавление и модификация типов данных для поддержки новых функций.

### Новые типы:
- **UpdateCategoryDto**: DTO для обновления категорий
  ```csharp
  public class UpdateCategoryDto
  {
      [Required]
      public string Name { get; set; } = string.Empty;
      [Required]
      public string Type { get; set; } = string.Empty;
  }
  ```

- **DeleteCategoryCommand**: Команда для удаления категорий
  ```csharp
  public record DeleteCategoryCommand : IRequest<bool>
  {
      public int CategoryId { get; init; }
      public string UserId { get; init; } = string.Empty;
  }
  ```

### Модифицированные типы:
- **CategoryDto**: Добавление флага IsEditable для UI
  ```csharp
  public bool IsEditable { get; set; } = true;
  ```

## [Files]
Создание и модификация файлов для реализации недостающего функционала.

### Новые файлы:
- `MoneyTreeAPI/Application/Categories/Commands/UpdateCategoryCommandHandler.cs` - Обработчик обновления категорий
- `MoneyTreeAPI/Application/Categories/Commands/DeleteCategoryCommandHandler.cs` - Обработчик удаления категорий
- `MoneyTreeAPI/Application/Transactions/Commands/DeleteTransactionCommandHandler.cs` - Обработчик удаления транзакций
- `MoneyTreeFront/Components/Pages/EditTransaction.razor` - Страница редактирования транзакций
- `MoneyTreeFront/Components/Pages/EditCategory.razor` - Страница редактирования категорий

### Модифицированные файлы:
- `MoneyTreeAPI/Controllers/TransactionsController.cs` - Добавление endpoints для редактирования/удаления
- `MoneyTreeAPI/Controllers/CategoriesController.cs` - Добавление endpoints для управления категориями
- `MoneyTreeFront/Components/Pages/Transactions.razor` - Добавление кнопок редактирования/удаления
- `MoneyTreeFront/Components/Pages/Categories.razor` - Добавление функций управления категориями
- `MoneyTreeFront/Services/CategoryService.cs` - Добавление методов Update/Delete
- `MoneyTreeFront/Services/TransactionService.cs` - Добавление методов Update/Delete

## [Functions]
Добавление и модификация функций для поддержки CRUD операций.

### Новые функции:
- **Backend (API)**:
  - `UpdateCategoryAsync(int id, UpdateCategoryDto dto)` - Обновление категории
  - `DeleteCategoryAsync(int id)` - Удаление категории
  - `UpdateTransactionAsync(int id, UpdateTransactionDto dto)` - Обновление транзакции
  - `DeleteTransactionAsync(int id)` - Удаление транзакции

- **Frontend (Services)**:
  - `CategoryService.UpdateAsync(int id, UpdateCategoryDto dto)`
  - `CategoryService.DeleteAsync(int id)`
  - `TransactionService.UpdateAsync(int id, UpdateTransactionDto dto)`
  - `TransactionService.DeleteAsync(int id)`

### Модифицированные функции:
- **TransactionService.GetAllAsync()**: Добавление параметров фильтрации по категории
- **CategoriesController**: Добавление валидации на удаление категорий, используемых в транзакциях

## [Classes]
Расширение существующих классов для поддержки новых функций.

### Модифицированные классы:
- **TransactionService** (Frontend):
  - Добавление методов UpdateTransactionAsync и DeleteTransactionAsync
  - Добавление расширенной фильтрации

- **CategoryService** (Frontend):
  - Добавление методов UpdateCategoryAsync и DeleteCategoryAsync
  - Добавление проверки на системные категории

## [Dependencies]
Нет новых зависимостей - все необходимое уже присутствует в проекте.

## [Testing]
Тестирование будет включать:

1. **Unit Tests**:
   - Тесты для новых команд MediatR
   - Тесты валидации (попытка удаления категории с транзакциями)

2. **Integration Tests**:
   - Тестирование API endpoints
   - Проверка изоляции данных между пользователями

3. **UI Tests**:
   - Тестирование форм редактирования
   - Проверка обновления интерфейса после операций

## [Implementation Order]

1. **Backend Implementation**:
   - Создать команды и обработчики для Update/Delete категорий
   - Создать команды и обработчики для Update/Delete транзакций
   - Добавить новые endpoints в контроллеры
   - Добавить валидацию и обработку ошибок

2. **Frontend Services**:
   - Расширить TransactionService методами Update/Delete
   - Расширить CategoryService методами Update/Delete
   - Добавить обработку ошибок и состояний загрузки

3. **UI Components**:
   - Создать страницы редактирования транзакций и категорий
   - Добавить кнопки действий в существующие страницы
   - Реализовать навигацию и обновление состояния

4. **Testing & Debugging**:
   - Провести модульное тестирование
   - Провести интеграционное тестирование
   - Провести тестирование пользовательского интерфейса

5. **Deployment**:
   - Обновить миграции базы данных
   - Развернуть изменения на сервере
   - Провести финальное тестирование