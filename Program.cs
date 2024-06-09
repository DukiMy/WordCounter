/**
* This repo spawned from a discussion about the multiple meanings of δέ in the New Testament.
* I had the bad luck of encountering a stubborn sophist who insisted that δέ never means "and"
* in the New Testament.
* Thus spawned the idea for this project.
* Space for optimisations and improvements are plentiful.
* If you wish to contribute, please do so.
*/
using Microsoft.Playwright;
namespace BibleTextScraper;
/**
* The main class of the program.
*/
class Program
{
    private const string BASE_PAGE = "https://biblehub.com/text/";
    private const string START_PAGE = $"{BASE_PAGE}matthew/1-1.htm";
    private const string END_PAGE = $"{BASE_PAGE}genesis/1-1.htm";
    private const string NEXT_BUTTON_XPATH = "/html/body/div[4]/table/tbody/tr/td/div[1]/table/tbody/tr/td/div/a[2]";
    private const string GREEK_WORD = "δὲ";
    private const string OUTPUT_PATH = "output.txt";
    private static List<Word> words = new List<Word>();
    /**
    * The programs starting point.
    * @param args The command line arguments.
    * @return A task. Necessary for calling async methods.
    */
    public static async Task Main(string[] args)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true, Timeout = 30000 });
        IBrowserContext context = await browser.NewContextAsync();
        IPage page = await context.NewPageAsync();

        // Go to the start page
        await page.GotoAsync(START_PAGE);

        // Crawl the website
        await CrawlAsync(page);

        // Close the browser
        await browser.CloseAsync();
    }
    /**
    * Crawls the website.
    * @param page The page to crawl.
    * @return A task.
    */
    public static async Task CrawlAsync(IPage page)
    {
        string currentUrl;

        do
        {
            currentUrl = page.Url;

            Console.WriteLine($"Current URL: {currentUrl}");
            await ReadTableAsync(page);
            await Task.WhenAll(LogResultsAsync(), ClickOnNextPageAsync(page));

        } while (currentUrl != END_PAGE);
    }
    /**
    * Reads the table.
    * @param page The page to read.
    * @return A task.
    */
    public static async Task ReadTableAsync(IPage page)
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
            await page.ReloadAsync();
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
    public static async Task ClickOnNextPageAsync(IPage page)
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
    public static async Task LogResultsAsync()
    {
        string output = "";

        List<(string, int)> wordCount = words
            .GroupBy(w => w.EnglishTranslation)
            .Select(g => (g.Key, g.Count()))
            .ToList();

        foreach (var (word, count) in wordCount)
        {
            output = $"{output}\n{word}: {count}";
        }

        using (StreamWriter writer = new StreamWriter(OUTPUT_PATH))
        {
            await writer.WriteAsync(output);
        }
    }
}
