using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Npgsql;
using Microsoft.EntityFrameworkCore;
    
public class AppDbContext : DbContext
{
    public DbSet<User> User { get; set; }
    public DbSet<Task> Task { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = "Host=10.30.0.137;Port=5432;Username=gr631_kaaal;Password=Sh48F3kF;Database=gr631_kaaal";
        optionsBuilder.UseNpgsql(connectionString);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Task>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.ID_user);
    }
}

public class User
{
    [Key]
    public int ID_user { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public List<Task> Tasks { get; set; }

    public User(string username, string password)
    {
        Username = username;
        Password = password;
        Tasks = new List<Task>();
    }
}

public class Task
{
    [Key]
    public int NumberTask { get; set; }
    public string NameTask { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; } = DateTime.UtcNow;
    public int ID_user { get; set; }
    public User User { get; set; }
    public bool IsCompleted { get; set; }

    public Task (string nameTask, string description, DateTime dueDate)
    {
        NameTask = nameTask;
        Description = description;
        DueDate = dueDate.ToUniversalTime();
    }
    
    public Task()
    {
    }
}

public class Diary
{
    static void Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("1. Регистрация\n" +
                              "2. Авторизация\n" +
                              "3. Выйти");
            int choice;

            if (!int.TryParse(Console.ReadLine(), out choice))
            {
                Console.WriteLine("Некорректный ввод");
                continue;
            }

