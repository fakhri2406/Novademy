namespace Novademy.API.EndPoints;

public class ApiEndPoints
{
    private const string ApiBaseUrl = "api/v1";
    
    public static class Auth
    {
        private const string BaseUrl = $"{ApiBaseUrl}/auth";
        
        public const string Register = BaseUrl + "/register";
        public const string Login = $"{BaseUrl}/login";
        public const string VerifyEmail = $"{BaseUrl}/verify-email";
        public const string Refresh = $"{BaseUrl}/refresh";
        public const string Logout = $"{BaseUrl}/logout/{{id::guid}}";
        public const string GetCurrentUser = $"{BaseUrl}/me";
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
    
    public static class Subscription
    {
        private const string BaseUrl = $"{ApiBaseUrl}/subscription";
        
        public const string GetActiveSubscriptions = $"{BaseUrl}/active/{{userId::guid}}";
        public const string Subscribe = $"{BaseUrl}";
    }
}