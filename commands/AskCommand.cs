using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Gitpod.Tool.Commands
{   
    class AskCommand : Command<AskCommand.Settings>
    {
        private static readonly HttpClient client = new();

        public class Settings : CommandSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            var question = AnsiConsole.Ask<string>("What's your question?");

            Task<GitpodAIAnswer> result = Task.Run<GitpodAIAnswer>(async() => await AskGitpodAI(question));
            
            GitpodAIAnswer answer = result.Result;

            if (answer.error != null && answer.error.Length > 0) {
                AnsiConsole.WriteLine("The following error occured:");
                AnsiConsole.WriteLine(answer.error);
                AnsiConsole.WriteLine(answer.message);

                return 0;
            }

            Rule rule = new() {Title = "[green]Answer[/]", Justification = Justify.Left};
            
            AnsiConsole.Write(rule);

            AnsiConsole.WriteLine(" ");
            AnsiConsole.WriteLine(answer.answer);
            AnsiConsole.WriteLine(" ");

            if (answer.sources != null && answer.sources.Length > 0) {
                rule = new() {Title = "[green]Sources[/]", Justification = Justify.Left};
                
                AnsiConsole.Write(rule);

                for (int i = 0; i < answer.sources.Length; i++) {
                    AnsiConsole.WriteLine(answer.sources[i]);
                }
            }

            rule = new() {Title = "[green]Info[/]", Justification = Justify.Left};

            AnsiConsole.Write(rule);

            AnsiConsole.WriteLine("You have " + answer.RateLimitRemaining + " questions remaining of " + answer.RateLimitLimit + ". The rate limit will reset every " + answer.RateLimitReset + " seconds");

            return 0;
        }

        private async Task<GitpodAIAnswer> AskGitpodAI(string question)
        {
            var data = new StringContent(
                JsonConvert.SerializeObject(
                    new {
                        question = question
                    }
                ),
                Encoding.UTF8, 
                new MediaTypeHeaderValue("application/json")
            );
            
            var response = await client.PostAsync("https://docs-ai.gitpod.io/v1/ask", data);
            var responseString = await response.Content.ReadAsStringAsync();

            var responseObject = JsonConvert.DeserializeObject<GitpodAIAnswer>(responseString);

            responseObject.RateLimitLimit = response.Headers.GetValues("X-Ratelimit-Limit").FirstOrDefault();
            responseObject.RateLimitRemaining = response.Headers.GetValues("X-Ratelimit-Remaining").FirstOrDefault();
            responseObject.RateLimitReset = response.Headers.GetValues("X-Ratelimit-Reset").FirstOrDefault();

            return responseObject;
        }
    }

    internal class GitpodAIAnswer
    {
        public string answer { get; set; }
        public string[] sources { get; set; }
        public string message { get; set; }
        public string error { get; set; }
        public string RateLimitLimit { get; set; }
        public string RateLimitRemaining { get; set; }
        public string RateLimitReset { get; set; }
    }
}
