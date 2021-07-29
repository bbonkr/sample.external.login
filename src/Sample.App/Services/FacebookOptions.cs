namespace Sample.App.Services
{
    public class FacebookServiceOptions
    {
        public const string Name = "Authentication:Facebook";

        public const string ExceptionMessage = @"Please check your appsetting.json 
Please add your appsettings as below
```json
    ""Authentication"": {
        ""Facebook"": {
            ""AppId"": ""<app-id>"",
            ""AppSecret"": ""<app-secret>""
        }
    }
```
";

        /// <summary>
        /// [Required] Facebook app id
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// [Required] Facebook app secret
        /// </summary>
        public string AppSecret { get; set; }

        /// <summary>
        /// [Optional] Access dined path
        /// </summary>
        public string AccessDeniedPath { get; set; }
    }
}
