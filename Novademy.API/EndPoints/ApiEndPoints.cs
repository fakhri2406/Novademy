namespace Novademy.API.EndPoints;

public class ApiEndPoints
{
    private const string ApiBaseUrl = "api/v1";
    
    public static class Auth
    {
        private const string BaseUrl = $"{ApiBaseUrl}/auth";
        
        public const string Register = BaseUrl + "/register";
        public const string Login = $"{BaseUrl}/login";
    }
    
    public static class Course
    {
        private const string BaseUrl = $"{ApiBaseUrl}/course";
        
        public const string GetCourses = $"{BaseUrl}";
        public const string GetCourse = $"{BaseUrl}/{{id::guid}}";
        public const string CreateCourse = $"{BaseUrl}";
        public const string UpdateCourse = $"{BaseUrl}/{{id::guid}}";
        public const string DeleteCourse = $"{BaseUrl}/{{id::guid}}";
    }
    
    public static class Lesson
    {
        private const string BaseUrl = $"{ApiBaseUrl}/lesson";
        
        public const string GetLessons = $"{BaseUrl}/course/{{courseId::guid}}";
        public const string GetLesson = $"{BaseUrl}/{{id::guid}}";
        public const string CreateLesson = $"{BaseUrl}";
        public const string UpdateLesson = $"{BaseUrl}/{{id::guid}}";
        public const string DeleteLesson = $"{BaseUrl}/{{id::guid}}";
    }
    
    public static class Package
    {
        private const string BaseUrl = $"{ApiBaseUrl}/package";
        
        public const string GetPackages = $"{BaseUrl}";
        public const string GetPackage = $"{BaseUrl}/{{id::guid}}";
        public const string CreatePackage = $"{BaseUrl}";
        public const string UpdatePackage = $"{BaseUrl}/{{id::guid}}";
        public const string DeletePackage = $"{BaseUrl}/{{id::guid}}";
    }
    
    public static class Quiz
    {
        private const string BaseUrl = $"{ApiBaseUrl}/quiz";
        
        public const string GetQuizzes = $"{BaseUrl}/lesson/{{lessonId::guid}}";
        public const string GetQuiz = $"{BaseUrl}/{{id::guid}}";
        public const string CreateQuiz = $"{BaseUrl}";
        public const string UpdateQuiz = $"{BaseUrl}/{{id::guid}}";
        public const string DeleteQuiz = $"{BaseUrl}/{{id::guid}}";
        
        public const string GetQuestion = $"{BaseUrl}/question/{{id::guid}}";
        public const string CreateQuestion = $"{BaseUrl}/question";
    }
    
    public static class Subscription
    {
        private const string BaseUrl = $"{ApiBaseUrl}/subscription";
        
        public const string GetActiveSubscriptions = $"{BaseUrl}/active/{{userId::guid}}";
        public const string Subscribe = $"{BaseUrl}";
    }
}