using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ORLtrack.Tests.Infrastructure;
using plt.Models.ViewModel;

namespace ORLtrack.Tests;

public sealed class UserInterfaceAndUsageScenarioTests
{
    [Fact]
    [Trait("Category", "UI")]
    public void StudentsView_ContainsPeriodFilterAndNavigationToStudentDetails()
    {
        var viewText = File.ReadAllText(SolutionPaths.CombineFromRoot("plt", "Views", "Home", "Students.cshtml"));

        Assert.Contains("absenceStartDate", viewText);
        Assert.Contains("absenceEndDate", viewText);
        Assert.Contains("asp-action=\"StudentDetails\"", viewText);
        Assert.Contains("Открыть страницу", viewText);
    }

    [Fact]
    [Trait("Category", "UI")]
    public void StudentDetailsView_ContainsMadeUpToggleAndReturnFlow()
    {
        var viewText = File.ReadAllText(SolutionPaths.CombineFromRoot("plt", "Views", "Home", "StudentDetails.cshtml"));

        Assert.Contains("returnToDetails", viewText);
        Assert.Contains("is-made-up-hidden", viewText);
        Assert.Contains("Отработано", viewText);
        Assert.Contains("Пропуски за период", viewText);
        Assert.Contains("Назад к ученикам", viewText);
    }

    [Fact]
    [Trait("Category", "UI")]
    public void StudentDetailsStyles_DefineScrollableHistoryColumnsAndSidebarCards()
    {
        var cssText = File.ReadAllText(SolutionPaths.CombineFromRoot("plt", "wwwroot", "css", "site.css"));

        Assert.Contains(".student-detail-panel .history-list", cssText);
        Assert.Contains("max-height: min(62vh, 620px);", cssText);
        Assert.Contains(".summary-grid.student-detail-summary-grid", cssText);
        Assert.Contains(".student-detail-sidebar .absence-filter-form", cssText);
    }

    [Fact]
    [Trait("Category", "Scenario")]
    public async Task UsageScenario_SkipLessonThenMarkAsMadeUp_VisibleInStudentDetails()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var arrangeContext = database.CreateDbContext(authenticatedUserId: 7);
        await TestDataSeeder.CreateTeacherAsync(arrangeContext, 7);
        var student = await TestDataSeeder.CreateStudentAsync(arrangeContext, 7, firstName: "Олег", balance: 3000m, lessonRate: 1000m);

        await using (var skipContext = database.CreateDbContext(authenticatedUserId: 7))
        {
            var controller = HomeControllerTestHost.Create(new HomeControllerDependencies(skipContext), 7);
            await controller.SkipLesson(student.Id, new DateTime(2026, 04, 14, 16, 00, 0, DateTimeKind.Utc), "Семейные обстоятельства");
        }

        int lessonId;
        await using (var intermediateContext = database.CreateDbContext(authenticatedUserId: 7))
        {
            lessonId = await intermediateContext.StudentLessons.Select(x => x.Id).SingleAsync();
        }

        await using (var updateContext = database.CreateDbContext(authenticatedUserId: 7))
        {
            var controller = HomeControllerTestHost.Create(new HomeControllerDependencies(updateContext), 7);
            await controller.UpdateMissedLessonStatus(
                lessonId,
                true,
                new DateTime(2026, 04, 01),
                new DateTime(2026, 04, 30),
                student.Id,
                returnToDetails: true);
        }

        await using var detailsContext = database.CreateDbContext(authenticatedUserId: 7);
        var detailsController = HomeControllerTestHost.Create(new HomeControllerDependencies(detailsContext), 7);
        var result = await detailsController.StudentDetails(student.Id, new DateTime(2026, 04, 01), new DateTime(2026, 04, 30));

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<StudentDetailsViewModel>(view.Model);

        var missedLesson = Assert.Single(model.Student.FilteredMissedLessons);
        Assert.True(missedLesson.IsMadeUp);
        Assert.Equal("Семейные обстоятельства", missedLesson.Comment);
    }
}
