using Microsoft.AspNetCore.Identity;
using plt.Models.Model;

namespace ORLtrack.Tests.Infrastructure;

internal static class TestDataSeeder
{
    public static async Task<User> CreateTeacherAsync(EducationDbContext context, int teacherId = 1)
    {
        var teacher = new User
        {
            Id = teacherId,
            Name = "Test",
            LastName = "Teacher",
            Email = $"teacher{teacherId}@orltrack.test",
            Password = new PasswordHasher<User>().HashPassword(null!, "password"),
            AvatarUrl = "/Images/BaseAvatar.jpg"
        };

        context.Users.Add(teacher);
        await context.SaveChangesAsync();
        return teacher;
    }

    public static async Task<Student> CreateStudentAsync(
        EducationDbContext context,
        int teacherId,
        string firstName = "Иван",
        decimal balance = 0,
        decimal lessonRate = 1000)
    {
        var student = new Student
        {
            TeacherId = teacherId,
            FirstName = firstName,
            LastName = "Тестов",
            Email = $"{Guid.NewGuid():N}@student.test",
            Balance = balance,
            LessonRate = lessonRate,
            CreatedAtUtc = DateTime.UtcNow
        };

        context.Students.Add(student);
        await context.SaveChangesAsync();
        return student;
    }
}
