using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ORLtrack.Tests.Infrastructure;
using plt.Models.ViewModel;

namespace ORLtrack.Tests;

public sealed class FunctionalPartTests
{
    [Fact]
    [Trait("Category", "Functional")]
    public async Task AddStudent_CreatesStudentAndStartingPayment()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var arrangeContext = database.CreateDbContext(authenticatedUserId: 1);
        await TestDataSeeder.CreateTeacherAsync(arrangeContext, 1);

        await using var actionContext = database.CreateDbContext(authenticatedUserId: 1);
        var controller = HomeControllerTestHost.Create(new HomeControllerDependencies(actionContext), 1);

        var result = await controller.AddStudent("Мария", "Иванова", "maria@test.local", "+79990000000", 1500m, 3000m);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Students", redirect.ActionName);

        await using var assertContext = database.CreateDbContext(authenticatedUserId: 1);
        var student = await assertContext.Students.SingleAsync();
        var payment = await assertContext.StudentPayments.SingleAsync();

        Assert.Equal("Мария", student.FirstName);
        Assert.Equal(3000m, student.Balance);
        Assert.Equal(3000m, student.TotalPaidIn);
        Assert.Equal(student.Id, payment.StudentId);
        Assert.Equal(3000m, payment.Amount);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task MarkLesson_ChargesBalanceAndAddsPaidLesson()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var arrangeContext = database.CreateDbContext(authenticatedUserId: 1);
        await TestDataSeeder.CreateTeacherAsync(arrangeContext, 1);
        var student = await TestDataSeeder.CreateStudentAsync(arrangeContext, 1, balance: 4000m, lessonRate: 1200m);

        await using var actionContext = database.CreateDbContext(authenticatedUserId: 1);
        var controller = HomeControllerTestHost.Create(new HomeControllerDependencies(actionContext), 1);
        var lessonMoment = new DateTime(2026, 04, 10, 12, 30, 0, DateTimeKind.Utc);

        var result = await controller.MarkLesson(student.Id, lessonMoment, "Обычное занятие");

        Assert.IsType<RedirectToActionResult>(result);

        await using var assertContext = database.CreateDbContext(authenticatedUserId: 1);
        var storedStudent = await assertContext.Students.SingleAsync();
        var lesson = await assertContext.StudentLessons.SingleAsync();

        Assert.Equal(2800m, storedStudent.Balance);
        Assert.Equal(1, storedStudent.LessonsAttendedCount);
        Assert.Equal(1200m, storedStudent.TotalCharged);
        Assert.Equal(1200m, lesson.ChargedAmount);
        Assert.False(lesson.IsMadeUp);
        Assert.Equal("Обычное занятие", lesson.Comment);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task SkipLesson_CreatesSkippedLessonWithoutChangingBalance()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var arrangeContext = database.CreateDbContext(authenticatedUserId: 1);
        await TestDataSeeder.CreateTeacherAsync(arrangeContext, 1);
        var student = await TestDataSeeder.CreateStudentAsync(arrangeContext, 1, balance: 2500m, lessonRate: 900m);

        await using var actionContext = database.CreateDbContext(authenticatedUserId: 1);
        var controller = HomeControllerTestHost.Create(new HomeControllerDependencies(actionContext), 1);

        var result = await controller.SkipLesson(student.Id, new DateTime(2026, 04, 11, 15, 00, 0, DateTimeKind.Utc), "Болел");

        Assert.IsType<RedirectToActionResult>(result);

        await using var assertContext = database.CreateDbContext(authenticatedUserId: 1);
        var storedStudent = await assertContext.Students.SingleAsync();
        var lesson = await assertContext.StudentLessons.SingleAsync();

        Assert.Equal(2500m, storedStudent.Balance);
        Assert.Equal(0m, lesson.ChargedAmount);
        Assert.False(lesson.IsMadeUp);
        Assert.Equal("Болел", lesson.Comment);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task UpdateMissedLessonStatus_UpdatesFlagAndRedirectsBackToDetails()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var arrangeContext = database.CreateDbContext(authenticatedUserId: 1);
        await TestDataSeeder.CreateTeacherAsync(arrangeContext, 1);
        var student = await TestDataSeeder.CreateStudentAsync(arrangeContext, 1, balance: 1800m, lessonRate: 900m);

