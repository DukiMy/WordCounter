/**
* This repo spawned from a discussion about the multiple meanings of δέ in the New Testament.
* I had the bad luck of encountering a stubborn sophist who insisted that δέ never means "and"
* in the New Testament.
* Thus spawned the idea for this project.
* Space for optimisations and improvements are plentiful.
* If you wish to contribute, please do so.
*/
using Microsoft.Playwright;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using static BibleTextScraper.Site;
namespace BibleTextScraper;
/**
* The main class of the program.
*/
class Program
{
    private const string NEXT_BUTTON_XPATH = "/html/body/div[4]/table/tbody/tr/td/div[1]/table/tbody/tr/td/div/a[2]";
    private const string GREEK_WORD = "δὲde";   // The scraped words returns "δὲ" in both latin and greek script. Like this: "δὲde"
    private const string OUTPUT_PATH = "output.txt";
    private static List<Word> words = new List<Word>();
    private static IBrowserContext? context;
    /**
    * The programs starting point.
    * @param args The command line arguments.
    * @return A task. Necessary for calling async methods.
    */
    public static async Task Main(string[] args)
    {
        // Crawl the website
        await CrawlAsync(
            await CreatePageAsync()
        );

        // Produce the results
        await Task.WhenAll(ToTxtAsync(), ToJsonAsync());
    }
    /**
    * Creates a new page.
    * @return A task.
    */
    private static async Task<IPage> CreatePageAsync()
    {
        context = await (await Playwright.CreateAsync())
            .Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true, Timeout = 30000 })
            .ContinueWith(browser => browser.Result.NewContextAsync())
            .Unwrap();

        return await context.NewPageAsync()
            .ContinueWith(pageTask => 
            {
                var page = pageTask.Result;
                return page.GotoAsync(START_PAGE).ContinueWith(_ => page);
            }).Unwrap();
    }
    /**
    * Crawls the website.
    * @param page The page to crawl.
    * @return A task.
    */
    private static async Task CrawlAsync(IPage page)
    {
        string currentUrl;

        do
        {
            currentUrl = page.Url;

            Console.WriteLine($"Current URL: {currentUrl}");
            await ReadTableAsync(page);
            await ClickOnNextPageAsync(page);

        } while (currentUrl != END_PAGE);
    }
    /**
    * Reads the table.
    * @param page The page to read.
    * @return A task.
    */
    private static async Task ReadTableAsync(IPage page)
    {
        string verse;
        IReadOnlyList<IElementHandle> rows;

        try
        {
            await page.WaitForSelectorAsync("table.maintext");
            rows = await page.QuerySelectorAllAsync("table.maintext tr");
            verse = await page.TitleAsync();
        }
        catch (TimeoutException te)
        {
            te.Data.Add("URL", page.Url);
            new ErrorLogger("error.log").LogError(te);
            await page.GoBackAsync();
            await Task.WhenAll(ToTxtAsync(), ToJsonAsync());

            return;
        }

        foreach (var row in rows)
        {
            var columns = await row.QuerySelectorAllAsync("td");
            if (columns.Count >= 3)
            {
                string? greekWord = await columns[1].TextContentAsync();
                string? englishTranslation = await columns[2].TextContentAsync();

                if (greekWord != null && greekWord.Contains(GREEK_WORD))
                {
                    words.Add(new Word(verse, greekWord ?? "", englishTranslation ?? ""));
                }
            }
        }
    }
    /**
    * Clicks on the next page.
    * @param page The page to click on.
    * @return A task.
    */
    private static async Task ClickOnNextPageAsync(IPage page)
    {
        ILocator nextLink = page.Locator($"xpath={NEXT_BUTTON_XPATH}");

        if (await nextLink.IsVisibleAsync())
        {
            await nextLink.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        }
        else
        {
            await page.ReloadAsync();
        }
    }
    /**
    * Logs the results.
    * @return A task.
    */
    private static async Task<List<(string, int)>> GetWordCountsAsync()
    {
        return await Task.Run(() =>
        {
            return words
                .GroupBy(w => w.EnglishTranslation)
                .Select(g => (g.Key, g.Count()))
                .ToList();
        });
    }
    /**
    * Writes the results to a text file.
    * @return A task.
    */
    private static async Task ToTxtAsync()
    {
        string output = "";

        foreach (var (word, count) in await GetWordCountsAsync())
        {
            output = $"{output}\n{word}: {count}";
        }

        using (StreamWriter writer = new StreamWriter(OUTPUT_PATH))
        {
            await writer.WriteAsync(output);
        }
    }
    /**
    * Writes the results to the console.
    * @return A task.
    */
    private static async Task ToConsoleAsync()
    {
        string output = "";

        foreach (var (word, count) in await GetWordCountsAsync())
        {
            output = $"{output}\n{word}: {count}";
        }

        Console.WriteLine(output);
    }
    /**
    * Writes the results to a JSON file.
    * @return A task.
    */
    private static async Task ToJsonAsync()
    {
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true
        };
        
        string json = JsonSerializer.Serialize(words, options);
        await File.WriteAllTextAsync("output.json", json);
    }
}