            switch (choice)
            {
                case 1:
                    Register();
                    break;
                case 2:
                    Login();
                    break;
                case 3:
                    return;
                default:
                    Console.WriteLine("Некорректный ввод");
                    break;
            }
        }
    }

    static void Register()
    {
        Console.Write("Введите логин: ");
        string username = Console.ReadLine();
        Console.Write("Введите пароль: ");
        string password = Console.ReadLine();

        using (var context = new AppDbContext())
        {
            var user = new User(username, password);
            context.User.Add(user);
            context.SaveChanges();
        }

        Console.WriteLine("Регистрация пройдена!");
    }

    static void Login()
    {
        Console.Write("Введите логин: ");
        string username = Console.ReadLine();
        Console.Write("Введите пароль: ");
        string password = Console.ReadLine();

        using (var context = new AppDbContext())
        {
            var user = context.User.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                Console.WriteLine("Авторизация пройдена!");
                ManageTasks(user);
            }
            else
            {
                Console.WriteLine("Неправильный логин или пароль");
            }
        }
    }

    static void ManageTasks(User user)
    {
        using (var context = new AppDbContext())
        {
            user.Tasks = context.Task.Where(t => t.ID_user == user.ID_user).ToList();
        }

        while (true)
        {
            Console.WriteLine("1. Добавить задачу\n" +
                              "2. Удалить задачу\n" +
                              "3. Изменить задачу\n" +
                              "4. Показать задачи на сегодня\n" +
                              "5. Показать задачи на завтра\n" +
                              "6. Показать задачи на неделю\n" +
                              "7. Показать все задачи\n" +
                              "8. Показать невыполненные задачи\n" +
                              "9. Показать выполненные задачи\n" +
                              "10. Выйти");

            int choice;
            if (!int.TryParse(Console.ReadLine(), out choice))
            {
                Console.WriteLine("Некорректный ввод");
                continue;
            }

            switch (choice)
            {
                case 1:
                    AddTask(user);
                    break;
                case 2:
                    DeleteTask(user);
                    break;
                case 3:
                    EditTask(user);
                    break;
                case 4:
                    ViewTasksForToday(user);
                    break;
                case 5:
                    ViewTasksForTomorrow(user);
                    break;
                case 6:
                    ViewTasksForThisWeek(user);
                    break;
                case 7:
                    ViewAllTasks(user);
                    break;
                case 8:
                    ViewPendingTasks(user);
                    break;
                case 9:
                    ViewCompletedTasks(user);
                    break;
                case 10:
                    return;
                default:
                    Console.WriteLine("Некорректный ввод");
                    break;
            }
        }
    }

    static void AddTask(User user)
    {
        Console.Write("Введите название задачи: ");
        string nameTask = Console.ReadLine();
        Console.Write("Введите описание задачи(если его нет, оставьте пустым): ");
        string description = Console.ReadLine();
        Console.Write("Введите срок выполнения задачи (yyyy-MM-dd): ");
        DateTime dueDate;
        if (!DateTime.TryParse(Console.ReadLine(), out dueDate))
        {
            Console.WriteLine("Некорректный ввод");
            return;
        }

        var newTask = new Task(nameTask, description, dueDate.ToUniversalTime());
        newTask.ID_user = user.ID_user;
        user.Tasks.Add(newTask);

        using (var context = new AppDbContext())
        {
            context.Task.Add(newTask);
            context.SaveChanges();
        }

        Console.WriteLine("Задача добавлена!");
    }

    static void DeleteTask(User user)
    {
        Console.Write("Введите название задачи: ");
        string nameTask = Console.ReadLine();

        var taskToRemove = user.Tasks.FirstOrDefault(t => t.NameTask == nameTask);
        if (taskToRemove != null)
        {
            user.Tasks.Remove(taskToRemove);

            using (var context = new AppDbContext())
            {
                context.Task.Remove(taskToRemove);
                context.SaveChanges();
            }

            Console.WriteLine("Задача удалена!");
        }
        else
        {
            Console.WriteLine("Задача не найдена");
        }
    }

    static void EditTask(User user)
    {
        Console.Write("Введите название задачи: ");
        string name = Console.ReadLine();
        var task = user.Tasks.FirstOrDefault(t => t.NameTask == name);

        if (task != null)
        {
            Console.Write("Введите новое название задачи (нажмите Enter для сохранения текущего имени): ");
            string newName = Console.ReadLine();
            if (!string.IsNullOrEmpty(newName))
            {
                task.NameTask = newName;
            }

            Console.Write("Введите новое описание задачи (нажмите Enter для сохранения текущего описания): ");
            string newDescription = Console.ReadLine();
            if (!string.IsNullOrEmpty(newDescription))
            {
                task.Description = newDescription;
            }

            Console.Write(
                "Введите новый срок выполнения задачи (yyyy-MM-dd) (нажмите Enter для сохранения текущего срока выполнения): ");
            string newDueDateInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(newDueDateInput))
            {
                if (!DateTime.TryParse(newDueDateInput, out DateTime newDueDate))
                {
                    Console.WriteLine("Некорректный ввод");
                    return;
                }

                task.DueDate = newDueDate;
            }

            Console.Write("Задача выполнена? (y/n): ");
            string isCompletedInput = Console.ReadLine();
            task.IsCompleted = isCompletedInput.ToLower() == "y";

            using (var context = new AppDbContext())
            {
                context.Task.Update(task);
                context.SaveChanges();
            }

            Console.WriteLine("Задача изменена!");
        }
        else
        {
            Console.WriteLine("Задача не найдена");
        }
    }

    static void ViewTasksForToday(User user)
    {
        ViewTasks(user, DateTime.Today, DateTime.Today.AddDays(1));
    }

    static void ViewTasksForTomorrow(User user)
    {
        ViewTasks(user, DateTime.Today.AddDays(1), DateTime.Today.AddDays(2));
    }

    static void ViewTasksForThisWeek(User user)
    {
        ViewTasks(user, DateTime.Today, DateTime.Today.AddDays(7));
    }

    static void ViewAllTasks(User user)
    {
        ViewTasks(user, null, null);
    }

    static void ViewCompletedTasks(User user)
    {
        ViewTasks(user, null, null, true);
    }

    static void ViewPendingTasks(User user)
    {
        ViewTasks(user, null, null, false);
    }

    static void ViewTasks(User user, DateTime? startDate = null, DateTime? endDate = null, bool? isCompleted = null)
    {
        List<Task> tasks = user.Tasks.Where(t =>
        {
            if (startDate.HasValue && t.DueDate < startDate.Value) return false;
            if (endDate.HasValue && t.DueDate >= endDate.Value) return false;
            if (isCompleted.HasValue && t.IsCompleted != isCompleted.Value) return false;
            return true;
        }).ToList();

        if (tasks.Count > 0)
        {
            foreach (Task task in tasks)
            {
                Console.WriteLine(
                    $"Название: {task.NameTask}, Описание: {task.Description}, Срок выполнения: {task.DueDate:yyyy-MM-dd}, Статус: {(task.IsCompleted ? "Выполнено" : "Не выполнено")}");
            }
        }
        else
        {
            Console.WriteLine("Задачи не найдены");
        }
    }
}