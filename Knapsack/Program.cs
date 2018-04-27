using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Knapsack
{
    public enum SpecifiedParameter
    {
        WEIGHT,
        VALUE
    }

    class KnapsackItem
    {
        public KnapsackItem(string name = "GenericItem", int weight = 0, int value = 0)
        {
            this.name = name;
            this.weight = weight;
            this.value = value;
        }

        public string name;
        public int weight;
        public int value;

        public static int Total(IEnumerable<KnapsackItem> list, SpecifiedParameter parameter)
        {
            int sum = 0;

            foreach (KnapsackItem item in list)
            {
                switch (parameter)
                {
                    case SpecifiedParameter.VALUE:
                        sum += item.value;
                        break;
                    case SpecifiedParameter.WEIGHT:
                        sum += item.weight;
                        break;
                }
            }

            return sum;
        }

        public static List<KnapsackItem> SortBy(IEnumerable<KnapsackItem> list, SpecifiedParameter parameter)
        {
            List<KnapsackItem> newList = null;
            switch (parameter)
            {
                case SpecifiedParameter.VALUE:
                    newList = list.OrderBy(x => x.value).ToList();
                    break;
                case SpecifiedParameter.WEIGHT:
                    newList = list.OrderBy(x => x.weight).ToList();
                    break;
            }

            return newList;
        }
    }

    class Program
    {
        static List<KnapsackItem> knapsackItems;
        static List<KnapsackItem> chosenItems;
        static List<KnapsackItem> seenItems;
        static int maxCapacity;

        static void Main(string[] args)
        {
            RestartProgram();
        }

        static void RestartProgram()
        {
            knapsackItems = new List<KnapsackItem>();
            chosenItems = new List<KnapsackItem>();
            seenItems = new List<KnapsackItem>();
            ParseItemsFromFile(GetFilePathFromUser());
            PrintItems(knapsackItems);
            FindBestItems();
            Console.WriteLine("Items I chose to put into knapsack:");
            PrintItems(chosenItems);
            Console.WriteLine($"Total Weight: {KnapsackItem.Total(chosenItems, SpecifiedParameter.WEIGHT)}");
            Console.WriteLine($"Total Value: {KnapsackItem.Total(chosenItems, SpecifiedParameter.VALUE)}");
        }

        static void FindBestItems()
        {
            List<KnapsackItem> valuedItems = KnapsackItem.SortBy(knapsackItems, SpecifiedParameter.VALUE);
            List<KnapsackItem> weightedItems = KnapsackItem.SortBy(knapsackItems, SpecifiedParameter.WEIGHT);
            int currentWeight = KnapsackItem.Total(chosenItems, SpecifiedParameter.WEIGHT);
            int currentValue = KnapsackItem.Total(chosenItems, SpecifiedParameter.VALUE);
            int minimumValue = 0;
            if (valuedItems.Count > 1)
            {
                minimumValue += valuedItems[0].value;
                minimumValue += valuedItems[1].value;
            }
            else if (valuedItems.Count > 0)
            {
                minimumValue += valuedItems[0].value;
            }

            for (int i = 0; i < weightedItems.Count; ++i)
            {
                KnapsackItem item = weightedItems[i];
                seenItems.Add(item);
                if (currentWeight + item.weight <= maxCapacity)
                {
                    chosenItems.Add(item);
                    currentWeight += item.weight;
                    currentValue += item.value;
                }
                else
                {
                    break;
                }
            }

            int chosenCount = chosenItems.Count;
            List<KnapsackItem> lightestByValue = KnapsackItem.SortBy(chosenItems, SpecifiedParameter.VALUE);
            int lightestValue = KnapsackItem.Total(lightestByValue, SpecifiedParameter.VALUE);
            for (int i = weightedItems.Count - 1; i >= chosenCount; ++i)
            {
                KnapsackItem heaviestItem = weightedItems[i];
                for (int j = 0; j < lightestByValue.Count; ++j)
                {
                    // Loop through lightest least valuable and keep removing them until you have enough room for heavy item
                    // If at that point the addition of a heavy item would have an overall NET LOSS, don't do it, move on
                    // If it is worth it to add the heavy item, then you will need to refresh the list and sort it again
                    // Removed items should be added to a list that will be checked after the heavy items are checked
                    // This way it will re-check the ones that were removed and there may be a large enough weight gap to slip it in
                    // This process will repeat until nothing can be done
                    // REMEMBER: Check and see if an item can be put in WITHOUT removing anything
                }
            }
        }

        static string GetFilePathFromUser()
        {
            Console.WriteLine("Please enter in a file path with a list of items:");

            string path = "";
            bool validInput = false;
            do
            {
                path = Console.ReadLine();

                if (string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("Woops, try entering in something!\n");
                }
                else if (!File.Exists(path))
                {
                    Console.WriteLine("That's not a valid file path, please try again:");
                }
                else
                {
                    validInput = true;
                }
            } while (!validInput);

            return path;
        }

        static void ParseItemsFromFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);

            int.TryParse(lines[0], out maxCapacity);
            int numOfItems = 0;
            int.TryParse(lines[1], out numOfItems);

            for (int i = 2; i < numOfItems; ++i)
            {
                string[] itemContents = lines[i].Split(',');

                KnapsackItem item = new KnapsackItem();
                item.name = itemContents[0];
                int.TryParse(itemContents[1], out item.weight);
                int.TryParse(itemContents[2], out item.value);

                knapsackItems.Add(item);
            }
        }

        static void PrintItems(List<KnapsackItem> items)
        {
            StringBuilder sb = new StringBuilder("\n");
            foreach (KnapsackItem item in items)
            {
                sb.Append(item.name);
                sb.Append(" - ");
                sb.Append(item.weight);
                sb.Append(" lbs. at $");
                sb.Append(item.value);
                sb.Append("\n");
            }

            Console.WriteLine(sb.ToString());
        }
    }
}
