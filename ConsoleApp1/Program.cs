using System;
using System.Security.Cryptography.X509Certificates;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Plus.v1;
using Google.Apis.Plus.v1.Data;
using Google.Apis.Services;

namespace ConsoleApp1
{
    class Program
    {
        private static String ACTIVITY_ID = "z12gtjhq3qn2xxl2o224exwiqruvtda0i";

        static void Main(string[] args)
        {
            Console.WriteLine("Plus API - Service Account");
            Console.WriteLine("==========================");

            String serviceAccountEmail = "SERVICE_ACCOUNT_EMAIL_HERE";

            var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

            ServiceAccountCredential credential = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(serviceAccountEmail)
               {
                   Scopes = new[] { PlusService.Scope.PlusMe }
               }.FromCertificate(certificate));

            // Create the service.
            var service = new PlusService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Plus API Sample",
            });

            Activity activity = service.Activities.Get(ACTIVITY_ID).Execute();
            Console.WriteLine("  Activity: " + activity.Object.Content);
            Console.WriteLine("  Video: " + activity.Object.Attachments[0].Url);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

        }
    }
}