        arrangeContext.StudentLessons.Add(new plt.Models.Model.StudentLesson
        {
            StudentId = student.Id,
            TeacherId = 1,
            LessonDateUtc = new DateTime(2026, 04, 12, 10, 00, 0, DateTimeKind.Utc),
            ChargedAmount = 0,
            IsMadeUp = false,
            Comment = "Пропуск",
            CreatedAtUtc = DateTime.UtcNow
        });
        await arrangeContext.SaveChangesAsync();

        var lessonId = await arrangeContext.StudentLessons.Select(x => x.Id).SingleAsync();

        await using var actionContext = database.CreateDbContext(authenticatedUserId: 1);
        var controller = HomeControllerTestHost.Create(new HomeControllerDependencies(actionContext), 1);

        var result = await controller.UpdateMissedLessonStatus(
            lessonId,
            true,
            new DateTime(2026, 04, 01),
            new DateTime(2026, 04, 30),
            student.Id,
            returnToDetails: true);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("StudentDetails", redirect.ActionName);
        Assert.Equal(student.Id, redirect.RouteValues?["id"]);

        await using var assertContext = database.CreateDbContext(authenticatedUserId: 1);
        var lesson = await assertContext.StudentLessons.SingleAsync();
        Assert.True(lesson.IsMadeUp);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task StudentDetails_ReturnsViewModelWithFilteredMissedLessons()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var arrangeContext = database.CreateDbContext(authenticatedUserId: 1);
        await TestDataSeeder.CreateTeacherAsync(arrangeContext, 1);
        var student = await TestDataSeeder.CreateStudentAsync(arrangeContext, 1, balance: 5000m, lessonRate: 1000m);

        arrangeContext.StudentPayments.Add(new plt.Models.Model.StudentPayment
        {
            StudentId = student.Id,
            TeacherId = 1,
            Amount = 5000m,
            PaymentDateUtc = new DateTime(2026, 04, 02, 9, 0, 0, DateTimeKind.Utc),
            Comment = "Первое пополнение",
            CreatedAtUtc = DateTime.UtcNow
        });

        arrangeContext.StudentLessons.AddRange(
            new plt.Models.Model.StudentLesson
            {
                StudentId = student.Id,
                TeacherId = 1,
                LessonDateUtc = new DateTime(2026, 04, 05, 11, 0, 0, DateTimeKind.Utc),
                ChargedAmount = 0,
                IsMadeUp = false,
                Comment = "Пропуск в диапазоне",
                CreatedAtUtc = DateTime.UtcNow
            },
            new plt.Models.Model.StudentLesson
            {
                StudentId = student.Id,
                TeacherId = 1,
                LessonDateUtc = new DateTime(2026, 03, 01, 11, 0, 0, DateTimeKind.Utc),
                ChargedAmount = 0,
                IsMadeUp = false,
                Comment = "Старый пропуск",
                CreatedAtUtc = DateTime.UtcNow
            },
            new plt.Models.Model.StudentLesson
            {
                StudentId = student.Id,
                TeacherId = 1,
                LessonDateUtc = new DateTime(2026, 04, 07, 11, 0, 0, DateTimeKind.Utc),
                ChargedAmount = 1000m,
                Comment = "Платное занятие",
                CreatedAtUtc = DateTime.UtcNow
            });

        await arrangeContext.SaveChangesAsync();

        await using var actionContext = database.CreateDbContext(authenticatedUserId: 1);
        var controller = HomeControllerTestHost.Create(new HomeControllerDependencies(actionContext), 1);

        var result = await controller.StudentDetails(student.Id, new DateTime(2026, 04, 01), new DateTime(2026, 04, 30));

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<StudentDetailsViewModel>(view.Model);

        Assert.Equal(student.Id, model.Student.Id);
        Assert.Single(model.Student.FilteredMissedLessons);
        Assert.Equal("Пропуск в диапазоне", model.Student.FilteredMissedLessons[0].Comment);
        Assert.Contains(model.Student.RecentPayments, x => x.Comment == "Первое пополнение");
        Assert.Contains(model.Student.RecentLessons, x => x.Comment == "Платное занятие");
    }
}
