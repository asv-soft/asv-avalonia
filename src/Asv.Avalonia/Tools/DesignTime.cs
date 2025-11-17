using System.Windows.Input;
using Asv.Cfg;
using Avalonia.Controls;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public static class DesignTime
{
    static DesignTime()
    {
        ThrowIfNotDesignMode();
    }

    const string Letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    // Realistic prefixes (as in real nicknames/titles)
    private static readonly string[] Prefixes =
    {
        "Neo",
        "Pro",
        "Max",
        "Ultra",
        "Super",
        "Alpha",
        "Beta",
        "Gamma",
        "Delta",
        "Omega",
        "Cyber",
        "Tech",
        "Ninja",
        "Ghost",
        "Shadow",
        "Storm",
        "Blade",
        "Viper",
        "Wolf",
        "Fox",
        "Dark",
        "Light",
        "Red",
        "Blue",
        "Green",
        "Silver",
        "Gold",
        "Iron",
        "Steel",
        "Titan",
        "Quantum",
        "Pixel",
        "Byte",
        "Code",
        "Dev",
        "Bot",
        "AI",
        "ML",
        "VR",
        "AR",
    };

    // Realistic suffixes
    private static readonly string[] Suffixes =
    {
        "X",
        "Z",
        "Pro",
        "Elite",
        "Master",
        "King",
        "Queen",
        "Lord",
        "Knight",
        "Warrior",
        "3000",
        "2K",
        "4K",
        "HD",
        "Ultra",
        "Prime",
        "One",
        "Zero",
        "Bit",
        "Byte",
        "Core",
        "Bot",
        "Droid",
        "Mech",
        "Unit",
        "Agent",
        "Spy",
        "Hawk",
        "Eagle",
        "Shark",
        "Lion",
    };

    // Common English words for natural-sounding messages
    private static readonly string[] Words =
    {
        "the",
        "be",
        "to",
        "of",
        "and",
        "a",
        "in",
        "that",
        "have",
        "I",
        "it",
        "for",
        "not",
        "on",
        "with",
        "he",
        "as",
        "you",
        "do",
        "at",
        "this",
        "but",
        "his",
        "by",
        "from",
        "they",
        "we",
        "say",
        "her",
        "she",
        "or",
        "an",
        "will",
        "my",
        "one",
        "all",
        "would",
        "there",
        "their",
        "what",
        "so",
        "up",
        "out",
        "if",
        "about",
        "who",
        "get",
        "which",
        "go",
        "me",
        "when",
        "make",
        "can",
        "like",
        "time",
        "no",
        "just",
        "him",
        "know",
        "take",
        "people",
        "into",
        "year",
        "your",
        "good",
        "some",
        "could",
        "them",
        "see",
        "other",
        "than",
        "then",
        "now",
        "look",
        "only",
        "come",
        "its",
        "over",
        "think",
        "also",
        "back",
        "after",
        "use",
        "two",
        "how",
        "our",
        "work",
        "first",
        "well",
        "way",
        "even",
        "new",
        "want",
        "because",
        "any",
        "these",
        "give",
        "day",
        "most",
        "us",
    };

    // Simple sentence templates for more natural flow
    private static readonly string[] Templates =
    {
        "{0} {1} {2}.",
        "I {0} {1} {2} yesterday.",
        "Did you {0} the {1}?",
        "The {0} is {1} today.",
        "We should {0} {1} soon.",
        "{0} {1}, {2} and {3}.",
        "How about {0} {1} {2}?",
        "I just {0} a {1} {2}.",
        "Why is {0} {1} {2}?",
        "Let's {0} {1} {2} later.",
    };

    // Digits and symbols for variations
    private static readonly string Numbers = "0123456789";
    private static readonly string Symbols = "_-.";

    public static IEnumerable<string> NameArray10 =>
        Enumerable.Range(0, 10).Select(i => $"Name{i}");

    public static IReadOnlyObservableList<IMenuItem> Menu =>
        new ObservableList<IMenuItem>(
            [
                new MenuItem("id_1", "Menu1", LoggerFactory)
                {
                    Icon = RandomImage,
                    Command = EmptyCommand,
                },
                new MenuItem("id_2", "Menu2", LoggerFactory)
                {
                    Icon = RandomImage,
                    Command = EmptyCommand,
                },
                new MenuItem("id_3", "Menu3", LoggerFactory)
                {
                    Icon = RandomImage,
                    Command = EmptyCommand,
                },
                new MenuItem("id_3_1", "Menu3_1", LoggerFactory, "id_1")
                {
                    Icon = RandomImage,
                    Command = EmptyCommand,
                },
                new MenuItem("id_3_2", "Menu3_2", LoggerFactory, "id_1")
                {
                    Icon = RandomImage,
                    Command = EmptyCommand,
                },
                new MenuItem("id_3_3", "Menu3_3", LoggerFactory, "id_1")
                {
                    Icon = RandomImage,
                    Command = EmptyCommand,
                },
            ]
        );

    public static NotifyCollectionChangedSynchronizedViewList<
        ObservableTreeNode<IMenuItem, NavigationId>
    > MenuTreeItems => MenuTree.Items;

    public static MenuTree MenuTree => new(Menu);

    public static NavigationId Id => NavigationId.GenerateRandom();
    public static ICommand EmptyCommand = new ReactiveCommand();
    public static IDialogService DialogService => NullDialogService.Instance;

    public static void ThrowIfNotDesignMode()
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This method is for design mode only");
        }
    }

    public static MaterialIconKind RandomImage =>
        Enum.GetValues(typeof(MaterialIconKind))
            .Cast<MaterialIconKind>()
            .Skip(Random.Shared.Next(1, Enum.GetValues<MaterialIconKind>().Length))
            .First();

    public static IShell Shell => DesignTimeShellViewModel.Instance;
    public static IAppStartupService AppStartupService => NullAppStartupService.Instance;
    public static IConfiguration Configuration { get; } = new InMemoryConfiguration();
    public static ILoggerFactory LoggerFactory => NullLoggerFactory.Instance;
    public static IShellHost ShellHost => NullShellHost.Instance;
    public static INavigationService Navigation => NullNavigationService.Instance;
    public static IUnitService UnitService => NullUnitService.Instance;
    public static IContainerHost ContainerHost => NullContainerHost.Instance;
    public static IThemeService ThemeService => NullThemeService.Instance;
    public static ILocalizationService LocalizationService => NullLocalizationService.Instance;
    public static ILogReaderService LogReaderService => NullLogReaderService.Instance;
    public static ICommandService CommandService => NullCommandService.Instance;
    public static IFileAssociationService FileAssociation => NullFileAssociationService.Instance;

    public static IAppPath AppPath => NullAppPath.Instance;
    public static IAppInfo AppInfo => NullAppInfo.Instance;

    /// <summary>
    /// Generates a plausible short title/nickname (3–12 characters)
    /// </summary>
    public static string RandomShortName(int minLength = 4, int maxLength = 10)
    {
        int length = Random.Shared.Next(minLength, maxLength + 1);
        var parts = new List<string>();

        // 60% chance — prefix + suffix
        // 25% chance — prefix + number
        // 15% chance — random combination
        double chance = Random.Shared.NextDouble();

        if (chance < 0.6 && length >= 5)
        {
            string prefix = Prefixes[Random.Shared.Next(Prefixes.Length)];
            string suffix = Suffixes[Random.Shared.Next(Suffixes.Length)];

            // Truncate if too long
            string candidate = prefix + suffix;
            if (candidate.Length > length)
            {
                candidate = candidate.Substring(0, length);
            }

            return candidate;
        }
        else if (chance < 0.85 && length >= 4)
        {
            string prefix = Prefixes[Random.Shared.Next(Prefixes.Length)];
            int numDigits = Random.Shared.Next(1, Math.Min(4, length - prefix.Length + 1));
            string number = new string(
                Enumerable
                    .Repeat(Numbers, numDigits)
                    .Select(s => s[Random.Shared.Next(s.Length)])
                    .ToArray()
            );

            string candidate = prefix + number;
            if (candidate.Length > length)
            {
                candidate = candidate.Substring(0, length);
            }

            return candidate;
        }
        else
        {
            // Random mix: letters + digits + 1 symbol (optional)
            var result = new char[length];
            bool hasSymbol = length >= 5 && Random.Shared.NextDouble() < 0.4;
            int symbolPos = hasSymbol ? Random.Shared.Next(1, length - 1) : -1;
            int digitsCount = Random.Shared.Next(1, Math.Min(3, (length / 2) + 1));

            for (int i = 0; i < length; i++)
            {
                if (i == symbolPos)
                {
                    result[i] = Symbols[Random.Shared.Next(Symbols.Length)];
                }
                else if (digitsCount > 0 && Random.Shared.NextDouble() < 0.3)
                {
                    result[i] = Numbers[Random.Shared.Next(Numbers.Length)];
                    digitsCount--;
                }
                else
                {
                    result[i] = Letters[Random.Shared.Next(Letters.Length)];
                }
            }

            // Ensure at least one digit if none were added
            if (digitsCount > 0)
            {
                int pos = Random.Shared.Next(length);
                if (result[pos] == Symbols[0] || char.IsPunctuation(result[pos]))
                {
                    pos = (pos + 1) % length;
                }

                result[pos] = Numbers[Random.Shared.Next(Numbers.Length)];
            }

            return new string(result);
        }
    }

    /// <summary>
    /// Generates a random natural-sounding message with a variable number of words.
    /// </summary>
    /// <param name="minWords">Minimum number of words (at least 1)</param>
    /// <param name="maxWords">Maximum number of words</param>
    /// <returns>A random message string, or null if parameters are invalid</returns>
    public static string RandomMessageText(int minWords, int maxWords)
    {
        if (minWords < 1 || maxWords < minWords)
        {
            return string.Empty;
        }

        int wordCount = Random.Shared.Next(minWords, maxWords + 1);

        // 70% chance to use a template for more natural structure
        if (wordCount >= 3 && Random.Shared.NextDouble() < 0.7)
        {
            var suitableTemplates = Templates
                .Where(t => CountPlaceholders(t) <= wordCount)
                .ToList();

            if (suitableTemplates.Count > 0)
            {
                string template = suitableTemplates[Random.Shared.Next(suitableTemplates.Count)];
                int placeholders = CountPlaceholders(template);
                string[] selectedWords = new string[placeholders];

                for (int i = 0; i < placeholders; i++)
                {
                    selectedWords[i] = Words[Random.Shared.Next(Words.Length)];
                }

                string message = string.Format(template, selectedWords);

                // Trim or pad with extra words if needed
                string[] currentWords = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (currentWords.Length < wordCount)
                {
                    var extra = GenerateWords(wordCount - currentWords.Length);
                    message = message.TrimEnd('.', ' ') + " " + string.Join(" ", extra) + ".";
                }
                else if (currentWords.Length > wordCount)
                {
                    message = string.Join(" ", currentWords.Take(wordCount)) + ".";
                }

                return CapitalizeFirst(message);
            }
        }

        // Fallback: generate random words and form a sentence
        var words = GenerateWords(wordCount);
        string result = string.Join(" ", words) + ".";
        return CapitalizeFirst(result);
    }

    private static string[] GenerateWords(int count)
    {
        var result = new string[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = Words[Random.Shared.Next(Words.Length)];
        }
        return result;
    }

    private static int CountPlaceholders(string template)
    {
        int count = 0;
        for (int i = 0; i < template.Length - 1; i++)
        {
            if (template[i] == '{' && template[i + 1] >= '0' && template[i + 1] <= '9')
            {
                count++;
            }
        }
        return count;
    }

    private static string CapitalizeFirst(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return s;
        }

        return char.ToUpper(s[0]) + s.Substring(1);
    }

    public static T RandomEnum<T>()
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(Random.Shared.Next(values.Length))!;
    }
}
